using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using SecurityService.Database;

namespace SecurityService.Pages.Account.Login;

public sealed class IndexModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;

    public IndexModel(SignInManager<ApplicationUser> signInManager)
    {
        this._signInManager = signInManager;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public IReadOnlyCollection<ExternalProviderViewModel> Providers { get; private set; } = Array.Empty<ExternalProviderViewModel>();

    public async Task<IActionResult> OnGet(string returnUrl)
    {
        this.Input.ReturnUrl = string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl;

        var queryString = new Uri("https://dummy" + this.Input.ReturnUrl).Query;
        var query = QueryHelpers.ParseQuery(queryString);
        this.Input.ClientId = query["client_id"].ToString();
        
        await this.LoadProvidersAsync();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await this.LoadProvidersAsync();

        if (this.Input.Button == "cancel")
        {
            return this.LocalRedirect("/");
        }

        // the user clicked the "cancel" button
        if (Input.Button == "forgotpassword")
        {
            return this.RedirectToPage("/Account/ForgotPassword/Index", new
            {
                returnUrl = this.Input.ReturnUrl,
                clientId = this.Input.ClientId
            });
        }

        var result = await this._signInManager.PasswordSignInAsync(this.Input.Username, this.Input.Password, this.Input.RememberLogin, lockoutOnFailure: true);
        if (result.Succeeded)
        {
            return this.LocalRedirect(string.IsNullOrWhiteSpace(this.Input.ReturnUrl) ? "/" : this.Input.ReturnUrl);
        }

        this.ModelState.AddModelError(string.Empty, "Invalid username or password.");
        return this.Page();
    }

    private async Task LoadProvidersAsync()
    {
        this.Providers = (await this._signInManager.GetExternalAuthenticationSchemesAsync())
            .Select(scheme => new ExternalProviderViewModel(scheme.Name, scheme.DisplayName ?? scheme.Name))
            .OrderBy(provider => provider.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public sealed class InputModel
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool RememberLogin { get; set; }
        public string ReturnUrl { get; set; } = "/";
        public string Button { get; set; } = "login";

        public string ClientId { get; set; }
    }

    public sealed record ExternalProviderViewModel(string Name, string DisplayName);
}
