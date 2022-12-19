using Azure.Storage.Blobs;
using CsvHelper.Configuration;
using LanguageExt;
using Microsoft.Extensions.Azure;

namespace Infrastructure.Messaging.Azure.Blobs;

public class AzureStorageBlobManager : IBlobManager
{
    private readonly IAzureClientFactory<BlobServiceClient> _factory;

    public AzureStorageBlobManager(IAzureClientFactory<BlobServiceClient> factory)
    {
        _factory = factory;
    }

    public Aff<List<TData>> ReadDataFromCsv<TData, TDataMap>(ReadFileRequest request)
        where TDataMap : ClassMap<TData> => _factory.ReadDataFromCsv<TData, TDataMap>(request);

    public Aff<Unit> Upload(FileUploadRequest request) => _factory.Upload(request);
}
