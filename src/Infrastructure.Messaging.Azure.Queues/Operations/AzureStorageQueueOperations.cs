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
                        operation,
                        error.ToException()
                    )
            );

    private static Aff<QueueClient> GetQueueClient(
        QueueServiceClient serviceClient,
        MessageOperation operation
    ) =>
        (
            from qc in Eff(() => serviceClient.GetQueueClient(operation.Queue))
            from _ in AffMaybe<Response<QueueProperties>>(async () => await qc.GetPropertiesAsync())
            select qc
        ).MapFail(
            error =>
                QueueOperationError.New(
                    ErrorCodes.QueueClientNotFound,
                    ErrorMessages.QueueClientNotFound,
                    operation,
                    error.ToException()
                )
        );
    
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
                            operation,
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
                            operation,
                            error.ToException()
                        )
                )
        from response in op.GetRawResponse().IsError
            ? FailAff<Response<SendReceipt>>(
                QueueOperationError.New(
                    ErrorCodes.PublishFailResponse,
                    ErrorMessages.PublishFailResponse,
                    operation
                )
            )
            : SuccessAff(op)
        select response;
}
