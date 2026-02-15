using System.Diagnostics.CodeAnalysis;
using VerticalSlice.Web.Api;
using VerticalSlice.Web.Api.Data;
using VerticalSlice.Web.Api.EndpointHandling.Endpoints;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.RegisterAllServices();

WebApplication app = builder.Build();

app.UseCors();
app.UseSwagger();
app.UseSwaggerUI();
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

IWebHostEnvironment environment = app.Services.GetRequiredService<IWebHostEnvironment>();
app.SetupGlobalErrorHandling(environment);

app.MapHealthChecks("/health");

RouteGroupBuilder api = app.MapGroup(ApiInfo.BasePath);
api.MapVerticalSliceEndpoints();

// Only seed database at actual runtime, not during build-time tooling (e.g., Swagger CLI)
// The SWAGGER_CLI env var is set by the csproj during OpenAPI generation
bool isSwaggerCli = Environment.GetEnvironmentVariable("SWAGGER_CLI") == "1";
if (!isSwaggerCli)
{
    using IServiceScope scope = app.Services.CreateScope();
    VerticalSliceDataContext db = scope.ServiceProvider.GetRequiredService<VerticalSliceDataContext>();

    await DatabaseSeeder.SeedFromEnvironmentAsync(db);
}

app.MapFallbackToFile("index.html");
await app.RunAsync();

namespace VerticalSlice.Web.Api
{
    [ExcludeFromCodeCoverage]
    public class Program
    {
    }
}
