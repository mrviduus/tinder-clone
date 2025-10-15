namespace App.DTOs.Common;

/// <summary>
/// Standard error response DTO
/// </summary>
public class ErrorResponse
{
    /// <summary>
    /// Error message
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Additional error details
    /// </summary>
    public Dictionary<string, string[]>? Details { get; set; }

    /// <summary>
    /// Error code for client-side handling
    /// </summary>
    public string? ErrorCode { get; set; }

    /// <summary>
    /// Timestamp when the error occurred
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Request ID for tracing
    /// </summary>
    public string? TraceId { get; set; }

    /// <summary>
    /// Creates an error response with a message
    /// </summary>
    public ErrorResponse(string message)
    {
        Message = message;
    }

    /// <summary>
    /// Creates an error response with message and details
    /// </summary>
    public ErrorResponse(string message, Dictionary<string, string[]> details)
    {
        Message = message;
        Details = details;
    }

    /// <summary>
    /// Default constructor for serialization
    /// </summary>
    public ErrorResponse()
    {
    }
}