using LanguageExt;
using LanguageExt.Effects.Traits;

namespace Infrastructure.Messaging.Azure.Blobs.Operations;

public interface IHaveBlobOperations<TRunTime> where TRunTime:struct, HasCancel<TRunTime>
{
    Aff<TRunTime, IBlobOperations> BlobOperations { get; }
}