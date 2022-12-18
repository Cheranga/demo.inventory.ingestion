using System.Threading;
using LanguageExt;
using MediatR;
using static LanguageExt.Prelude;

namespace Demo.Inventory.Ingestion.Functions.Extensions;



public static class MediatorExtensions
{
    public static Aff<TResponse> SendAff<TRequest, TResponse>(
        this IMediator mediator,
        TRequest request,
        CancellationToken token
    ) where TRequest : IRequest<TResponse> =>
        from result in Aff(async () => await mediator.Send(request, token))
        select result;
}