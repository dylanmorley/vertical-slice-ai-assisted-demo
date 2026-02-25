using Kommand.Abstractions;

namespace VerticalSlice.Web.Api.EndpointHandling.Endpoints.Audits.Data.Queries;

public class GetAllAuditQuery : IQuery<GetAllAuditQueryResult>
{
    public int? OrganizationId { get; set; }
    public string? Operation { get; set; }
    public string? EntityType { get; set; }
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool? IsSuccess { get; set; }
    public string? SearchTerm { get; set; }

    // Pagination parameters
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}
