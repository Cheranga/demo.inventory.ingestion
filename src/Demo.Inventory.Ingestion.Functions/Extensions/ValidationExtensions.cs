using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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
