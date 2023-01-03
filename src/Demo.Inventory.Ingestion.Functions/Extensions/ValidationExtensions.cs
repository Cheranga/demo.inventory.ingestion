using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Demo.Inventory.Ingestion.Domain;
using FluentValidation;
using FluentValidation.Results;
using Infrastructure.Messaging.Azure.Queues;
using LanguageExt;
using LanguageExt.Common;
using static LanguageExt.Prelude;

namespace Demo.Inventory.Ingestion.Functions.Extensions;

[ExcludeFromCodeCoverage]
public record InvalidDataError : Error
{
    private InvalidDataError(
        ValidationException validationException,
        int errorCode,
        string errorMessage
    )
    {
        ValidationException = validationException;
        Message = errorMessage;
        Code = errorCode;
    }

    public ValidationException ValidationException { get; }

    public override string Message { get; }
    public override bool IsExceptional => true;
    public override bool IsExpected => false;

    public override int Code { get; }

    public override bool Is<E>() => ValidationException is E;

    public override ErrorException ToErrorException() =>
        new ExceptionalException(ValidationException);

    public static InvalidDataError New(
        ValidationResult validationResult,
        int errorCode,
        string errorMessage
    ) => new(new ValidationException(validationResult.Errors), errorCode, errorMessage);
}

[ExcludeFromCodeCoverage]
public static class ValidationExtensions
{
    public static Aff<ValidationResult> ValidateAff<T>(
        this IValidator<T> validator,
        T data,
        CancellationToken token
    ) where T : ITrackable =>
        from op in AffMaybe<ValidationResult>(
                async () => await validator.ValidateAsync(data, token)
            )
            .MapFail(error => Error.New(ErrorCodes.InternalServerError, "invalid data", error))
        from validationResult in op.IsValid
            ? SuccessAff(op)
            : FailAff<ValidationResult>(InvalidDataError.New(op, 400, "invalid input"))
        select validationResult;
}
