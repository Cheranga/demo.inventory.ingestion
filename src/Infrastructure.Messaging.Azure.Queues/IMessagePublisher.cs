using LanguageExt;

namespace Infrastructure.Messaging.Azure.Queues;

public interface IMessagePublisher
{
    public Aff<Unit> PublishAsync(
        string category,
        string queue,
        Func<string> messageContentFunc,
        MessageSettings settings
    );
}