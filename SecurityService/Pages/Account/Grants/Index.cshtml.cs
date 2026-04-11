using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SecurityService.Database;
using SecurityService.Models;
using SecurityService.BusinessLogic.Oidc;
using SecurityService.Services;

namespace SecurityService.Pages.Account.Grants;

public sealed class IndexModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IGrantService _grantService;

    public IndexModel(UserManager<ApplicationUser> userManager, IGrantService grantService)
    {
        this._userManager = userManager;
        this._grantService = grantService;
    }

    public IReadOnlyCollection<GrantDetails> Grants { get; private set; } = Array.Empty<GrantDetails>();

    public string StatusMessage { get; private set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        var user = await this._userManager.GetUserAsync(this.User);
        if (user is null)
        {
            return this.Redirect($"/Account/Login?returnUrl={Uri.EscapeDataString(OidcHelpers.BuildCurrentRequestUrl(this.Request))}");
        }

        this.Grants = await this._grantService.GetUserGrantsAsync(user.Id, cancellationToken);
        return this.Page();
    }

    public async Task<IActionResult> OnPostRevokeAsync(string authorizationId, CancellationToken cancellationToken)
    {
        var user = await this._userManager.GetUserAsync(this.User);
        if (user is null)
        {
            return this.Redirect($"/Account/Login?returnUrl={Uri.EscapeDataString(OidcHelpers.BuildCurrentRequestUrl(this.Request))}");
        }

        var result = await this._grantService.RevokeAsync(user.Id, authorizationId, cancellationToken);
        if (result.IsSuccess == false)
        {
            this.StatusMessage = result.Message ?? "The grant could not be revoked.";
            this.Grants = await this._grantService.GetUserGrantsAsync(user.Id, cancellationToken);
            return this.Page();
        }

        return this.RedirectToPage();
    }
}
