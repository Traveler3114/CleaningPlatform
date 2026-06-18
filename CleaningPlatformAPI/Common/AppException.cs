namespace CleaningPlatformAPI.Common;

public class AppException(string code, string message, int statusCode = 422)
    : Exception(message)
{
    public string Code { get; } = code;
    public int StatusCode { get; } = statusCode;
}
