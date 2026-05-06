
using System.Text.Json;

namespace SecurityService.DataTransferObjects;

public sealed class ApiResourceResponse {
    public string Name { get; set; }

    public string? DisplayName { get; set; }

    public string? Description { get; set; }

    public IReadOnlyCollection<string> Scopes { get; set; }

    public IReadOnlyCollection<string> UserClaims { get; set; }
}

public sealed record ApiScopeResponse {
    public string Name { get; set; }

    public string? DisplayName { get; set; }

    public string? Description { get; set; }
}

public sealed record ClientResponse {
    public string ClientId { get; set; }
    
    public string ClientName { get; set; }

    public string? Description { get; set; }

    public string? ClientUri { get; set; }

    public IReadOnlyCollection<string> AllowedScopes { get; set; }

    public IReadOnlyCollection<string> AllowedGrantTypes { get; set; }

    public IReadOnlyCollection<string> ClientRedirectUris { get; set; }

    public IReadOnlyCollection<string> ClientPostLogoutRedirectUris { get; set; }

    public bool RequireConsent { get; set; }

    public bool AllowOfflineAccess { get; set; }

    public string ClientType { get; set; }
}

public sealed record IdentityResourceResponse
{
    public string Name { get; set; }

    public string? DisplayName { get; set; }

    public string? Description { get; set; }

    public bool Required { get; set; }

    public bool Emphasize { get; set; }

    public bool ShowInDiscoveryDocument { get; set; }

    public IReadOnlyCollection<string> Claims { get; set; }
}


public sealed record RoleResponse
{
    public String RoleId { get; set; }
    public string RoleName { get; set; }
}

public sealed record UserResponse {
    public String UserId { get; set; }

    public string UserName { get; set; }

    public string? EmailAddress { get; set; }

    public string? PhoneNumber { get; set; }

    public string? GivenName { get; set; }

    public string? MiddleName { get; set; }
    
    public string? FamilyName { get; set; }

    public IReadOnlyDictionary<string, string> Claims{ get; set; }

    public IReadOnlyCollection<string> Roles { get; set; }

    public DateTime RegistrationDateTime { get; set; }
}

public class TokenResponse
{
    public String AccessToken { get; private set; }
    public DateTimeOffset Expires { get; private set; }
    public Int64 ExpiresIn { get; private set; }
    public DateTimeOffset Issued { get; private set; }
    public String RefreshToken { get; private set; }
    public static TokenResponse Create(String token) {
        using var doc = JsonDocument.Parse(token);
        var root = doc.RootElement;

        long expiresIn = root.GetProperty("expires_in").GetInt64();
        string accessToken = root.GetProperty("access_token").GetString();

        DateTimeOffset issued = DateTimeOffset.Now;
        DateTimeOffset expires = issued.AddSeconds(expiresIn);

        string refreshToken = null;
        if (root.TryGetProperty("refresh_token", out var refreshElement))
        {
            refreshToken = refreshElement.GetString();
        }

        return TokenResponse.Create(accessToken, refreshToken, expiresIn, issued, expires);
    }

    public static TokenResponse Create(String accessToken,
                                       String refreshToken,
                                       Int64 expiresIn,
                                       DateTimeOffset issued = default(DateTimeOffset),
                                       DateTimeOffset expires = default(DateTimeOffset))
    {
        return new TokenResponse(accessToken, refreshToken, expiresIn, issued, expires);
    }

    private TokenResponse(String accessToken,
                          String refreshToken,
                          Int64 expiresIn,
                          DateTimeOffset issued = default(DateTimeOffset),
                          DateTimeOffset expires = default(DateTimeOffset))
    {
        this.AccessToken = accessToken;
        this.RefreshToken = refreshToken;
        this.ExpiresIn = expiresIn;
        this.Issued = issued;
        this.Expires = expires;
    }
}