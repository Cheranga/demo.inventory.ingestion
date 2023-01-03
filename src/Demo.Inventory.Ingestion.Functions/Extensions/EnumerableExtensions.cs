using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Demo.Inventory.Ingestion.Functions.Extensions;

[ExcludeFromCodeCoverage]
public static class EnumerableExtensions
{
    public static List<List<T>> GetGroups<T>(this IEnumerable<T> collection, int groupSize = 100) =>
        collection
            .Select((x, i) => new { Index = i, Value = x })
            .GroupBy(x => x.Index / groupSize)
            .Select(x => x.Select(v => v.Value).ToList())
            .ToList();
}
