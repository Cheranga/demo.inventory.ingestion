using System.Threading;
using System.Threading.Tasks;
using Demo.Inventory.Ingestion.Functions.Extensions;
using Demo.Inventory.Ingestion.Functions.Infrastructure.Messaging;
using FluentValidation;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.Logging;

namespace Demo.Inventory.Ingestion.Functions.Features.AcceptInventoryChanges;

public record AcceptInventoryChangeRequestHandler
    : MediatR.IRequestHandler<AcceptInventoryChangeRequest, Fin<Unit>>
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

    public async Task<Fin<Unit>> Handle(
        AcceptInventoryChangeRequest request,
        CancellationToken cancellationToken
    )
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            _logger.LogWarning(
                "{CorrelationId} invalid data {@Data}",
                request.CorrelationId,
                request
            );

            return Prelude.FinFail<Unit>(Error.New(ErrorCodes.InvalidData, ErrorMessages.InvalidData));
        }

        var result = (
            await _messagePublisher
                .PublishAsync(
                    _settings.Category,
                    _settings.Queue,
                    request.ToJson,
                    MessageSettings.DefaultSettings
                )
                .Run()
        ).Match(
            _ =>
            {
                _logger.LogInformation(
                    "{CorrelationId} inventory changes accepted",
                    request.CorrelationId
                );
                return Prelude.FinSucc(Prelude.unit);
            },
            err =>
            {
                _logger.LogError(
                    err.ToException(),
                    "{CorrelationId} accepting inventory changes failed {@Data}",
                    request.CorrelationId,
                    request
                );
                return err;
            }
        );
        return result;
    }
}