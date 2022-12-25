using LanguageExt;

namespace Infrastructure.Messaging.Azure.Queues.Demo;

public interface IQueueOperations
{
    Aff<Unit> Publish(MessageOperation operation);
}