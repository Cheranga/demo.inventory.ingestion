using FluentValidation.Results;

namespace Demo.Inventory.Ingestion.Domain;

public class ErrorResponse
{
    public int ErrorCode { get; }
    public string ErrorMessage { get; }
    public List<ErrorData> Errors { get; }

    private ErrorResponse(int errorCode, string errorMessage, IEnumerable<ErrorData> errors)
    {
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
        Errors = errors?.ToList() ?? new List<ErrorData>();
    }

    public static ErrorResponse New(int errorCode, string errorMessage) =>
        new(errorCode, errorMessage, new[] { ErrorData.New("", "error occurred") });

    public static ErrorResponse New(
        int errorCode,
        string errorMessage,
        IEnumerable<ValidationFailure> errors
    ) =>
        new(
            errorCode,
            errorMessage,
            errors.Select(x => ErrorData.New(x.PropertyName, x.ErrorMessage))
        );

    public class ErrorData
    {
        public string ErrorCode { get; }
        public string ErrorMessage { get; }

        private ErrorData(string errorCode, string errorMessage)
        {
            ErrorCode = errorCode;
            ErrorMessage = errorMessage;
        }

        public static ErrorData New(string errorCode, string errorMessage) =>
            new(errorCode, errorMessage);
    }
}
