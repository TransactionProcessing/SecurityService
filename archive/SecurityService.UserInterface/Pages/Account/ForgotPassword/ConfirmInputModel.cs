namespace IdentityServerHost.Pages.ForgotPassword;

using System.ComponentModel.DataAnnotations;

public class ConfirmInputModel
{
    public string Username { get; set; }
    public string Token { get; set; }

    [Required]
    public string Password { get; set; }

    [Required]
    public string ConfirmPassword { get; set; }

    public string Button { get; set; }
    public string ClientId { get; set; }
}