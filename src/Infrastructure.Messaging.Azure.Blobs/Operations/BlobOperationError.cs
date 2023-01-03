using System.Diagnostics.CodeAnalysis;
using Infrastructure.Messaging.Azure.Blobs.Requests;
using LanguageExt.Common;

namespace Infrastructure.Messaging.Azure.Blobs.Operations;

[ExcludeFromCodeCoverage]
public record BlobOperationError<TData> : Error where TData : IBlobRequest
{
    private readonly BlobOperationException<TData> _blobOperationException;

    private BlobOperationError(
        int errorCode,
        string errorMessage,
        TData? data,
        Exception? exception
    )
    {
        Code = errorCode;
        Message = errorMessage;
        Data = data;
        _blobOperationException = new BlobOperationException<TData>(data, errorMessage, exception);
    }

    public override bool Is<E>() => _blobOperationException is E;

    public override ErrorException ToErrorException() =>
        ErrorException.New(Code, Message, ErrorException.New(_blobOperationException));

    public override int Code { get; }
    public override string Message { get; }
    public TData? Data { get; }

    public override bool IsExceptional => true;
    public override bool IsExpected => false;

    public static BlobOperationError<TData> New(
        int code,
        string message,
        TData? data = default,
        Exception? exception = null
    ) => new(code, message, data, exception);
}
