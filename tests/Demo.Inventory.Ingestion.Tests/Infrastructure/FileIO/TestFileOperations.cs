using LanguageExt;

namespace Demo.Inventory.Ingestion.Tests.Infrastructure.FileIO;

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

    public ValueTask<string> ReadContent(string filePath) => ReadAllText(filePath);

    public static TestFileOperations New(IDictionary<string, string> files) => new(files);
}