namespace Infrastructure.Messaging.Azure.Blobs;

public record struct ReadFileRequest(
    string CorrelationId,
    string Category,
    string Container,
    string FileName
);