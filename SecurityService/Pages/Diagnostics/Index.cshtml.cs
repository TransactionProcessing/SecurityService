using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SecurityService.BusinessLogic.Oidc;
using TransactionProcessing.UnionResults;

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

        return result.Value switch
        {
            Success<DiagnosticsQueryResult> { Value: DiagnosticsNotFoundResult } => this.NotFound(),
            Success<DiagnosticsQueryResult> { Value: DiagnosticsChallengeResult challenge } => this.Challenge(challenge.AuthenticationScheme),
            Success<DiagnosticsQueryResult> { Value: DiagnosticsPageResult page } => this.ApplyPageResult(page),
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

