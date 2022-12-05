using FluentValidation;
using MediatR;

namespace Demo.Inventory.Ingestion.Functions.Extensions;

public record BaseMediatorRequest<TRequest, TResponse, TValidator>
    where TRequest : BaseMediatorRequest<TRequest, TResponse, TValidator>, IRequest<TResponse>, ITrackable
    where TValidator : IValidator<TRequest>;