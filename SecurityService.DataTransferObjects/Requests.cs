namespace SecurityService.DataTransferObjects;

public sealed class CreateClientRequest
{
    public string ClientId { get; set; } = string.Empty;

    public string? Secret { get; set; }

    public string ClientName { get; set; } = string.Empty;

    public string? ClientDescription { get; set; }

    public List<string> AllowedScopes { get; set; } = new();

    public List<string> AllowedGrantTypes { get; set; } = new();

    public string? ClientUri { get; set; }

    public List<string> ClientRedirectUris { get; set; } = new();

    public List<string> ClientPostLogoutRedirectUris { get; set; } = new();

    public bool RequireConsent { get; set; }

    public bool AllowOfflineAccess { get; set; }
}

public sealed class CreateApiScopeRequest
{
    public string Name { get; set; } = string.Empty;

    public string? DisplayName { get; set; }

    public string? Description { get; set; }
}

public sealed class CreateApiResourceRequest
{
    public string Name { get; set; } = string.Empty;

    public string? DisplayName { get; set; }

    public string? Description { get; set; }

    public string? Secret { get; set; }

    public List<string> Scopes { get; set; } = new();

    public List<string> UserClaims { get; set; } = new();
}

public sealed class CreateIdentityResourceRequest
{
    public string Name { get; set; } = string.Empty;
    public string? DisplayName { get; set; }

    public string? Description { get; set; }

    public bool Required { get; set; }

    public bool Emphasize { get; set; }
    
    public bool ShowInDiscoveryDocument { get; set; } = true;

    public List<string> Claims { get; set; } = new();
}

public sealed class CreateRoleRequest
{
    public string RoleName { get; set; } = string.Empty;
}

public sealed class CreateUserRequest
{
    public string? GivenName { get; set; }
    
    public string? MiddleName { get; set; }

    public string? FamilyName { get; set; }

    public string UserName { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string? EmailAddress { get; set; }

    public string? PhoneNumber { get; set; }
    
    public Dictionary<string, string> Claims { get; set; } = new();

    public List<string> Roles { get; set; } = new();
}

