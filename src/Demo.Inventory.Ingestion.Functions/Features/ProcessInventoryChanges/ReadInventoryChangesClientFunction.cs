using System.Threading.Tasks;
using System;
using Microsoft.Azure.WebJobs;

namespace Demo.Inventory.Ingestion.Functions.Features.ProcessInventoryChanges;

public class ReadInventoryChangesClientFunction
{
    [FunctionName(nameof(ReadInventoryChangesClientFunction))]
    public async Task RunAsync([QueueTrigger("%AcceptInventorySettings::Queue%")] string message)
    {
        await Task.Delay(TimeSpan.FromSeconds(2));
    }
}