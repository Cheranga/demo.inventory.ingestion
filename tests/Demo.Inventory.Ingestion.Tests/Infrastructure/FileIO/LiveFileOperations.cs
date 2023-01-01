using System.Globalization;
using Azure.Storage.Blobs;
using CsvHelper;
using CsvHelper.Configuration;
using LanguageExt;
using static LanguageExt.Prelude;

namespace Demo.Inventory.Ingestion.Tests.Infrastructure.FileIO;

public struct LiveFileOperations : IFileOperations
{
    public static readonly IFileOperations Default = new LiveFileOperations();

    public ValueTask<string> ReadAllText(string filePath) =>
        File.ReadAllText(filePath).AsValueTask();

    
}
