using LanguageExt;
using LanguageExt.Effects.Traits;

namespace Infrastructure.Messaging.Azure.Queues.Operations;

public static class QueueOperationsSchema<TRunTime>
    where TRunTime : struct, IHaveQueueOperations<TRunTime>, HasCancel<TRunTime>
{
    public static Aff<TRunTime, Unit> Publish(MessageOperation operation) =>
        from rto in default(TRunTime).QueueOperations
        from op in rto.Publish(operation)
        select op;
}
