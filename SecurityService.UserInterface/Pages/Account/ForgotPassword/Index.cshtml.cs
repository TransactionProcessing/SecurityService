using Duende.IdentityServer.Events;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Duende.IdentityServer.Stores;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IdentityServerHost.Pages.ForgotPassword;

using System;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using MediatR;
using SecurityService.BusinessLogic;
using SecurityService.BusinessLogic.Requests;

[SecurityHeaders]
[AllowAnonymous]
public class Index : PageModel{
    private readonly IMediator Mediator;
    
    public ViewModel View { get; set; }
        
    [BindProperty]
    public IndexInputModel Input { get; set; }
        
    public Index(IMediator mediator) {
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
        if (Input.Button == "cancel") {
            return Redirect("Login/Index");
        }
        SecurityServiceCommands.ProcessPasswordResetRequestCommand command = new(Input.Username, Input.EmailAddress, Input.ClientId);
        
        await this.Mediator.Send(command, cancellationToken);
        
        View = new ViewModel() {
                                   UserMessage = "Password Reset sent, please check your registered email for further instructions."
                               };
        return Page();
    }
        
    private async Task BuildModelAsync(string returnUrl)
    {
        NameValueCollection queryString = HttpUtility.ParseQueryString(Request.QueryString.ToString());

        View = new ViewModel();

        Input = new IndexInputModel
        {
            ReturnUrl = returnUrl,
            ClientId = queryString["clientId"]
        };
    }
}