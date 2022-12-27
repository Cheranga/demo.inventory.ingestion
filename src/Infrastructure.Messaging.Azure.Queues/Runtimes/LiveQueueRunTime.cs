using Infrastructure.Messaging.Azure.Queues.Operations;
using LanguageExt;
using LanguageExt.Effects.Traits;
using static LanguageExt.Prelude;

namespace Infrastructure.Messaging.Azure.Queues.Runtimes;

public readonly struct LiveQueueRunTime
    : IHaveQueueOperations<LiveQueueRunTime>,
        HasCancel<LiveQueueRunTime>
{
    private RuntimeEnv Env { get; }

    private LiveQueueRunTime(RuntimeEnv env)
    {
        Env = env;
    }

    public Aff<LiveQueueRunTime, IQueueOperations> QueueOperations =>
        Eff<LiveQueueRunTime, IQueueOperations>(static rt => new LiveQueueOperations(rt.Env.Factory));
    public LiveQueueRunTime LocalCancel => new(Env);
    public CancellationToken CancellationToken => Env.Token;
    public CancellationTokenSource CancellationTokenSource => Env.Source;

    public static LiveQueueRunTime New(RuntimeEnv env) => new(env);
}
