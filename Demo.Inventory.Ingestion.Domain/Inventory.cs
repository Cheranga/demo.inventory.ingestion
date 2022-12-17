namespace Demo.Inventory.Ingestion.Domain;

public record struct Inventory(
    string ItemNumber,
    string LocationCode,
    string StockCount,
    string LevelIndicator
);


