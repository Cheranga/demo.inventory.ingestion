using LanguageExt;
using LanguageExt.Effects.Traits;

namespace Infrastructure.Messaging.Azure.Queues;

public static class QueueOperationsSchema<RT>
    where RT : struct, IHaveQueueOperations<RT>, HasCancel<RT>
{
    public static Aff<RT, Unit> Publish(MessageOperation operation) =>
        from rto in default(RT).QueueOperations
        from op in rto.Publish(operation)
        select op;
}
