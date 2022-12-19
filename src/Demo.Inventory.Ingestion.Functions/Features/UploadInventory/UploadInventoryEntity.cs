using System.Collections.Generic;
using System.Threading.Tasks;
using Demo.Inventory.Ingestion.Functions.Extensions;
using Infrastructure.Messaging.Azure.Blobs;
using LanguageExt;
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
    private readonly IBlobManager _blobManager;
    private readonly ILogger<UploadInventoryEntity> _logger;

    [JsonProperty]
    public BulkUploadRequest<Domain.Inventory> Request { get; set; }

    public UploadInventoryEntity(IBlobManager blobManager, ILogger<UploadInventoryEntity> logger)
    {
        _blobManager = blobManager;
        _logger = logger;
    }

    private async Task Upload()
    {
        
        (await Request.Items.SequenceParallel(UploadInventory).Run()).Match(
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
    }

    private Aff<Unit> UploadInventory(Domain.Inventory inventory)
    {
        var fileName = $"{inventory.LocationCode}/{inventory.ItemNumber}.json";
        var content = JsonConvert.SerializeObject(inventory);
        var uploadRequest = new FileUploadRequest(
            Request.CorrelationId,
            Request.Category,
            Request.Container,
            fileName,
            content
        );

        return _blobManager.Upload(uploadRequest);
    }

    public void Init(BulkUploadRequest<Domain.Inventory> request)
    {
        Request = request;
        Entity.Current.SignalEntity(Entity.Current.EntityId, nameof(Upload));
    }

    [FunctionName(nameof(UploadInventoryEntity))]
    public static async Task HandleEntityOperation([EntityTrigger] IDurableEntityContext context)
    {
        await context.DispatchAsync<UploadInventoryEntity>();
    }
}
