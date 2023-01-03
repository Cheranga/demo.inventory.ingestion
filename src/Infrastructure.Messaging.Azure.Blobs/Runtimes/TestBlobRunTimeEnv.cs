using System.Diagnostics.CodeAnalysis;

namespace Infrastructure.Messaging.Azure.Blobs.Runtimes;

[ExcludeFromCodeCoverage]
public class TestBlobRunTimeEnv
{
    public Dictionary<string, string> Blobs { get; }
    public CancellationTokenSource Source { get; }
    
    public CancellationToken Token { get; }

    private TestBlobRunTimeEnv(Dictionary<string, string> blobs, CancellationTokenSource source)
    {
        Blobs = blobs;
        Source = source;
        Token = source.Token;
    }

    public static TestBlobRunTimeEnv New(Dictionary<string, string> blobs) => new(blobs, new CancellationTokenSource());
}