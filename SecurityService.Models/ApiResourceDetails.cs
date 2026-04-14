namespace SecurityService.Models;

public record ApiResourceDetails(string Name,
                                 string? DisplayName,
                                 string? Description,
                                 IReadOnlyCollection<string> Scopes,
                                 IReadOnlyCollection<string> UserClaims);