using LanguageExt;

namespace Infrastructure.Messaging.Azure.Blobs.Operations;

public interface IBlobOperations
{
    Aff<Unit> Upload(FileUploadRequest request);
}