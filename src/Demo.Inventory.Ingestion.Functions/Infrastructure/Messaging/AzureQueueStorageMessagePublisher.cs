using System;
using Azure.Storage.Queues;
using LanguageExt;
using Microsoft.Extensions.Azure;

namespace Demo.Inventory.Ingestion.Functions.Infrastructure.Messaging;

public class AzureQueueStorageMessagePublisher : IMessagePublisher
{
    private readonly IAzureClientFactory<QueueServiceClient> _factory;

    public AzureQueueStorageMessagePublisher(IAzureClientFactory<QueueServiceClient> factory) =>
        _factory = factory;

    public Aff<Unit> PublishAsync(
        string category,
        string queue,
        Func<string> messageContentFunc,
        MessageSettings settings
    ) =>
        from serviceClient in AzureStorageQueueSchema.GetQueueServiceClient(_factory, category)
        from queueClient in AzureStorageQueueSchema.GetQueueClient(serviceClient, queue)
        from response in AzureStorageQueueSchema.PublishToQueue(
            queueClient,
            settings,
            messageContentFunc
        )
        select response;
}