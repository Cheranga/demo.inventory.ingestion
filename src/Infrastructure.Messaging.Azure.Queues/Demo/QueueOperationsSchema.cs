using LanguageExt;
using LanguageExt.Effects.Traits;

namespace Infrastructure.Messaging.Azure.Queues.Demo;

public static class QueueOperationsSchema<RT>
    where RT : struct, IHaveQueueOperations<RT>, HasCancel<RT>
{
    public static Aff<RT, Unit> Publish(MessageOperation operation) =>
        from op in default(RT).QueueOperations.MapAsync(op => op.Publish(operation))
        select op;
}
