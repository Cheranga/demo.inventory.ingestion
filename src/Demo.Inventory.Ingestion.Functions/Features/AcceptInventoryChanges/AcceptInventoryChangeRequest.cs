using System.Threading;
using System.Threading.Tasks;
using Demo.Inventory.Ingestion.Functions.Extensions;
using Demo.Inventory.Ingestion.Functions.Infrastructure.Messaging;
using FluentValidation;
using LanguageExt;
using LanguageExt.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Demo.Inventory.Ingestion.Functions.Features.AcceptInventoryChanges;

public record AcceptInventoryChangeRequest(string FileName)
    : BaseMediatorRequest<
            AcceptInventoryChangeRequest,
            bool,
            AcceptInventoryChangeRequest.Validator
        >,
        IRequest<Fin<bool>>,
        ITrackable
{
    public string CorrelationId { get; set; }

    public class Validator : AbstractValidator<AcceptInventoryChangeRequest>
    {
        public Validator()
        {
            RuleFor(x => x.CorrelationId)
                .NotNull()
                .NotEmpty()
                .WithMessage("correlationid is required");
            RuleFor(x => x.FileName).NotNull().NotEmpty().WithMessage("filename is required");
        }
    }
}

public record AcceptInventoryChangeRequestHandler
    : IRequestHandler<AcceptInventoryChangeRequest, Fin<bool>>
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

    public async Task<Fin<bool>> Handle(
        AcceptInventoryChangeRequest request,
        CancellationToken cancellationToken
    )
    {
        from vr in _validator.ValidateAff(request, cancellationToken)
                .MapFail(err => ErrorCodes.BadInputData(err.ToException()))
            from op in _messagePublisher.PublishAsync(_settings.Category, _settings.Queue, request.ToJson, MessageSettings.DefaultSettings)

        // var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        // if (!validationResult.IsValid)
        // {
        //     return FinFail<bool>(
        //         Error.New(ErrorCodes.BadInventoryChangeData, ErrorMessages.BadInventoryChangeData)
        //     );
        // }
        //
        // return (
        //     await _messagePublisher
        //         .PublishAsync(
        //             _settings.Category,
        //             _settings.Queue,
        //             request.ToJson,
        //             MessageSettings.DefaultSettings
        //         )
        //         .Run()
        // ).Match(_ => FinSucc(true), err => err);
    }
    // (
    //     await (
    //         from validationResult in Aff(async()=> await _validator.ValidateAsync(request, cancellationToken))//_validator.ValidateAff(request, _logger, cancellationToken)
    //         from a in validationResult.IsValid? FinFail<bool>(Error.New("")) : _messagePublisher.PublishAsync(
    //             _settings.Category,
    //             _settings.Queue,
    //             request.ToJson,
    //             MessageSettings.DefaultSettings
    //         )
    //         select operation
    //     ).Run()
    // ).Match(
    //     x =>
    //     {
    //         _logger.LogInformation(
    //             "{CorrelationId} successfully accepted inventory change request {@Request}",
    //             request.CorrelationId,
    //             request
    //         );
    //         return FinSucc(true);
    //     },
    //     error =>
    //     {
    //         _logger.LogError(
    //             "{CorrelationId} error occurred when processing the inventory change request",
    //             request.CorrelationId
    //         );
    //         return FinFail<bool>(error);
    //     }
    // );
}