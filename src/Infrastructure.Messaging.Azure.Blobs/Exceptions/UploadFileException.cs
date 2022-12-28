using LanguageExt.Common;

namespace Infrastructure.Messaging.Azure.Blobs.Exceptions;

public class UploadFileException : Exception
{
    public Error Error { get; }

    public UploadFileException(Error error)
    {
        Error = error;
    }
}