namespace SecurityService.Database.Entities;

public sealed class ResourceDefinition
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? DisplayName { get; set; }

    public string? Description { get; set; }

    public ResourceType Type { get; set; }

    public string? SecretHash { get; set; }

    public string ClaimsJson { get; set; } = "[]";

    public string ScopesJson { get; set; } = "[]";

    public bool Required { get; set; }

    public bool Emphasize { get; set; }

    public bool ShowInDiscoveryDocument { get; set; } = true;
}

public enum ResourceType
{
    ApiScope = 1,
    ApiResource = 2,
    IdentityResource = 3
}
