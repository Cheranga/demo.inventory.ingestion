namespace Infrastructure.Messaging.Azure.Blobs;

public class ErrorCodes
{
    public const int UnregisteredBlobServiceClient = 500;
    public const int CannotGetBlobContainerClient = 501;
    public const int CannotGetBlobClient = 502;
    public const int CannotUpload = 503;
}

public class ErrorMessages
{
    public const string UnregisteredBlobServiceClient = "unregistered blob service client";
    public const string CannotGetBlobContainerClient = "cannot get blob container client";
    public const string CannotGetBlobClient = "cannot get blob client";
    public const string CannotUpload = "cannot upload content";
}