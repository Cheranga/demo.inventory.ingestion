using System;
using FluentValidation.Results;

namespace Demo.Inventory.Ingestion.Functions.Features.AcceptInventoryChanges;

public class InvalidDataException : Exception
{
    public InvalidDataException(ValidationResult result) : base("invalid data") => Result = result;

    public ValidationResult Result { get; }
}