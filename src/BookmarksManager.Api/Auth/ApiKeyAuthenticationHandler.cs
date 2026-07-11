using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace BookmarksManager.Api.Auth;

public class ApiKeyAuthenticationSchemeOptions : AuthenticationSchemeOptions;

public class ApiKeyAuthenticationHandler(
    IOptionsMonitor<ApiKeyAuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IConfiguration configuration)
    : AuthenticationHandler<ApiKeyAuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var configuredKey = configuration["ApiKey"];
        if (string.IsNullOrEmpty(configuredKey))
        {
            return Task.FromResult(AuthenticateResult.Fail("API key authentication is not configured."));
        }

        if (!Request.Headers.TryGetValue(ApiKeyDefaults.HeaderName, out var provided) ||
            string.IsNullOrEmpty(provided))
        {
            return Task.FromResult(AuthenticateResult.Fail($"Missing '{ApiKeyDefaults.HeaderName}' header."));
        }

        if (!IsMatch(provided.ToString(), configuredKey))
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid API key."));
        }

        var identity = new ClaimsIdentity(ApiKeyDefaults.SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, ApiKeyDefaults.SchemeName);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    private static bool IsMatch(string provided, string configured)
    {
        var providedBytes = Encoding.UTF8.GetBytes(provided);
        var configuredBytes = Encoding.UTF8.GetBytes(configured);

        // Hash both to a fixed length first so FixedTimeEquals doesn't leak the length via timing.
        var providedHash = SHA256.HashData(providedBytes);
        var configuredHash = SHA256.HashData(configuredBytes);

        return CryptographicOperations.FixedTimeEquals(providedHash, configuredHash);
    }
}
