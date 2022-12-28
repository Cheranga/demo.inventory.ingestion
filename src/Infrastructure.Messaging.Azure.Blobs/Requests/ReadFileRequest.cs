namespace Infrastructure.Messaging.Azure.Blobs.Requests;

public record struct ReadFileRequest(
    string CorrelationId,
    string Category,
    string Container,
    string FileName
);