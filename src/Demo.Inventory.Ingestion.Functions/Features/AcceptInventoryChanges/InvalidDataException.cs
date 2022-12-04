using System;
using FluentValidation.Results;

namespace Demo.Inventory.Ingestion.Functions.Features.AcceptInventoryChanges;

public class InvalidDataException : Exception
{
    public ValidationResult Result { get; }

    public InvalidDataException(ValidationResult result): base("invalid data")
    {
        Result = result;
    }
}