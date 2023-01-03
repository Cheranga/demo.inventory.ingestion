using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using CsvHelper.Configuration;
using Infrastructure.Messaging.Azure.Blobs.Extensions;
using Infrastructure.Messaging.Azure.Blobs.Requests;
using LanguageExt;
using Microsoft.Extensions.Azure;
using static LanguageExt.Prelude;

namespace Infrastructure.Messaging.Azure.Blobs.Operations;

internal class AzureStorageBlobOperations : IBlobOperations
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
        from blobServiceClient in GetBlobServiceClient(_factory, request)
        from blobContainerClient in Eff(
            () => blobServiceClient.GetBlobContainerClient(request.Container)
        )
        from blobClient in Eff(() => blobContainerClient.GetBlobClient(request.FileName))
        from response in blobClient.GetCsvDataFromBlob<TData, TDataMap>()
        select response;

    public static AzureStorageBlobOperations New(IAzureClientFactory<BlobServiceClient> factory) =>
        new(factory);

    private static Eff<BlobServiceClient> GetBlobServiceClient<TBlobRequest>(
        IAzureClientFactory<BlobServiceClient> factory,
        TBlobRequest request
    ) where TBlobRequest:IBlobRequest
        =>
        EffMaybe<BlobServiceClient>(() => factory.CreateClient(request.Category))
            .MapFail(
                error =>
                    BlobOperationError<TBlobRequest>.New(
                        ErrorCodes.UnregisteredBlobServiceClient,
                        ErrorMessages.UnregisteredBlobServiceClient,
                        exception: error.ToException()
                    )
            );

    private static Eff<BlobContainerClient> GetBlobContainerClient<TBlobRequest>(
        BlobServiceClient serviceClient,
        TBlobRequest request
    ) where TBlobRequest:IBlobRequest
        =>
        EffMaybe<BlobContainerClient>(() => serviceClient.GetBlobContainerClient(request.Container))
            .MapFail(
                error =>
                    BlobOperationError<TBlobRequest>.New(
                        ErrorCodes.CannotGetBlobContainerClient,
                        ErrorMessages.CannotGetBlobContainerClient,
                        request,
                        error.ToException()
                    )
            );

    private static Eff<BlobClient> GetBlobClient<TBlobRequest>(
        BlobContainerClient containerClient,
        TBlobRequest request
    ) where TBlobRequest:IBlobRequest
        =>
        EffMaybe<BlobClient>(() => containerClient.GetBlobClient(request.FileName))
            .MapFail(
                error =>
                    BlobOperationError<TBlobRequest>.New(
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
                    BlobOperationError<FileUploadRequest>.New(
                        ErrorCodes.CannotUpload,
                        ErrorMessages.CannotUpload,
                        request,
                        error.ToException()
                    )
            )
        from response in op.GetRawResponse().IsError
            ? FailAff<Response<BlobContentInfo>>(
                BlobOperationError<FileUploadRequest>.New(
                    ErrorCodes.UploadFailResponse,
                    ErrorMessages.UploadFailResponse,
                    request
                )
            )
            : SuccessAff(op)
        select response;
}
