namespace SecurityService.Models;

public record ClientDetails(string ClientId,
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