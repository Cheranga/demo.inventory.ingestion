using System.Diagnostics.CodeAnalysis;
using FluentValidation;
using FluentValidation.Results;
using LanguageExt;
using LanguageExt.Common;

namespace Demo.Inventory.Ingestion.Domain;

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

    public static Error New(
        ValidationResult validationResult,
        int errorCode,
        string errorMessage
    ) =>
        new InvalidDataError(
            new ValidationException(validationResult.Errors),
            errorCode,
            errorMessage
        );

    public static Error New(Seq<ValidationFailure> errors, int errorCode, string errorMessage) =>
        New(new ValidationResult(errors.ToList()), errorCode, errorMessage);

    public ErrorResponse ToErrorResponse =>
        ErrorResponse.New(400, "invalid data", ValidationException.Errors);
}