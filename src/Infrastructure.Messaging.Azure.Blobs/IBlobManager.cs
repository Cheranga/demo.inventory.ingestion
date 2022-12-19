using CsvHelper.Configuration;
using LanguageExt;

namespace Infrastructure.Messaging.Azure.Blobs;

public interface IBlobManager
{
    Aff<List<TData>> ReadDataFromCsv<TData, TDataMap>(ReadFileRequest request)
        where TDataMap : ClassMap<TData>;

    Aff<Unit> Upload(FileUploadRequest request);
}