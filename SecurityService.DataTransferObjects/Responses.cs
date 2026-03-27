using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace SecurityService.DataTransferObjects;

public sealed class ApiResourceResponse {
    [JsonProperty("name")]
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonProperty("display_name")]
    [JsonPropertyName("display_name")]
    public string? DisplayName { get; set; }

    [JsonProperty("description")]
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonProperty("scopes")]
    [JsonPropertyName("scopes")]
    public IReadOnlyCollection<string> Scopes { get; set; }

    [JsonProperty("user_claims")]
    [JsonPropertyName("user_claims")]
    public IReadOnlyCollection<string> UserClaims { get; set; }
}

public sealed record ApiScopeResponse {
    [JsonProperty("name")]
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonProperty("display_name")]
    [JsonPropertyName("display_name")]
    public string? DisplayName { get; set; }

    [JsonProperty("description")]
    [JsonPropertyName("description")]
    public string? Description { get; set; }
}

public sealed record ClientResponse {
    [JsonProperty("client_id")]
    [JsonPropertyName("client_id")]
    public string ClientId { get; set; }
    [JsonProperty("client_name")]
    [JsonPropertyName("client_name")]
    public string ClientName { get; set; }

    [JsonProperty("client_description")]
    [JsonPropertyName("client_description")]
    public string? Description { get; set; }

    [JsonProperty("client_uri")]
    [JsonPropertyName("client_uri")]
    public string? ClientUri { get; set; }

    [JsonProperty("allowed_scopes")]
    [JsonPropertyName("allowed_scopes")]
    public IReadOnlyCollection<string> AllowedScopes { get; set; }

    [JsonProperty("allowed_grant_types")]
    [JsonPropertyName("allowed_grant_types")]
    public IReadOnlyCollection<string> AllowedGrantTypes { get; set; }

    [JsonProperty("client_redirect_uris")]
    [JsonPropertyName("client_redirect_uris")]
    public IReadOnlyCollection<string> RedirectUris { get; set; }

    [JsonProperty("client_post_logout_redirect_uris")]
    [JsonPropertyName("client_post_logout_redirect_uris")]
    public IReadOnlyCollection<string> PostLogoutRedirectUris { get; set; }

    [JsonProperty("require_consent")]
    [JsonPropertyName("require_consent")]
    public bool RequireConsent { get; set; }

    [JsonProperty("allow_offline_access")]
    [JsonPropertyName("allow_offline_access")]
    public bool AllowOfflineAccess { get; set; }

    [JsonProperty("client_type")]
    [JsonPropertyName("client_type")]
    public string ClientType { get; set; }
}

public sealed record IdentityResourceResponse
{
    [JsonProperty("name")]
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonProperty("display_name")]
    [JsonPropertyName("display_name")]
    public string? DisplayName { get; set; }

    [JsonProperty("description")]
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonProperty("required")]
    [JsonPropertyName("required")]
    public bool Required { get; set; }

    [JsonProperty("emphasize")]
    [JsonPropertyName("emphasize")]
    public bool Emphasize { get; set; }

    [JsonProperty("show_in_discovery_document")]
    [JsonPropertyName("show_in_discovery_document")]
    public bool ShowInDiscoveryDocument { get; set; }

    [JsonProperty("claims")]
    [JsonPropertyName("claims")]
    public IReadOnlyCollection<string> Claims { get; set; }
}


public sealed record RoleResponse
{
    [JsonProperty("role_id")]
    [JsonPropertyName("role_id")]
    public String RoleId { get; set; }

    [JsonProperty("role_name")]
    [JsonPropertyName("role_name")]
    public string Name { get; set; }
}

public sealed record UserResponse {
    [JsonProperty("user_id")]
    [JsonPropertyName("user_id")]
    public String UserId { get; set; }

    [JsonProperty("user_name")]
    [JsonPropertyName("user_name")]
    public string UserName { get; set; }

    [JsonProperty("email_address")]
    [JsonPropertyName("email_address")]
    public string? EmailAddress { get; set; }

    [JsonProperty("phone_number")]
    [JsonPropertyName("phone_number")]
    public string? PhoneNumber { get; set; }

    [JsonProperty("given_name")]
    [JsonPropertyName("given_name")]
    public string? GivenName { get; set; }

    [JsonProperty("middle_name")]
    [JsonPropertyName("middle_name")]
    public string? MiddleName { get; set; }
    
    [JsonProperty("family_name")]
    [JsonPropertyName("family_name")]
    public string? FamilyName { get; set; }

    [JsonProperty("claims")]
    [JsonPropertyName("claims")]
    public IReadOnlyDictionary<string, string> Claims{ get; set; }

    [JsonProperty("roles")]
    [JsonPropertyName("roles")]
    public IReadOnlyCollection<string> Roles { get; set; }
}

public class TokenResponse
{
    /// <summary>
    /// The access token
    /// </summary>
    /// <value>The access token.</value>
    public String AccessToken { get; private set; }

    /// <summary>
    /// Gets the expires.
    /// </summary>
    /// <value>The expires.</value>
    public DateTimeOffset Expires { get; private set; }

    /// <summary>
    /// The expires
    /// </summary>
    /// <value>The expires in.</value>
    public Int64 ExpiresIn { get; private set; }

    /// <summary>
    /// Gets the issued.
    /// </summary>
    /// <value>The issued.</value>
    public DateTimeOffset Issued { get; private set; }

    /// <summary>
    /// The refresh token
    /// </summary>
    /// <value>The refresh token.</value>
    public String RefreshToken { get; private set; }

    public static TokenResponse Create(String token)
    {
        dynamic auth = JsonConvert.DeserializeObject(token);

        Int64 expiresIn = auth["expires_in"].Value;
        String accessToken = auth["access_token"].Value;

        DateTimeOffset issued = DateTimeOffset.Now;
        DateTimeOffset expires = DateTimeOffset.Now.AddSeconds(expiresIn);

        String refreshToken = null;
        //For client credentials, the refresh_token will not be present
        if (auth["refresh_token"] != null)
        {
            refreshToken = auth["refresh_token"].Value;
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