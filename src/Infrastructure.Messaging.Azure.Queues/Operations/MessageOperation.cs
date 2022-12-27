using System.Diagnostics.CodeAnalysis;
using Infrastructure.Messaging.Azure.Queues.Settings;

namespace Infrastructure.Messaging.Azure.Queues.Operations;

[ExcludeFromCodeCoverage(Justification = "no specific logic to test")]
public record MessageOperation(
    string CorrelationId,
    string Category,
    string Queue,
    MessageSettings Settings,
    Func<string> MessageContentFunc
);