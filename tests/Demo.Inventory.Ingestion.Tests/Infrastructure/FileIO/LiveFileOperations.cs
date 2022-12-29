using LanguageExt;

namespace Demo.Inventory.Ingestion.Tests.Infrastructure.FileIO;

public struct LiveFileOperations : IFileOperations
{
    public static readonly IFileOperations Default = new LiveFileOperations();

    public ValueTask<string> ReadAllText(string filePath) =>
        File.ReadAllText(filePath).AsValueTask();
}