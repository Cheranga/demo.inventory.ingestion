using Azure.Storage.Queues;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.Azure;
using static LanguageExt.Prelude;

namespace Infrastructure.Messaging.Azure.Queues;

public static class AzureStorageQueueSchema
{
    public static Eff<QueueServiceClient> GetQueueServiceClient(
        IAzureClientFactory<QueueServiceClient> factory,
        string name
    ) =>
        Eff(() => factory.CreateClient(name))
            .MapFail(
                error =>
                    Error.New(
                        ErrorCodes.QueueServiceClientNotFound,
                        ErrorMessages.QueueServiceClientNotFound,
                        error.ToException()
                    )
            );

    public static Eff<QueueClient> GetQueueClient(QueueServiceClient serviceClient, string name) =>
        Eff(() => serviceClient.GetQueueClient(name))
            .MapFail(
                error =>
                    Error.New(
                        ErrorCodes.QueueClientNotFound,
                        ErrorMessages.QueueClientNotFound,
                        error.ToException()
                    )
            );

    public static Aff<Unit> PublishToQueue(
        QueueClient client,
        MessageSettings settings,
        Func<string> messageContentFunc
    ) =>
        from op in settings.IsDefaultSettings()
            ? PublishDefault(client, messageContentFunc)
            : PublishUsingSettings(client, settings, messageContentFunc)
        select op;

    private static Aff<Unit> PublishUsingSettings(
        QueueClient client,
        MessageSettings settings,
        Func<string> messageContentFunc
    ) =>
        from op in Aff(
                async () =>
                    await client.SendMessageAsync(
                        BinaryData.FromString(messageContentFunc()),
                        settings.Visibility,
                        settings.TimeToLive
                    )
            )
            .MapFail(
                error =>
                    Error.New(
                        ErrorCodes.UnableToPublishWithProvidedMessageSettings,
                        ErrorMessages.UnableToPublishWithProvidedMessageSettings,
                        error.ToException()
                    )
            )
        from resp in op.GetRawResponse().IsError
            ? FailAff<Unit>(
                Error.New(ErrorCodes.UnableToPublishToQueue, ErrorMessages.UnableToPublishToQueue)
            )
            : SuccessAff(unit)
        select resp;

    private static Aff<Unit> PublishDefault(QueueClient client, Func<string> messageContentFunc) =>
        from op in Aff(
                async () =>
                    await client.SendMessageAsync(BinaryData.FromString(messageContentFunc()))
            )
            .MapFail(
                error =>
                    Error.New(
                        ErrorCodes.UnableToPublishWithDefaultMessageSettings,
                        ErrorMessages.UnableToPublishWithDefaultMessageSettings,
                        error.ToException()
                    )
            )
        from resp in op.GetRawResponse().IsError
            ? FailAff<Unit>(
                Error.New(ErrorCodes.UnableToPublishToQueue, ErrorMessages.UnableToPublishToQueue)
            )
            : SuccessAff(unit)
        select resp;
}