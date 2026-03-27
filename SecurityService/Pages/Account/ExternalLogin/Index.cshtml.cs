using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SecurityService.Database;

namespace SecurityService.Pages.Account.ExternalLogin;

public sealed class IndexModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public IndexModel(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
    {
        this._signInManager = signInManager;
        this._userManager = userManager;
    }

    [BindProperty(SupportsGet = true)]
    public string ReturnUrl { get; set; } = "/";

    public IReadOnlyCollection<ExternalProviderViewModel> Providers { get; private set; } = Array.Empty<ExternalProviderViewModel>();

    public async Task OnGetAsync(string? returnUrl)
    {
        this.ReturnUrl = string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl;
        await this.LoadProvidersAsync();
    }

    public async Task<IActionResult> OnPostAsync(string provider, string? returnUrl)
    {
        this.ReturnUrl = string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl;
        await this.LoadProvidersAsync();

        if (this.Providers.Any(item => string.Equals(item.Name, provider, StringComparison.Ordinal)) == false)
        {
            this.ModelState.AddModelError(string.Empty, "The selected provider is not available.");
            return this.Page();
        }

        var redirectUrl = this.Url.Page("/Account/ExternalLogin/Index", pageHandler: "Callback", values: new { returnUrl = this.ReturnUrl });
        var properties = this._signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        return this.Challenge(properties, provider);
    }

    public async Task<IActionResult> OnGetCallbackAsync(string? returnUrl = null, string? remoteError = null)
    {
        this.ReturnUrl = string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl;
        await this.LoadProvidersAsync();

        if (string.IsNullOrWhiteSpace(remoteError) == false)
        {
            this.ModelState.AddModelError(string.Empty, remoteError);
            return this.Page();
        }

        var info = await this._signInManager.GetExternalLoginInfoAsync();
        if (info is null)
        {
            this.ModelState.AddModelError(string.Empty, "The external login details could not be loaded.");
            return this.Page();
        }

        var signInResult = await this._signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, false, true);
        if (signInResult.Succeeded)
        {
            return this.LocalRedirect(this.ReturnUrl);
        }

        var email = info.Principal.FindFirstValue(ClaimTypes.Email) ?? info.Principal.FindFirstValue(OpenIddict.Abstractions.OpenIddictConstants.Claims.Email);
        if (string.IsNullOrWhiteSpace(email))
        {
            this.ModelState.AddModelError(string.Empty, "The external provider did not return an email address.");
            return this.Page();
        }

        var user = await this._userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                GivenName = info.Principal.FindFirstValue(ClaimTypes.GivenName),
                FamilyName = info.Principal.FindFirstValue(ClaimTypes.Surname)
            };

            var createResult = await this._userManager.CreateAsync(user);
            if (createResult.Succeeded == false)
            {
                foreach (var error in createResult.Errors)
                {
                    this.ModelState.AddModelError(string.Empty, error.Description);
                }

                return this.Page();
            }
        }

        var addLoginResult = await this._userManager.AddLoginAsync(user, info);
        if (addLoginResult.Succeeded == false && addLoginResult.Errors.Any(error => error.Code == "LoginAlreadyAssociated") == false)
        {
            foreach (var error in addLoginResult.Errors)
            {
                this.ModelState.AddModelError(string.Empty, error.Description);
            }

            return this.Page();
        }

        await this._signInManager.SignInAsync(user, isPersistent: false, info.LoginProvider);
        return this.LocalRedirect(this.ReturnUrl);
    }

    private async Task LoadProvidersAsync()
    {
        this.Providers = (await this._signInManager.GetExternalAuthenticationSchemesAsync())
            .Select(scheme => new ExternalProviderViewModel(scheme.Name, scheme.DisplayName ?? scheme.Name))
            .OrderBy(provider => provider.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public sealed record ExternalProviderViewModel(string Name, string DisplayName);
}
