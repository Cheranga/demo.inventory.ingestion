using LanguageExt;
using LanguageExt.Effects.Traits;

namespace Demo.Inventory.Ingestion.Tests.Infrastructure.Demo.FileIO;

public interface IHaveFileOperations<RT> where RT : struct, HasCancel<RT>
{
    Aff<RT, IFileOperations> FileOperations { get; }
}