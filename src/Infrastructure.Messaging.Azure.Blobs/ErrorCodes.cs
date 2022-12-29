using System.Diagnostics.CodeAnalysis;

namespace Infrastructure.Messaging.Azure.Blobs;

[ExcludeFromCodeCoverage]
public class ErrorCodes
{
    public const int UnregisteredBlobServiceClient = 500;
    public const int CannotGetBlobContainerClient = 501;
    public const int CannotGetBlobClient = 502;
    public const int CannotUpload = 503;
    public const int UploadFailResponse = 504;
}

[ExcludeFromCodeCoverage]
public class ErrorMessages
{
    public const string UnregisteredBlobServiceClient = "unregistered blob service client";
    public const string CannotGetBlobContainerClient = "cannot get blob container client";
    public const string CannotGetBlobClient = "cannot get blob client";
    public const string CannotUpload = "cannot upload content";
    public const string UploadFailResponse = "upload operation returned unsuccessful response";
}
