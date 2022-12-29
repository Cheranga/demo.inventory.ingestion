using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Demo.Inventory.Ingestion.Domain;

[ExcludeFromCodeCoverage]
[DebuggerDisplay("{LocationCode}-{ItemNumber}")]
public record struct Inventory(
    string ItemNumber,
    string LocationCode,
    string StockCount,
    string LevelIndicator
);


