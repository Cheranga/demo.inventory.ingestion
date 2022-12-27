using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Demo.Inventory.Ingestion.Domain;
using Demo.Inventory.Ingestion.Functions.Extensions;
using FluentValidation;
using Infrastructure.Messaging.Azure.Queues;
using Infrastructure.Messaging.Azure.Queues.Runtimes;
using LanguageExt;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Unit = LanguageExt.Unit;

namespace Demo.Inventory.Ingestion.Functions.Features.AcceptInventoryChanges;

public class AcceptInventoryChangesFunction
{
    private readonly AzureStorageQueueRuntimeEnv _runTimeEnv;
    private readonly IInventoryChangesHandler _handler;
    private readonly AcceptInventorySettings _settings;
    private readonly IValidator<AcceptInventoryChangeRequest> _validator;

    public AcceptInventoryChangesFunction(
        AzureStorageQueueRuntimeEnv runTimeEnv,
        IValidator<AcceptInventoryChangeRequest> validator,
        AcceptInventorySettings settings,
        IInventoryChangesHandler handler
    )
    {
        _runTimeEnv = runTimeEnv;
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
            AzureStorageQueueRunTime.New(_runTimeEnv),
            addOrderRequest,
            _validator,
            _settings,
            CancellationToken.None
        );

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
