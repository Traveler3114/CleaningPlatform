namespace VechileCleaningAPI.Common;

public class OperationResult<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }

    public static OperationResult<T> Ok(T data) => new() { Success = true, Data = data };
    public static OperationResult<T> Fail(string message) => new() { Success = false, Message = message };
}
