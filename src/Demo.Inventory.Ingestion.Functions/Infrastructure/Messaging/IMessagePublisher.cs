using System;
using LanguageExt;
namespace Demo.Inventory.Ingestion.Functions.Infrastructure.Messaging;

public interface IMessagePublisher
{
    public Aff<Unit> PublishAsync(
        string category,
        string queue,
        Func<string> messageContentFunc,
        MessageSettings settings
    );
}