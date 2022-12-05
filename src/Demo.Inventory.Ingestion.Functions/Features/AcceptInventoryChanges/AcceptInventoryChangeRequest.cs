using Demo.Inventory.Ingestion.Functions.Core;
using Demo.Inventory.Ingestion.Functions.Extensions;
using FluentValidation;
using LanguageExt;
using MediatR;
using Unit = LanguageExt.Unit;

namespace Demo.Inventory.Ingestion.Functions.Features.AcceptInventoryChanges;

public record AcceptInventoryChangeRequest(string CorrelationId, string FileName)
    : BaseMediatorRequest<
        AcceptInventoryChangeRequest,
        Either<ErrorResponse, Unit>,
        AcceptInventoryChangeRequest.Validator
    >,
        ITrackable,
        IRequest<Either<ErrorResponse, Unit>>
{
    public class Validator : BaseModelValidator<AcceptInventoryChangeRequest>
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