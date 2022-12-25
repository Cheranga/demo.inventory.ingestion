using Azure;
using Azure.Storage.Queues.Models;

namespace Infrastructure.Messaging.Azure.Queues.Demo;

public class MessagePublishException : Exception
{
    public MessagePublishException(Response<SendReceipt> response)
        : base("message publish error") => Response = response;

    public MessagePublishException(string message, Exception exception) : base(message, exception)
    { }

    public Response<SendReceipt> Response { get; }
}