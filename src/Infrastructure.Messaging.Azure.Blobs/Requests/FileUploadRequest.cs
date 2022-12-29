using System.Diagnostics.CodeAnalysis;

namespace Infrastructure.Messaging.Azure.Blobs.Requests;

[ExcludeFromCodeCoverage]
public record struct FileUploadRequest(
    string CorrelationId,
    string Category,
    string Container,
    string FileName,
    string Content
);