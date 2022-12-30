using Azure;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using LanguageExt;
using Microsoft.Extensions.Azure;
using static LanguageExt.Prelude;

namespace Infrastructure.Messaging.Azure.Queues.Operations;

internal class AzureStorageQueueOperations : IQueueOperations
{
    private readonly IAzureClientFactory<QueueServiceClient> _factory;

    internal AzureStorageQueueOperations(IAzureClientFactory<QueueServiceClient> factory) =>
        _factory = factory;

    public Aff<Unit> Publish(MessageOperation operation) =>
        (
            from sc in GetQueueServiceClient(_factory, operation)
            from qc in GetQueueClient(sc, operation)
            from op in PublishMessage(qc, operation)
            select op
        ).Map(_ => unit);

    private static Eff<QueueServiceClient> GetQueueServiceClient(
        IAzureClientFactory<QueueServiceClient> factory,
        MessageOperation operation
    ) =>
        EffMaybe<QueueServiceClient>(() => factory.CreateClient(operation.Category))
            .MapFail(
                error =>
                    QueueOperationError.New(
                        ErrorCodes.QueueServiceClientNotFound,
                        ErrorMessages.QueueServiceClientNotFound,
                        operation.Category,
                        operation.Queue,
                        error.ToException()
                    )
            );

    private static Aff<QueueClient> GetQueueClient(
        QueueServiceClient serviceClient,
        MessageOperation operation
    ) =>
        from qc in Eff(() => serviceClient.GetQueueClient(operation.Queue))
            .MapFail(
                error =>
                    QueueOperationError.New(
                        ErrorCodes.InternalServerError,
                        ErrorMessages.InternalServerError,
                        operation.Category,
                        operation.Queue,
                        error.ToException()
                    )
            )
        from response in AffMaybe<Response<QueueProperties>>(
                async () => await qc.GetPropertiesAsync()
            )
            .MapFail(
                error =>
                    QueueOperationError.New(
                        ErrorCodes.InternalServerError,
                        ErrorMessages.InternalServerError,
                        operation.Category,
                        operation.Queue,
                        error.ToException()
                    )
            )
        from op in response.GetRawResponse().IsError
            ? FailAff<QueueClient>(
                QueueOperationError.New(
                    ErrorCodes.QueueClientNotFound,
                    ErrorMessages.QueueClientNotFound,
                    operation.Category,
                    operation.Queue
                )
            )
            : SuccessAff(qc)
        select op;

    private static Aff<Response<SendReceipt>> PublishMessage(
        QueueClient client,
        MessageOperation operation
    ) =>
        from op in operation.Settings.IsDefaultSettings()
            ? AffMaybe<Response<SendReceipt>>(
                    async () => await client.SendMessageAsync(operation.MessageContentFunc())
                )
                .MapFail(
                    error =>
                        QueueOperationError.New(
                            ErrorCodes.UnableToPublishWithDefaultMessageSettings,
                            ErrorMessages.UnableToPublishWithDefaultMessageSettings,
                            operation.Category,
                            operation.Queue,
                            error.ToException()
                        )
                )
            : AffMaybe<Response<SendReceipt>>(
                    async () =>
                        await client.SendMessageAsync(
                            operation.MessageContentFunc(),
                            operation.Settings.Visibility,
                            operation.Settings.TimeToLive
                        )
                )
                .MapFail(
                    error =>
                        QueueOperationError.New(
                            ErrorCodes.UnableToPublishWithProvidedMessageSettings,
                            ErrorMessages.UnableToPublishWithProvidedMessageSettings,
                            operation.Category,
                            operation.Queue,
                            error.ToException()
                        )
                )
        from response in op.GetRawResponse().IsError
            ? FailAff<Response<SendReceipt>>(
                QueueOperationError.New(
                    ErrorCodes.PublishFailResponse,
                    ErrorMessages.PublishFailResponse,
                    operation.Category,
                    operation.Queue
                )
            )
            : SuccessAff(op)
        select response;
}
