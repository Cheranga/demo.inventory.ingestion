using Infrastructure.Messaging.Azure.Blobs.Requests;
using LanguageExt.Common;

namespace Infrastructure.Messaging.Azure.Blobs.Operations;

public class FileUploadException : Exception
{
    public FileUploadRequest Request { get; }

    public FileUploadException(FileUploadRequest request, Exception? exception)
        : base("file upload error", exception)
    {
        Request = request;
    }
}

public record FileUploadError : Error
{
    private readonly FileUploadException _exception;

    public FileUploadError(
        int errorCode,
        string errorMessage,
        FileUploadRequest request,
        Exception? exception
    )
    {
        Code = errorCode;
        Message = errorMessage;
        _exception = new FileUploadException(request, exception);
    }

    public override bool Is<E>() => _exception is E;

    public override ErrorException ToErrorException() =>
        ErrorException.New(Code, Message, ErrorException.New(_exception));

    public override int Code { get; }
    public override string Message { get; }
    public override bool IsExceptional { get; } = true;
    public override bool IsExpected { get; } = false;

    public static FileUploadError New(
        int errorCode,
        string errorMessage,
        FileUploadRequest request,
        Exception? exception = null
    ) => new(errorCode, errorMessage, request, exception);
}
