
using System.Text.Json;

namespace SecurityService.DataTransferObjects;

/// <summary>Public description of a protected API resource. Secrets are intentionally excluded.</summary>
/// <example>{"name":"orders-api","displayName":"Orders API","description":"Order management service","scopes":["orders.read"],"userClaims":["sub","email"]}</example>
public sealed class ApiResourceResponse {
    /// <summary>Unique API resource/audience name.</summary>
    public string Name { get; set; }

    /// <summary>Optional human-readable API name.</summary>
    public string? DisplayName { get; set; }

    /// <summary>Optional description of the API.</summary>
    public string? Description { get; set; }

    /// <summary>Scopes associated with this API.</summary>
    public IReadOnlyCollection<string> Scopes { get; set; }

    /// <summary>User claim names that may be included in tokens for this API.</summary>
    public IReadOnlyCollection<string> UserClaims { get; set; }
}

/// <summary>Public description of an API scope.</summary>
/// <example>{"name":"orders.read","displayName":"Read orders","description":"View orders belonging to the signed-in user."}</example>
public sealed record ApiScopeResponse {
    /// <summary>Unique scope name used in OAuth requests.</summary>
    public string Name { get; set; }

    /// <summary>Optional human-readable scope name.</summary>
    public string? DisplayName { get; set; }

    /// <summary>Optional explanation of the permission.</summary>
    public string? Description { get; set; }
}

/// <summary>Public client registration details. The client secret is never returned.</summary>
/// <example>{"clientId":"orders-web","clientName":"Orders web app","description":"Customer order portal","clientUri":"https://orders.example.com","allowedScopes":["openid","profile","orders.read"],"allowedGrantTypes":["authorization_code","refresh_token"],"clientRedirectUris":["https://orders.example.com/signin-oidc"],"clientPostLogoutRedirectUris":["https://orders.example.com/signed-out"],"requireConsent":true,"allowOfflineAccess":true,"clientType":"confidential"}</example>
public sealed record ClientResponse {
    /// <summary>OAuth client identifier.</summary>
    public string ClientId { get; set; }
    
    /// <summary>Human-readable client name.</summary>
    public string ClientName { get; set; }

    /// <summary>Optional administrative description.</summary>
    public string? Description { get; set; }

    /// <summary>Optional homepage URI.</summary>
    public string? ClientUri { get; set; }

    /// <summary>Scopes the client is permitted to request.</summary>
    public IReadOnlyCollection<string> AllowedScopes { get; set; }

    /// <summary>OAuth grant types permitted for the client.</summary>
    public IReadOnlyCollection<string> AllowedGrantTypes { get; set; }

    /// <summary>Registered authorization callback URIs.</summary>
    public IReadOnlyCollection<string> ClientRedirectUris { get; set; }

    /// <summary>Registered post-logout callback URIs.</summary>
    public IReadOnlyCollection<string> ClientPostLogoutRedirectUris { get; set; }

    /// <summary>Whether user consent is required for requested scopes.</summary>
    public bool RequireConsent { get; set; }

    /// <summary>Whether offline access/refresh tokens are permitted.</summary>
    public bool AllowOfflineAccess { get; set; }

    /// <summary>Configured client classification, such as public or confidential.</summary>
    public string ClientType { get; set; }
}

/// <summary>Public description of an OIDC identity resource.</summary>
/// <example>{"name":"profile","displayName":"Profile","description":"Basic profile claims","required":false,"emphasize":false,"showInDiscoveryDocument":true,"claims":["name","given_name","family_name"]}</example>
public sealed record IdentityResourceResponse
{
    /// <summary>Unique identity resource name.</summary>
    public string Name { get; set; }

    /// <summary>Optional human-readable name.</summary>
    public string? DisplayName { get; set; }

    /// <summary>Optional explanation of the identity data.</summary>
    public string? Description { get; set; }

    /// <summary>Whether clients must request this resource.</summary>
    public bool Required { get; set; }

    /// <summary>Whether this resource is highlighted on consent screens.</summary>
    public bool Emphasize { get; set; }

    /// <summary>Whether this resource is listed in OIDC discovery.</summary>
    public bool ShowInDiscoveryDocument { get; set; }

    /// <summary>Claims provided by this resource.</summary>
    public IReadOnlyCollection<string> Claims { get; set; }
}


/// <summary>Role identifier and name.</summary>
/// <example>{"roleId":"2f0c1d2e-3456-4789-a123-123456789abc","roleName":"OrderManager"}</example>
public sealed record RoleResponse
{
    /// <summary>Identity system's stable role ID.</summary>
    public String RoleId { get; set; }
    /// <summary>Unique role name.</summary>
    public string RoleName { get; set; }
}

/// <summary>User profile and authorization data returned by management endpoints.</summary>
/// <example>{"userId":"7a0c1d2e-3456-4789-a123-123456789abc","userName":"avery.nguyen","emailAddress":"avery@example.com","phoneNumber":"+15550102020","givenName":"Avery","middleName":null,"familyName":"Nguyen","claims":{"department":"sales"},"roles":["OrderManager"],"registrationDateTime":"2026-10-22T10:30:00Z"}</example>
public sealed record UserResponse {
    /// <summary>Identity system's stable user ID.</summary>
    public String UserId { get; set; }

    /// <summary>Sign-in name.</summary>
    public string UserName { get; set; }

    /// <summary>Optional email address.</summary>
    public string? EmailAddress { get; set; }

    /// <summary>Optional phone number.</summary>
    public string? PhoneNumber { get; set; }

    /// <summary>Optional given/first name.</summary>
    public string? GivenName { get; set; }

    /// <summary>Optional middle name.</summary>
    public string? MiddleName { get; set; }
    
    /// <summary>Optional family/last name.</summary>
    public string? FamilyName { get; set; }

    /// <summary>Additional string claims assigned to the user.</summary>
    public IReadOnlyDictionary<string, string> Claims{ get; set; }

    /// <summary>Role names assigned to the user.</summary>
    public IReadOnlyCollection<string> Roles { get; set; }

    /// <summary>UTC date and time when the account was registered.</summary>
    public DateTime RegistrationDateTime { get; set; }
}

/// <summary>OAuth token response details with expiry timestamps.</summary>
/// <example>{"accessToken":"eyJ...","expires":"2026-10-22T11:30:00Z","expiresIn":3600,"issued":"2026-10-22T10:30:00Z","refreshToken":"def..."}</example>
public class TokenResponse
{
    /// <summary>Bearer access token issued by the authorization server.</summary>
    public String AccessToken { get; private set; }
    /// <summary>Absolute expiration timestamp for the access token.</summary>
    public DateTimeOffset Expires { get; private set; }
    /// <summary>Access token lifetime in seconds.</summary>
    public Int64 ExpiresIn { get; private set; }
    /// <summary>Timestamp when this response was received/issued.</summary>
    public DateTimeOffset Issued { get; private set; }
    /// <summary>Refresh token when the grant and client are permitted to receive one; otherwise null.</summary>
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
