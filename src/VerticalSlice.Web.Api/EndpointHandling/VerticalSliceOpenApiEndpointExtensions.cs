namespace VerticalSlice.Web.Api.OpenApi;

internal static class VerticalSliceOpenApiEndpointExtensions
{
    internal static TBuilder AddVerticalSliceOpenApi<TBuilder>(this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder
    {
        builder.AddOpenApiOperationTransformer((operation, context, ct) =>
        {
            string fallback = operation.OperationId ?? "Vertical Slice API endpoint";

            if (string.IsNullOrWhiteSpace(operation.Summary))
            {
                operation.Summary = fallback;
            }

            if (string.IsNullOrWhiteSpace(operation.Description))
            {
                operation.Description = operation.Summary;
            }

            return Task.CompletedTask;
        });

        return builder;
    }
}
