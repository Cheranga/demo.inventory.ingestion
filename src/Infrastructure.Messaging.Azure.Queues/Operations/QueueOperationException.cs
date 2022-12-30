using System.Diagnostics.CodeAnalysis;

namespace Infrastructure.Messaging.Azure.Queues.Operations;

[ExcludeFromCodeCoverage]
public class QueueOperationException : Exception
{
    public QueueOperationException(string category, string queue, Exception? exception)
        : base("queue operation error", exception)
    {
        Category = category;
        Queue = queue;
    }

    public string Category { get; }
    public string Queue { get; }
}
