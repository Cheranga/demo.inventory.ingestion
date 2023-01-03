using System.Diagnostics.CodeAnalysis;
using Infrastructure.Messaging.Azure.Blobs.Operations;
using LanguageExt;
using LanguageExt.Effects.Traits;
using static LanguageExt.Prelude;

namespace Infrastructure.Messaging.Azure.Blobs.Runtimes;

[ExcludeFromCodeCoverage]
public readonly struct TestBlobRunTime
    : IHaveBlobOperations<TestBlobRunTime>,
        HasCancel<TestBlobRunTime>
{
    private readonly TestBlobRunTimeEnv _env;

    private TestBlobRunTime(TestBlobRunTimeEnv env)
    {
        _env = env;
    }

    public Aff<TestBlobRunTime, IBlobOperations> BlobOperations =>
        Eff<TestBlobRunTime, IBlobOperations>(static rt => new TestBlobOperations(rt._env.Blobs));
    public TestBlobRunTime LocalCancel => new(_env);
    public CancellationToken CancellationToken => _env.Token;
    public CancellationTokenSource CancellationTokenSource => _env.Source;

    public static TestBlobRunTime New(Dictionary<string, string> blobs) =>
        new(TestBlobRunTimeEnv.New(blobs));
}
