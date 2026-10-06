using System.Collections.Immutable;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace SecurityService.BusinessLogic.Oidc;

public sealed record OAuthGrantPolicyResult(
    ImmutableHashSet<string> Permissions,
    ImmutableHashSet<string> Requirements);

public static class OAuthGrantPolicy
{
    private static readonly HashSet<string> ApprovedGrantTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        GrantTypes.AuthorizationCode,
        GrantTypes.ClientCredentials,
        GrantTypes.RefreshToken
    };

    private static readonly HashSet<string> LegacyGrantTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        GrantTypes.Password,
        GrantTypes.Implicit,
        "hybrid"
    };

    public static bool IsGrantAllowed(string grantType, string clientId, OAuthOptions options)
    {
        if (ApprovedGrantTypes.Contains(grantType))
        {
            return true;
        }

        if (LegacyGrantTypes.Contains(grantType) == false || options.LegacyGrantTypeClients.TryGetValue(grantType, out List<string>? clients) == false)
        {
            return false;
        }

        return clients.Any(client => string.Equals(client, clientId, StringComparison.OrdinalIgnoreCase));
    }

    public static OAuthGrantPolicyResult CreatePermissions(
        IReadOnlyCollection<string> allowedGrantTypes,
        string clientId,
        OAuthOptions options)
    {
        ImmutableHashSet<string>.Builder permissions = ImmutableHashSet.CreateBuilder<string>(StringComparer.Ordinal);
        ImmutableHashSet<string>.Builder requirements = ImmutableHashSet.CreateBuilder<string>(StringComparer.Ordinal);

        foreach (string grantType in allowedGrantTypes.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (IsGrantAllowed(grantType, clientId, options) == false)
            {
                throw new InvalidOperationException($"Grant type '{grantType}' is not allowed for client '{clientId}'.");
            }

            switch (grantType.ToLowerInvariant())
            {
                case GrantTypes.AuthorizationCode:
                    permissions.Add(Permissions.Endpoints.Authorization);
                    permissions.Add(Permissions.Endpoints.Token);
                    permissions.Add(Permissions.GrantTypes.AuthorizationCode);
                    permissions.Add(Permissions.ResponseTypes.Code);
                    requirements.Add(Requirements.Features.ProofKeyForCodeExchange);
                    break;

                case GrantTypes.ClientCredentials:
                    permissions.Add(Permissions.Endpoints.Token);
                    permissions.Add(Permissions.GrantTypes.ClientCredentials);
                    break;

                case GrantTypes.RefreshToken:
                    permissions.Add(Permissions.Endpoints.Token);
                    permissions.Add(Permissions.GrantTypes.RefreshToken);
                    break;

                case GrantTypes.Password:
                    permissions.Add(Permissions.Endpoints.Token);
                    permissions.Add(Permissions.GrantTypes.Password);
                    break;

                case GrantTypes.Implicit:
                    permissions.Add(Permissions.Endpoints.Authorization);
                    permissions.Add(Permissions.GrantTypes.Implicit);
                    permissions.Add(Permissions.ResponseTypes.Token);
                    permissions.Add(Permissions.ResponseTypes.IdToken);
                    break;

                case "hybrid":
                    permissions.Add(Permissions.Endpoints.Authorization);
                    permissions.Add(Permissions.Endpoints.Token);
                    permissions.Add(Permissions.GrantTypes.AuthorizationCode);
                    permissions.Add(Permissions.ResponseTypes.CodeIdToken);
                    break;

                default:
                    throw new InvalidOperationException($"Unsupported grant type '{grantType}'.");
            }
        }

        return new OAuthGrantPolicyResult(permissions.ToImmutable(), requirements.ToImmutable());
    }
}
