namespace App.DTOs.Common;

/// <summary>
/// Generic API response wrapper
/// </summary>
/// <typeparam name="T">Type of the response data</typeparam>
public class ApiResponse<T>
{
    /// <summary>
    /// Indicates if the request was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Response data (null if error)
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// Error message (null if success)
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Additional error details
    /// </summary>
    public Dictionary<string, string[]>? Errors { get; set; }

    /// <summary>
    /// HTTP status code
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Timestamp of the response
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Creates a successful response with data
    /// </summary>
    public static ApiResponse<T> Ok(T data, string? message = null)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data,
            Message = message,
            StatusCode = 200
        };
    }

    /// <summary>
    /// Creates a created response with data
    /// </summary>
    public static ApiResponse<T> Created(T data, string? message = null)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data,
            Message = message,
            StatusCode = 201
        };
    }

    /// <summary>
    /// Creates an error response
    /// </summary>
    public static ApiResponse<T> Error(string message, int statusCode = 400)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            StatusCode = statusCode
        };
    }

    /// <summary>
    /// Creates an error response with validation errors
    /// </summary>
    public static ApiResponse<T> ValidationError(Dictionary<string, string[]> errors)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = "Validation failed",
            Errors = errors,
            StatusCode = 400
        };
    }

    /// <summary>
    /// Creates a not found response
    /// </summary>
    public static ApiResponse<T> NotFound(string message = "Resource not found")
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            StatusCode = 404
        };
    }

    /// <summary>
    /// Creates an unauthorized response
    /// </summary>
    public static ApiResponse<T> Unauthorized(string message = "Unauthorized")
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            StatusCode = 401
        };
    }

    /// <summary>
    /// Creates a forbidden response
    /// </summary>
    public static ApiResponse<T> Forbidden(string message = "Forbidden")
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            StatusCode = 403
        };
    }
}

/// <summary>
/// Non-generic API response for void operations
/// </summary>
public class ApiResponse : ApiResponse<object>
{
    /// <summary>
    /// Creates a successful response
    /// </summary>
    public static new ApiResponse Ok(string? message = null)
    {
        return new ApiResponse
        {
            Success = true,
            Message = message,
            StatusCode = 200
        };
    }

    /// <summary>
    /// Creates a no content response
    /// </summary>
    public static ApiResponse NoContent()
    {
        return new ApiResponse
        {
            Success = true,
            StatusCode = 204
        };
    }
}