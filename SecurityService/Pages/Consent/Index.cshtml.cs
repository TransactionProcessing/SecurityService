using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SecurityService.BusinessLogic.Oidc;

namespace SecurityService.Pages.Consent;

public sealed class IndexModel : PageModel
{
    private readonly IMediator _mediator;

    public IndexModel(IMediator mediator)
    {
        this._mediator = mediator;
    }

    public string ClientName { get; private set; } = string.Empty;
    public IReadOnlyCollection<ScopeDisplayItem> IdentityScopes { get; private set; } = Array.Empty<ScopeDisplayItem>();
    public IReadOnlyCollection<ScopeDisplayItem> ApiScopes { get; private set; } = Array.Empty<ScopeDisplayItem>();

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(string returnUrl, CancellationToken cancellationToken)
    {
        this.Input.ReturnUrl = returnUrl;
        var result = await this._mediator.Send(new OidcCommands.ConsentGetQuery(this.HttpContext, returnUrl), cancellationToken);

        return result.Data switch
        {
            ConsentGetLocalRedirectResult redirect => this.LocalRedirect(redirect.Url),
            ConsentGetPageResult page => this.ApplyPageResult(page),
            _ => this.Page()
        };
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        var result = await this._mediator.Send(
            new OidcCommands.ConsentPostCommand(this.Input.ReturnUrl, this.Input.Button, this.Input.SelectedScopes),
            cancellationToken);

        return result.Data switch
        {
            ConsentPostRedirectResult redirect => this.Redirect(redirect.Url),
            ConsentPostPageResult page => this.HandlePageResult(page),
            _ => this.Page()
        };
    }

    private IActionResult ApplyPageResult(ConsentGetPageResult page)
    {
        this.ClientName = page.ClientName;
        this.IdentityScopes = page.IdentityScopes;
        this.ApiScopes = page.ApiScopes;
        return this.Page();
    }

    private IActionResult HandlePageResult(ConsentPostPageResult page)
    {
        this.ModelState.AddModelError(string.Empty, page.ModelError);
        return this.Page();
    }

    public sealed class InputModel
    {
        public string ReturnUrl { get; set; } = "/";
        public List<string> SelectedScopes { get; set; } = new();
        public string Button { get; set; } = "accept";
    }
}

