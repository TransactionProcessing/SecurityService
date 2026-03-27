namespace SecurityService.Models;

public sealed record GrantDetails(
    string AuthorizationId,
    string ClientId,
    string DisplayName,
    IReadOnlyCollection<string> Scopes,
    DateTimeOffset? CreatedAt);
