using Azure;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.Azure;
using static LanguageExt.Prelude;

namespace Infrastructure.Messaging.Azure.Queues.Demo;

internal class LiveQueueOperations : IQueueOperations
{
    private readonly IAzureClientFactory<QueueServiceClient> _factory;

    internal LiveQueueOperations(IAzureClientFactory<QueueServiceClient> factory) =>
        _factory = factory;

    public Aff<Unit> Publish(MessageOperation operation) =>
        (
            from sc in GetQueueServiceClient(_factory, operation.Category)
            from qc in GetQueueClient(sc, operation.Queue)
            from op in PublishMessage(qc, operation.MessageContentFunc, operation.Settings)
            select op
        ).Map(
            response =>
                response.GetRawResponse().IsError
                    ? throw new MessagePublishException(
                        Error.New(
                            ErrorCodes.UnableToPublishToQueue,
                            response.GetRawResponse().ReasonPhrase
                        )
                    )
                    : unit
        );

    private static Eff<QueueServiceClient> GetQueueServiceClient(
        IAzureClientFactory<QueueServiceClient> factory,
        string name
    ) =>
        EffMaybe<QueueServiceClient>(() => factory.CreateClient(name))
            .MapFail(
                _ =>
                    Error.New(
                        ErrorCodes.QueueServiceClientNotFound,
                        ErrorMessages.QueueServiceClientNotFound
                    )
            );

    private static Eff<QueueClient> GetQueueClient(
        QueueServiceClient serviceclient,
        string queue
    ) =>
        EffMaybe<QueueClient>(() => serviceclient.GetQueueClient(queue))
            .MapFail(
                _ => Error.New(ErrorCodes.QueueClientNotFound, ErrorMessages.QueueClientNotFound)
            );

    private static Aff<Response<SendReceipt>> PublishMessage(
        QueueClient client,
        Func<string> messageContentFunc,
        MessageSettings settings
    ) =>
        from op in settings.IsDefaultSettings()
            ? AffMaybe<Response<SendReceipt>>(
                    async () => await client.SendMessageAsync(messageContentFunc())
                )
                .MapFail(
                    _ =>
                        Error.New(
                            ErrorCodes.UnableToPublishWithDefaultMessageSettings,
                            ErrorMessages.UnableToPublishWithDefaultMessageSettings
                        )
                )
            : AffMaybe<Response<SendReceipt>>(
                    async () =>
                        await client.SendMessageAsync(
                            messageContentFunc(),
                            settings.Visibility,
                            settings.TimeToLive
                        )
                )
                .MapFail(
                    _ =>
                        Error.New(
                            ErrorCodes.UnableToPublishWithProvidedMessageSettings,
                            ErrorMessages.UnableToPublishWithProvidedMessageSettings
                        )
                )
        select op;
}
