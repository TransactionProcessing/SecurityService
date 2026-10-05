using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SecurityService.BusinessLogic.Requests;
using SimpleResults;
using System.ComponentModel.DataAnnotations;

namespace SecurityService.Pages.Account.ActivateAccount;

public sealed class IndexModel : PageModel
{
    private readonly IMediator _mediator;

    public ViewModel View { get; private set; } = new();

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public IndexModel(IMediator mediator)
    {
        this._mediator = mediator;
    }

    public IActionResult OnGet()
    {
        this.Input = new InputModel
        {
            Username = this.Request.Query["userName"].FirstOrDefault() ?? string.Empty,
            Token = this.Request.Query["activationToken"].FirstOrDefault() ?? string.Empty
        };
        return this.Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (this.ModelState.IsValid == false || String.Equals(this.Input.Password, this.Input.ConfirmPassword, StringComparison.Ordinal) == false)
        {
            if (String.Equals(this.Input.Password, this.Input.ConfirmPassword, StringComparison.Ordinal) == false)
                this.ModelState.AddModelError(string.Empty, "Password does not match Confirm Password");
            return this.Page();
        }

        Result result = await this._mediator.Send(
            new SecurityServiceCommands.ProcessAccountActivationCommand(this.Input.Username, this.Input.Token, this.Input.Password),
            cancellationToken);

        this.View = new ViewModel
        {
            UserMessage = result.IsSuccess
                ? "Your account has been activated. You can now log in."
                : "The account activation link is invalid or has expired."
        };
        return this.Page();
    }

    public sealed class ViewModel
    {
        public string UserMessage { get; set; } = string.Empty;
    }

    public sealed class InputModel
    {
        public string Username { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
