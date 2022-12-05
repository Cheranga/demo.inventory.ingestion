using System.Diagnostics.CodeAnalysis;

namespace Demo.Inventory.Ingestion.Functions.Features.AcceptInventoryChanges;

[ExcludeFromCodeCoverage]
public record struct AcceptInventorySettings(
    string Account,
    string Category,
    string Queue
);