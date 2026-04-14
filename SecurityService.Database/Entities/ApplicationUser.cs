using Microsoft.AspNetCore.Identity;

namespace SecurityService.Database.Entities;

public sealed class ApplicationUser : IdentityUser
{
    public string? GivenName { get; set; }

    public string? MiddleName { get; set; }

    public string? FamilyName { get; set; }

    public DateTime RegistrationDateTime { get; set; }
}
