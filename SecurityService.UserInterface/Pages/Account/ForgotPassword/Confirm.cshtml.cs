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
        
    public async Task<IActionResult> OnGet(string returnUrl) {
        // Process the query string and populate the view model with username (read only)
        await BuildModelAsync(returnUrl, Request.QueryString);

        return Page();
    }


    public async Task<IActionResult> OnPost(CancellationToken cancellationToken)
    {
        // the user clicked the "cancel" button
        if (Input.Button == "cancel")
        {
            return Redirect("Login/Index");
        }

        // TODO: Check passwords match etc here

        if (ModelState.IsValid) {
            // process the password change
            ProcessPasswordResetConfirmationRequest request = ProcessPasswordResetConfirmationRequest.Create(Input.Username, Input.Token, Input.Password, Input.ClientId);
            String redirect = await this.Mediator.Send(request, cancellationToken);
            return this.Redirect(redirect);
        }

        return Page();
    }

    private async Task BuildModelAsync(string returnUrl, QueryString queryString)
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