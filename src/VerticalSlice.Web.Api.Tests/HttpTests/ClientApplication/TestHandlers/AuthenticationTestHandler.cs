using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace VerticalSlice.Web.Api.Tests.HttpTests.ClientApplication.TestHandlers;

public class AuthenticationTestHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    private const string DefaultUserName = "Test user";
    private const string DefaultRole = "Admin";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new List<Claim>
        {
            Context.Request.Headers.TryGetValue("UserPrincipal", out var userId)
                ? new Claim(ClaimTypes.Name, userId[0]!)
                : new Claim(ClaimTypes.Name, DefaultUserName),

            Context.Request.Headers.TryGetValue("Role", out var roleDetail)
                ? new Claim(ClaimTypes.Role, roleDetail[0]!)
                : new Claim(ClaimTypes.Role, DefaultRole)
        };

        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");

        var result = AuthenticateResult.Success(ticket);

        return Task.FromResult(result);
    }
}

