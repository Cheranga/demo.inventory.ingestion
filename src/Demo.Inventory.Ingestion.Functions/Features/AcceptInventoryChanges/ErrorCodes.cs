namespace Demo.Inventory.Ingestion.Functions.Features.AcceptInventoryChanges;

public class ErrorCodes
{
    public const int InvalidData = 400;
    public const int CannotPublishToQueue = 500;
}

public class ErrorMessages
{
    public const string InvalidData = "invalid data";
    public const string CannotPublishToQueue = "error when publishing message to queue";
}