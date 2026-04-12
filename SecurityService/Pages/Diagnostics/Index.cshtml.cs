using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SecurityService.BusinessLogic.Oidc;

namespace SecurityService.Pages.Diagnostics;

public sealed class IndexModel : PageModel
{
    private readonly IMediator _mediator;

    public IndexModel(IMediator mediator)
    {
        this._mediator = mediator;
    }

    public IReadOnlyCollection<DiagnosticItem> Claims { get; private set; } = Array.Empty<DiagnosticItem>();

    public IReadOnlyCollection<DiagnosticItem> Properties { get; private set; } = Array.Empty<DiagnosticItem>();

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        var result = await this._mediator.Send(new OidcCommands.DiagnosticsQuery(this.HttpContext), cancellationToken);

        return result.Data switch
        {
            DiagnosticsNotFoundResult => this.NotFound(),
            DiagnosticsChallengeResult challenge => this.Challenge(challenge.AuthenticationScheme),
            DiagnosticsPageResult page => this.ApplyPageResult(page),
            _ => this.Page()
        };
    }

    private IActionResult ApplyPageResult(DiagnosticsPageResult page)
    {
        this.Claims = page.Claims;
        this.Properties = page.Properties;
        return this.Page();
    }
}

