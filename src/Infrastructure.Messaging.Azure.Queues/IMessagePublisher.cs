using LanguageExt;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Messaging.Azure.Queues;

public interface IMessagePublisher
{
    public Aff<Unit> PublishAsync(
        string correlationId,
        string category,
        string queue,
        Func<string> messageContentFunc,
        MessageSettings settings,
        ILogger logger
    );
}