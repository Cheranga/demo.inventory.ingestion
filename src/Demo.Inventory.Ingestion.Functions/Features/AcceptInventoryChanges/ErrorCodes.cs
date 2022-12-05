namespace Demo.Inventory.Ingestion.Functions.Features.AcceptInventoryChanges;

public class ErrorCodes
{
    public const int InvalidData = 400;
    public const int ErrorWhenAcceptingInventoryChanges = 500;
}

public class ErrorMessages
{
    public const string InvalidData = "invalid data";
    public const string ErrorWhenAcceptingInventoryChanges =
        "error occurred when accepting inventory changes";
}