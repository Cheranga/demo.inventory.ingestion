using System.Threading;
using FluentValidation;
using FluentValidation.Results;
using LanguageExt;
using static LanguageExt.Prelude;

namespace Demo.Inventory.Ingestion.Functions.Extensions;

public static class ValidationExtensions
{
    public static Aff<ValidationResult> ValidateAff<T>(
        this IValidator<T> validator,
        T data,
        CancellationToken token
    ) =>
        from validationResult in Aff(async () =>
        {
            var validationResult = await validator.ValidateAsync(data, token);
            if (validationResult.IsValid)
            {
                return validationResult;
            }

            throw new ValidationException(validationResult.Errors);
        })
        select validationResult;
}
