using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Demo.Inventory.Ingestion.Functions.Core;
using Demo.Inventory.Ingestion.Functions.Extensions;
using LanguageExt;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Unit = LanguageExt.Unit;

namespace Demo.Inventory.Ingestion.Functions.Features.AcceptInventoryChanges;

public class AcceptInventoryChangesFunction
{
    private readonly IMediator _mediator;

    public AcceptInventoryChangesFunction(IMediator mediator) => _mediator = mediator;

    [FunctionName(nameof(AcceptInventoryChangesFunction))]
    public async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, WebRequestMethods.Http.Post, Route = "inventory")]
            HttpRequest request
    )
    {
        var addOrderRequest = await request.ToModelAsync<AcceptInventoryChangeRequest>();
        var operation = await _mediator.Send(addOrderRequest);
        
        return ToResponse(operation);
    }

    private static IActionResult ToResponse(Either<ErrorResponse, Unit> operation)
    {
        return operation.Match<IActionResult>(
            _ => new AcceptedResult(),
            error => 
                error.ErrorCode switch
                {
                    ErrorCodes.InvalidData => new BadRequestObjectResult(error),
                    _ => new InternalServerErrorResult()
                }
        );
    }
}
