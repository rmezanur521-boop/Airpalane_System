namespace AirplaneSystem.Application.Common.Models;

public class ApiResponse<T>
{
    public bool Success { get; init; }
    public string? Message { get; init; }
    public T? Data { get; init; }
    public IReadOnlyList<string> Errors { get; init; } = new List<string>();

    public static ApiResponse<T> Ok(T data, string? message = null) =>
        new() { Success = true, Data = data, Message = message };

    public static ApiResponse<T> Fail(string error) =>
        new() { Success = false, Errors = new[] { error } };

    public static ApiResponse<T> Fail(IEnumerable<string> errors) =>
        new() { Success = false, Errors = errors.ToList().AsReadOnly() };
}

public class ApiResponse : ApiResponse<object>
{
    public static ApiResponse Ok(string? message = null) =>
        new() { Success = true, Message = message };

    public new static ApiResponse Fail(string error) =>
        new() { Success = false, Errors = new[] { error } };
}
