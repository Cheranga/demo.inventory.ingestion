using System.Diagnostics;

namespace Demo.Inventory.Ingestion.Domain;

[DebuggerDisplay("{LocationCode}-{ItemNumber}")]
public record struct Inventory(
    string ItemNumber,
    string LocationCode,
    string StockCount,
    string LevelIndicator
);


