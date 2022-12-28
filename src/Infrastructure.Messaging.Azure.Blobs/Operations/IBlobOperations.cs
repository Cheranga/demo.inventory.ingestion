using CsvHelper.Configuration;
using Infrastructure.Messaging.Azure.Blobs.Requests;
using LanguageExt;

namespace Infrastructure.Messaging.Azure.Blobs.Operations;

public interface IBlobOperations
{
    Aff<Unit> Upload(FileUploadRequest request);

    Aff<List<TData>> ReadDataFromCsv<TData, TDataMap>(ReadFileRequest request)
        where TDataMap : ClassMap<TData>;
}
