namespace VerticalSlice.Web.Api;

public static class AppBuilderExtensions
{
    /// <summary>
    ///     We're running a UI and API in the same application, so we need to handle errors differently. For the API
    ///     we want to return a JSON response, for the UI we want to return a view.
    ///     We can determine what to do by looking at the request path, all requests to APIs will start with "basePath"
    ///     style, otherwise we assume it's a UI request.
    /// </summary>
    public static void SetupGlobalErrorHandling(this IApplicationBuilder app, IWebHostEnvironment env) =>
        app.UseWhen(
            context => context.Request.Path.Value != null && context.Request.Path.Value.StartsWith(ApiInfo.BasePath,
                StringComparison.OrdinalIgnoreCase),
            builder => builder.ConfigureExceptionHandler());
}
