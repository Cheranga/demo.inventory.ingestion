using System.Diagnostics.CodeAnalysis;
using LanguageExt.Common;

namespace Infrastructure.Messaging.Azure.Queues.Operations;

[ExcludeFromCodeCoverage]
public record QueueOperationError : Error
{
    private readonly QueueOperationException _exception;

    private QueueOperationError(
        int errorCode,
        string errorMessage,
        string category,
        string queue,
        Exception? exception = null
    )
        : base(
            exception is null
                ? Error.New(errorCode, errorMessage)
                : Error.New(errorCode, errorMessage, exception)
        )
    {
        Code = errorCode;
        Message = errorMessage;
        Category = category;
        Queue = queue;
        _exception = new QueueOperationException(category, queue, exception);
    }

    public string Category { get; }
    public string Queue { get; }

    public override int Code { get; }
    public override string Message { get; }
    public override bool IsExceptional => true;
    public override bool IsExpected => false;

    public override bool Is<E>() => _exception is E;

    public override ErrorException ToErrorException() =>
        ErrorException.New(Code, Message, ErrorException.New(_exception));

    public static QueueOperationError New(
        int errorCode,
        string errorMessage,
        string category,
        string queue,
        Exception? exception = null
    ) => new(errorCode, errorMessage, category, queue, exception);
}
