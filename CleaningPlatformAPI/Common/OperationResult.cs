namespace CleaningPlatformAPI.Common;

public class OperationResult<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? Code { get; set; }
    public T? Data { get; set; }

    public static OperationResult<T> Ok(T data) => new() { Success = true, Data = data };
    public static OperationResult<T> Fail(string message) => new() { Success = false, Message = message };
    public static OperationResult<T> Fail(string code, string message) => new() { Success = false, Code = code, Message = message };
}
