using System.Threading;
using System.Threading.Tasks;
using Demo.Inventory.Ingestion.Domain;
using FluentValidation;
using Infrastructure.Messaging.Azure.Queues.Demo;
using LanguageExt;
using LanguageExt.Effects.Traits;

namespace Demo.Inventory.Ingestion.Functions.Features.AcceptInventoryChanges;

public interface IInventoryChangesHandler
{
    ValueTask<Either<ErrorResponse, Unit>> Execute<TRunTime>(
        TRunTime runTime,
        AcceptInventoryChangeRequest request,
        IValidator<AcceptInventoryChangeRequest> validator,
        AcceptInventorySettings settings,
        CancellationToken token
    ) where TRunTime : struct, IHaveQueueOperations<TRunTime>, HasCancel<TRunTime>;
}