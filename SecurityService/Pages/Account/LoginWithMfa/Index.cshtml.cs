using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SecurityService.BusinessLogic.Mfa;
using SecurityService.BusinessLogic.Requests;

namespace SecurityService.Pages.Account.LoginWithMfa;

public sealed class IndexModel : PageModel
{
    private readonly IMediator _mediator;

    public IndexModel(IMediator mediator)
    {
        _mediator = mediator;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public IActionResult OnGet(string transaction)
    {
        Input.Transaction = transaction;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var result = await _mediator.Send(new SecurityServiceCommands.CompleteMfaLoginCommand(Input.Transaction, Input.Code));
        if (result.IsSuccess && result.Data is LoginCompleted completed)
        {
            return LocalRedirect(string.IsNullOrWhiteSpace(completed.ReturnUrl) ? "/" : completed.ReturnUrl);
        }

        var rejection = result.Data as LoginRejected;
        ModelState.AddModelError(string.Empty, rejection?.Message ?? "The authenticator or recovery code was invalid.");
        return Page();
    }

    public sealed class InputModel
    {
        public string Transaction { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }
}
