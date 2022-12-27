using System.Diagnostics.CodeAnalysis;
using LanguageExt;

namespace Infrastructure.Messaging.Azure.Queues.Operations;

public interface IQueueOperations
{
    Aff<Unit> Publish(MessageOperation operation);
}