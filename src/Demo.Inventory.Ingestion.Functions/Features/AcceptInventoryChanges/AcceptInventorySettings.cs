namespace Demo.Inventory.Ingestion.Functions.Features.AcceptInventoryChanges;

public record struct AcceptInventorySettings(
    string Account,
    string Category,
    string Queue
);