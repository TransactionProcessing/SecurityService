namespace SecurityService.Models;

public record GrantDetails(string AuthorizationId,
                           string ClientId,
                           string DisplayName,
                           IReadOnlyCollection<string> Scopes,
                           DateTimeOffset? CreatedAt);
