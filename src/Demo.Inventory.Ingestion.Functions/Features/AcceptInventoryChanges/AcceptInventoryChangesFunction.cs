using System;
using System.Net;
using System.Threading.Tasks;
using Demo.Inventory.Ingestion.Functions.Extensions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace Demo.Inventory.Ingestion.Functions.Features.AcceptInventoryChanges;

public class AcceptInventoryChangesFunction
{
    private readonly IMediator _mediator;

    public AcceptInventoryChangesFunction(IMediator mediator)
    {
        _mediator = mediator;
    }

    [FunctionName(nameof(AcceptInventoryChangesFunction))]
    public async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Function, WebRequestMethods.Http.Post, Route = "inventory")]
        HttpRequest request
    )
    {
        var addOrderRequest = await request.ToModelAsync<AcceptInventoryChangeRequest>();
        await _mediator.Send(addOrderRequest);
        return new AcceptedResult();
    }
}
