using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using SecurityService.Database;
using SecurityService.Database.DbContexts;
using SecurityService.Database.Entities;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace SecurityService.BusinessLogic.Oidc;

public static class OidcHelpers
{
    public static string BuildCurrentRequestUrl(HttpRequest request)
    {
        var path = request.PathBase + request.Path;
        return path + request.QueryString;
    }

    public static string AppendQueryValue(string url, string name, string value)
    {
        var delimiter = url.Contains('?') ? '&' : '?';
        return $"{url}{delimiter}{Uri.EscapeDataString(name)}={Uri.EscapeDataString(value)}";
    }

    public static string AppendQueryValues(string url, string name, IEnumerable<string> values)
    {
        foreach (var value in values)
        {
            url = AppendQueryValue(url, name, value);
        }

        return url;
    }

    public static IReadOnlyCollection<string> ReadMultiValue(IQueryCollection query, string key)
        => query.TryGetValue(key, out StringValues values) ? values.Where(value => value is not null).Cast<string>().ToArray() : Array.Empty<string>();

    public static async Task<ClaimsPrincipal> CreatePrincipal(
        ApplicationUser user,
        UserManager<ApplicationUser> userManager,
        IEnumerable<string> scopes,
        IEnumerable<string> resources,
        string? authorizationId)
    {
        var identity = new ClaimsIdentity(TokenValidationParameters.DefaultAuthenticationType, Claims.Name, Claims.Role);

        identity.SetClaim(Claims.Subject, user.Id)
                .SetClaim(Claims.Email, user.Email)
                .SetClaim(Claims.Name, user.UserName)
                .SetClaim(Claims.PreferredUsername, user.UserName)
                .SetClaim(ClaimTypes.NameIdentifier, user.Id);

        if (string.IsNullOrWhiteSpace(user.GivenName) == false)
        {
            identity.SetClaim(Claims.GivenName, user.GivenName);
        }

        if (string.IsNullOrWhiteSpace(user.FamilyName) == false)
        {
            identity.SetClaim(Claims.FamilyName, user.FamilyName);
        }

        if (string.IsNullOrWhiteSpace(user.MiddleName) == false)
        {
            identity.SetClaim("middle_name", user.MiddleName);
        }

        foreach (var role in await userManager.GetRolesAsync(user))
        {
            AddClaimIfMissing(identity, Claims.Role, role);
        }

        foreach (var claim in await userManager.GetClaimsAsync(user))
        {
            AddClaimIfMissing(identity, claim.Type, claim.Value);
        }

        identity.SetScopes(scopes);
        identity.SetResources(resources);
        if (string.IsNullOrWhiteSpace(authorizationId) == false)
        {
            identity.SetAuthorizationId(authorizationId);
        }

        identity.SetDestinations(GetDestinations);
        return new ClaimsPrincipal(identity);
    }

    private static void AddClaimIfMissing(ClaimsIdentity identity, string type, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        if (identity.Claims.Any(existing => existing.Type == type && string.Equals(existing.Value, value, StringComparison.Ordinal)))
        {
            return;
        }

        identity.AddClaim(new Claim(type, value));
    }

    public static IEnumerable<string> GetDestinations(Claim claim)
    {
        return claim.Type switch
        {
            Claims.Name or Claims.PreferredUsername or Claims.Email or Claims.GivenName or Claims.FamilyName or "middle_name" =>
                [Destinations.AccessToken, Destinations.IdentityToken],
            Claims.Role => [Destinations.AccessToken, Destinations.IdentityToken],
            Claims.Subject => [Destinations.AccessToken, Destinations.IdentityToken],
            _ => [Destinations.AccessToken]
        };
    }

    public static async Task<(IReadOnlyCollection<ScopeDisplayItem> IdentityScopes, IReadOnlyCollection<ScopeDisplayItem> ApiScopes)> BuildScopeDisplay(
        OpenIddictRequest request,
        SecurityServiceDbContext dbContext,
        CancellationToken cancellationToken)
    {
        return await BuildScopeDisplay(request.GetScopes(), dbContext, cancellationToken);
    }

    public static async Task<(IReadOnlyCollection<ScopeDisplayItem> IdentityScopes, IReadOnlyCollection<ScopeDisplayItem> ApiScopes)> BuildScopeDisplay(
        IEnumerable<string> scopeNames,
        SecurityServiceDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var scopes = scopeNames
            .Where(scope => string.IsNullOrWhiteSpace(scope) == false)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        var definitions = await dbContext.ResourceDefinitions
            .Where(resource => scopes.Contains(resource.Name))
            .ToListAsync(cancellationToken);

        var identityScopes = definitions
            .Where(resource => resource.Type == ResourceType.IdentityResource)
            .Select(resource => new ScopeDisplayItem(resource.Name, resource.DisplayName ?? resource.Name, resource.Description, resource.Required, resource.Emphasize))
            .OrderBy(scope => scope.Name)
            .ToArray();

        var apiScopes = scopes
            .Where(scopeName => definitions.Any(resource => resource.Name == scopeName && resource.Type == ResourceType.IdentityResource) == false)
            .Select(scopeName =>
            {
                var definition = definitions.SingleOrDefault(resource => resource.Name == scopeName);
                return new ScopeDisplayItem(scopeName, definition?.DisplayName ?? scopeName, definition?.Description, false, definition?.Emphasize ?? false);
            })
            .OrderBy(scope => scope.Name)
            .ToArray();

        return (identityScopes, apiScopes);
    }

    public static async Task<IReadOnlyCollection<string>> ResolveClientCredentialsScopes(
        OpenIddictRequest request,
        SecurityServiceDbContext dbContext,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<string> grantedScopes = request.GetScopes().ToArray();
        if (grantedScopes.Count > 0)
        {
            return grantedScopes;
        }

        var clientDefinition = await dbContext.ClientDefinitions
            .SingleOrDefaultAsync(client => client.ClientId == request.ClientId, cancellationToken);

        return clientDefinition is null
            ? Array.Empty<string>()
            : JsonListSerializer.Deserialize(clientDefinition.AllowedScopesJson);
    }
}

public sealed record ScopeDisplayItem(string Name, string DisplayName, string? Description, bool Required, bool Emphasize);
