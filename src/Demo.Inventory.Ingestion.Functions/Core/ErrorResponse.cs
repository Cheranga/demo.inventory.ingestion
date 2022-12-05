using System.Collections.Generic;
using System.Linq;
using FluentValidation.Results;

namespace Demo.Inventory.Ingestion.Functions.Core;

public class ErrorResponse
{
    public int ErrorCode { get; set; }
    public string ErrorMessage { get; set; }
    public List<ErrorData> Errors { get; set; } = new List<ErrorData>();

    public class ErrorData
    {
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
    }

    public static ErrorResponse ToError(int errorCode, string errorMessage) =>
        ToError(
            errorCode,
            errorMessage,
            new[]
            {
                new ValidationFailure { ErrorCode = "", ErrorMessage = "error occurred" }
            }
        );

    public static ErrorResponse ToError(
        int errorCode,
        string errorMessage,
        IEnumerable<ValidationFailure> errors
    ) =>
        new()
        {
            ErrorCode = errorCode,
            ErrorMessage = errorMessage,
            Errors = errors
                .Select(
                    x => new ErrorData { ErrorCode = x.PropertyName, ErrorMessage = x.ErrorMessage }
                )
                .ToList()
        };
}