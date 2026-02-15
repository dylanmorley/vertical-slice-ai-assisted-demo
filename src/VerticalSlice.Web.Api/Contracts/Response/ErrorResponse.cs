namespace VerticalSlice.Web.Api.Contracts.Response;

/// <summary>
///     Standard error response
/// </summary>
public class ErrorResponse
{
    /// <summary>
    ///     Error code
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    ///     Human-readable error message
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    ///     Detailed error description (optional)
    /// </summary>
    public string? Details { get; set; }

    /// <summary>
    ///     Field-specific errors for validation failures
    /// </summary>
    public Dictionary<string, List<string>>? Errors { get; set; }

    /// <summary>
    ///     Request ID for tracking
    /// </summary>
    public string? RequestId { get; set; }

    /// <summary>
    ///     Timestamp of the error
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
