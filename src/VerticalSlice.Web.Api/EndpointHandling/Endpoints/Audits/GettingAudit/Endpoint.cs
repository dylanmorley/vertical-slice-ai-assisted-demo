using CQRS.Mediatr.Lite;
using VerticalSlice.Web.Api.Contracts.Response;
using VerticalSlice.Web.Api.EndpointHandling.Endpoints.Audits.Data.Mappers;
using VerticalSlice.Web.Api.EndpointHandling.Endpoints.Audits.Data.Queries;
using VerticalSlice.Web.Api.OpenApi;

namespace VerticalSlice.Web.Api.EndpointHandling.Endpoints.Audits.GettingAudit;

internal static class GetAuditDetailsEndpoint
{
    internal static IEndpointRouteBuilder UseGetAuditEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints
            .MapGet(
                "/audit",
                async (
                    IQueryService queryHandler,
                    int page = 1,
                    int pageSize = 50,
                    int? organizationId = null,
                    string? operation = null,
                    string? entityType = null,
                    string? userId = null,
                    string? userName = null,
                    DateTime? startDate = null,
                    DateTime? endDate = null,
                    bool? isSuccess = null,
                    string? searchTerm = null,
                    CancellationToken ct = default
                ) =>
                {
                    GetAllAuditQuery query = new()
                    {
                        Page = page,
                        PageSize = pageSize,
                        OrganizationId = organizationId,
                        Operation = operation,
                        EntityType = entityType,
                        UserId = userId,
                        UserName = userName,
                        StartDate = startDate,
                        EndDate = endDate,
                        IsSuccess = isSuccess,
                        SearchTerm = searchTerm
                    };

                    GetAllAuditQueryResult? result = await queryHandler.Query(query);

                    return result.Audits.ToPagedResponse(page, pageSize, result.TotalCount);
                })
            .Produces<PagedAuditResponse>()
            .Produces(StatusCodes.Status400BadRequest)
            .WithName("GetAuditRecords")
            .AddVerticalSliceOpenApi();

        return endpoints;
    }
}
