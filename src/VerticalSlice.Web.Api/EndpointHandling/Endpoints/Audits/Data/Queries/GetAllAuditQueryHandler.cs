using Kommand.Abstractions;
using Microsoft.EntityFrameworkCore;
using VerticalSlice.Web.Api.Data;
using VerticalSlice.Web.Api.Model;

namespace VerticalSlice.Web.Api.EndpointHandling.Endpoints.Audits.Data.Queries;

public class GetAllAuditQueryResult
{
    public IEnumerable<Audit> Audits { get; set; } = [];
    public int TotalCount { get; set; }
}

public class GetAllAuditQueryHandler(VerticalSliceDataContext dataContext)
    : IQueryHandler<GetAllAuditQuery, GetAllAuditQueryResult>
{
    private readonly VerticalSliceDataContext _dataContext =
        dataContext ?? throw new ArgumentNullException(nameof(dataContext));

    public async Task<GetAllAuditQueryResult> HandleAsync(GetAllAuditQuery request, CancellationToken cancellationToken)
    {
        IQueryable<Audit> baseQuery = _dataContext.Audit.AsQueryable();

        // Apply filters
        if (request.OrganizationId.HasValue)
        {
            baseQuery = baseQuery.Where(a => a.OrganizationId == request.OrganizationId.Value.ToString());
        }

        if (!string.IsNullOrWhiteSpace(request.Operation))
        {
            baseQuery = baseQuery.Where(a => a.Operation.Contains(request.Operation));
        }

        if (!string.IsNullOrWhiteSpace(request.EntityType))
        {
            baseQuery = baseQuery.Where(a => a.EntityType.Contains(request.EntityType));
        }

        if (!string.IsNullOrWhiteSpace(request.UserId))
        {
            baseQuery = baseQuery.Where(a => a.UserId.Contains(request.UserId));
        }

        if (!string.IsNullOrWhiteSpace(request.UserName))
        {
            baseQuery = baseQuery.Where(a => a.UserName.Contains(request.UserName));
        }

        if (request.StartDate.HasValue)
        {
            baseQuery = baseQuery.Where(a => a.Timestamp >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            baseQuery = baseQuery.Where(a => a.Timestamp <= request.EndDate.Value);
        }

        if (request.IsSuccess.HasValue)
        {
            baseQuery = baseQuery.Where(a => a.IsSuccess == request.IsSuccess.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            string searchTerm = request.SearchTerm.ToLower();
            baseQuery = baseQuery.Where(a =>
                (a.Operation ?? string.Empty).ToLower().Contains(searchTerm) ||
                (a.EntityType ?? string.Empty).ToLower().Contains(searchTerm) ||
                (a.UserName ?? string.Empty).ToLower().Contains(searchTerm) ||
                (a.Endpoint ?? string.Empty).ToLower().Contains(searchTerm) ||
                (a.Context ?? string.Empty).ToLower().Contains(searchTerm));
        }

        int totalCount = await baseQuery.CountAsync();

        IOrderedQueryable<Audit> sortedQuery = baseQuery.OrderByDescending(a => a.Timestamp);
        int skip = (request.Page - 1) * request.PageSize;
        List<Audit> audits = await sortedQuery
            .Skip(skip)
            .Take(request.PageSize)
            .ToListAsync();

        return new GetAllAuditQueryResult { Audits = audits, TotalCount = totalCount };
    }
}
