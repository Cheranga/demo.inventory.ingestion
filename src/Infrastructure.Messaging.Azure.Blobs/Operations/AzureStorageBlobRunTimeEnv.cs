using Azure.Storage.Blobs;
using Microsoft.Extensions.Azure;

namespace Infrastructure.Messaging.Azure.Blobs.Operations;

public class AzureStorageBlobRunTimeEnv
{
    private AzureStorageBlobRunTimeEnv(IAzureClientFactory<BlobServiceClient> factory,
        CancellationTokenSource source)
    {
        Factory = factory;
        Source = source;
    }

    public IAzureClientFactory<BlobServiceClient> Factory { get; }
    public CancellationTokenSource Source { get; }

    public static AzureStorageBlobRunTimeEnv New(IAzureClientFactory<BlobServiceClient> factory) => new(factory, new CancellationTokenSource());
}