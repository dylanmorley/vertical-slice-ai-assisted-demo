namespace VerticalSlice.Web.Api.Contracts.Response;

/// <summary>
///     Summary information for an audit record
/// </summary>
public class AuditSummary
{
    /// <summary>
    ///     Unique identifier for the audit record
    /// </summary>
    public int AuditId { get; set; }

    /// <summary>
    ///     The operation that was performed (CREATE, UPDATE, DELETE, etc.)
    /// </summary>
    public string Operation { get; set; } = string.Empty;

    /// <summary>
    ///     The entity type that was modified (NodeType, Geography, Organization, etc.)
    /// </summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    ///     The ID of the entity that was modified
    /// </summary>
    public string EntityId { get; set; } = string.Empty;

    /// <summary>
    ///     The user who performed the operation
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    ///     The username/email of the user who performed the operation
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    ///     The timestamp when the operation occurred
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    ///     The IP address of the client that performed the operation
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    ///     The user agent of the client that performed the operation
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    ///     The original values before the change (for UPDATE operations)
    /// </summary>
    public string? OldValues { get; set; }

    /// <summary>
    ///     The new values after the change (for CREATE/UPDATE operations)
    /// </summary>
    public string? NewValues { get; set; }

    /// <summary>
    ///     Additional context or metadata about the operation
    /// </summary>
    public string? Context { get; set; }

    /// <summary>
    ///     The HTTP method used (GET, POST, PUT, DELETE, etc.)
    /// </summary>
    public string? HttpMethod { get; set; }

    /// <summary>
    ///     The endpoint/route that was called
    /// </summary>
    public string? Endpoint { get; set; }

    /// <summary>
    ///     Whether the operation was successful
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    ///     Error message if the operation failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    ///     The organization context where the operation was performed
    /// </summary>
    public string? OrganizationId { get; set; }

    /// <summary>
    ///     Additional tags for categorization and filtering
    /// </summary>
    public string? Tags { get; set; }

    /// <summary>
    ///     The duration of the operation in milliseconds
    /// </summary>
    public long? DurationMs { get; set; }

    /// <summary>
    ///     The correlation ID for tracking related operations
    /// </summary>
    public string? CorrelationId { get; set; }
}
