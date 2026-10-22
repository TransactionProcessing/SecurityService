namespace SecurityService.DataTransferObjects;

/// <summary>Configuration for registering an OAuth/OIDC client.</summary>
/// <example>{"clientId":"orders-web","secret":"replace-with-a-secret","clientName":"Orders web app","allowedScopes":["openid","profile","orders.read"],"allowedGrantTypes":["authorization_code","refresh_token"],"clientUri":"https://orders.example.com","clientRedirectUris":["https://orders.example.com/signin-oidc"],"clientPostLogoutRedirectUris":["https://orders.example.com/signed-out"],"requireConsent":true,"allowOfflineAccess":true}</example>
public sealed class CreateClientRequest
{
    /// <summary>Unique identifier sent as the OAuth <c>client_id</c>.</summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>Shared secret for a confidential client. Omit for a public client; never returned by read endpoints.</summary>
    public string? Secret { get; set; }

    /// <summary>Human-readable client name shown on consent and management screens.</summary>
    public string ClientName { get; set; } = string.Empty;

    /// <summary>Optional administrative description of this client.</summary>
    public string? ClientDescription { get; set; }

    /// <summary>OAuth scopes this client is permitted to request, such as <c>openid</c> and <c>orders.read</c>.</summary>
    public List<string> AllowedScopes { get; set; } = new();

    /// <summary>OAuth grant types this client may use, such as <c>authorization_code</c>, <c>refresh_token</c>, or <c>client_credentials</c>.</summary>
    public List<string> AllowedGrantTypes { get; set; } = new();

    /// <summary>Optional homepage URI for the client application.</summary>
    public string? ClientUri { get; set; }

    /// <summary>Exact OAuth callback URIs allowed for authorization responses.</summary>
    public List<string> ClientRedirectUris { get; set; } = new();

    /// <summary>Exact URIs allowed after an OIDC logout completes.</summary>
    public List<string> ClientPostLogoutRedirectUris { get; set; } = new();

    /// <summary>Whether the user must approve requested scopes through a consent screen.</summary>
    public bool RequireConsent { get; set; }

    /// <summary>Whether the client may request offline access and receive refresh tokens when supported by its grant.</summary>
    public bool AllowOfflineAccess { get; set; }
}

/// <summary>Definition of a permission scope exposed by a protected API.</summary>
/// <example>{"name":"orders.read","displayName":"Read orders","description":"View orders belonging to the signed-in user."}</example>
public sealed class CreateApiScopeRequest
{
    /// <summary>Unique scope name requested by OAuth clients, for example <c>orders.read</c>.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Optional human-readable name shown in consent and documentation.</summary>
    public string? DisplayName { get; set; }

    /// <summary>Optional explanation of the access granted by this scope.</summary>
    public string? Description { get; set; }
}

/// <summary>Definition of a protected API resource, its scopes, and the user claims it can receive.</summary>
/// <example>{"name":"orders-api","displayName":"Orders API","description":"Order management service","secret":"replace-with-a-secret","scopes":["orders.read"],"userClaims":["sub","email"]}</example>
public sealed class CreateApiResourceRequest
{
    /// <summary>Unique audience/resource name used to identify this API.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Optional human-readable API name.</summary>
    public string? DisplayName { get; set; }

    /// <summary>Optional description of the API resource.</summary>
    public string? Description { get; set; }

    /// <summary>Optional shared secret used by the resource server for applicable token validation flows.</summary>
    public string? Secret { get; set; }

    /// <summary>Scope names that grant access to this resource.</summary>
    public List<string> Scopes { get; set; } = new();

    /// <summary>User claim names that may be included in tokens issued for this resource.</summary>
    public List<string> UserClaims { get; set; } = new();
}

/// <summary>Definition of an OIDC identity resource and the claims it exposes.</summary>
/// <example>{"name":"profile","displayName":"Profile","description":"Basic profile claims","required":false,"emphasize":false,"showInDiscoveryDocument":true,"claims":["name","given_name","family_name"]}</example>
public sealed class CreateIdentityResourceRequest
{
    /// <summary>Unique identity scope/resource name, such as <c>profile</c> or <c>email</c>.</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>Optional human-readable name shown to the user.</summary>
    public string? DisplayName { get; set; }

    /// <summary>Optional explanation of the identity data represented by this resource.</summary>
    public string? Description { get; set; }

    /// <summary>Whether requesting clients must request this resource to complete authentication.</summary>
    public bool Required { get; set; }

    /// <summary>Whether this resource should be highlighted on the consent screen.</summary>
    public bool Emphasize { get; set; }
    
    /// <summary>Whether this resource is listed in the OIDC discovery document. Defaults to <see langword="true"/>.</summary>
    public bool ShowInDiscoveryDocument { get; set; } = true;

    /// <summary>Claim names provided when this identity resource is granted, such as <c>name</c> or <c>email</c>.</summary>
    public List<string> Claims { get; set; } = new();
}

/// <summary>Role name to create.</summary>
/// <example>{"roleName":"OrderManager"}</example>
public sealed class CreateRoleRequest
{
    /// <summary>Unique role name to create.</summary>
    public string RoleName { get; set; } = string.Empty;
}

/// <summary>Account details, credentials, and optional authorization data for a new user.</summary>
/// <example>{"givenName":"Avery","familyName":"Nguyen","userName":"avery.nguyen","password":"replace-with-a-policy-compliant-password","emailAddress":"avery@example.com","phoneNumber":"+15550102020","claims":{"department":"sales"},"roles":["OrderManager"]}</example>
public sealed class CreateUserRequest
{
    /// <summary>Optional given/first name.</summary>
    public string? GivenName { get; set; }
    
    /// <summary>Optional middle name.</summary>
    public string? MiddleName { get; set; }

    /// <summary>Optional family/last name.</summary>
    public string? FamilyName { get; set; }

    /// <summary>Unique sign-in name for the account.</summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>Initial password; must satisfy the service's configured identity password policy.</summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>Optional email address. Confirmation requirements depend on service configuration.</summary>
    public string? EmailAddress { get; set; }

    /// <summary>Optional phone number, preferably in international format.</summary>
    public string? PhoneNumber { get; set; }
    
    /// <summary>Additional string claims to assign, represented as claim-name/value pairs.</summary>
    public Dictionary<string, string> Claims { get; set; } = new();

    /// <summary>Names of existing roles to assign to this user.</summary>
    public List<string> Roles { get; set; } = new();
}

