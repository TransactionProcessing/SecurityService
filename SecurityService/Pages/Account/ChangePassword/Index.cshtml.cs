using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SecurityService.BusinessLogic.Requests;
using SecurityService.Models;
using SimpleResults;
using System.ComponentModel.DataAnnotations;

namespace SecurityService.Pages.Account.ChangePassword;

public sealed class IndexModel : PageModel
{
    private readonly IMediator _mediator;

    public ViewModel View { get; set; } = new();

    [BindProperty]
    public IndexInputModel Input { get; set; } = new();

    public IndexModel(IMediator mediator)
    {
        this._mediator = mediator;
    }

    public IActionResult OnGet(string? returnUrl)
    {
        this.BuildModel(returnUrl);
        return Page();
    }

    private const string DontMatchMessage = "New Password does not match Confirm Password";

    private const string ErrorChangingMessage = "An error occurred changing password";

    public async Task<IActionResult> OnPost(CancellationToken cancellationToken)
    {
        this.BuildModel(this.Input.ReturnUrl);

        if (string.CompareOrdinal(this.Input.NewPassword, this.Input.ConfirmPassword) != 0)
        {
            this.ModelState.AddModelError(string.Empty, DontMatchMessage);
            return this.Page();
        }

        SecurityServiceCommands.ChangeUserPasswordCommand command = new(this.Input.EmailAddress, this.Input.CurrentPassword, this.Input.NewPassword, this.Input.ClientId);
        Result<ChangeUserPasswordResult>? result = await this._mediator.Send(command, cancellationToken);

        if (result.IsFailed)
        {
            this.ModelState.AddModelError(string.Empty, ErrorChangingMessage);
            return this.Page();
        }

        return this.Redirect(result.Data.RedirectUri);
    }

    private void BuildModel(string? returnUrl)
    {
        string? queryClientId = this.Request.Query["clientId"].FirstOrDefault();

        this.View = new ViewModel();
        this.Input = new IndexInputModel
        {
            ReturnUrl = returnUrl ?? this.Input.ReturnUrl,
            ClientId = queryClientId ?? this.Input.ClientId,
            EmailAddress = this.User.Identity?.Name,
            ConfirmPassword = this.Input.ConfirmPassword,
            CurrentPassword = this.Input.CurrentPassword,
            NewPassword = this.Input.NewPassword
        };
    }
}

public class IndexInputModel
{
    public string EmailAddress { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; } = string.Empty;

    [Required]
    public string CurrentPassword { get; set; } = string.Empty;
    [Required]
    public string NewPassword { get; set; } = string.Empty;

    [Required]
    public string ConfirmPassword { get; set; } = string.Empty;

    public string Button { get; set; } = string.Empty;

    public string ClientId { get; set; } = string.Empty;
}

public class ViewModel
{
    public string EmailAddress { get; set; } = string.Empty;

    public string Token { get; set; } = string.Empty;

    public string ClientId { get; set; } = string.Empty;

    public string UserMessage { get; set; } = string.Empty;
}
