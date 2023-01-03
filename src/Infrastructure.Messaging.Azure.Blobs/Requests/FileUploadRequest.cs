using System.Diagnostics.CodeAnalysis;

namespace Infrastructure.Messaging.Azure.Blobs.Requests;

[ExcludeFromCodeCoverage]
public record struct FileUploadRequest(string CorrelationId, string Category, string Container, string FileName, string Content) : IBlobRequest;
public interface IBlobRequest
{
    string CorrelationId { get; }
    string Category { get; }
    string Container { get; }
    string FileName { get; }
}