using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SecurityService.BusinessLogic.Oidc;

namespace SecurityService.Pages.Account.Logout;

public sealed class IndexModel : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public void OnGet(string? returnUrl)
    {
        this.Input.ReturnUrl = string.IsNullOrWhiteSpace(returnUrl) ? "/connect/logout" : returnUrl;
    }

    public IActionResult OnPost()
    {
        return this.Redirect(OidcHelpers.AppendQueryValue(this.Input.ReturnUrl, "logout", "confirmed"));
    }

    public sealed class InputModel
    {
        public string ReturnUrl { get; set; } = "/connect/logout";
    }
}
