using System.Threading;
using System.Threading.Tasks;
using Demo.Inventory.Ingestion.Domain;
using Demo.Inventory.Ingestion.Functions.Extensions;
using FluentValidation;
using Infrastructure.Messaging.Azure.Queues;
using LanguageExt;
using LanguageExt.Effects.Traits;
using Microsoft.Extensions.Logging;
using static LanguageExt.Prelude;

namespace Demo.Inventory.Ingestion.Functions.Features.AcceptInventoryChanges;

public class InventoryChangesHandler : IInventoryChangesHandler
{
    private readonly ILogger<InventoryChangesHandler> _logger;

    public InventoryChangesHandler(ILogger<InventoryChangesHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<Either<ErrorResponse, Unit>> Execute<TRunTime>(
        TRunTime runTime,
        AcceptInventoryChangeRequest request,
        IValidator<AcceptInventoryChangeRequest> validator,
        AcceptInventorySettings settings,
        CancellationToken token
    ) where TRunTime : struct, IHaveQueueOperations<TRunTime>, HasCancel<TRunTime> =>
        (
            await (
                from _ in validator.ValidateAff(request, _logger, token)
                from __ in QueueOperationsSchema<TRunTime>.Publish(
                    new MessageOperation(
                        request.CorrelationId,
                        settings.Category,
                        settings.Queue,
                        MessageSettings.DefaultSettings,
                        () => request.ToJson()
                    )
                )
                select __
            ).Run(runTime)
        ).Match(
            _ =>
            {
                _logger.LogInformation(
                    "{CorrelationId} inventory changes accepted",
                    request.CorrelationId
                );
                return Right(unit);
            },
            error =>
            {
                _logger.LogError(
                    error.ToException(),
                    "{CorrelationId}:{ErrorCode} inventory changes were not accepted",
                    request.CorrelationId,
                    error.Code
                );
                return Left<ErrorResponse, Unit>(
                    error.ToException() is ValidationException
                        ? ErrorResponse.ToError(
                            ErrorCodes.InvalidData,
                            ErrorMessages.InvalidData,
                            ((ValidationException)error.ToException()).Errors
                        )
                        : ErrorResponse.ToError(error.Code, error.Message)
                );
            }
        );
}
