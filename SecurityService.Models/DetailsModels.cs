namespace SecurityService.Models;

public sealed record ClientDetails(
    string ClientId,
    string ClientName,
    string? Description,
    string? ClientUri,
    IReadOnlyCollection<string> AllowedScopes,
    IReadOnlyCollection<string> AllowedGrantTypes,
    IReadOnlyCollection<string> RedirectUris,
    IReadOnlyCollection<string> PostLogoutRedirectUris,
    bool RequireConsent,
    bool AllowOfflineAccess,
    string ClientType);

public sealed record ApiScopeDetails(
    string Name,
    string? DisplayName,
    string? Description);

public sealed record ApiResourceDetails(
    string Name,
    string? DisplayName,
    string? Description,
    IReadOnlyCollection<string> Scopes,
    IReadOnlyCollection<string> UserClaims);

public sealed record IdentityResourceDetails(
    string Name,
    string? DisplayName,
    string? Description,
    bool Required,
    bool Emphasize,
    bool ShowInDiscoveryDocument,
    IReadOnlyCollection<string> Claims);

public sealed record RoleDetails(
    String RoleId,
    string Name);

public sealed record UserDetails(
    String UserId,
    string UserName,
    string? EmailAddress,
    string? PhoneNumber,
    string? GivenName,
    string? MiddleName,
    string? FamilyName,
    IReadOnlyDictionary<string, string> Claims,
    IReadOnlyCollection<string> Roles);

public sealed record ExternalProviderDetails(string Name, string DisplayName);
