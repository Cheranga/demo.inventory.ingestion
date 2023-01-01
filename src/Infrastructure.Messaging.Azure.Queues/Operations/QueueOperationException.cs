using System.Diagnostics.CodeAnalysis;

namespace Infrastructure.Messaging.Azure.Queues.Operations;

[ExcludeFromCodeCoverage]
public class QueueOperationException : Exception
{
    public QueueOperationException(MessageOperation operation, Exception? exception)
        : base("queue operation error", exception)
    {
        Operation = operation;
    }

    public MessageOperation Operation { get; }
}
