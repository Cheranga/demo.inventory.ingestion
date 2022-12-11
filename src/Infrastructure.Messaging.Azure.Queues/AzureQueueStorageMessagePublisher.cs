using Azure.Storage.Queues;
using LanguageExt;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Messaging.Azure.Queues;

public class AzureQueueStorageMessagePublisher : IMessagePublisher
{
    private readonly IAzureClientFactory<QueueServiceClient> _factory;

    public AzureQueueStorageMessagePublisher(IAzureClientFactory<QueueServiceClient> factory) =>
        _factory = factory;

    public Aff<Unit> PublishAsync(
        string correlationId,
        string category,
        string queue,
        Func<string> messageContentFunc,
        MessageSettings settings,
        ILogger logger
    ) =>
        from serviceClient in AzureStorageQueueSchema.GetQueueServiceClient(
            _factory,
            category,
            correlationId,
            logger
        )
        from queueClient in AzureStorageQueueSchema.GetQueueClient(
            serviceClient,
            queue,
            correlationId,
            logger
        )
        from response in AzureStorageQueueSchema.PublishToQueue(
            queueClient,
            settings,
            messageContentFunc,
            correlationId,
            logger
        )
        select response;
}
