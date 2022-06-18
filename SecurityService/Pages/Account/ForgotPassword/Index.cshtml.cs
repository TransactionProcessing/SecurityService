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
using SecurityService.BusinessLogic;

[SecurityHeaders]
[AllowAnonymous]
public class Index : PageModel
{
    private readonly ISecurityServiceManager SecurityServiceManager;
    
    public ViewModel View { get; set; }
        
    [BindProperty]
    public IndexInputModel Input { get; set; }
        
    public Index(ISecurityServiceManager securityServiceManager) {
        this.SecurityServiceManager = securityServiceManager;
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

        await this.SecurityServiceManager.ProcessPasswordResetRequest(Input.Username, Input.EmailAddress, Input.ClientId, cancellationToken);

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

        //var context = await _interaction.GetAuthorizationContextAsync(returnUrl);
        //if (context?.IdP != null && await _schemeProvider.GetSchemeAsync(context.IdP) != null)
        //{
        //    var local = context.IdP == Duende.IdentityServer.IdentityServerConstants.LocalIdentityProvider;

        //    // this is meant to short circuit the UI and only trigger the one external IdP
        //    View = new ViewModel
        //    {
        //        EnableLocalLogin = local,
        //    };

        //    Input.Username = context?.LoginHint;

        //    if (!local)
        //    {
        //        View.ExternalProviders = new[] { new ViewModel.ExternalProvider { AuthenticationScheme = context.IdP } };
        //    }

        //    return;
        //}

        //var schemes = await _schemeProvider.GetAllSchemesAsync();

        //var providers = schemes
        //    .Where(x => x.DisplayName != null)
        //    .Select(x => new ViewModel.ExternalProvider
        //    {
        //        DisplayName = x.DisplayName ?? x.Name,
        //        AuthenticationScheme = x.Name
        //    }).ToList();

        //var dyanmicSchemes = (await _identityProviderStore.GetAllSchemeNamesAsync())
        //    .Where(x => x.Enabled)
        //    .Select(x => new ViewModel.ExternalProvider
        //    {
        //        AuthenticationScheme = x.Scheme,
        //        DisplayName = x.DisplayName
        //    });
        //providers.AddRange(dyanmicSchemes);


        //var allowLocal = true;
        //var client = context?.Client;
        //if (client != null)
        //{
        //    allowLocal = client.EnableLocalLogin;
        //    if (client.IdentityProviderRestrictions != null && client.IdentityProviderRestrictions.Any())
        //    {
        //        providers = providers.Where(provider => client.IdentityProviderRestrictions.Contains(provider.AuthenticationScheme)).ToList();
        //    }
        //}

        //View = new ViewModel
        //{
        //    AllowRememberLogin = LoginOptions.AllowRememberLogin,
        //    EnableLocalLogin = allowLocal && LoginOptions.AllowLocalLogin,
        //    ExternalProviders = providers.ToArray()
        //};
    }
}