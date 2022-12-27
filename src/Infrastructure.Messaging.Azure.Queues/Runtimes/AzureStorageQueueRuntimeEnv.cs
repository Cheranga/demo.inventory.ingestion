using Azure.Storage.Queues;
using Microsoft.Extensions.Azure;

namespace Infrastructure.Messaging.Azure.Queues.Runtimes;

public class AzureStorageQueueRuntimeEnv
{
    public IAzureClientFactory<QueueServiceClient> Factory { get; }
    public readonly CancellationTokenSource Source;
    public readonly CancellationToken Token;

    public AzureStorageQueueRuntimeEnv(IAzureClientFactory<QueueServiceClient> factory)
    {
        Factory = factory;
        Source = new CancellationTokenSource();
        Token = Source.Token;
    }
}