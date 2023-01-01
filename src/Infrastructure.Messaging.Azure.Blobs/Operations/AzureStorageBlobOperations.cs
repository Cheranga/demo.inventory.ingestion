using System.Globalization;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using CsvHelper;
using CsvHelper.Configuration;
using Infrastructure.Messaging.Azure.Blobs.Requests;
using LanguageExt;
using Microsoft.Extensions.Azure;
using static LanguageExt.Prelude;

namespace Infrastructure.Messaging.Azure.Blobs.Operations;

public class AzureStorageBlobOperations : IBlobOperations
{
    private readonly IAzureClientFactory<BlobServiceClient> _factory;

    private AzureStorageBlobOperations(IAzureClientFactory<BlobServiceClient> factory) =>
        _factory = factory;

    public Aff<Unit> Upload(FileUploadRequest request) =>
        (
            from bsc in GetBlobServiceClient(_factory, request)
            from bcc in GetBlobContainerClient(bsc, request)
            from bc in GetBlobClient(bcc, request)
            from response in UploadContent(bc, request)
            select response
        ).Map(_ => unit);

    public Aff<List<TData>> ReadDataFromCsv<TData, TDataMap>(ReadFileRequest request)
        where TDataMap : ClassMap<TData> =>
        from blobServiceClient in Eff(() => _factory.CreateClient(request.Category))
        from blobContainerClient in Eff(
            () => blobServiceClient.GetBlobContainerClient(request.Container)
        )
        from blobClient in Eff(() => blobContainerClient.GetBlobClient(request.FileName))
        from response in GetRecords<TData, TDataMap>(blobClient)
        select response;

    private static Aff<List<TData>> GetRecords<TData, TDataMap>(BlobClient client)
        where TDataMap : ClassMap<TData> =>
        (
            from stream in use(
                new MemoryStream(),
                async st =>
                {
                    await client.DownloadToAsync(st);
                    st.Position = 0;
                    return st;
                }
            )
            let items = use(
                new StreamReader(stream),
                sr =>
                    use(
                        () =>
                        {
                            var csvReader = new CsvReader(sr, CultureInfo.InvariantCulture);
                            csvReader.Context.RegisterClassMap<TDataMap>();
                            return csvReader;
                        },
                        csvr => csvr.GetRecords<TData>().ToList()
                    )
            )
            select items
        ).ToAff();

    // from response in Aff(async () =>
    // {
    //     using var stream = new MemoryStream();
    //     await blobClient.DownloadToAsync(stream);
    //     stream.Position = 0;
    //
    //     using var streamReader = new StreamReader(stream);
    //     using var csvReader = new CsvReader(streamReader, CultureInfo.InvariantCulture);
    //     csvReader.Context.RegisterClassMap<TDataMap>();
    //     return csvReader.GetRecords<TData>().ToList();
    // })
    // select response;

    public static AzureStorageBlobOperations New(IAzureClientFactory<BlobServiceClient> factory) =>
        new(factory);

    private static Eff<BlobServiceClient> GetBlobServiceClient(
        IAzureClientFactory<BlobServiceClient> factory,
        FileUploadRequest request
    ) =>
        EffMaybe<BlobServiceClient>(() => factory.CreateClient(request.Category))
            .MapFail(
                error =>
                    FileUploadError.New(
                        ErrorCodes.UnregisteredBlobServiceClient,
                        ErrorMessages.UnregisteredBlobServiceClient,
                        request,
                        error.ToException()
                    )
            );

    private static Eff<BlobContainerClient> GetBlobContainerClient(
        BlobServiceClient serviceClient,
        FileUploadRequest request
    ) =>
        EffMaybe<BlobContainerClient>(() => serviceClient.GetBlobContainerClient(request.Container))
            .MapFail(
                error =>
                    FileUploadError.New(
                        ErrorCodes.CannotGetBlobContainerClient,
                        ErrorMessages.CannotGetBlobContainerClient,
                        request,
                        error.ToException()
                    )
            );

    private static Eff<BlobClient> GetBlobClient(
        BlobContainerClient containerClient,
        FileUploadRequest request
    ) =>
        EffMaybe<BlobClient>(() => containerClient.GetBlobClient(request.FileName))
            .MapFail(
                error =>
                    FileUploadError.New(
                        ErrorCodes.CannotGetBlobClient,
                        ErrorMessages.CannotGetBlobClient,
                        request,
                        error.ToException()
                    )
            );

    private static Aff<Response<BlobContentInfo>> UploadContent(
        BlobClient blobClient,
        FileUploadRequest request
    ) =>
        from op in AffMaybe<Response<BlobContentInfo>>(
                async () =>
                    await blobClient.UploadAsync(BinaryData.FromString(request.Content), true)
            )
            .MapFail(
                error =>
                    FileUploadError.New(
                        ErrorCodes.CannotUpload,
                        ErrorMessages.CannotUpload,
                        request,
                        error.ToException()
                    )
            )
        from response in op.GetRawResponse().IsError
            ? FailAff<Response<BlobContentInfo>>(
                FileUploadError.New(
                    ErrorCodes.UploadFailResponse,
                    ErrorMessages.UploadFailResponse,
                    request
                )
            )
            : SuccessAff(op)
        select response;
}
