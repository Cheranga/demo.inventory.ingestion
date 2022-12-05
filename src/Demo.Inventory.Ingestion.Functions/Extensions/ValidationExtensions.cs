using System.Threading;
using Demo.Inventory.Ingestion.Functions.Features.AcceptInventoryChanges;
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
    ) where T : ITrackable
        =>
            from validationResult in Aff(async () =>
            {
                var validationResult = await validator.ValidateAsync(data, token);
                if (validationResult.IsValid) return validationResult;

                throw new InvalidDataException(validationResult);
            })
            select validationResult;
}