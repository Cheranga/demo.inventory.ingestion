using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using CsvHelper.Configuration;
using Demo.Inventory.Ingestion.Domain;
using Demo.Inventory.Ingestion.Functions.Extensions;
using Demo.Inventory.Ingestion.Functions.Features.AcceptInventoryChanges;
using Demo.Inventory.Ingestion.Functions.Features.UploadInventory;
using Infrastructure.Messaging.Azure.Blobs.Operations;
using Infrastructure.Messaging.Azure.Blobs.Requests;
using Infrastructure.Messaging.Azure.Blobs.Runtimes;
using LanguageExt;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using static LanguageExt.Prelude;

namespace Demo.Inventory.Ingestion.Functions.Features.ReadInventoryChanges;

public interface IReadInventoryEntity
{
    void Init(ReadFileRequest request);
}

public sealed class InventoryMap : ClassMap<Domain.Inventory>
{
    public InventoryMap()
    {
        AutoMap(CultureInfo.InvariantCulture);
    }
}

[JsonObject(MemberSerialization.OptIn)]
public class ReadInventoriesEntity : IReadInventoryEntity, IActor
{
    [JsonProperty]
    public ReadFileRequest Request { get; set; }

    private readonly DestinationInventorySettings _settings;
    private readonly AzureStorageBlobRunTime _runTime;
    private readonly ILogger<ReadInventoriesEntity> _logger;

    private Either<ErrorResponse, List<Domain.Inventory>> _operation = Either<
        ErrorResponse,
        List<Domain.Inventory>
    >.Left(ErrorResponse.New(404, "no data found"));

    public ReadInventoriesEntity(
        DestinationInventorySettings settings,
        AzureStorageBlobRunTime runTime,
        ILogger<ReadInventoriesEntity> logger
    )
    {
        _settings = settings;
        _runTime = runTime;
        _logger = logger;
    }

    public void Init(ReadFileRequest request)
    {
        Request = request;
        Entity.Current.SignalEntity(Entity.Current.EntityId, nameof(GetInventories));
    }

    private async Task GetInventories() =>
        (
            await (
                from inventories in BlobOperationsSchema<AzureStorageBlobRunTime>.ReadDataFromCsv<Domain.Inventory, InventoryMap>(
                    Request
                )
                from _ in Upload(inventories)
                select _
            ).Run(_runTime)
        ).Match(
            _ =>
            {
                _logger.LogInformation(
                    "{CorrelationId} successfully uploaded file content",
                    Request.CorrelationId
                );
            },
            error =>
            {
                _logger.LogError(
                    error.ToException(),
                    "{CorrelationId} file upload operation/s failed",
                    Request.CorrelationId
                );
            }
        );

    // private Aff<Unit> Upload(List<Domain.Inventory> inventories)
    // {
    //     var groups = inventories
    //         .Select((x, i) => new { Index = i, Value = x })
    //         .GroupBy(x => x.Index / 100)
    //         .Select(x => x.Select(v => v.Value).ToList())
    //         .ToList();
    //     
    //     var index = 1;
    //     foreach (var group in groups)
    //     {
    //         var entityId = $"{Request.CorrelationId}::{index++}"
    //             .ToString()
    //             .GetEntityId<UploadInventoryEntity>();
    //     
    //         Entity.Current.SignalEntity<IUploadInventoryEntity>(
    //             entityId,
    //             entity =>
    //                 entity.Init(
    //                     new BulkUploadRequest<Domain.Inventory>(
    //                         Request.CorrelationId,
    //                         Request.Category,
    //                         _settings.Container,
    //                         group
    //                     )
    //                 )
    //         );
    //     }
    //     
    //     return SuccessAff(unit);
    // }
    
    private Aff<Unit> Upload(List<Domain.Inventory> inventories)
    {
        var groupId = 1;
        
        var operation =(
            from groups in Eff(() => inventories.GetGroups())
            from op in groups.SequenceParallel(@group =>
            {
                var entityId = $"{Request.CorrelationId}::{groupId++}"
                    .ToString()
                    .GetEntityId<UploadInventoryEntity>();
        
                return Eff(() =>
                {
                    Entity.Current.SignalEntity<IUploadInventoryEntity>(
                        entityId,
                        entity =>
                            entity.Init(
                                new BulkUploadRequest<Domain.Inventory>(
                                    Request.CorrelationId,
                                    Request.Category,
                                    _settings.Container,
                                    @group
                                )
                            )
                    );
                    return unit;
                }).ToAff();
            })
            select op
        ).Map(_ => unit);

        return operation;
    }

    // private Aff<Unit> UploadInventory(Domain.Inventory inventory)
    // {
    //     var fileName = $"{inventory.LocationCode}/{inventory.ItemNumber}.json";
    //     var content = JsonConvert.SerializeObject(inventory);
    //     var uploadRequest = new FileUploadRequest(
    //         Request.CorrelationId,
    //         _settings.Category,
    //         _settings.Container,
    //         fileName,
    //         content
    //     );
    //
    //     return _blobManager.Upload(uploadRequest);
    // }

    [FunctionName(nameof(ReadInventoriesEntity))]
    public static async Task HandleEntityOperation([EntityTrigger] IDurableEntityContext context)
    {
        await context.DispatchAsync<ReadInventoriesEntity>();
    }
}
