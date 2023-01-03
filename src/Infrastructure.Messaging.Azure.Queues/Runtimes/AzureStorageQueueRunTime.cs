using System.Diagnostics.CodeAnalysis;
using Infrastructure.Messaging.Azure.Queues.Operations;
using LanguageExt;
using LanguageExt.Effects.Traits;
using static LanguageExt.Prelude;

namespace Infrastructure.Messaging.Azure.Queues.Runtimes;

[ExcludeFromCodeCoverage(Justification = "no specific logic to test")]
public readonly struct AzureStorageQueueRunTime
    : IHaveQueueOperations<AzureStorageQueueRunTime>,
        HasCancel<AzureStorageQueueRunTime>
{
    private AzureStorageQueueRuntimeEnv Env { get; }

    private AzureStorageQueueRunTime(AzureStorageQueueRuntimeEnv env) => Env = env;

    public Aff<AzureStorageQueueRunTime, IQueueOperations> QueueOperations =>
        Eff<AzureStorageQueueRunTime, IQueueOperations>(
            static rt => new AzureStorageQueueOperations(rt.Env.Factory)
        );

    public AzureStorageQueueRunTime LocalCancel => new(Env);

    public CancellationToken CancellationToken => Env.Token;

    public CancellationTokenSource CancellationTokenSource => Env.Source;

    public static AzureStorageQueueRunTime New(AzureStorageQueueRuntimeEnv env) => new(env);
}
