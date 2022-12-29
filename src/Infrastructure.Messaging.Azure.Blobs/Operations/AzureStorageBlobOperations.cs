using System.Globalization;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using CsvHelper;
using CsvHelper.Configuration;
using Infrastructure.Messaging.Azure.Blobs.Exceptions;
using Infrastructure.Messaging.Azure.Blobs.Requests;
using LanguageExt;
using LanguageExt.Common;
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
            from bsc in GetBlobServiceClient(_factory, request.Category)
            from bcc in GetBlobContainerClient(bsc, request.Container)
            from bc in GetBlobClient(bcc, request.FileName)
            from response in UploadContent(bc, request.Content)
            select response
        ).Map(
            response =>
                response.GetRawResponse().IsError
                    ? throw new UploadFileException(
                        Error.New(ErrorCodes.CannotUpload, response.GetRawResponse().ReasonPhrase)
                    )
                    : unit
        );

    public Aff<List<TData>> ReadDataFromCsv<TData, TDataMap>(ReadFileRequest request)
        where TDataMap : ClassMap<TData> =>
        from blobServiceClient in Eff(() => _factory.CreateClient(request.Category))
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

    public static AzureStorageBlobOperations New(IAzureClientFactory<BlobServiceClient> factory) =>
        new(factory);

    private static Eff<BlobServiceClient> GetBlobServiceClient(
        IAzureClientFactory<BlobServiceClient> factory,
        string category
    ) =>
        EffMaybe<BlobServiceClient>(() => factory.CreateClient(category))
            .MapFail(
                error =>
                    Error.New(
                        ErrorCodes.UnregisteredBlobServiceClient,
                        ErrorMessages.UnregisteredBlobServiceClient,
                        error
                    )
            );

    private static Eff<BlobContainerClient> GetBlobContainerClient(
        BlobServiceClient serviceClient,
        string container
    ) =>
        EffMaybe<BlobContainerClient>(() => serviceClient.GetBlobContainerClient(container))
            .MapFail(
                error =>
                    Error.New(
                        ErrorCodes.CannotGetBlobContainerClient,
                        ErrorMessages.CannotGetBlobContainerClient,
                        error
                    )
            );

    private static Eff<BlobClient> GetBlobClient(
        BlobContainerClient containerClient,
        string fileName
    ) =>
        EffMaybe<BlobClient>(() => containerClient.GetBlobClient(fileName))
            .MapFail(
                error =>
                    Error.New(ErrorCodes.CannotGetBlobClient, ErrorMessages.CannotGetBlobClient)
            );

    private static Aff<Response<BlobContentInfo>>UploadContent(
        BlobClient blobClient,
        string content
    ) =>
        AffMaybe<Response<BlobContentInfo>>(
                async () => await blobClient.UploadAsync(BinaryData.FromString(content), true)
            )
            .MapFail(
                error => Error.New(ErrorCodes.CannotUpload, ErrorMessages.CannotUpload, error)
            );
}
