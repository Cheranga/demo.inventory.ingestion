using Azure.Storage.Queues;
using LanguageExt;
using LanguageExt.Effects.Traits;
using Microsoft.Extensions.Azure;
using static LanguageExt.Prelude;

namespace Infrastructure.Messaging.Azure.Queues.Demo;

public readonly struct LiveQueueRunTime
    : IHaveQueueOperations<LiveQueueRunTime>,
        HasCancel<LiveQueueRunTime>
{
    private readonly RuntimeEnv _env;
    private readonly IAzureClientFactory<QueueServiceClient> _factory;

    public LiveQueueRunTime(RuntimeEnv env, IAzureClientFactory<QueueServiceClient> factory)
    {
        _env = env;
        _factory = factory;
    }

    public Aff<LiveQueueRunTime, IQueueOperations> QueueOperations =>
        Eff<LiveQueueRunTime, IQueueOperations>(static rt => new LiveQueueOperations(rt._factory));
    public LiveQueueRunTime LocalCancel => new(_env, _factory);
    public CancellationToken CancellationToken => _env.Token;
    public CancellationTokenSource CancellationTokenSource => _env.Source;

    public static LiveQueueRunTime New(IAzureClientFactory<QueueServiceClient> factory) => new(RuntimeEnv.New(), factory);
}