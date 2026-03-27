using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpenIddict.Abstractions;
using SecurityService.Database.DbContexts;
using SecurityService.Oidc;

namespace SecurityService.Pages.Consent;

public sealed class IndexModel : PageModel
{
    private readonly SecurityServiceDbContext _dbContext;
    private readonly IOpenIddictApplicationManager _applicationManager;

    public IndexModel(SecurityServiceDbContext dbContext, IOpenIddictApplicationManager applicationManager)
    {
        this._dbContext = dbContext;
        this._applicationManager = applicationManager;
    }

    public string ClientName { get; private set; } = string.Empty;
    public IReadOnlyCollection<ScopeDisplayItem> IdentityScopes { get; private set; } = Array.Empty<ScopeDisplayItem>();
    public IReadOnlyCollection<ScopeDisplayItem> ApiScopes { get; private set; } = Array.Empty<ScopeDisplayItem>();

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(string returnUrl, CancellationToken cancellationToken)
    {
        this.Input.ReturnUrl = returnUrl;
        var request = this.HttpContext.GetOpenIddictServerRequest();
        if (request is null)
        {
            return this.LocalRedirect(returnUrl);
        }

        var application = await this._applicationManager.FindByClientIdAsync(request.ClientId!, cancellationToken);
        this.ClientName = application is null ? request.ClientId! : await this._applicationManager.GetDisplayNameAsync(application, cancellationToken) ?? request.ClientId!;
        var scopes = await OidcHelpers.BuildScopeDisplayAsync(request, this._dbContext, cancellationToken);
        this.IdentityScopes = scopes.IdentityScopes;
        this.ApiScopes = scopes.ApiScopes;
        return this.Page();
    }

    public IActionResult OnPost()
    {
        if (string.Equals(this.Input.Button, "deny", StringComparison.OrdinalIgnoreCase))
        {
            return this.Redirect(OidcHelpers.AppendQueryValue(this.Input.ReturnUrl, "consent", "denied"));
        }

        if (this.Input.SelectedScopes.Count == 0)
        {
            this.ModelState.AddModelError(string.Empty, "Select at least one scope to continue.");
            return this.Page();
        }

        var redirectUrl = OidcHelpers.AppendQueryValue(this.Input.ReturnUrl, "consent", "accepted");
        redirectUrl = OidcHelpers.AppendQueryValues(redirectUrl, "granted_scope", this.Input.SelectedScopes);
        return this.Redirect(redirectUrl);
    }

    public sealed class InputModel
    {
        public string ReturnUrl { get; set; } = "/";
        public List<string> SelectedScopes { get; set; } = new();
        public string Button { get; set; } = "accept";
    }
}
