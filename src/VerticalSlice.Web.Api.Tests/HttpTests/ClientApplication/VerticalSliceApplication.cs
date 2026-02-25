using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using VerticalSlice.Web.Api.Data;
using VerticalSlice.Web.Api.Tests.HttpTests.ClientApplication.TestHandlers;

namespace VerticalSlice.Web.Api.Tests.HttpTests.ClientApplication;

public class VerticalSliceApplication : WebApplicationFactory<VerticalSlice.Web.Api.Program>
{
    public const string BaseUrl = "https://localhost:5001";

    private string HostUrl { get; } = BaseUrl;

    // Use a shared database name for performance
    private const string DatabaseName = "InMemoryDbForTesting";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseUrls(HostUrl);
        builder.ConfigureServices(services =>
        {
            services.AddCors();

            var providerDescriptors = services
                .Where(d => d.ServiceType.FullName != null &&
                            d.ServiceType.FullName.Contains("Microsoft.EntityFrameworkCore"))
                .ToList();

            foreach (var descriptor in providerDescriptors)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<VerticalSliceDataContext>(options => options.UseInMemoryDatabase(DatabaseName));

            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var scopedServices = scope.ServiceProvider;

            var db = scopedServices.GetRequiredService<VerticalSliceDataContext>();

            db.Database.EnsureCreated();
            DatabaseSeeder.SeedAsync(db).GetAwaiter().GetResult();

            services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme,
                options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateAudience = false,
                        ValidateLifetime = false,
                        ValidateIssuerSigningKey = false,
                        ValidateIssuer = false,
                        ValidateActor = false,
                        SignatureValidator = SignatureValidator
                    };
                });
        });

        builder.ConfigureTestServices(services =>
        {
            services
                .AddAuthentication("Test")
                .AddScheme<AuthenticationSchemeOptions, AuthenticationTestHandler>("Test", null);

            var descriptors = services.Where(d => d.ServiceType ==
                                                  typeof(IConfigureOptions<OpenIdConnectOptions>)).ToList();
            foreach (var descriptor in descriptors) services.Remove(descriptor);

            services.AddOptions<OpenIdConnectOptions>("OpenIdConnect").Configure(options =>
            {
                options.ClientId = "test-rig";
                options.Authority = "test-setup";
                options.MetadataAddress = "https://fake-auth.com";
            });
        });
    }

    private SecurityToken SignatureValidator(string token, TokenValidationParameters validationParameters)
    {
        return new JsonWebToken(token);
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.UseEnvironment("InMemoryTesting");
        return base.CreateHost(builder);
    }

    /// <summary>
    /// Clears the database and reseeds it for the next test
    /// </summary>
    public async Task ResetDatabaseAsync()
    {
        Console.WriteLine("Resetting database...");
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<VerticalSliceDataContext>();

        // Clear all data
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();

        await DatabaseSeeder.SeedAsync(db);
        Console.WriteLine("Database reset completed.");
    }
}
