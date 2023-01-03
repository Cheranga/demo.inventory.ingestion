using System.Diagnostics.CodeAnalysis;
using Infrastructure.Messaging.Azure.Blobs.Requests;

namespace Infrastructure.Messaging.Azure.Blobs.Operations;

[ExcludeFromCodeCoverage]
public class BlobOperationException<TData> : Exception where TData : IBlobRequest
{
    public TData? BlobOperation { get; }

    public BlobOperationException(TData? data, string errorMessage, Exception? exception)
        : base(errorMessage, exception)
    {
        BlobOperation = data;
    }
}
