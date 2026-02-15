using CQRS.Mediatr.Lite;

namespace VerticalSlice.Web.Api.EndpointHandling.Endpoints.Audits.Data.Queries;

public class GetAllAuditQuery : Query<GetAllAuditQueryResult>
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

    public override string DisplayName => nameof(GetAllAuditQuery);
    public override string Id { get; } = Guid.NewGuid().ToString();

    public override bool Validate(out string? validationErrorMessage)
    {
        if (Page < 1)
        {
            validationErrorMessage = "Page must be greater than 0";
            return false;
        }

        if (PageSize is < 1 or > 100)
        {
            validationErrorMessage = "PageSize must be between 1 and 100";
            return false;
        }

        if (StartDate.HasValue && EndDate.HasValue && StartDate.Value > EndDate.Value)
        {
            validationErrorMessage = "StartDate must be before EndDate";
            return false;
        }

        validationErrorMessage = null;
        return true;
    }
}
