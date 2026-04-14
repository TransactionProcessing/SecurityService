namespace SecurityService.Models;

public record UserDetails(String UserId,
                          string UserName,
                          string? EmailAddress,
                          string? PhoneNumber,
                          string? GivenName,
                          string? MiddleName,
                          string? FamilyName,
                          DateTime RegistrationDateTime,
                          IReadOnlyDictionary<string, string> Claims,
                          IReadOnlyCollection<string> Roles);