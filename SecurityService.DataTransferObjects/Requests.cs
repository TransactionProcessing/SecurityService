using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace SecurityService.DataTransferObjects;

public sealed class CreateClientRequest
{
    [JsonProperty("client_id")]
    [JsonPropertyName("client_id")]
    public string ClientId { get; set; } = string.Empty;

    [JsonPropertyName("secret")]
    public string? Secret { get; set; }

    [JsonProperty("client_name")]
    [JsonPropertyName("client_name")]
    public string ClientName { get; set; } = string.Empty;

    [JsonProperty("client_description")]
    [JsonPropertyName("client_description")]
    public string? ClientDescription { get; set; }

    [JsonProperty("allowed_scopes")]
    [JsonPropertyName("allowed_scopes")]
    public List<string> AllowedScopes { get; set; } = new();

    [JsonProperty("allowed_grant_types")]
    [JsonPropertyName("allowed_grant_types")]
    public List<string> AllowedGrantTypes { get; set; } = new();

    [JsonProperty("client_uri")]
    [JsonPropertyName("client_uri")]
    public string? ClientUri { get; set; }

    [JsonProperty("client_redirect_uris")]
    [JsonPropertyName("client_redirect_uris")]
    public List<string> ClientRedirectUris { get; set; } = new();

    [JsonProperty("client_post_logout_redirect_uris")]
    [JsonPropertyName("client_post_logout_redirect_uris")]
    public List<string> ClientPostLogoutRedirectUris { get; set; } = new();

    [JsonProperty("require_consent")]
    [JsonPropertyName("require_consent")]
    public bool RequireConsent { get; set; }

    [JsonProperty("allow_offline_access")]
    [JsonPropertyName("allow_offline_access")]
    public bool AllowOfflineAccess { get; set; }
}

public sealed class CreateApiScopeRequest
{
    [JsonProperty("name")]
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("display_name")]
    [JsonPropertyName("display_name")]
    public string? DisplayName { get; set; }

    [JsonProperty("description")]
    [JsonPropertyName("description")]
    public string? Description { get; set; }
}

public sealed class CreateApiResourceRequest
{
    [JsonProperty("name")]
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("display_name")]
    [JsonPropertyName("display_name")]
    public string? DisplayName { get; set; }

    [JsonProperty("description")]
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonProperty("secret")]
    [JsonPropertyName("secret")]
    public string? Secret { get; set; }

    [JsonProperty("scopes")]
    [JsonPropertyName("scopes")]
    public List<string> Scopes { get; set; } = new();

    [JsonProperty("user_claims")]
    [JsonPropertyName("user_claims")]
    public List<string> UserClaims { get; set; } = new();
}

public sealed class CreateIdentityResourceRequest
{
    [JsonProperty("name")]
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

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
    public bool ShowInDiscoveryDocument { get; set; } = true;

    [JsonProperty("claims")]
    [JsonPropertyName("claims")]
    public List<string> Claims { get; set; } = new();
}

public sealed class CreateRoleRequest
{
    [JsonProperty("role_name")]
    [JsonPropertyName("role_name")]
    public string Name { get; set; } = string.Empty;
}

public sealed class CreateUserRequest
{
    [JsonProperty("given_name")]
    [JsonPropertyName("given_name")]
    public string? GivenName { get; set; }
    
    [JsonProperty("middle_name")]
    [JsonPropertyName("middle_name")]
    public string? MiddleName { get; set; }

    [JsonProperty("family_name")]
    [JsonPropertyName("family_name")]
    public string? FamilyName { get; set; }

    [JsonProperty("user_name")]
    [JsonPropertyName("user_name")]
    public string UserName { get; set; } = string.Empty;

    [JsonProperty("password")]
    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;

    [JsonProperty("email_address")]
    [JsonPropertyName("email_address")]
    public string? EmailAddress { get; set; }

    [JsonProperty("phone_number")]
    [JsonPropertyName("phone_number")]
    public string? PhoneNumber { get; set; }
    
    [JsonProperty("claims")]
    [JsonPropertyName("claims")]
    public Dictionary<string, string> Claims { get; set; } = new();

    [JsonProperty("roles")]
    [JsonPropertyName("roles")]
    public List<string> Roles { get; set; } = new();
}

