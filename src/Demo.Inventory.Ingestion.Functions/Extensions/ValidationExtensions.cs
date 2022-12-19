using System.Threading;
using Demo.Inventory.Ingestion.Domain;
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
        ILogger logger,
        CancellationToken token
    ) where T : ITrackable =>
        from validationResult in Aff(async () =>
        {
            var validationResult = await validator.ValidateAsync(data, token);
            if (validationResult.IsValid)
            {
                logger.LogInformation("{CorrelationId} valid data {@Data}", data.CorrelationId, data);
                return validationResult;
            }

            logger.LogInformation("{CorrelationId} invalid data {@Data}", data.CorrelationId, data);
            throw new ValidationException(validationResult.Errors);
        })
        select validationResult;
}
