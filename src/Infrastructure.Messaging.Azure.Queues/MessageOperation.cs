namespace Infrastructure.Messaging.Azure.Queues;

public record MessageOperation(
    string CorrelationId,
    string Category,
    string Queue,
    MessageSettings Settings,
    Func<string> MessageContentFunc
);