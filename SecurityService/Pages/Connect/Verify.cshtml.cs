using System.Collections.Immutable;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using SecurityService.Database;
using SecurityService.Database.DbContexts;
using SecurityService.Oidc;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace SecurityService.Pages.Connect;

public sealed class VerifyModel : PageModel
{
    private readonly IOpenIddictApplicationManager _applicationManager;
    private readonly IOpenIddictScopeManager _scopeManager;
    private readonly SecurityServiceDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;

    public VerifyModel(IOpenIddictApplicationManager applicationManager, IOpenIddictScopeManager scopeManager, SecurityServiceDbContext dbContext, UserManager<ApplicationUser> userManager)
    {
        this._applicationManager = applicationManager;
        this._scopeManager = scopeManager;
        this._dbContext = dbContext;
        this._userManager = userManager;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string ClientName { get; private set; } = string.Empty;

    public IReadOnlyCollection<string> RequestedScopes { get; private set; } = Array.Empty<string>();

    public IReadOnlyCollection<ScopeDisplayItem> IdentityScopes { get; private set; } = Array.Empty<ScopeDisplayItem>();

    public IReadOnlyCollection<ScopeDisplayItem> ApiScopes { get; private set; } = Array.Empty<ScopeDisplayItem>();

    public string StatusMessage { get; private set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        var result = await this.LoadVerificationContextAsync(cancellationToken);
        if (result.Valid == false)
        {
            this.StatusMessage = string.IsNullOrWhiteSpace(this.Input.UserCode) ? string.Empty : "The specified user code is invalid.";
            return this.Page();
        }

        if (this.User.Identity?.IsAuthenticated != true)
        {
            return this.Redirect($"/Account/Login?returnUrl={Uri.EscapeDataString(OidcHelpers.BuildCurrentRequestUrl(this.Request))}");
        }

        await this.PopulateAsync(result.AuthenticationResult!, cancellationToken);
        return this.Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (string.Equals(this.Input.Action, "lookup", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(this.Input.UserCode))
            {
                this.ModelState.AddModelError(string.Empty, "Enter the user code shown on the device.");
                return this.Page();
            }

            return this.Redirect($"/connect/verify?user_code={Uri.EscapeDataString(this.Input.UserCode)}");
        }

        var result = await this.LoadVerificationContextAsync(cancellationToken);
        if (result.Valid == false || result.AuthenticationResult is null)
        {
            this.ModelState.AddModelError(string.Empty, "The specified user code is invalid.");
            return this.Page();
        }

        if (this.User.Identity?.IsAuthenticated != true)
        {
            return this.Redirect($"/Account/Login?returnUrl={Uri.EscapeDataString(OidcHelpers.BuildCurrentRequestUrl(this.Request))}");
        }

        if (string.Equals(this.Input.Action, "deny", StringComparison.OrdinalIgnoreCase))
        {
            return this.Forbid(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        var user = await this._userManager.GetUserAsync(this.User);
        if (user is null)
        {
            return this.Redirect($"/Account/Login?returnUrl={Uri.EscapeDataString(OidcHelpers.BuildCurrentRequestUrl(this.Request))}");
        }

        var scopes = result.AuthenticationResult.Principal!.GetScopes();
        var resources = await this._scopeManager.ListResourcesAsync(ImmutableArray.CreateRange(scopes), cancellationToken).ToListAsync(cancellationToken);
        var principal = await OidcHelpers.CreatePrincipalAsync(user, this._userManager, scopes, resources, authorizationId: null);

        return this.SignIn(principal, new AuthenticationProperties { RedirectUri = "/" }, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    private async Task PopulateAsync(AuthenticateResult authenticationResult, CancellationToken cancellationToken)
    {
        this.RequestedScopes = authenticationResult.Principal!.GetScopes().ToArray();
        var scopeDisplay = await OidcHelpers.BuildScopeDisplayAsync(this.RequestedScopes, this._dbContext, cancellationToken);
        this.IdentityScopes = scopeDisplay.IdentityScopes;
        this.ApiScopes = scopeDisplay.ApiScopes;
        this.Input.UserCode = authenticationResult.Properties?.GetTokenValue(OpenIddictServerAspNetCoreConstants.Tokens.UserCode) ?? this.Request.Query["user_code"].FirstOrDefault() ?? string.Empty;

        var clientId = authenticationResult.Principal?.GetClaim(Claims.ClientId);
        if (string.IsNullOrWhiteSpace(clientId))
        {
            this.ClientName = string.Empty;
            return;
        }

        var application = await this._applicationManager.FindByClientIdAsync(clientId, cancellationToken);
        this.ClientName = application is null ? clientId : await this._applicationManager.GetDisplayNameAsync(application, cancellationToken) ?? clientId;
    }

    private async Task<(bool Valid, AuthenticateResult? AuthenticationResult)> LoadVerificationContextAsync(CancellationToken cancellationToken)
    {
        this.Input.UserCode = this.Request.Query["user_code"].FirstOrDefault() ?? this.Input.UserCode;
        var authenticationResult = await this.HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        if (authenticationResult.Succeeded == false || authenticationResult.Principal?.GetClaim(Claims.ClientId) is null)
        {
            return (false, null);
        }

        await this.PopulateAsync(authenticationResult, cancellationToken);
        return (true, authenticationResult);
    }

    public sealed class InputModel
    {
        public string UserCode { get; set; } = string.Empty;

        public string Action { get; set; } = "lookup";
    }
}
