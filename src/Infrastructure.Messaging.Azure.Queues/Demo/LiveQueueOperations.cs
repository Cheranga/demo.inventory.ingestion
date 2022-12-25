using Azure;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using LanguageExt;
using Microsoft.Extensions.Azure;
using static LanguageExt.Prelude;

namespace Infrastructure.Messaging.Azure.Queues.Demo;

public class LiveQueueOperations : IQueueOperations
{
    private readonly IAzureClientFactory<QueueServiceClient> _factory;

    public LiveQueueOperations(IAzureClientFactory<QueueServiceClient> factory) =>
        _factory = factory;

    public async ValueTask<Unit> Publish(MessageOperation operation)
    {
        var a = (
            await (
                from sc in TryAsync(
                    async () => await _factory.CreateClient(operation.Category).AsValueTask()
                )
                from qc in TryAsync(
                    async () => await sc.GetQueueClient(operation.Queue).AsValueTask()
                )
                from po in operation.Settings.IsDefaultSettings()
                    ? TryAsync<Response<SendReceipt>>(
                        async () => await qc.SendMessageAsync(operation.MessageContentFunc())
                    )
                    : TryAsync<Response<SendReceipt>>(
                        async () =>
                            await qc.SendMessageAsync(
                                operation.MessageContentFunc(),
                                operation.Settings.Visibility,
                                operation.Settings.TimeToLive
                            )
                    )
                select po
            )
        ).Match(
            response =>
                response.GetRawResponse().IsError
                    ? throw new MessagePublishException(response)
                    : unit,
            exception => throw new MessagePublishException(exception.Message, exception)
        );

        return a;

        // var a =   await (
        //        from sc in EffMaybe<QueueServiceClient>(() => _factory.CreateClient(operation.Category))
        //        from qc in EffMaybe<QueueClient>(() => sc.GetQueueClient(operation.Queue))
        //        from po in operation.Settings.IsDefaultSettings()
        //            ? AffMaybe<Response<SendReceipt>>(
        //                async () => await qc.SendMessageAsync(operation.MessageContentFunc())
        //            )
        //            : AffMaybe<Response<SendReceipt>>(
        //                async () =>
        //                    await qc.SendMessageAsync(
        //                        operation.MessageContentFunc(),
        //                        operation.Settings.Visibility,
        //                        operation.Settings.TimeToLive
        //                    )
        //            )
        //        select po
        //    ).BiMap(
        //        op => op.GetRawResponse().IsError ? throw new MessagePublishException(op) : unit.AsValueTask(),
        //        error => throw new MessagePublishException(error.Message, error.ToException())
        //    ).Run().Map(fin => fin.);
    }
}
