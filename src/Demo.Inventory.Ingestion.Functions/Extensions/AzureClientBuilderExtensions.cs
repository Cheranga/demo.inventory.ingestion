using Azure.Core.Extensions;
using Azure.Identity;
using Microsoft.Extensions.Azure;

namespace Demo.Inventory.Ingestion.Functions.Extensions;

public static class AzureClientBuilderExtensions
{
    public static IAzureClientBuilder<TClient, TOptions> WithManagedIdentity<TClient, TOptions>(
        this IAzureClientBuilder<TClient, TOptions> builder
    ) where TOptions : class
    {
        return builder.WithCredential(new ManagedIdentityCredential());
        //     _ =>
        //         new DefaultAzureCredential(
        //             new DefaultAzureCredentialOptions
        //             {
        //                 ExcludeManagedIdentityCredential = false,
        //                 ExcludeEnvironmentCredential = true,
        //                 ExcludeAzureCliCredential = true,
        //                 ExcludeInteractiveBrowserCredential = true,
        //                 ExcludeVisualStudioCredential = true,
        //                 ExcludeAzurePowerShellCredential = true,
        //                 ExcludeSharedTokenCacheCredential = true,
        //                 ExcludeVisualStudioCodeCredential = true
        //             }
        //         )
        // );
    }
}