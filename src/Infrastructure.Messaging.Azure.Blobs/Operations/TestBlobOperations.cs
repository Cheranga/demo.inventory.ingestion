using System.Diagnostics.CodeAnalysis;
using CsvHelper.Configuration;
using Infrastructure.Messaging.Azure.Blobs.Requests;
using LanguageExt;
using LanguageExt.Common;
using Newtonsoft.Json;
using static LanguageExt.Prelude;

namespace Infrastructure.Messaging.Azure.Blobs.Operations;

[ExcludeFromCodeCoverage]
internal class TestBlobOperations : IBlobOperations
{
    private readonly Dictionary<string, string> _blobs;

    public TestBlobOperations(Dictionary<string, string> blobs)
    {
        _blobs = blobs;
    }

    public Aff<Unit> Upload(FileUploadRequest request) =>
        Try(() =>
            {
                _blobs.Add(request.FileName, request.Content);
                return unit;
            })
            .ToEff();

    public Aff<List<TData>> ReadDataFromCsv<TData, TDataMap>(ReadFileRequest request)
        where TDataMap : ClassMap<TData> =>
        (
            from _ in guard(
                _blobs.TryGetValue(request.FileName, out var content),
                Error.New(404,"blob not found")
            )
            from data in Eff(() => JsonConvert.DeserializeObject<List<TData>>(content))
            select data
        ).MapFail(
            error =>
                BlobOperationError<ReadFileRequest>.New(
                    666,
                    "cannot read data",
                    request,
                    error.ToException()
                )
        );
}
