using System.Diagnostics.CodeAnalysis;
using FluentValidation;
using MediatR;

namespace Demo.Inventory.Ingestion.Domain;

[ExcludeFromCodeCoverage]
public record BaseMediatorRequest<TRequest, TResponse, TValidator>
    where TRequest : BaseMediatorRequest<TRequest, TResponse, TValidator>, IRequest<TResponse>, ITrackable
    where TValidator : IValidator<TRequest>;