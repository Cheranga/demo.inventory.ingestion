using System.Diagnostics.CodeAnalysis;

namespace Infrastructure.Messaging.Azure.Queues.Runtimes;

[ExcludeFromCodeCoverage(Justification = "no specific logic to test")]
public class TestQueueRunTimeEnv
{
    public Queue<string> Queue { get; }
    public CancellationTokenSource Source { get; }
    public CancellationToken Token { get; }

    private TestQueueRunTimeEnv(Queue<string> queue, CancellationTokenSource source)
    {
        Queue = queue;
        Source = source;
        Token = source.Token;
    }

    public static TestQueueRunTimeEnv New(Queue<string> queue) => new(queue, new CancellationTokenSource());
}