using System.Diagnostics.CodeAnalysis;

namespace Infrastructure.Messaging.Azure.Blobs.Requests;

[ExcludeFromCodeCoverage]
public record struct ReadFileRequest(
    string CorrelationId,
    string Category,
    string Container,
    string FileName
);