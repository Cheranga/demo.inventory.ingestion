using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Azure.Storage.Queues;
using Demo.Inventory.Ingestion.Domain;
using Demo.Inventory.Ingestion.Functions.Extensions;
using FluentValidation;
using Infrastructure.Messaging.Azure.Queues.Demo;
using LanguageExt;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Azure;
using Unit = LanguageExt.Unit;

namespace Demo.Inventory.Ingestion.Functions.Features.AcceptInventoryChanges;

public class AcceptInventoryChangesFunction
{
    private readonly IAzureClientFactory<QueueServiceClient> _factory;
    private readonly IAcceptInventoryChangesHandler _handler;
    private readonly IMediator _mediator;
    private readonly AcceptInventorySettings _settings;
    private readonly IValidator<AcceptInventoryChangeRequest> _validator;

    public AcceptInventoryChangesFunction(
        IMediator mediator,
        IAzureClientFactory<QueueServiceClient> factory,
        IValidator<AcceptInventoryChangeRequest> validator,
        AcceptInventorySettings settings,
        IAcceptInventoryChangesHandler handler
    )
    {
        _mediator = mediator;
        _factory = factory;
        _validator = validator;
        _settings = settings;
        _handler = handler;
    }

    [FunctionName(nameof(AcceptInventoryChangesFunction))]
    public async Task<IActionResult> RunAsync(
        [HttpTrigger(
            AuthorizationLevel.Anonymous,
            WebRequestMethods.Http.Post,
            Route = "inventory"
        )]
            HttpRequest request
    )
    {
        var addOrderRequest = await request.ToModelAsync<AcceptInventoryChangeRequest>();
        var operation = await _handler.Execute(
            LiveQueueRunTime.New(_factory),
            addOrderRequest,
            _validator,
            _settings,
            CancellationToken.None
        );
        // var operation = await _mediator.Send(addOrderRequest);

        return ToResponse(operation);
    }

    private static IActionResult ToResponse(Either<ErrorResponse, Unit> operation) =>
        operation.Match<IActionResult>(
            _ => new AcceptedResult(),
            error =>
                error.ErrorCode switch
                {
                    ErrorCodes.InvalidData => new BadRequestObjectResult(error),
                    _ => new InternalServerErrorResult()
                }
        );
}
