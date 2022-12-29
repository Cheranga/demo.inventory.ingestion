using System;
using System.Diagnostics.CodeAnalysis;
using Demo.Inventory.Ingestion.Domain;
using FluentValidation;
using FluentValidation.Validators;
using LanguageExt;
using MediatR;
using Unit = LanguageExt.Unit;

namespace Demo.Inventory.Ingestion.Functions.Features.AcceptInventoryChanges;

[ExcludeFromCodeCoverage(Justification = "This doesn't include any logic to unit test")]
public record struct AcceptInventoryChangeRequest(string CorrelationId, string FileName)
    : ITrackable,
        IRequest<Either<ErrorResponse, Unit>>
{
    [ExcludeFromCodeCoverage(Justification = "This uses fluent validation to perform validation")]
    public class Validator : BaseModelValidator<AcceptInventoryChangeRequest>
    {
        public Validator()
        {
            RuleFor(x => x.CorrelationId)
                .NotNull()
                .WithMessage("correlationid is null")
                .NotEmpty()
                .WithMessage("correlationid cannot be empty");
            RuleFor(x => x.FileName).NotNull().NotEmpty().WithMessage("filename is required");
        }
    }
}
