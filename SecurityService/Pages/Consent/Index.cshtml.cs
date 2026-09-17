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

    public async Task<IActionResult> OnGetAsync(string transactionId, CancellationToken cancellationToken)
    {
        this.Input.TransactionId = transactionId;
        var result = await this._mediator.Send(new OidcCommands.ConsentGetQuery(this.HttpContext, transactionId), cancellationToken);

        return result.Data switch
        {
            ConsentGetPageResult page => this.ApplyPageResult(page),
            ConsentGetInvalidResult invalid => this.BadRequest(invalid.Message),
            _ => this.Page()
        };
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        var result = await this._mediator.Send(
            new OidcCommands.ConsentPostCommand(this.HttpContext, this.Input.TransactionId, this.Input.Button, this.Input.SelectedScopes),
            cancellationToken);

        return result.Data switch
        {
            ConsentPostRedirectResult redirect => this.Redirect(redirect.Url),
            ConsentPostPageResult page => this.HandlePageResult(page),
            ConsentPostInvalidResult invalid => this.BadRequest(invalid.Message),
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
        public string TransactionId { get; set; } = string.Empty;
        public List<string> SelectedScopes { get; set; } = new();
        public string Button { get; set; } = "accept";
    }
}

