using Duende.IdentityServer.Events;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Duende.IdentityServer.Stores;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IdentityServerHost.Pages.EmailConfirmation;

using System;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Http;
using SecurityService.BusinessLogic;

[SecurityHeaders]
[AllowAnonymous]
public class Confirm : PageModel
{
    private readonly ISecurityServiceManager SecurityServiceManager;

    private readonly UserManager<IdentityUser> UserManager;

    public ViewModel View { get; set; }
        
    [BindProperty]
    public ConfirmInputModel Input { get; set; }
        
    public Confirm(ISecurityServiceManager securityServiceManager,
                   UserManager<IdentityUser> userManager) {
        this.SecurityServiceManager = securityServiceManager;
        this.UserManager = userManager;
    }
        
    public async Task<IActionResult> OnGet(CancellationToken cancellationToken) {
        // Process the query string and populate the view model with username (read only)
        await BuildModelAsync();

        // mark the email address as confirmed
        Boolean result = await this.SecurityServiceManager.ConfirmEmailAddress(Input.Username, Input.Token, cancellationToken);

        if (result == false) {
            this.View.UserMessage = $"Failed confirming user email address for username {Input.Username}";
        }
        else {
            this.View.UserMessage = $"Thanks for confirming your email address, you should receive a welcome email soon.";
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