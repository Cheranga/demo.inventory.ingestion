using CsvHelper.Configuration;
using Demo.Inventory.Ingestion.Domain;
using LanguageExt;

namespace Infrastructure.Messaging.Azure.Blobs;

public interface IBlobManager
{
    Task<Either<ErrorResponse, List<TData>>> ReadDataFromCsv<TData, TDataMap>(ReadFileRequest request)
        where TDataMap : ClassMap<TData>;

    Aff<Unit> Upload(FileUploadRequest request);
}