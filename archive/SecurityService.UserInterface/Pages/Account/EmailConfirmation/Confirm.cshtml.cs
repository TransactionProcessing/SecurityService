using Duende.IdentityServer.Events;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Duende.IdentityServer.Stores;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Shared.Logger;
using SimpleResults;

namespace IdentityServerHost.Pages.EmailConfirmation;

using System;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using MediatR;
using Microsoft.AspNetCore.Http;
using SecurityService.BusinessLogic;
using SecurityService.BusinessLogic.Requests;

[SecurityHeaders]
[AllowAnonymous]
public class Confirm : PageModel
{
    private readonly IMediator Mediator;

    public ViewModel View { get; set; }
        
    [BindProperty]
    public ConfirmInputModel Input { get; set; }
        
    public Confirm(IMediator mediator){
        this.Mediator = mediator;
    }
        
    public async Task<IActionResult> OnGet(CancellationToken cancellationToken) {
        // Process the query string and populate the view model with username (read only)
        await BuildModelAsync();

        // mark the email address as confirmed
        SecurityServiceCommands.ConfirmUserEmailAddressCommand confirmUserEmailAddressCommand= new(Input.Username, Input.Token);
        Result? confirmUserEmailAddressResult = await this.Mediator.Send(confirmUserEmailAddressCommand, cancellationToken);

        if (confirmUserEmailAddressResult.IsFailed) {
            this.View.UserMessage = $"Failed confirming user email address for username {Input.Username}";
        }
        else {
            this.View.UserMessage = $"Thanks for confirming your email address, you should receive a welcome email soon.";
            // Send the welcome email 
            SecurityServiceCommands.SendWelcomeEmailCommand command = new(Input.Username);
            Result? sendWelcomeEmailResult = await this.Mediator.Send(command, cancellationToken);
            if (sendWelcomeEmailResult.IsFailed)
                Logger.LogWarning($"Error sending welcome email to {this.Input.Username} [{sendWelcomeEmailResult.Message}]");

        }
        return Page();
    }

    private async Task BuildModelAsync() {
        NameValueCollection parsedQueryString = HttpUtility.ParseQueryString(Request.QueryString.ToString());
        String userName = parsedQueryString["userName"];
        String token = parsedQueryString["confirmationToken"];
        this.View = new ViewModel();
        Input = new ConfirmInputModel {
                                          Username = userName,
                                          Token = token,
                                      };
    }
}