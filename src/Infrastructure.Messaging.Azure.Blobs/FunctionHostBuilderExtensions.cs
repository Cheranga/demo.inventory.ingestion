using Azure.Identity;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Messaging.Azure.Queues;

public static class FunctionHostBuilderExtensions
{
    public static bool IsNonProd(this IFunctionsHostBuilder builder)
    {
        var environmentName = builder.GetContext().EnvironmentName;
        return !(
            string.Equals("staging", environmentName, StringComparison.OrdinalIgnoreCase)
            || string.Equals("production", environmentName, StringComparison.OrdinalIgnoreCase)
        );
    }

    public static void RegisterBlobServiceClient(
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
                        ? clientBuilder.AddBlobServiceClient(account)
                        : clientBuilder.AddBlobServiceClient(
                            new Uri($"https://{account}.blob.core.windows.net")
                        )
                )
                .WithName(name);

            if (!builder.IsNonProd())
            {
                queueBuilder.WithCredential(new ManagedIdentityCredential());
            }
        });
    }
}
