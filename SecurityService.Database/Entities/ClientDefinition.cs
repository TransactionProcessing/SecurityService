namespace SecurityService.Database.Entities;

public sealed class ClientDefinition
{
    public Guid Id { get; set; }

    public string ClientId { get; set; } = string.Empty;

    public string ClientName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? ClientUri { get; set; }

    public string? SecretHash { get; set; }

    public string AllowedScopesJson { get; set; } = "[]";

    public string AllowedGrantTypesJson { get; set; } = "[]";

    public string RedirectUrisJson { get; set; } = "[]";

    public string PostLogoutRedirectUrisJson { get; set; } = "[]";

    public bool RequireConsent { get; set; }

    public bool AllowOfflineAccess { get; set; }

    public string ClientType { get; set; } = string.Empty;
}
