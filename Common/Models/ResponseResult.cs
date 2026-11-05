using System.Text.Json.Serialization;

namespace Common.Models;

/// <summary>
/// Represents an operation result with a status, optional data, and an optional error message.
/// </summary>
public class ResponseResult<T>: ResponseResult
{
    private ResponseResult(T? data, ResponseStatus status, string? errorMessage = null) : base(status, errorMessage)
    {
        Data = data;
    }
    
    public T? Data { get; }
    public bool IsFailedOrNull => !IsSuccess || Data == null;
    
    public static ResponseResult<T> InternalError(string? errorMessage, T? data = default)
    {
        return new ResponseResult<T>(data, ResponseStatus.InternalError, errorMessage);
    }
    
    public static ResponseResult<T> FromResponseResult(ResponseResult source, T? data = default)
    {
        return new ResponseResult<T>(data, source.Status, source.ErrorMessage);
    }
    
    public static ResponseResult<T> Success(T? data)
    {
        return new ResponseResult<T>(data, ResponseStatus.Success);
    }
    
    public new static ResponseResult<T> NotFound()
    {
        return new ResponseResult<T>(default, ResponseStatus.NotFound);
    }
}

/// <summary>
/// Generic version of ResponseResult{T}.
/// Can be used by responses which do not expect data.
/// <seealso cref="ResponseResult{T}"/>>
/// </summary>
public class ResponseResult(ResponseStatus status, string? errorMessage = null)
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ResponseStatus Status { get; set; } = status;
    public string? ErrorMessage { get; set; } = errorMessage;
    public bool IsSuccess => Status == ResponseStatus.Success;

    public static ResponseResult InternalError(string? errorMessage)
    {
        return new ResponseResult(ResponseStatus.InternalError, errorMessage);
    }
    
    public static ResponseResult CreateFrom(ResponseResult source)
    {
        return new ResponseResult(source.Status, source.ErrorMessage);
    }
    
    public static ResponseResult Success()
    {
        return new ResponseResult(ResponseStatus.Success);
    }
    
    public static ResponseResult NotFound()
    {
        return new ResponseResult(ResponseStatus.NotFound);
    }
}