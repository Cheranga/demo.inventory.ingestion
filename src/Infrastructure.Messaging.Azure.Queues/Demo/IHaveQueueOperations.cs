using LanguageExt;
using LanguageExt.Effects.Traits;

namespace Infrastructure.Messaging.Azure.Queues.Demo;

public interface IHaveQueueOperations<RT> where RT : struct, HasCancel<RT>
{
    Aff<RT, IQueueOperations> QueueOperations { get; }
}