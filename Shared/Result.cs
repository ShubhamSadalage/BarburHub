namespace BarberHub.Web.Shared;

public class Result
{
    public bool IsSuccess { get; init; }
    public string? Error { get; init; }
    public List<string> Errors { get; init; } = new();

    public static Result Success() => new() { IsSuccess = true };
    public static Result Failure(string error) => new() { IsSuccess = false, Error = error, Errors = new List<string> { error } };
    public static Result Failure(List<string> errors) => new() { IsSuccess = false, Error = errors.FirstOrDefault(), Errors = errors };
}

public class Result<T> : Result
{
    public T? Value { get; init; }

    public static Result<T> Success(T value) => new() { IsSuccess = true, Value = value };
    public static new Result<T> Failure(string error) => new() { IsSuccess = false, Error = error, Errors = new List<string> { error } };
    public static new Result<T> Failure(List<string> errors) => new() { IsSuccess = false, Error = errors.FirstOrDefault(), Errors = errors };
}
