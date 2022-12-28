using LanguageExt;
using LanguageExt.Effects.Traits;
using static LanguageExt.Prelude;

namespace Infrastructure.Messaging.Azure.Blobs.Operations;

public readonly struct AzureStorageBlobRunTime
    : IHaveBlobOperations<AzureStorageBlobRunTime>,
        HasCancel<AzureStorageBlobRunTime>
{
    private AzureStorageBlobRunTimeEnv Env { get; }

    private AzureStorageBlobRunTime(AzureStorageBlobRunTimeEnv env) => Env = env;

    public Aff<AzureStorageBlobRunTime, IBlobOperations> BlobOperations =>
        Eff<AzureStorageBlobRunTime, IBlobOperations>(
            static rt => AzureStorageBlobOperations.New(rt.Env.Factory)
        );

    public AzureStorageBlobRunTime LocalCancel => new(AzureStorageBlobRunTimeEnv.New(Env.Factory));
    public CancellationToken CancellationToken => Env.Source.Token;
    public CancellationTokenSource CancellationTokenSource => Env.Source;

    public static AzureStorageBlobRunTime New(AzureStorageBlobRunTimeEnv env) => new(env);
}
