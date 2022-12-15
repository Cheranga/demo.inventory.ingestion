using System.Diagnostics.CodeAnalysis;

namespace Demo.Inventory.Ingestion.Functions.Features.AcceptInventoryChanges;

[ExcludeFromCodeCoverage]
public record struct AcceptInventorySettings(
    string Account,
    string Category,
    string Queue
);

[ExcludeFromCodeCoverage]
public record struct SourceInventorySettings(string Account, string Category, string Container);

[ExcludeFromCodeCoverage]
public record struct DestinationInventorySettings(string Account, string Category, string Container);