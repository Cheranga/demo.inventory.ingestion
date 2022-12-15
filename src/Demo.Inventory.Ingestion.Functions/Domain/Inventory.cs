using System.Globalization;
using CsvHelper.Configuration;

namespace Demo.Inventory.Ingestion.Functions.Domain;

public record struct Inventory(
    string ItemNumber,
    string LocationCode,
    string StockCount,
    string LevelIndicator
);


