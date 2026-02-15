using VerticalSlice.Web.Api.Contracts.Response;
using VerticalSlice.Web.Api.Model;

namespace VerticalSlice.Web.Api.EndpointHandling.Endpoints.Audits.Data.Mappers;

/// <summary>
///     Mapper for converting Audit entities to response DTOs
/// </summary>
public static class AuditMapper
{
    /// <summary>
    ///     Maps an Audit entity to AuditSummary
    /// </summary>
    /// <param name="audit">The audit entity to map</param>
    /// <returns>AuditSummary response object</returns>
    public static AuditSummary ToAuditSummary(this Audit audit)
    {
        if (audit == null)
        {
            throw new ArgumentNullException(nameof(audit));
        }

        return new AuditSummary
        {
            AuditId = audit.AuditId,
            Operation = audit.Operation,
            EntityType = audit.EntityType,
            EntityId = audit.EntityId,
            UserId = audit.UserId,
            UserName = audit.UserName,
            Timestamp = audit.Timestamp,
            IpAddress = audit.IpAddress,
            UserAgent = audit.UserAgent,
            OldValues = audit.OldValues,
            NewValues = audit.NewValues,
            Context = audit.Context,
            HttpMethod = audit.HttpMethod,
            Endpoint = audit.Endpoint,
            IsSuccess = audit.IsSuccess,
            ErrorMessage = audit.ErrorMessage,
            OrganizationId = audit.OrganizationId,
            Tags = audit.Tags,
            DurationMs = audit.DurationMs,
            CorrelationId = audit.CorrelationId
        };
    }

    /// <summary>
    ///     Maps a collection of Audit entities to AuditSummary collection
    /// </summary>
    /// <param name="audits">The audits to map</param>
    /// <returns>Collection of AuditSummary objects</returns>
    public static IEnumerable<AuditSummary> ToAuditSummaries(this IEnumerable<Audit> audits)
    {
        if (audits == null)
        {
            throw new ArgumentNullException(nameof(audits));
        }

        return audits.Select(ToAuditSummary);
    }

    /// <summary>
    ///     Creates a paged response from audits and pagination parameters
    /// </summary>
    /// <param name="audits">The audits to include in the response</param>
    /// <param name="page">Current page number (1-indexed)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="totalItems">Total number of items across all pages</param>
    /// <returns>PagedAuditResponse</returns>
    public static PagedAuditResponse ToPagedResponse(
        this IEnumerable<Audit> audits,
        int page,
        int pageSize,
        int totalItems)
    {
        if (audits == null)
        {
            throw new ArgumentNullException(nameof(audits));
        }

        IEnumerable<AuditSummary> auditSummaries = audits.ToAuditSummaries();
        int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

        return new PagedAuditResponse
        {
            Data = auditSummaries,
            Pagination = new PaginationMetadata
            {
                Page = page,
                PageSize = pageSize,
                TotalPages = totalPages,
                TotalItems = totalItems,
                HasNextPage = page < totalPages,
                HasPreviousPage = page > 1
            }
        };
    }
}
