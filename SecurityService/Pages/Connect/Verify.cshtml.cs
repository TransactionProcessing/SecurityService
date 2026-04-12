using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SecurityService.BusinessLogic.Oidc;

namespace SecurityService.Pages.Connect;

public sealed class VerifyModel : PageModel
{
    private readonly IMediator _mediator;

    public VerifyModel(IMediator mediator)
    {
        this._mediator = mediator;
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
        var result = await this._mediator.Send(new OidcCommands.VerifyGetQuery(this.HttpContext), cancellationToken);

        if (result.Data is VerifyGetRedirectResult redirect)
        {
            return this.Redirect(redirect.Url);
        }

        if (result.Data is VerifyGetPageResult page)
        {
            this.StatusMessage = page.StatusMessage;
            this.ApplyDisplayData(page.Data);
        }

        return this.Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        var result = await this._mediator.Send(new OidcCommands.VerifyPostCommand(this.HttpContext, this.Input.Action, this.Input.UserCode), cancellationToken);

        return result.Data switch
        {
            VerifyPostRedirectResult redirect => this.Redirect(redirect.Url),
            VerifyPostForbidResult forbid => this.Forbid(forbid.AuthenticationScheme),
            VerifyPostSignInResult signIn => this.SignIn(signIn.Principal, signIn.Properties, signIn.AuthenticationScheme),
            VerifyPostPageResult page => this.HandlePageResult(page),
            _ => this.Page()
        };
    }

    private IActionResult HandlePageResult(VerifyPostPageResult page)
    {
        if (page.ModelError is not null)
        {
            this.ModelState.AddModelError(string.Empty, page.ModelError);
        }

        this.ApplyDisplayData(page.Data);
        return this.Page();
    }

    private void ApplyDisplayData(VerifyDisplayData? data)
    {
        if (data is null)
        {
            return;
        }

        this.ClientName = data.ClientName;
        this.RequestedScopes = data.RequestedScopes;
        this.IdentityScopes = data.IdentityScopes;
        this.ApiScopes = data.ApiScopes;
        this.Input.UserCode = data.UserCode;
    }

    public sealed class InputModel
    {
        public string UserCode { get; set; } = string.Empty;

        public string Action { get; set; } = "lookup";
    }
}

