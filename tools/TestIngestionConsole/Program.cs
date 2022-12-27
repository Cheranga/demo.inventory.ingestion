using LanguageExt;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TestIngestionConsole;

var host = CreateHostBuilder(args).Build();
var inventoryService = host.Services.GetRequiredService<IInventoryService>();

var limit = 10;
var request = new SendInventoryEventRequest("999", "AU_10.csv");
var requests = Seq.generate(limit, _ => inventoryService.SendInventoryEvent(request));
await requests.SequenceParallel(x => x).ContinueWith(_ =>
{
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("====== DONE! ======");
    Console.ResetColor();
});

Console.ReadLine();

static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureServices(
            (_, services) => services.AddHttpClient<IInventoryService, InventoryService>()
        );
