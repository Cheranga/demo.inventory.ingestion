using System;
using Demo.Inventory.Ingestion.Functions;
using Demo.Inventory.Ingestion.Functions.Extensions;
using Demo.Inventory.Ingestion.Functions.Features.AcceptInventoryChanges;
using FluentValidation;
using Infrastructure.Messaging.Azure.Blobs;
using Infrastructure.Messaging.Azure.Queues;
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

        RegisterSettings(builder.Services, configuration);
        RegisterBlobServices(builder, configuration);
        RegisterQueueServices(builder, configuration);
        RegisterCustomServices(builder.Services);
        RegisterLogging(builder.Services);
        RegisterValidator(builder.Services);
    }

    private static void RegisterValidator(IServiceCollection services) =>
        services.AddValidatorsFromAssembly(typeof(Startup).Assembly);

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

    private static void RegisterSettings(IServiceCollection services, IConfiguration configuration)
    {
        services.RegisterFromConfiguration<AcceptInventorySettings>(
            configuration,
            ServiceLifetime.Singleton
        );

        services.RegisterFromConfiguration<SourceInventorySettings>(
            configuration,
            ServiceLifetime.Singleton
        );

        services.RegisterFromConfiguration<DestinationInventorySettings>(
            configuration,
            ServiceLifetime.Singleton
        );
    }

    private static void RegisterBlobServices(
        IFunctionsHostBuilder builder,
        IConfiguration configuration
    )
    {
        var sourceInventorySettings = configuration
            .GetSection(nameof(SourceInventorySettings))
            .Get<SourceInventorySettings>();

        var destinationInventorySettings = configuration
            .GetSection(nameof(DestinationInventorySettings))
            .Get<DestinationInventorySettings>();

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

    private static void RegisterQueueServices(
        IFunctionsHostBuilder builder,
        IConfiguration configuration
    )
    {
        var addOrderSettings = configuration
            .GetSection(nameof(AcceptInventorySettings))
            .Get<AcceptInventorySettings>();

        var environment = configuration.GetValue<string>("Environment");
        var isLocal = string.Equals(environment, "local", StringComparison.OrdinalIgnoreCase);

        if (isLocal)
        {
            builder.Services
                .RegisterLiveQueueRunTime()
                .RegisterQueuesWithConnectionString(
                    (addOrderSettings.Account, addOrderSettings.Category)
                );
            return;
        }

        builder.Services
            .RegisterLiveQueueRunTime()
            .RegisterQueuesWithManagedIdentity(
                (addOrderSettings.Account, addOrderSettings.Category)
            );
    }

    private static void RegisterCustomServices(IServiceCollection services) =>
        services.AddSingleton<IInventoryChangesHandler, InventoryChangesHandler>();

    private static void RegisterLogging(IServiceCollection services) =>
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
