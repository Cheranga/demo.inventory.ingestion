using System.Threading;
using System.Threading.Tasks;
using Demo.Inventory.Ingestion.Functions.Core;
using Demo.Inventory.Ingestion.Functions.Extensions;
using Demo.Inventory.Ingestion.Functions.Infrastructure.Messaging;
using FluentValidation;
using LanguageExt;
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

    public AcceptInventoryChangeRequestHandler(
        IMessagePublisher messagePublisher,
        AcceptInventorySettings settings,
        IValidator<AcceptInventoryChangeRequest> validator,
        ILogger<AcceptInventoryChangeRequestHandler> logger
    )
    {
        _messagePublisher = messagePublisher;
        _settings = settings;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Either<ErrorResponse, Unit>> Handle(
        AcceptInventoryChangeRequest request,
        CancellationToken cancellationToken
    )
    {
        return (
            await (
                from a in _validator.ValidateAff(request, cancellationToken)
                from op in _messagePublisher.PublishAsync(
                    _settings.Category,
                    _settings.Queue,
                    request.ToJson,
                    MessageSettings.DefaultSettings
                )
                select op
            ).Run()
        ).Match(
            _ =>
            {
                _logger.LogInformation(
                    "{CorrelationId} inventory changes accepted",
                    request.CorrelationId
                );
                return Either<ErrorResponse, Unit>.Right(unit);
            },
            error =>
            {
                _logger.LogError(
                    "{CorrelationId}:{ErrorCode} inventory changes were not accepted",
                    request.CorrelationId,
                    error.Code
                );
                return Either<ErrorResponse, Unit>.Left(
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
}
