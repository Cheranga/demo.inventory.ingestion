using System.Threading;
using Demo.Inventory.Ingestion.Functions.Features.AcceptInventoryChanges;
using FluentValidation;
using FluentValidation.Results;
using LanguageExt;
using Microsoft.Extensions.Logging;
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
                if (validationResult.IsValid)
                {
                    return validationResult;
                }

                throw new InvalidDataException(validationResult);
            })
            select validationResult;
}

// =>
// (
//     from result in Aff(async () => await validator.ValidateAsync(data, token))
//     select result
// ).Match(
//     result =>
//     {
//         if (result.IsValid)
//         {
//             logger.LogInformation("data is valid");
//             return FinSucc(unit);
//         }
//
//         logger.LogWarning("invalid data {@Data}", data);
//         return FinFail<Unit>(Error.New(400, "invalid data"));
//     },
//     error =>
//     {
//         logger.LogError(
//             error.ToException(),
//             "error occurred when validating data {@Data}",
//             data
//         );
//         return FinFail<Unit>(error);
//     }
// );
// }