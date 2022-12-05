using Demo.Inventory.Ingestion.Functions.Extensions;
using FluentValidation;
using LanguageExt;
using MediatR;
using Unit = LanguageExt.Unit;

namespace Demo.Inventory.Ingestion.Functions.Features.AcceptInventoryChanges;

public record Blah(string CorrelationId, string FileName) : ITrackable;

public record AcceptInventoryChangeRequest(string CorrelationId, string FileName)
    : BaseMediatorRequest<
        AcceptInventoryChangeRequest,
        Fin<Unit>,
        AcceptInventoryChangeRequest.Validator
    >,
        ITrackable,
        IRequest<Fin<Unit>>
{
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