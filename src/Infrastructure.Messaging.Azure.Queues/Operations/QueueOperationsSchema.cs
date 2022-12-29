using Infrastructure.Messaging.Azure.Queues.Settings;
using LanguageExt;
using LanguageExt.Effects.Traits;

namespace Infrastructure.Messaging.Azure.Queues.Operations;

public static class QueueOperationsSchema<TRunTime>
    where TRunTime : struct, IHaveQueueOperations<TRunTime>, HasCancel<TRunTime>
{
    public static Aff<TRunTime, Unit> Publish(
        string correlationId,
        string category,
        string queue,
        Func<string> messageContentFunc
    ) =>
        from op in PublishUsingSettings(
            correlationId,
            category,
            queue,
            messageContentFunc,
            MessageSettings.DefaultSettings
        ).MapFail(error=> QueueOperationError.New(error.Code, error.Message, error))
        select op;

    public static Aff<TRunTime, Unit> PublishUsingSettings(
        string correlationId,
        string category,
        string queue,
        Func<string> messageContentFunc,
        MessageSettings settings
    ) =>
        from rto in default(TRunTime).QueueOperations
        from op in rto.Publish(
            new MessageOperation(correlationId, category, queue, settings, messageContentFunc)
        ).MapFail(error=>QueueOperationError.New(error.Code, error.Message, error))
        select op;
}
