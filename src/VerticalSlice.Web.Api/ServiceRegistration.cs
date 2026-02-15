using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using VerticalSlice.Web.Api.Data;
using VerticalSlice.Web.Api.Telemetry;

namespace VerticalSlice.Web.Api;

[ExcludeFromCodeCoverage]
public static class ServiceRegistration
{
    public static void RegisterAllServices(this WebApplicationBuilder builder)
    {
        // register basic API configuration and endpoint behaviors
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddHealthChecks();
        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(b =>
            {
                b.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });
        builder.Services.AddHttpContextAccessor();

        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            options.SerializerOptions.WriteIndented = true;
            options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        });

        // Register Auth0 Authentication
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                var isE2ETesting = builder.Environment.IsEnvironment("EndToEndTesting");

                if (isE2ETesting)
                {
                    // E2E Testing mode: Use symmetric key for test JWTs
                    var testSigningKey = builder.Configuration["Auth0:TestSigningKey"]
                        ?? throw new InvalidOperationException("Auth0:TestSigningKey is required for E2E testing");
                    var testIssuer = builder.Configuration["Auth0:TestIssuer"]
                        ?? "https://e2e-test.verticalslice.local/";
                    var testAudience = builder.Configuration["Auth0:TestAudience"]
                        ?? "https://vertical-slic.com/";

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = testIssuer,
                        ValidAudience = testAudience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(testSigningKey)),
                        ClockSkew = TimeSpan.Zero
                    };
                }
                else
                {
                    // Production/Development: Use Auth0
                    options.Authority = $"https://{builder.Configuration["Auth0:Domain"]}/";
                    options.Audience = builder.Configuration["Auth0:Audience"];
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ClockSkew = TimeSpan.Zero
                    };
                }
            });

        builder.Services.AddAuthorization();
        builder.Services.AddOpenTelemetry(builder.Configuration);
        builder.Services.AddCqrsServices();

        string? sqlConnectionString = builder.Configuration.GetConnectionString("VerticalSliceDatabase");
        var providerTypeString = builder.Configuration["Database:ProviderType"];
        var providerType = string.IsNullOrWhiteSpace(providerTypeString)
            ? DataConfiguration.SqlProviderType.SqlLite
            : Enum.TryParse<DataConfiguration.SqlProviderType>(providerTypeString, ignoreCase: true, out var parsed)
                ? parsed
                : throw new InvalidOperationException(
                    $"Invalid Database:ProviderType '{providerTypeString}'. Valid values: {string.Join(", ", Enum.GetNames<DataConfiguration.SqlProviderType>())}");

        builder.Services.AddEntityFramework(providerType, sqlConnectionString);

    }
}
