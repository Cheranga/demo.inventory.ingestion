namespace Demo.Inventory.Ingestion.Tests.Infrastructure.FileIO;

public interface IFileOperations
{
    ValueTask<string> ReadAllText(string filePath);
}