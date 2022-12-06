namespace Infrastructure.Messaging.Azure.Queues;

public class ErrorCodes
{
    public const int QueueServiceClientNotFound = 500;
    public const int QueueClientNotFound = 501;
    public const int UnableToPublishToQueue = 502;
    public const int UnableToPublishWithDefaultMessageSettings = 503;
    public const int UnableToPublishWithProvidedMessageSettings = 504;
}

public class ErrorMessages
{
    public const string QueueServiceClientNotFound = "unregistered queue service client";
    public const string QueueClientNotFound = "queue client not found";
    public const string UnableToPublishToQueue = "unable to publish message to the queue";

    public const string UnableToPublishWithDefaultMessageSettings =
        "unable to publish message to the queue with default message settings";

    public const string UnableToPublishWithProvidedMessageSettings =
        "unable to publish message to the queue with provided message settings";
}