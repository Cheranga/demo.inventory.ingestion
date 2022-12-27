using LanguageExt;

namespace Infrastructure.Messaging.Azure.Queues;

public interface IQueueOperations
{
    Aff<Unit> Publish(MessageOperation operation);
}