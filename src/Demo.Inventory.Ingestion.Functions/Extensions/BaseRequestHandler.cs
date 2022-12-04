using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using LanguageExt;
using MediatR;
using Microsoft.Extensions.Logging;
using static LanguageExt.Prelude;

namespace Demo.Inventory.Ingestion.Functions.Extensions;

public abstract record BaseRequestHandler<TRequest, TResponse, TRequestValidator>
    : IRequestHandler<TRequest, Fin<TResponse>>
    where TRequest : BaseMediatorRequest<TRequest, TResponse, TRequestValidator>,
        IRequest<Fin<TResponse>>,
        ITrackable,
        IRequest<TResponse>
    where TRequestValidator : IValidator<TRequest>
{
    private readonly IValidator<TRequest> _validator;
    private readonly ILogger<BaseRequestHandler<TRequest, TResponse, TRequestValidator>> _logger;

    protected BaseRequestHandler(
        IValidator<TRequest> validator,
        ILogger<BaseRequestHandler<TRequest, TResponse, TRequestValidator>> logger
    )
    {
        _validator = validator;
        _logger = logger;
    }

    protected abstract Aff<TResponse> Execute(TRequest request, CancellationToken token);

    public virtual async Task<Fin<TResponse>> Handle(
        TRequest request,
        CancellationToken cancellationToken
    ) =>
        await (
            from validationResult in _validator.ValidateAff(request, _logger, cancellationToken)
            from operation in Execute(request, cancellationToken)
            select operation
        ).Run();

    // (
    //     await (
    //         from validationResult in _validator.ValidateAff(request, _logger, cancellationToken)
    //         from operation in Execute(request, cancellationToken)
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
    //         return x;
    //     },
    //     error =>
    //     {
    //         _logger.LogError(
    //             "{CorrelationId} error occurred when processing the inventory change request",
    //             request.CorrelationId
    //         );
    //         return Prelude.FinFail<TResponse>(error);
    //     }
    // );
}
