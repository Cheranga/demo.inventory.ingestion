using System.Diagnostics.CodeAnalysis;
using Azure.Identity;
using Azure.Storage.Queues;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Messaging.Azure.Queues;

[ExcludeFromCodeCoverage(Justification = "no specific logic to test")]
public static class FunctionHostBuilderExtensions
{
    public static void RegisterQueueServiceClient(
        this IFunctionsHostBuilder builder,
        IConfiguration configuration,
        string account,
        string name
    )
    {
        var environment = configuration.GetValue<string>("Environment");
        var isLocal = string.Equals(environment, "local", StringComparison.OrdinalIgnoreCase);

        builder.Services.AddAzureClients(clientBuilder =>
        {
            var queueBuilder = (
                isLocal
                    ? clientBuilder.AddQueueServiceClient(account)
                    : clientBuilder.AddQueueServiceClient(
                        new Uri($"https://{account}.queue.core.windows.net")
                    )
            )
                .ConfigureOptions(options =>
                {
                    options.MessageEncoding = QueueMessageEncoding.Base64;
                })
                .WithName(name);

            if (!isLocal)
            {
                queueBuilder.WithCredential(new ManagedIdentityCredential());
            }
        });
    }
}
