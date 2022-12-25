namespace Infrastructure.Messaging.Azure.Queues.Demo;

public record MessageOperation(
    string CorrelationId,
    string Category,
    string Queue,
    MessageSettings Settings,
    Func<string> MessageContentFunc
);