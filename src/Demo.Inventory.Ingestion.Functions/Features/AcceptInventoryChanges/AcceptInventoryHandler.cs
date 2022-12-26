using System.Threading;
using System.Threading.Tasks;
using Demo.Inventory.Ingestion.Domain;
using Demo.Inventory.Ingestion.Functions.Extensions;
using FluentValidation;
using Infrastructure.Messaging.Azure.Queues;
using Infrastructure.Messaging.Azure.Queues.Demo;
using LanguageExt;
using LanguageExt.Effects.Traits;
using Microsoft.Extensions.Logging;
using static LanguageExt.Prelude;

namespace Demo.Inventory.Ingestion.Functions.Features.AcceptInventoryChanges;

// public class SomeBaseClass<T, TValidator> where T : SomeBaseClass<T, TValidator>
//     where TValidator : IValidator<T>
// {
//
// }
//
// public class SomeRequestValidator : AbstractValidator<SomeRequest>
// {
//
// }
//
// public class SomeRequest : SomeBaseClass<SomeRequest, SomeRequestValidator>
// {
//
// }

public interface IAcceptInventoryChangesHandler
{
    ValueTask<Either<ErrorResponse, Unit>> Execute<TRunTime>(
        TRunTime runTime,
        AcceptInventoryChangeRequest request,
        IValidator<AcceptInventoryChangeRequest> validator,
        AcceptInventorySettings settings,
        CancellationToken token
    ) where TRunTime : struct, IHaveQueueOperations<TRunTime>, HasCancel<TRunTime>;
}

public class LiveInventoryChangesHandler : IAcceptInventoryChangesHandler
{
    private readonly ILogger<LiveInventoryChangesHandler> _logger;

    public LiveInventoryChangesHandler(ILogger<LiveInventoryChangesHandler> logger)
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
                        request.ToJson
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



// public record AcceptInventoryHandler<TQueueRunTime>
//     where TQueueRunTime : struct, IHaveQueueOperations<TQueueRunTime>, HasCancel<TQueueRunTime>
// {
//     private readonly IValidator<AcceptInventoryChangeRequest> _validator;
//     private readonly AcceptInventorySettings _settings;
//     private readonly ILogger<AcceptInventoryHandler<TQueueRunTime>> _logger;
//
//     public AcceptInventoryHandler(
//         IValidator<AcceptInventoryChangeRequest> validator,
//         AcceptInventorySettings settings,
//         ILogger<AcceptInventoryHandler<TQueueRunTime>> logger
//     )
//     {
//         _validator = validator;
//         _settings = settings;
//         _logger = logger;
//     }
//
//     public async Task<Either<ErrorResponse, Unit>> ExecuteAsync(
//         TQueueRunTime runtime,
//         AcceptInventoryChangeRequest request,
//         CancellationToken token
//     ) =>
//     (
//         await (
//             from _ in _validator.ValidateAff(request, _logger, token)
//             from __ in QueueOperationsSchema<TQueueRunTime>.Publish(
//                 new MessageOperation(
//                     request.CorrelationId,
//                     _settings.Category,
//                     _settings.Queue,
//                     MessageSettings.DefaultSettings,
//                     request.ToJson
//                 )
//             )
//             select __
//         ).Run(runtime)
//     ).Match(
//         _ =>
//         {
//             _logger.LogInformation(
//                 "{CorrelationId} inventory changes accepted",
//                 request.CorrelationId
//             );
//             return Right(unit);
//         },
//         error =>
//         {
//             _logger.LogError(
//                 error.ToException(),
//                 "{CorrelationId}:{ErrorCode} inventory changes were not accepted",
//                 request.CorrelationId,
//                 error.Code
//             );
//             return Left<ErrorResponse, Unit>(
//                 error.ToException() is ValidationException
//                     ? ErrorResponse.ToError(
//                         ErrorCodes.InvalidData,
//                         ErrorMessages.InvalidData,
//                         ((ValidationException)error.ToException()).Errors
//                     )
//                     : ErrorResponse.ToError(error.Code, error.Message)
//             );
//         }
//     );
// }
