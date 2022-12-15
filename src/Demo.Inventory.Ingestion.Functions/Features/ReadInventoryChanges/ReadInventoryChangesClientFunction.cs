using System.IO;
using System.Threading.Tasks;
using Demo.Inventory.Ingestion.Functions.Extensions;
using Demo.Inventory.Ingestion.Functions.Features.AcceptInventoryChanges;
using Infrastructure.Messaging.Azure.Blobs;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace Demo.Inventory.Ingestion.Functions.Features.ReadInventoryChanges;

public class ReadInventoryChangesClientFunction
{
    private readonly SourceInventorySettings _settings;

    public ReadInventoryChangesClientFunction(SourceInventorySettings settings)
    {
        _settings = settings;
    }

    [FunctionName(nameof(ReadInventoryChangesClientFunction))]
    public async Task RunAsync(
        [QueueTrigger("%AcceptInventorySettings:Queue%")] string message,
        [DurableClient] IDurableEntityClient entityClient
    )
    {
        var acceptInventoryChangeRequest = message.ToModel<AcceptInventoryChangeRequest>();
        var readFileRequest = new ReadFileRequest(
            acceptInventoryChangeRequest.CorrelationId,
            _settings.Category,
            _settings.Container,
            acceptInventoryChangeRequest.FileName
        );

        var entityId = Path.GetFileNameWithoutExtension(readFileRequest.FileName)
            .GetEntityId<ReadInventoriesEntity>();

        await entityClient.SignalEntityAsync<IReadInventoryEntity>(
            entityId,
            x => x.Init(readFileRequest)
        );
    }
}
