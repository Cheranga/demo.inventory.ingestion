using System.Threading.Tasks;
using Demo.Inventory.Ingestion.Domain;
using Demo.Inventory.Ingestion.Functions.Features.AcceptInventoryChanges;
using FluentValidation.Results;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Newtonsoft.Json;
using static LanguageExt.Prelude;

namespace Demo.Inventory.Ingestion.Functions.Extensions;

public static class HttpExtensions
{
    private static readonly Error EmptyRequestBody = InvalidDataError.New(
        Seq1(new ValidationFailure("body", ErrorMessages.EmptyRequestBody)),
        ErrorCodes.EmptyRequestBody,
        ErrorMessages.EmptyRequestBody
    );

    private static readonly Error InvalidRequestBody = InvalidDataError.New(
        Seq1(new ValidationFailure("body", ErrorMessages.InvalidRequestBody)),
        ErrorCodes.InvalidRequestBody,
        ErrorMessages.InvalidRequestBody
    );

    public static async Task<Either<ErrorResponse, TModel>> ToModel<TModel>(
        this HttpRequest request
    ) =>
        (
            await (
                from content in Aff(async () => await request.ReadAsStringAsync())
                from _ in guard(!string.IsNullOrEmpty(content), EmptyRequestBody)
                from data in EffMaybe<TModel>(() => JsonConvert.DeserializeObject<TModel>(content))
                    .MapFail(_ => InvalidRequestBody)
                select data
            ).Run()
        ).Match(
            Right<ErrorResponse, TModel>,
            error => error.ToErrorResponse()
        );

    public static async Task<Either<ErrorResponse, TModel>> ToModelAsync<TModel>(
        this HttpRequest request
    ) =>
        (
            await (
                from content in Aff(async () => await request.ReadAsStringAsync())
                from _ in guard(
                    !string.IsNullOrEmpty(content),
                    Error.New(401, "empty request body")
                )
                from data in EffMaybe<TModel>(() => JsonConvert.DeserializeObject<TModel>(content))
                    .MapFail(
                        error =>
                            Error.New(
                                402,
                                "cannot convert to requested data type",
                                error.ToException()
                            )
                    )
                select data
            ).Run()
        )
            .ToEither()
            .Match(
                Right<ErrorResponse, TModel>,
                error =>
                    Left<ErrorResponse, TModel>(
                        ErrorResponse.New(
                            400,
                            "invalid request",
                            new[] { new ValidationFailure(error.Code.ToString(), error.Message) }
                        )
                    )
            );
}
