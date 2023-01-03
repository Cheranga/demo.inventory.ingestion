using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Azure.Storage.Blobs;
using CsvHelper;
using CsvHelper.Configuration;
using LanguageExt;
using static LanguageExt.Prelude;

namespace Infrastructure.Messaging.Azure.Blobs.Extensions;

[ExcludeFromCodeCoverage]
public static class BlobExtensions
{
    public static Aff<List<TData>> GetCsvDataFromBlob<TData, TDataMap>(this BlobClient client)
        where TDataMap : ClassMap<TData> =>
        from response in Aff(async () =>
        {
            using var stream = new MemoryStream();
            await client.DownloadToAsync(stream);
            stream.Position = 0;

            using var streamReader = new StreamReader(stream);
            using var csvReader = new CsvReader(streamReader, CultureInfo.InvariantCulture);
            csvReader.Context.RegisterClassMap<TDataMap>();
            return csvReader.GetRecords<TData>().ToList();
        })
        select response;
}
