namespace IdentityServerHost.Pages.EmailConfirmation;

using System.ComponentModel.DataAnnotations;

public class ConfirmInputModel
{
    public string Username { get; set; }
    public string Token { get; set; }

    [Required]
    public string Password { get; set; }
    
}