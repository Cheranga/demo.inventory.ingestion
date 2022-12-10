using Azure.Storage.Queues;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Logging;
using static LanguageExt.Prelude;

namespace Infrastructure.Messaging.Azure.Queues;

public static class AzureStorageQueueSchema
{
    public static Eff<QueueServiceClient> GetQueueServiceClient(
        IAzureClientFactory<QueueServiceClient> factory,
        string name,
        string correlationId,
        ILogger logger
    ) =>
        Eff(() =>
            {
                var serviceClient = factory.CreateClient(name);
                logger.LogInformation("{CorrelationId} getting queue service client successful", correlationId);
                return serviceClient;
            })
            .MapFail(error =>
            {
                logger.LogError( error.ToException(),"{CorrelationId} cannot create queue service client", correlationId);
                return Error.New(
                    ErrorCodes.QueueServiceClientNotFound,
                    ErrorMessages.QueueServiceClientNotFound,
                    error.ToException()
                );
            });

    public static Eff<QueueClient> GetQueueClient(QueueServiceClient serviceClient, string name, string correlationId, ILogger logger) =>
        Eff(() =>
            {
                var queueClient = serviceClient.GetQueueClient(name);
                logger.LogInformation("{CorrelationId} getting queue client successful", correlationId);
                return queueClient;
            })
            .MapFail(
                error =>
                {
                    logger.LogError( error.ToException(),"{CorrelationId} cannot get queue client", correlationId);
                    return Error.New(
                        ErrorCodes.QueueClientNotFound,
                        ErrorMessages.QueueClientNotFound,
                        error.ToException()
                    );
                }
            );

    public static Aff<Unit> PublishToQueue(
        QueueClient client,
        MessageSettings settings,
        Func<string> messageContentFunc,
        string correlationId,
        ILogger logger
    ) =>
        from op in settings.IsDefaultSettings()
            ? PublishDefault(client, messageContentFunc, correlationId, logger)
            : PublishUsingSettings(client, settings, messageContentFunc, correlationId, logger)
        select op;

    private static Aff<Unit> PublishUsingSettings(
        QueueClient client,
        MessageSettings settings,
        Func<string> messageContentFunc,
        string correlationId,
        ILogger logger
    ) =>
        from op in Aff(
                async () =>
                {
                    var response = await client.SendMessageAsync(
                        BinaryData.FromString(messageContentFunc()),
                        settings.Visibility,
                        settings.TimeToLive
                    );

                    logger.LogInformation("{CorrelationId} message published successfully using specific settings", correlationId);
                    return response;
                })
            .MapFail(
                error =>
                {
                    logger.LogError( error.ToException(),"{CorrelationId} error when publishing message using specific settings", correlationId);
                    return Error.New(
                        ErrorCodes.UnableToPublishWithProvidedMessageSettings,
                        ErrorMessages.UnableToPublishWithProvidedMessageSettings,
                        error.ToException()
                    );
                }
            )
        from resp in op.GetRawResponse().IsError
            ? FailAff<Unit>(
                Error.New(ErrorCodes.UnableToPublishToQueue, ErrorMessages.UnableToPublishToQueue)
            )
            : SuccessAff(unit)
        select resp;

    private static Aff<Unit> PublishDefault(QueueClient client, Func<string> messageContentFunc, string correlationId, ILogger logger) =>
        from op in Aff(
                async () =>
                {
                    var response = await client.SendMessageAsync(BinaryData.FromString(messageContentFunc()));
                    logger.LogInformation("{CorrelationId} message published successfully using default settings", correlationId);
                    return response;
                })
            .MapFail(
                error =>
                {
                    logger.LogError(error.ToException(),"{CorrelationId} error when publishing message using default settings", correlationId);
                    return Error.New(
                        ErrorCodes.UnableToPublishWithDefaultMessageSettings,
                        ErrorMessages.UnableToPublishWithDefaultMessageSettings,
                        error.ToException()
                    );
                }
            )
        from resp in op.GetRawResponse().IsError
            ? FailAff<Unit>(
                Error.New(ErrorCodes.UnableToPublishToQueue, ErrorMessages.UnableToPublishToQueue)
            )
            : SuccessAff(unit)
        select resp;
}
