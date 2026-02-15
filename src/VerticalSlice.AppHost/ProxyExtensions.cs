using Aspire.Hosting.JavaScript;

namespace VerticalSlice.AppHost;

public static class ProxyExtensions
{
    public static IResourceBuilder<ViteAppResource> WithReverseProxy(this IResourceBuilder<ViteAppResource> builder, EndpointReference upstreamEndpoint)
    {
        // For local runs and for tests (Aspire.Hosting.Testing), we want the Vite dev server
        // to run as a local process and point at the upstream API via BACKEND_URL.
        // Only in Publish mode do we switch to the Dockerfile/Caddy reverse-proxy approach.
        if (!builder.ApplicationBuilder.ExecutionContext.IsPublishMode)
        {
            return builder.WithEnvironment("BACKEND_URL", upstreamEndpoint);
        }

        return builder.PublishAsDockerFile(c => c.WithReverseProxy(upstreamEndpoint));
    }

    public static IResourceBuilder<ContainerResource> WithReverseProxy(this IResourceBuilder<ContainerResource> builder, EndpointReference upstreamEndpoint)
    {
        // Caddy listens on port 80
        builder.WithEndpoint("http", e => e.TargetPort = 80);

        return builder.WithEnvironment(context =>
        {
            // In the docker file, caddy uses the host and port without the scheme
            var hostAndPort = ReferenceExpression.Create($"{upstreamEndpoint.Property(EndpointProperty.Host)}:{upstreamEndpoint.Property(EndpointProperty.Port)}");

            context.EnvironmentVariables["BACKEND_URL"] = hostAndPort;
            context.EnvironmentVariables["SPAN"] = builder.Resource.Name;
        });
    }
}
