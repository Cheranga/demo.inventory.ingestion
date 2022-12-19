using FluentValidation;
using FluentValidation.Results;

namespace Demo.Inventory.Ingestion.Functions.Core;

public class BaseModelValidator<T> : AbstractValidator<T>
{
    public BaseModelValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Stop;
        RuleLevelCascadeMode = CascadeMode.Stop;
    }

    protected override bool PreValidate(ValidationContext<T> context, ValidationResult result)
    {
        if (context.InstanceToValidate == null)
        {
            result.Errors.Add(new ValidationFailure("", "instance is null"));
            return false;
        }

        return true;
    }
}