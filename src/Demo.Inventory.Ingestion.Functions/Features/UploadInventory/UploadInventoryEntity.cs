using System.Collections.Generic;
using System.Threading.Tasks;
using Demo.Inventory.Ingestion.Functions.Extensions;
using Infrastructure.Messaging.Azure.Blobs.Operations;
using Infrastructure.Messaging.Azure.Blobs.Requests;
using Infrastructure.Messaging.Azure.Blobs.Runtimes;
using LanguageExt;
using LanguageExt.Effects.Traits;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using static LanguageExt.Prelude;

namespace Demo.Inventory.Ingestion.Functions.Features.UploadInventory;

public record struct BulkUploadRequest<TData>(
    string CorrelationId,
    string Category,
    string Container,
    List<TData> Items
);

public interface IUploadInventoryEntity
{
    void Init(BulkUploadRequest<Domain.Inventory> request);
}

[JsonObject(MemberSerialization.OptIn)]
public class UploadInventoryEntity : IUploadInventoryEntity, IActor
{
    private readonly AzureStorageBlobRunTime _runTime;
    private readonly ILogger<UploadInventoryEntity> _logger;

    public UploadInventoryEntity(
        AzureStorageBlobRunTime runTime,
        ILogger<UploadInventoryEntity> logger
    )
    {
        _runTime = runTime;
        _logger = logger;
    }

    [JsonProperty]
    public BulkUploadRequest<Domain.Inventory> Request { get; set; }

    public void Init(BulkUploadRequest<Domain.Inventory> request)
    {
        Request = request;
        Entity.Current.SignalEntity(Entity.Current.EntityId, nameof(Upload));
    }

    private async Task Upload() =>
        (
            await Request.Items
                .SequenceParallel(UploadInventory<AzureStorageBlobRunTime>)
                .Run(_runTime)
        ).Match(
            _ =>
            {
                _logger.LogInformation(
                    "{CorrelationId} successfully uploaded file content",
                    Request.CorrelationId
                );

                Entity.Current.DeleteState();
            },
            error =>
            {
                _logger.LogError(
                    error.ToException(),
                    "{CorrelationId} file upload operation/s failed",
                    Request.CorrelationId
                );

                Entity.Current.DeleteState();
            }
        );

    private Aff<TRunTime, Unit> UploadInventory<TRunTime>(Domain.Inventory inventory)
        where TRunTime : struct, IHaveBlobOperations<TRunTime>, HasCancel<TRunTime> =>
        from fileName in Eff(() => $"{inventory.LocationCode}/{inventory.ItemNumber}.json")
        from content in Eff(() => JsonConvert.SerializeObject(inventory))
        from uploadRequest in Eff(
            () =>
                new FileUploadRequest(
                    Request.CorrelationId,
                    Request.Category,
                    Request.Container,
                    fileName,
                    content
                )
        )
        from op in BlobOperationsSchema<TRunTime>.Upload(uploadRequest)
        select op;

    [FunctionName(nameof(UploadInventoryEntity))]
    public static async Task HandleEntityOperation([EntityTrigger] IDurableEntityContext context) =>
        await context.DispatchAsync<UploadInventoryEntity>();
}
