using System.Threading;
using FluentValidation;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.Logging;
using static LanguageExt.Prelude;

namespace Demo.Inventory.Ingestion.Functions.Extensions;

public static class ValidationExtensions
{
    public static Aff<Fin<Unit>> ValidateAff<T>(
        this IValidator<T> validator,
        T data,
        ILogger logger,
        CancellationToken token
    ) =>
        (
            from result in Aff(async () => await validator.ValidateAsync(data, token))
            select result
        ).Match(
            result =>
            {
                if (result.IsValid)
                {
                    logger.LogInformation("data is valid");
                    return FinSucc(unit);
                }

                logger.LogWarning("invalid data {@Data}", data);
                return FinFail<Unit>(Error.New(400, "invalid data"));
            },
            error =>
            {
                logger.LogError(
                    error.ToException(),
                    "error occurred when validating data {@Data}",
                    data
                );
                return FinFail<Unit>(error);
            }
        );
}
