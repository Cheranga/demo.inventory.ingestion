using LanguageExt;
using LanguageExt.Effects.Traits;

namespace Infrastructure.Messaging.Azure.Queues.Operations;

public interface IHaveQueueOperations<TRunTime> where TRunTime : struct, HasCancel<TRunTime>
{
    Aff<TRunTime, IQueueOperations> QueueOperations { get; }
}