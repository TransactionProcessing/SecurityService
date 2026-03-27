using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using SecurityService.BusinessLogic.Requests;
using SecurityService.Database;
using SecurityService.Pages.Account.ChangePassword;
using SecurityService.Services;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Web;

namespace SecurityService.Pages.Account.ForgotPassword;

public sealed class IndexModel : PageModel
{
    private readonly IMediator Mediator;

    public ViewModel View { get; set; }

    [BindProperty]
    public IndexInputModel Input { get; set; }

    public IndexModel(IMediator mediator)
    {
        this.Mediator = mediator;
    }

    public async Task<IActionResult> OnGet(string returnUrl)
    {
        await BuildModelAsync(returnUrl);

        return Page();
    }

    public async Task<IActionResult> OnPost(CancellationToken cancellationToken)
    {
        // the user clicked the "cancel" button
        if (Input.Button == "cancel")
        {
            return Redirect("Login/Index");
        }
        SecurityServiceCommands.ProcessPasswordResetRequestCommand command = new(Input.EmailAddress, Input.EmailAddress, Input.ClientId);

        await this.Mediator.Send(command, cancellationToken);

        View = new ViewModel()
        {
            UserMessage = "Password Reset sent, please check your registered email for further instructions."
        };
        return Page();
    }

    private async Task BuildModelAsync(string returnUrl)
    {
        var queryString = new Uri("https://dummy" + Request.QueryString.ToString()).Query;
        var query = QueryHelpers.ParseQuery(queryString);
        
        View = new ViewModel();

        Input = new IndexInputModel
        {
            ReturnUrl = returnUrl,
            ClientId = query["clientId"].ToString()
        };
    }
}

public class IndexInputModel
{

    public string ReturnUrl { get; set; } = string.Empty;

    [Required]
    public string EmailAddress { get; set; } = string.Empty;
    
    public string Button { get; set; } = string.Empty;

    public string ClientId { get; set; } = string.Empty;
}

public class ViewModel
{
    public string Token { get; set; } = string.Empty;

    public string ClientId { get; set; } = string.Empty;

    public string UserMessage { get; set; } = string.Empty;
}
