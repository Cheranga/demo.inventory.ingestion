using System.Threading;
using System.Threading.Tasks;
using Demo.Inventory.Ingestion.Domain;
using Demo.Inventory.Ingestion.Functions.Extensions;
using FluentValidation;
using FluentValidation.Results;
using Infrastructure.Messaging.Azure.Queues;
using Infrastructure.Messaging.Azure.Queues.Operations;
using LanguageExt;
using LanguageExt.Common;
using LanguageExt.Effects.Traits;
using static LanguageExt.Prelude;

namespace Demo.Inventory.Ingestion.Functions.Features.AcceptInventoryChanges;

public static class InventoryChangesHandler
{
    public static async ValueTask<Either<ErrorResponse, Unit>> Execute<TRunTime>(
        TRunTime runTime,
        AcceptInventoryChangeRequest request,
        IValidator<AcceptInventoryChangeRequest> validator,
        AcceptInventorySettings settings,
        CancellationToken token
    ) where TRunTime : struct, IHaveQueueOperations<TRunTime>, HasCancel<TRunTime> =>
        (
            await (
                from _ in validator.ValidateAff(request, token)
                from __ in QueueOperationsSchema<TRunTime>.Publish(
                    request.CorrelationId,
                    settings.Category,
                    settings.Queue,
                    () => request.ToJson()
                )
                select __
            ).Run(runTime)
        ).Match(
            _ => Right<ErrorResponse, Unit>(unit),
            error => Left<ErrorResponse, Unit>(GetErrorResponse(error))
        );

    private static ErrorResponse GetErrorResponse(Error error) =>
        error switch
        {
            InvalidDataError invalidDataError
                => ErrorResponse.New(
                    ErrorCodes.InvalidData,
                    ErrorMessages.InvalidData,
                    invalidDataError.ValidationException.Errors
                ),
            QueueOperationError queueOperationError
                => ErrorResponse.New(
                    ErrorCodes.CannotPublishToQueue,
                    ErrorMessages.CannotPublishToQueue,
                    new[]
                    {
                        new ValidationFailure("", queueOperationError.Message)
                        {
                            ErrorCode = queueOperationError.Code.ToString()
                        }
                    }
                ),
            _ => ErrorResponse.New(error.Code, error.Message)
        };
}
