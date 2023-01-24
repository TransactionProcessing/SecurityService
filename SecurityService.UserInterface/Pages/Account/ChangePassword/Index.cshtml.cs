using Duende.IdentityServer.Events;
using 
    Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Duende.IdentityServer.Stores;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IdentityServerHost.Pages.ChangePassword;

using System;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Duende.IdentityServer.EntityFramework.Entities;
using MediatR;
using SecurityService.BusinessLogic;
using SecurityService.BusinessLogic.Requests;
using Shared.Logger;

[SecurityHeaders]
[AllowAnonymous]
public class Index : PageModel
{
    private readonly IMediator Mediator;
    
    public ViewModel View { get; set; }
        
    [BindProperty]
    public IndexInputModel Input { get; set; }
        
    public Index(IMediator mediator) {
        this.Mediator = mediator;
    }
        
    public async Task<IActionResult> OnGet(String returnUrl)
    {
        await BuildModelAsync(returnUrl);

        return Page();
    }

    public const String PasswordsDontMatch = "New Password does not match Confirm Password";

    public const String ErrorChangingPassword = "An error occurred changing password";

    public async Task<IActionResult> OnPost(CancellationToken cancellationToken)
    {
        await BuildModelAsync(Input.ReturnUrl);
        //// the user clicked the "cancel" button
        //if (Input.Button == "cancel") {
        //    return Redirect("Login/Index");
        //}

        if (String.CompareOrdinal(Input.NewPassword, Input.ConfirmPassword) != 0) {
            await BuildModelAsync(Input.ReturnUrl);
            ModelState.AddModelError(String.Empty, PasswordsDontMatch);
            return this.Page();
        }
        ChangeUserPasswordRequest request = ChangeUserPasswordRequest.Create(Input.Username, Input.CurrentPassword, Input.NewPassword, Input.ClientId);
        (Boolean, String) result = await this.Mediator.Send(request, cancellationToken);

        Logger.LogWarning(Input.ClientId);
        Logger.LogWarning(result.Item1.ToString());
        Logger.LogWarning(result.Item2);
        if (result.Item1 == false) {
            ModelState.AddModelError(String.Empty, ErrorChangingPassword);
            return this.Page();
        }

        return this.Redirect(result.Item2);
    }
        
    private async Task BuildModelAsync(String returnUrl)
    {
        NameValueCollection queryString = HttpUtility.ParseQueryString(Request.QueryString.ToString());

        View = new ViewModel();

        Input = new IndexInputModel
        {
            ReturnUrl = returnUrl,
            ClientId = queryString["clientId"] != null ? queryString["clientId"] : Input.ClientId,
            Username = this.User.Identity?.Name,
            ConfirmPassword = this.Input?.ConfirmPassword,
            CurrentPassword = this.Input?.CurrentPassword,
            NewPassword = this.Input?.NewPassword
        };
    }
}