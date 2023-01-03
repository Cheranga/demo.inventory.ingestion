using System.Diagnostics.CodeAnalysis;
using Azure.Identity;
using Azure.Storage.Blobs;
using Infrastructure.Messaging.Azure.Blobs.Runtimes;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Messaging.Azure.Blobs;

[ExcludeFromCodeCoverage]
public static class FunctionHostBuilderExtensions
{
    public static IServiceCollection RegisterBlobsWithManagedIdentity(
        this IServiceCollection services,
        params (string account, string name)[] blobSettings
    )
    {
        var blobSettingsList =
            blobSettings?.ToList() ?? new List<(string account, string name)>();

        blobSettingsList.ForEach(x =>
        {
            services.AddAzureClients(clientBuilder =>
            {
                clientBuilder
                    .AddBlobServiceClient(new Uri($"https://{x.account}.blob.core.windows.net"))
                    .WithName(x.name)
                    .WithCredential(new ManagedIdentityCredential());
            });
        });

        return services;
    }
    
    public static IServiceCollection RegisterBlobsWithConnectionString(
        this IServiceCollection services,
        params (string connectionString, string name)[] blobSettings
    )
    {
        var blobSettingsList =
            blobSettings?.ToList() ?? new List<(string account, string name)>();

        blobSettingsList.ForEach(x =>
        {
            services.AddAzureClients(clientBuilder =>
            {
                clientBuilder
                    .AddBlobServiceClient(x.connectionString)
                    .WithName(x.name);
            });
        });

        return services;
    }
    
    public static IServiceCollection RegisterLiveBlobRunTime(this IServiceCollection services)
    {
        services.AddTransient(
            typeof(AzureStorageBlobRunTime),
            provider =>
                AzureStorageBlobRunTime.New(
                    AzureStorageBlobRunTimeEnv.New(
                        provider.GetRequiredService<IAzureClientFactory<BlobServiceClient>>()
                    )
                )
        );
        return services;
    }
}
