using System.Text;

namespace Infrastructure.Messaging.Azure.Queues.Demo;

public class RuntimeEnv
{
    public readonly CancellationTokenSource Source;
    public readonly CancellationToken Token;
    public readonly Encoding Encoding;

    public static RuntimeEnv New() => new(new CancellationTokenSource(), Encoding.Default);

    public RuntimeEnv(CancellationTokenSource source, Encoding encoding)
    {
        Source = source;
        Token = source.Token;
        Encoding = encoding;
    }
}