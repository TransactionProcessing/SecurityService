using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SecurityService.BusinessLogic.Requests;
using SimpleResults;

namespace SecurityService.Pages.Account.ConfirmEmail;

public sealed class IndexModel : PageModel
{
    private readonly IMediator _mediator;

    public ViewModel View { get; set; } = new();

    [BindProperty]
    public ConfirmInputModel Input { get; set; } = new();

    public IndexModel(IMediator mediator)
    {
        this._mediator = mediator;
    }

    public async Task<IActionResult> OnGet(CancellationToken cancellationToken)
    {
        this.BuildModel();

        if (string.IsNullOrWhiteSpace(this.Input.Username) || string.IsNullOrWhiteSpace(this.Input.Token))
        {
            this.View.UserMessage = $"The email confirmation link is invalid.";
            return this.Page();
        }

        SecurityServiceCommands.ConfirmUserEmailAddressCommand confirmUserEmailAddressCommand = new(this.Input.Username, this.Input.Token);
        Result? confirmUserEmailAddressResult = await this._mediator.Send(confirmUserEmailAddressCommand, cancellationToken);

        if (confirmUserEmailAddressResult.IsFailed)
        {
            this.View.UserMessage = $"Failed confirming user email address for username {this.Input.Username}";
        }
        else
        {
            this.View.UserMessage = $"Thanks for confirming your email address, you should receive a welcome email soon.";
            SecurityServiceCommands.SendWelcomeEmailCommand command = new(this.Input.Username);
            await this._mediator.Send(command, cancellationToken);
        }

        return Page();
    }

    private void BuildModel()
    {
        this.View = new ViewModel();
        this.Input = new ConfirmInputModel
        {
            Username = this.Request.Query["userName"].FirstOrDefault() ?? string.Empty,
            Token = this.Request.Query["confirmationToken"].FirstOrDefault() ?? string.Empty,
        };
    }
}

public class ViewModel
{
    public string Username { get; set; } = string.Empty;

    public string Token { get; set; } = string.Empty;

    public string UserMessage { get; set; } = string.Empty;
}

public class ConfirmInputModel
{
    public string Username { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;

}
