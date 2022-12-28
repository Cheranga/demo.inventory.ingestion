using CsvHelper.Configuration;
using Infrastructure.Messaging.Azure.Blobs.Requests;
using LanguageExt;
using LanguageExt.Effects.Traits;

namespace Infrastructure.Messaging.Azure.Blobs.Operations;

public class BlobOperationsSchema<TRunTime>
    where TRunTime : struct, IHaveBlobOperations<TRunTime>, HasCancel<TRunTime>
{
    public static Aff<TRunTime, Unit> Upload(FileUploadRequest request) =>
        from op in default(TRunTime).BlobOperations
        from resp in op.Upload(request)
        select resp;

    public static Aff<TRunTime, List<TData>> ReadDataFromCsv<TData, TClassMap>(
        ReadFileRequest request
    ) where TClassMap : ClassMap<TData> =>
        from op in default(TRunTime).BlobOperations
        from resp in op.ReadDataFromCsv<TData, TClassMap>(request)
        select resp;
}
