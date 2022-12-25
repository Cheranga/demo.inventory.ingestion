using LanguageExt;

namespace Demo.Inventory.Ingestion.Tests.Infrastructure.Demo.FileIO;

public struct TestFileOperations : IFileOperations
{
    private readonly IDictionary<string, string> _files;

    private TestFileOperations(IDictionary<string, string> files)
    {
        _files = files;
    }

    public ValueTask<string> ReadAllText(string filePath) =>
        _files.ContainsKey(filePath)
            ? _files[filePath].AsValueTask()
            : throw new FileNotFoundException($"{filePath} not found");

    public static TestFileOperations New(IDictionary<string, string> files) => new(files);
}