namespace SecurityService.Models;

public enum OperationResultStatus
{
    Success,
    Invalid,
    NotFound,
    Conflict,
    Error
}

public class OperationResult
{
    protected OperationResult(OperationResultStatus status, string? errorMessage = null)
    {
        this.Status = status;
        this.ErrorMessage = errorMessage;
    }

    public OperationResultStatus Status { get; }

    public string? ErrorMessage { get; }

    public bool IsSuccess => this.Status == OperationResultStatus.Success;

    public static OperationResult Success() => new(OperationResultStatus.Success);

    public static OperationResult Invalid(string message) => new(OperationResultStatus.Invalid, message);

    public static OperationResult NotFound(string message) => new(OperationResultStatus.NotFound, message);

    public static OperationResult Conflict(string message) => new(OperationResultStatus.Conflict, message);

    public static OperationResult Error(string message) => new(OperationResultStatus.Error, message);
}

public sealed class OperationResult<T> : OperationResult
{
    private OperationResult(OperationResultStatus status, T? data, string? errorMessage)
        : base(status, errorMessage)
    {
        this.Data = data;
    }

    public T? Data { get; }

    public static OperationResult<T> Success(T data) => new(OperationResultStatus.Success, data, null);

    public static new OperationResult<T> Invalid(string message) => new(OperationResultStatus.Invalid, default, message);

    public static new OperationResult<T> NotFound(string message) => new(OperationResultStatus.NotFound, default, message);

    public static new OperationResult<T> Conflict(string message) => new(OperationResultStatus.Conflict, default, message);

    public static new OperationResult<T> Error(string message) => new(OperationResultStatus.Error, default, message);
}
