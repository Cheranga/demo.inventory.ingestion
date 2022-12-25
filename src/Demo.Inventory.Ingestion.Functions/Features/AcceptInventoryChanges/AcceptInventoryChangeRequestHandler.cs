using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Queues;
using Demo.Inventory.Ingestion.Domain;
using Demo.Inventory.Ingestion.Functions.Extensions;
using FluentValidation;
using Infrastructure.Messaging.Azure.Queues;
using Infrastructure.Messaging.Azure.Queues.Demo;
using LanguageExt;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Logging;
using static LanguageExt.Prelude;

namespace Demo.Inventory.Ingestion.Functions.Features.AcceptInventoryChanges;

public record AcceptInventoryChangeRequestHandler
    : MediatR.IRequestHandler<AcceptInventoryChangeRequest, Either<ErrorResponse, Unit>>
{
    private readonly ILogger<AcceptInventoryChangeRequestHandler> _logger;
    private readonly IMessagePublisher _messagePublisher;
    private readonly AcceptInventorySettings _settings;
    private readonly IValidator<AcceptInventoryChangeRequest> _validator;
    private readonly IAzureClientFactory<QueueServiceClient> _factory;

    public AcceptInventoryChangeRequestHandler(
        IMessagePublisher messagePublisher,
        AcceptInventorySettings settings,
        IValidator<AcceptInventoryChangeRequest> validator,
        IAzureClientFactory<QueueServiceClient> factory,
        ILogger<AcceptInventoryChangeRequestHandler> logger
    )
    {
        _messagePublisher = messagePublisher;
        _settings = settings;
        _validator = validator;
        _factory = factory;
        _logger = logger;
    }

    public async Task<Either<ErrorResponse, Unit>> Handle(
        AcceptInventoryChangeRequest request,
        CancellationToken cancellationToken
    ) =>
        (
            await (
                from _ in _validator.ValidateAff(request, _logger, cancellationToken)
                from __ in QueueOperationsSchema<LiveQueueRunTime>.Publish(new MessageOperation(request.CorrelationId,
                    _settings.Category,
                    _settings.Queue,
                    MessageSettings.DefaultSettings, 
                    request.ToJson))
                    // _messagePublisher.PublishAsync(
                    // request.CorrelationId,
                    // _settings.Category,
                    // _settings.Queue,
                    // request.ToJson,
                    // MessageSettings.DefaultSettings,
                    // _logger
                select __
                ).Run(LiveQueueRunTime.New(_factory))
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
