namespace Demo.Inventory.Ingestion.Tests.Infrastructure.Demo.FileIO;

public interface IFileOperations
{
    ValueTask<string> ReadAllText(string filePath);
}