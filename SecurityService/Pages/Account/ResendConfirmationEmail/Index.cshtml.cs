using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SecurityService.BusinessLogic.Requests;

namespace SecurityService.Pages.Account.ResendConfirmationEmail;

public sealed class IndexModel : PageModel
{
    private readonly IMediator Mediator;

    public IndexModel(IMediator mediator) {
        this.Mediator = mediator;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public bool EmailQueued { get; private set; }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(this.Input.EmailOrUserName))
        {
            this.ModelState.AddModelError(string.Empty, "Enter an email address or username.");
            return this.Page();
        }

        SecurityServiceCommands.ResendWelcomeEmailCommand command = new(this.Input.EmailOrUserName);
        await this.Mediator.Send(command, cancellationToken);
        


        this.EmailQueued = true;
        return this.Page();
    }

    public sealed class InputModel
    {
        public string EmailOrUserName { get; set; } = string.Empty;
    }
}
