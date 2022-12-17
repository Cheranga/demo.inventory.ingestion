using System.Globalization;
using Azure.Storage.Blobs;
using CsvHelper;
using CsvHelper.Configuration;
using LanguageExt;
using Microsoft.Extensions.Azure;
using static LanguageExt.Prelude;

namespace Infrastructure.Messaging.Azure.Blobs;

public static class AzureStorageBlobSchema
{
    public static Aff<string> ReadAsString(
        this IAzureClientFactory<BlobServiceClient> factory,
        ReadFileRequest request
    ) =>
        from blobServiceClient in Eff(() => factory.CreateClient(request.Category))
        from blobContainerClient in Eff(
            () => blobServiceClient.GetBlobContainerClient(request.Container)
        )
        from blobClient in Eff(() => blobContainerClient.GetBlobClient(request.FileName))
        from response in Aff(async () => await blobClient.DownloadContentAsync())
        from blobContent in Eff(() => response.GetRawResponse().Content.ToString())
        select blobContent;

    public static Aff<List<TData>> ReadDataFromCsv<TData, TDataMap>(
        this IAzureClientFactory<BlobServiceClient> factory,
        ReadFileRequest request
    ) where TDataMap : ClassMap<TData> =>
        from blobServiceClient in Eff(() => factory.CreateClient(request.Category))
        from blobContainerClient in Eff(
            () => blobServiceClient.GetBlobContainerClient(request.Container)
        )
        from blobClient in Eff(() => blobContainerClient.GetBlobClient(request.FileName))
        from response in Aff(async () =>
        {
            using var stream = new MemoryStream();
            await blobClient.DownloadToAsync(stream);
            stream.Position = 0;

            using var streamReader = new StreamReader(stream);
            using var csvReader = new CsvReader(streamReader, CultureInfo.InvariantCulture);
            csvReader.Context.RegisterClassMap<TDataMap>();
            return csvReader.GetRecords<TData>().ToList();
        })
        select response;

    public static Aff<Unit> Upload(
        this IAzureClientFactory<BlobServiceClient> factory,
        FileUploadRequest request
    )
    {
        return (
            from blobServiceClient in Eff(() => factory.CreateClient(request.Category))
            from blobContainerClient in Eff(
                () => blobServiceClient.GetBlobContainerClient(request.Container)
            )
            from blobClient in Eff(() => blobContainerClient.GetBlobClient(request.FileName))
            from response in Aff(
                async () => await blobClient.UploadAsync(BinaryData.FromString(request.Content), true)
            )
            select response
        ).Map(_ => unit);
    }
}
