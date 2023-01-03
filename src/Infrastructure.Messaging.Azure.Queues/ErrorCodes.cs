using System.Diagnostics.CodeAnalysis;

namespace Infrastructure.Messaging.Azure.Queues;

[ExcludeFromCodeCoverage(Justification = "no specific logic to test")]
public class ErrorCodes
{
    public const int QueueServiceClientNotFound = 500;
    public const int QueueClientNotFound = 501;
    public const int UnableToPublishWithDefaultMessageSettings = 503;
    public const int UnableToPublishWithProvidedMessageSettings = 504;
    public const int PublishFailResponse = 505;
    public const int InternalServerError = 506;
}

[ExcludeFromCodeCoverage(Justification = "no specific logic to test")]
public class ErrorMessages
{
    public const string QueueServiceClientNotFound =
        "queue service is not registered for the requested category";
    public const string QueueClientNotFound = "queue not found";

    public const string UnableToPublishWithDefaultMessageSettings =
        "unable to publish message to the queue with default message settings";

    public const string UnableToPublishWithProvidedMessageSettings =
        "unable to publish message to the queue with provided message settings";

    public const string PublishFailResponse =
        "publish to queue operation returned unsuccessful response";
}
