using System;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace Demo.Inventory.Ingestion.Functions.Extensions;

public interface IActor
{
        
}

public static class EntityIdGenerator
{
    public static EntityId GetEntityId<TModel>(this string identifier) where TModel : IActor
    {
        if (string.IsNullOrEmpty(identifier))
        {
            throw new ArgumentNullException(nameof(identifier));
        }

        return new EntityId(typeof(TModel).Name, identifier.ToUpper());
    }
}