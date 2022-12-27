using System.Diagnostics.CodeAnalysis;
using Infrastructure.Messaging.Azure.Queues.Operations;
using LanguageExt;
using LanguageExt.Effects.Traits;
using static LanguageExt.Prelude;

namespace Infrastructure.Messaging.Azure.Queues.Runtimes;

[ExcludeFromCodeCoverage(Justification = "no specific logic to test")]
public readonly struct TestQueueRunTime
    : IHaveQueueOperations<TestQueueRunTime>,
        HasCancel<TestQueueRunTime>
{
    public TestQueueRunTimeEnv Env { get; }

    private TestQueueRunTime(TestQueueRunTimeEnv env)
    {
        Env = env;
    }

    public Aff<TestQueueRunTime, IQueueOperations> QueueOperations =>
        Eff<TestQueueRunTime, IQueueOperations>(static rt => new TestQueueOperations(rt.Env.Queue));

    public TestQueueRunTime LocalCancel => new(Env);
    public CancellationToken CancellationToken => Env.Token;
    public CancellationTokenSource CancellationTokenSource => Env.Source;

    public static TestQueueRunTime New(Queue<string> queue) => new(TestQueueRunTimeEnv.New(queue));
}
