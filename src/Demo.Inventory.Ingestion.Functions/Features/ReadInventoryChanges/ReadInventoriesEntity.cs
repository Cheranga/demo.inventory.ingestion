using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using CsvHelper.Configuration;
using Demo.Inventory.Ingestion.Functions.Core;
using Demo.Inventory.Ingestion.Functions.Extensions;
using Infrastructure.Messaging.Azure.Blobs;
using LanguageExt;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Azure;
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

    private readonly IAzureClientFactory<BlobServiceClient> _factory;
    private readonly ILogger<ReadInventoriesEntity> _logger;

    private Either<ErrorResponse, List<Domain.Inventory>> _operation = Either<
        ErrorResponse,
        List<Domain.Inventory>
    >.Left(ErrorResponse.ToError(404, "no data found"));

    public ReadInventoriesEntity(
        IAzureClientFactory<BlobServiceClient> factory,
        ILogger<ReadInventoriesEntity> logger
    )
    {
        _factory = factory;
        _logger = logger;
    }

    public void Init(ReadFileRequest request)
    {
        Request = request;
        Entity.Current.SignalEntity(Entity.Current.EntityId, nameof(GetInventories));
    }

    private async Task GetInventories()
    {
        _operation = (
            await _factory.ReadDataFromCsv<Domain.Inventory, InventoryMap>(Request).Run()
        ).Match(
            inventories =>
            {
                _logger.LogInformation(
                    "{CorrelationId} CSV data read successfully",
                    Request.CorrelationId
                );
                return Right(inventories);
            },
            error =>
            {
                _logger.LogError(
                    error.ToException(),
                    "{CorrelationId} CSV data read operation failed",
                    Request.CorrelationId
                );
                return Left<ErrorResponse, List<Domain.Inventory>>(
                    ErrorResponse.ToError(error.Code, error.Message)
                );
            }
        );

        // TODO: call the next actor depending on the operation
    }

    [FunctionName(nameof(ReadInventoriesEntity))]
    public static async Task HandleEntityOperation([EntityTrigger] IDurableEntityContext context)
    {
        await context.DispatchAsync<ReadInventoriesEntity>();
    }
}
