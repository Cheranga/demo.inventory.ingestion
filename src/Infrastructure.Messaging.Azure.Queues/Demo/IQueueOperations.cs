using LanguageExt;

namespace Infrastructure.Messaging.Azure.Queues.Demo;

public interface IQueueOperations
{
    ValueTask<Unit> Publish(MessageOperation operation);
}