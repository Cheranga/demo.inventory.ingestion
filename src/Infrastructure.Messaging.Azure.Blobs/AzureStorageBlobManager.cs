using Azure.Storage.Blobs;
using CsvHelper.Configuration;
using Demo.Inventory.Ingestion.Domain;
using LanguageExt;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Logging;
using static LanguageExt.Prelude;

namespace Infrastructure.Messaging.Azure.Blobs;

public class AzureStorageBlobManager : IBlobManager
{
    private readonly IAzureClientFactory<BlobServiceClient> _factory;
    private readonly ILogger<AzureStorageBlobManager> _logger;

    public AzureStorageBlobManager(
        IAzureClientFactory<BlobServiceClient> factory,
        ILogger<AzureStorageBlobManager> logger
    )
    {
        _factory = factory;
        _logger = logger;
    }

    public async Task<Either<ErrorResponse, List<TData>>> ReadDataFromCsv<TData, TDataMap>(
        ReadFileRequest request
    ) where TDataMap : ClassMap<TData> =>
        (
            await (from op in _factory.ReadDataFromCsv<TData, TDataMap>(request) select op).Run()
        ).Match(
            items =>
            {
                _logger.LogInformation(
                    "{CorrelationId} reading CSV data successful",
                    request.CorrelationId
                );
                return Right(items);
            },
            error =>
            {
                _logger.LogError(
                    error.ToException(),
                    "{CorrelationId} error occurred when reading data from CSV",
                    request.CorrelationId
                );
                return Left<ErrorResponse, List<TData>>(ErrorResponse.ToError(500, ""));
            }
        );

    public Aff<Unit> Upload(FileUploadRequest request) => _factory.Upload(request);
}
