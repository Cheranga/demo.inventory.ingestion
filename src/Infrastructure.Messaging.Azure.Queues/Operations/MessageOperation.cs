using Infrastructure.Messaging.Azure.Queues.Settings;

namespace Infrastructure.Messaging.Azure.Queues.Operations;

public record MessageOperation(
    string CorrelationId,
    string Category,
    string Queue,
    MessageSettings Settings,
    Func<string> MessageContentFunc
);