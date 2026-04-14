namespace SecurityService.Models;

public record IdentityResourceDetails(string Name,
                                      string? DisplayName,
                                      string? Description,
                                      bool Required,
                                      bool Emphasize,
                                      bool ShowInDiscoveryDocument,
                                      IReadOnlyCollection<string> Claims);