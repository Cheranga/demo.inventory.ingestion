using LanguageExt.Common;
using Microsoft.Azure.WebJobs;

namespace Infrastructure.Messaging.Azure.Queues;

public class QueueOperationException : Exception
{
    public Error Error { get; }

    public QueueOperationException(Error error)
    {
        Error = error;
    }
}

public record QueueOperationError : Error
{
    private readonly QueueOperationException _exception;

    private QueueOperationError(int errorCode, string errorMessage, Error error) : base(error)
    {
        Code = errorCode;
        Message = errorMessage;
        _exception = new QueueOperationException(error);
    }

    public override bool Is<E>() => _exception is E;

    public override ErrorException ToErrorException() =>
        ErrorException.New(Code, Message, ErrorException.New(_exception));

    public override int Code { get; }
    public override string Message { get; }
    public override bool IsExceptional => true;
    public override bool IsExpected => false;

    public new static QueueOperationError New(int errorCode, string errorMessage, Error error) =>
        new(errorCode, errorMessage, error);
}
