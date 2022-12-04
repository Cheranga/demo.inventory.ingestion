using System;
using LanguageExt.Common;

namespace Demo.Inventory.Ingestion.Functions.Features.AcceptInventoryChanges;

public class ErrorCodes
{
    public static Error BadInputData(Exception exception) =>
        Error.New(400, "invalid data", exception);

    public static int BadInventoryChangeData = 400;
}

public class ErrorMessages
{
    public static string BadInventoryChangeData = "invalid inventory changes";
}
