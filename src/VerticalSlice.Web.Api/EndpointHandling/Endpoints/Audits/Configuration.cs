using VerticalSlice.Web.Api.EndpointHandling.Endpoints.Audits.GettingAudit;

namespace VerticalSlice.Web.Api.EndpointHandling.Endpoints.Audits;

public static class Configuration
{
    public static IEndpointRouteBuilder UseAuditEndpoints(this IEndpointRouteBuilder endpoints) =>
        endpoints
            .UseGetAuditEndpoint();
}
