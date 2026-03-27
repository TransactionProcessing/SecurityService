using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using SecurityService.Database;
using SecurityService.Services;

namespace SecurityService.Pages.Account.ResendConfirmationEmail;

public sealed class IndexModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    
    public IndexModel(UserManager<ApplicationUser> userManager)
    {
        this._userManager = userManager;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public bool EmailQueued { get; private set; }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(this.Input.EmailOrUserName))
        {
            this.ModelState.AddModelError(string.Empty, "Enter an email address or username.");
            return this.Page();
        }

        var user = await this.ResolveUserAsync(this.Input.EmailOrUserName);
        if (user is not null && string.IsNullOrWhiteSpace(user.Email) == false && user.EmailConfirmed == false)
        {
            var code = await this._userManager.GenerateEmailConfirmationTokenAsync(user);
            var encoded = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = this.Url.Page("/Account/ConfirmEmail/Index", null, new { userId = user.Id, code = encoded }, this.Request.Scheme) ?? $"/Account/ConfirmEmail?userId={Uri.EscapeDataString(user.Id)}&code={Uri.EscapeDataString(encoded)}";
            // TODO: Send email with callback URL
            //await this._emailService.SendAsync(new AccountMessage(user.Email!, "Confirm your email", $"Confirm your account by visiting {callbackUrl}"), cancellationToken);
        }

        this.EmailQueued = true;
        return this.Page();
    }

    private async Task<ApplicationUser?> ResolveUserAsync(string value)
    {
        var user = await this._userManager.FindByEmailAsync(value);
        return user ?? await this._userManager.FindByNameAsync(value);
    }

    public sealed class InputModel
    {
        public string EmailOrUserName { get; set; } = string.Empty;
    }
}
