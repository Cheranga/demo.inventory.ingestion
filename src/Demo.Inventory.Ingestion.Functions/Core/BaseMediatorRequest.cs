using Demo.Inventory.Ingestion.Functions.Extensions;
using FluentValidation;
using MediatR;

namespace Demo.Inventory.Ingestion.Functions.Core;

public record BaseMediatorRequest<TRequest, TResponse, TValidator>
    where TRequest : BaseMediatorRequest<TRequest, TResponse, TValidator>, IRequest<TResponse>, ITrackable
    where TValidator : IValidator<TRequest>;