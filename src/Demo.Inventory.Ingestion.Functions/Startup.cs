using Demo.Inventory.Ingestion.Functions;
using Demo.Inventory.Ingestion.Functions.Extensions;
using Demo.Inventory.Ingestion.Functions.Features.AcceptInventoryChanges;
using FluentValidation;
using Infrastructure.Messaging.Azure.Blobs;
using Infrastructure.Messaging.Azure.Queues;
using Infrastructure.Messaging.Azure.Queues.Demo;
using MediatR;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Events;

[assembly: FunctionsStartup(typeof(Startup))]

namespace Demo.Inventory.Ingestion.Functions;

public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        var configuration = GetConfiguration(builder);

        RegisterSettings(builder, configuration);
        RegisterAzureClients(builder, configuration);
        RegisterMessaging(builder);
        RegisterLogging(builder.Services);

        builder.Services.AddValidatorsFromAssembly(typeof(Startup).Assembly);
        builder.Services.AddMediatR(typeof(Startup).Assembly);
    }

    protected virtual IConfiguration GetConfiguration(IFunctionsHostBuilder builder)
    {
        var executionContextOptions = builder.Services
            .BuildServiceProvider()
            .GetService<IOptions<ExecutionContextOptions>>()
            .Value;

        return new ConfigurationBuilder()
            .SetBasePath(executionContextOptions.AppDirectory)
            .AddJsonFile("local.settings.json", true, true)
            .AddEnvironmentVariables()
            .Build();
    }

    private static void RegisterSettings(
        IFunctionsHostBuilder builder,
        IConfiguration configuration
    )
    {
        builder.Services.RegisterFromConfiguration<AcceptInventorySettings>(
            configuration,
            ServiceLifetime.Singleton
        );

        builder.Services.RegisterFromConfiguration<SourceInventorySettings>(
            configuration,
            ServiceLifetime.Singleton
        );

        builder.Services.RegisterFromConfiguration<DestinationInventorySettings>(
            configuration,
            ServiceLifetime.Singleton
        );
    }

    private static void RegisterAzureClients(
        IFunctionsHostBuilder builder,
        IConfiguration configuration
    )
    {
        var addOrderSettings = configuration
            .GetSection(nameof(AcceptInventorySettings))
            .Get<AcceptInventorySettings>();

        var sourceInventorySettings = configuration
            .GetSection(nameof(SourceInventorySettings))
            .Get<SourceInventorySettings>();

        var destinationInventorySettings = configuration
            .GetSection(nameof(DestinationInventorySettings))
            .Get<DestinationInventorySettings>();

        builder.RegisterQueueServiceClient(
            configuration,
            addOrderSettings.Account,
            addOrderSettings.Category
        );
        builder.RegisterBlobServiceClient(
            configuration,
            sourceInventorySettings.Account,
            sourceInventorySettings.Category
        );
        builder.RegisterBlobServiceClient(
            configuration,
            destinationInventorySettings.Account,
            destinationInventorySettings.Category
        );
    }

    private static void RegisterMessaging(IFunctionsHostBuilder builder)
    {
        var services = builder.Services;
        services.AddTransient(typeof(RuntimeEnv));
        services.AddSingleton<IInventoryChangesHandler, InventoryChangesHandler>();
    }

    private static void RegisterLogging(IServiceCollection services)
    {
        services.AddLogging(builder =>
        {
            var logger = new LoggerConfiguration().MinimumLevel
                .Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .MinimumLevel.Override("Worker", LogEventLevel.Warning)
                .MinimumLevel.Override("Host", LogEventLevel.Warning)
                .MinimumLevel.Override("Function", LogEventLevel.Warning)
                .MinimumLevel.Override("Azure", LogEventLevel.Warning)
                .MinimumLevel.Override("DurableTask", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.ApplicationInsights(
                    TelemetryConfiguration.CreateDefault(),
                    TelemetryConverter.Traces
                )
                .CreateLogger();

            builder.AddSerilog(logger);
        });
    }
}
