using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Azure.Identity;
using Azure.Storage.Queues;
using Infrastructure.Messaging.Azure.Queues.Runtimes;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Messaging.Azure.Queues;

[ExcludeFromCodeCoverage(Justification = "no specific logic to test")]
public static class FunctionHostBuilderExtensions
{
    public static IServiceCollection RegisterQueuesWithManagedIdentity(
        this IServiceCollection services,
        params (string account, string name)[] queueSettings
    )
    {
        var queueSettingsList =
            queueSettings?.ToList() ?? new List<(string account, string name)>();

        queueSettingsList.ForEach(x =>
        {
            services.AddAzureClients(clientBuilder =>
            {
                clientBuilder
                    .AddQueueServiceClient(new Uri($"https://{x.account}.queue.core.windows.net"))
                    .ConfigureOptions(options => { options.MessageEncoding = QueueMessageEncoding.Base64; })
                    .WithName(x.name)
                    .WithCredential(new ManagedIdentityCredential());
            });
        });

        return services;
    }

    public static IServiceCollection RegisterQueuesWithConnectionString(
        this IServiceCollection services,
        params (string connectionString, string name)[] queueSettings
    )
    {
        var queueSettingsList =
            queueSettings?.ToList() ?? new List<(string account, string name)>();

        queueSettingsList.ForEach(x =>
        {
            services.AddAzureClients(clientBuilder =>
            {
                clientBuilder
                    .AddQueueServiceClient(x.connectionString)
                    .ConfigureOptions(options => { options.MessageEncoding = QueueMessageEncoding.Base64; })
                    .WithName(x.name);
            });
        });

        return services;
    }

    public static IServiceCollection RegisterLiveQueueRunTime(this IServiceCollection services)
    {
        services.AddTransient(
            typeof(AzureStorageQueueRunTime),
            provider =>
                AzureStorageQueueRunTime.New(
                    AzureStorageQueueRuntimeEnv.New(
                        provider.GetRequiredService<IAzureClientFactory<QueueServiceClient>>()
                    )
                )
        );
        return services;
    }
}