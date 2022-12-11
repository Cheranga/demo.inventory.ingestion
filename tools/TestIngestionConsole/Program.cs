using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TestIngestionConsole;

var host = CreateHostBuilder(args).Build();
var inventoryService = host.Services.GetRequiredService<IInventoryService>();

var limit = 500;
var request = new SendInventoryEventRequest("999", "some file.csv");
var requests = Enumerable.Range(1, limit).Select(x => inventoryService.SendInventoryEvent(request));
await Task.WhenAll(requests);

Console.WriteLine("Done");
Console.ReadLine();

static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureServices(
            (_, services) => services.AddHttpClient<IInventoryService, InventoryService>()
        );