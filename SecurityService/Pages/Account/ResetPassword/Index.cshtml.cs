using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using SecurityService.BusinessLogic.Requests;
using SecurityService.Database;
using SecurityService.Pages.Account.ConfirmEmail;
using SimpleResults;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Web;

namespace SecurityService.Pages.Account.ResetPassword;

public sealed class IndexModel : PageModel
{
    private const string DontMatchMessage = "Password does not match Confirm Password";

    private readonly IMediator Mediator;

    public ViewModel View { get; set; }

    [BindProperty]
    public ConfirmInputModel Input { get; set; }

    public IndexModel(IMediator mediator)
    {
        this.Mediator = mediator;
    }

    public async Task<IActionResult> OnGet()
    {
        // Process the query string and populate the view model with username (read only)
        await BuildModelAsync(Request.QueryString);

        return Page();
    }


    public async Task<IActionResult> OnPost(CancellationToken cancellationToken)
    {
        if (ModelState.IsValid == false)
        {
            return Page();
        }

        if (String.CompareOrdinal(this.Input.Password, this.Input.ConfirmPassword) != 0)
        {
            this.ModelState.AddModelError(string.Empty, DontMatchMessage);
            return this.Page();
        }

        // process the password change
        SecurityServiceCommands.ProcessPasswordResetConfirmationCommand command = new(Input.Username, Input.Token, Input.Password, Input.ClientId);
        Result<String>? result = await this.Mediator.Send(command, cancellationToken);

        if (result.IsFailed)
        {
            this.View = new ViewModel
            {
                Username = Input.Username,
                Token = Input.Token,
                UserMessage = $"Failed processing password reset for username {Input.Username}: {result.Message}"
            };
            return Page();
        }

        return this.Redirect(result.Data);
    }

    private async Task BuildModelAsync(QueryString queryString)
    {
        NameValueCollection parsedQueryString = HttpUtility.ParseQueryString(Request.QueryString.ToString());
        String userName = parsedQueryString["userName"];
        String token = parsedQueryString["resetToken"];
        String clientId = parsedQueryString["clientId"];

        View = new ViewModel
        {
            Username = userName,
            Token = token
        };

        Input = new ConfirmInputModel
        {
            Username = userName,
            Token = token,
            ClientId = clientId
        };
    }
}

public class ViewModel
{
    public string Username { get; set; }

    public string Token { get; set; }

    public String UserMessage { get; set; }
}

public class ConfirmInputModel
{
    public string Username { get; set; }
    public string Token { get; set; }

    [Required]
    public string Password { get; set; }

    [Required]
    public string ConfirmPassword { get; set; }

    public string ClientId { get; set; }
}
