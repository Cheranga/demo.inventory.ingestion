namespace Infrastructure.Messaging.Azure.Blobs;

public record struct FileUploadRequest(
    string CorrelationId,
    string Category,
    string Container,
    string FileName,
    string Content
);