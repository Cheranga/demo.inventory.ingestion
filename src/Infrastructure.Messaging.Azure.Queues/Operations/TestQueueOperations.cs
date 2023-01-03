using System.Diagnostics.CodeAnalysis;
using LanguageExt;
using static LanguageExt.Prelude;

namespace Infrastructure.Messaging.Azure.Queues.Operations;

[ExcludeFromCodeCoverage]
internal class TestQueueOperations : IQueueOperations
{
    public Queue<string> Queue { get; }

    internal TestQueueOperations(Queue<string> queue)
    {
        Queue = queue;
    }

    public Aff<Unit> Publish(MessageOperation operation) =>
        from op in Eff(() =>
        {
            Queue.Enqueue(operation.MessageContentFunc());
            return unit;
        })
        select op;
}
