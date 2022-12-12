using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;

namespace Demo.Inventory.Ingestion.Functions.Features.ReadInventoryChanges;

public class ReadInventoryChangesClientFunction
{
    [FunctionName(nameof(ReadInventoryChangesClientFunction))]
    public async Task RunAsync([QueueTrigger("%AcceptInventorySettings:Queue%")] string message)
    {
        await Task.Delay(TimeSpan.FromSeconds(2));
    }
}