using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SecurityService.BusinessLogic.Oidc;
using SecurityService.BusinessLogic.Requests;
using SecurityService.Database;
using SecurityService.Models;

namespace SecurityService.Pages.Account.Grants;

public sealed class IndexModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMediator _mediator;

    public IndexModel(UserManager<ApplicationUser> userManager, IMediator mediator)
    {
        this._userManager = userManager;
        this._mediator = mediator;
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

        var result = await this._mediator.Send(new SecurityServiceQueries.GetUserGrantsQuery(user.Id), cancellationToken);
        this.Grants = result.IsSuccess ? result.Data! : Array.Empty<GrantDetails>();
        return this.Page();
    }

    public async Task<IActionResult> OnPostRevokeAsync(string authorizationId, CancellationToken cancellationToken)
    {
        var user = await this._userManager.GetUserAsync(this.User);
        if (user is null)
        {
            return this.Redirect($"/Account/Login?returnUrl={Uri.EscapeDataString(OidcHelpers.BuildCurrentRequestUrl(this.Request))}");
        }

        var result = await this._mediator.Send(new SecurityServiceCommands.RevokeGrantCommand(user.Id, authorizationId), cancellationToken);
        if (result.IsSuccess == false)
        {
            this.StatusMessage = result.Message ?? "The grant could not be revoked.";
            var grantsResult = await this._mediator.Send(new SecurityServiceQueries.GetUserGrantsQuery(user.Id), cancellationToken);
            this.Grants = grantsResult.IsSuccess ? grantsResult.Data! : Array.Empty<GrantDetails>();
            return this.Page();
        }

        return this.RedirectToPage();
    }
}

