namespace SecurityService.BusinessLogic
{
    using Microsoft.AspNetCore.Identity;
    using System;

    public class ApplicationUser : IdentityUser
    {
        public DateTime RegistrationDateTime { get; set; }
    }
}