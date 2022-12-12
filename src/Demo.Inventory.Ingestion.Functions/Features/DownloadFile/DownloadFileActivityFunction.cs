using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace Demo.Inventory.Ingestion.Functions.Features.DownloadFile;

public record ReadFileRequest(string Category, string Container, string FileName);

public record InventoryData(string LocationCode, string InventoryCode);

public record InventoryRecordsCollection(List<InventoryData> Records);


public class DownloadFileActivityFunction
{
    [FunctionName(nameof(DownloadFileActivityFunction))]
    public async Task<InventoryRecordsCollection> ExecuteAsync([ActivityTrigger] IDurableActivityContext context)
    {
        var fileReadRequest = context.GetInput<ReadFileRequest>();
        await Task.Delay(TimeSpan.FromSeconds(2));

        return new InventoryRecordsCollection(new List<InventoryData>());
    }
}