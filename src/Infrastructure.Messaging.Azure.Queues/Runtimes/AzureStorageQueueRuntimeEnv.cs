using System.Diagnostics.CodeAnalysis;
using Azure.Storage.Queues;
using Microsoft.Extensions.Azure;

namespace Infrastructure.Messaging.Azure.Queues.Runtimes;

[ExcludeFromCodeCoverage(Justification = "no specific logic to test")]
public class AzureStorageQueueRuntimeEnv
{
    public IAzureClientFactory<QueueServiceClient> Factory { get; }
    public readonly CancellationTokenSource Source;
    public readonly CancellationToken Token;

    private AzureStorageQueueRuntimeEnv(IAzureClientFactory<QueueServiceClient> factory)
    {
        Factory = factory;
        Source = new CancellationTokenSource();
        Token = Source.Token;
    }

    public static AzureStorageQueueRuntimeEnv New(IAzureClientFactory<QueueServiceClient> factory) => new(factory);
}