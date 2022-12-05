﻿using Demo.Inventory.Ingestion.Functions;
using Demo.Inventory.Ingestion.Functions.Extensions;
using Demo.Inventory.Ingestion.Functions.Features.AcceptInventoryChanges;
using Demo.Inventory.Ingestion.Functions.Infrastructure.Messaging;
using FluentValidation;
using MediatR;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

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
            .AddJsonFile("local.settings.json", false, true)
            .AddEnvironmentVariables()
            .Build();
    }

    private static void RegisterSettings(
        IFunctionsHostBuilder builder,
        IConfiguration configuration
    ) =>
        builder.Services.RegisterFromConfiguration<AcceptInventorySettings>(
            configuration,
            ServiceLifetime.Singleton
        );

    private static void RegisterAzureClients(
        IFunctionsHostBuilder builder,
        IConfiguration configuration
    )
    {
        var addOrderSettings = configuration
            .GetSection(nameof(AcceptInventorySettings))
            .Get<AcceptInventorySettings>();

        builder.RegisterQueueServiceClient(addOrderSettings.Account, addOrderSettings.Category);
    }

    private static void RegisterMessaging(IFunctionsHostBuilder builder)
    {
        var services = builder.Services;
        services.AddSingleton<IMessagePublisher, AzureQueueStorageMessagePublisher>();
    }
}