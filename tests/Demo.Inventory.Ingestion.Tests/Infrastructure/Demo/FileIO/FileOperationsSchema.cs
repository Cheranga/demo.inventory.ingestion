using LanguageExt;
using LanguageExt.Effects.Traits;

namespace Demo.Inventory.Ingestion.Tests.Infrastructure.Demo.FileIO;

public static class FileOperationsSchema<RT>
    where RT : struct, IHaveFileOperations<RT>, HasCancel<RT>
{
    public static Aff<RT, string> ReadAllText(string path) =>
        default(RT).FileOperations.MapAsync(operations => operations.ReadAllText(path));
}