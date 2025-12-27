using Duende.IdentityServer.Events;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Duende.IdentityServer.Stores;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SecurityService.BusinessLogic;

namespace IdentityServerHost.Pages.Login;

using Polly;
using System;
using System.Linq;
using System.Threading.Tasks;

[SecurityHeaders]
[AllowAnonymous]
public class Index : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IIdentityServerInteractionService _interaction;
    private readonly IEventService _events;
    private readonly IAuthenticationSchemeProvider _schemeProvider;
    private readonly IIdentityProviderStore _identityProviderStore;

    public ViewModel View { get; set; }
        
    [BindProperty]
    public InputModel Input { get; set; }
        
    public Index(
        IIdentityServerInteractionService interaction,
        IAuthenticationSchemeProvider schemeProvider,
        IIdentityProviderStore identityProviderStore,
        IEventService events,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _interaction = interaction;
        _schemeProvider = schemeProvider;
        _identityProviderStore = identityProviderStore;
        _events = events;
    }
        
    public async Task<IActionResult> OnGet(string returnUrl)
    {
        await BuildModelAsync(returnUrl);
            
        if (View.IsExternalLoginOnly)
        {
            // we only have one option for logging in and it's an external provider
            return RedirectToPage("/ExternalLogin/Challenge", new { scheme = View.ExternalLoginScheme, returnUrl });
        }

        return Page();
    }
        
    public async Task<IActionResult> OnPost()
    {
        // check if we are in the context of an authorization request
        AuthorizationRequest? context = await _interaction.GetAuthorizationContextAsync(Input.ReturnUrl);

        // the user clicked the "cancel" button
        if (Input.Button == "forgotpassword") {
            return Redirect($"ForgotPassword/Index?clientId={Input.ClientId}");
        }

        // the user clicked the "cancel" button
        if (Input.Button != "login") {
            return await HandleCancelButton(context);
        }

        if (ModelState.IsValid) {
            return await HandleLoginButton(context);
        }

        // something went wrong, show form with error
        await BuildModelAsync(Input.ReturnUrl);
        return Page();
    }

    private async Task<IActionResult> HandleCancelButton(AuthorizationRequest? context) {
        if (context != null)
        {
            // if the user cancels, send a result back into IdentityServer as if they 
            // denied the consent (even if this client does not require consent).
            // this will send back an access denied OIDC error response to the client.
            await _interaction.DenyAuthorizationAsync(context, AuthorizationError.AccessDenied);

            // we can trust model.ReturnUrl since GetAuthorizationContextAsync returned non-null
            if (context.IsNativeClient())
            {
                // The client is native, so this change in how to
                // return the response is for better UX for the end user.
                return this.LoadingPage(Input.ReturnUrl);
            }

            return Redirect(Input.ReturnUrl);
        }
        
        // since we don't have a valid context, then we just go back to the home page
        return Redirect("~/");
    }

    private async Task<IActionResult> HandleLoginButton(AuthorizationRequest? context) {
        var result = await _signInManager.PasswordSignInAsync(Input.Username, Input.Password, Input.RememberLogin, lockoutOnFailure: true);
        if (result.Succeeded) {
            var user = await _userManager.FindByNameAsync(Input.Username);
            await _events.RaiseAsync(new UserLoginSuccessEvent(user.UserName, user.Id, user.UserName, clientId: context?.Client.ClientId));

            if (context != null) {
                if (context.IsNativeClient()) {
                    // The client is native, so this change in how to
                    // return the response is for better UX for the end user.
                    return this.LoadingPage(Input.ReturnUrl);
                }

                // we can trust model.ReturnUrl since GetAuthorizationContextAsync returned non-null
                return Redirect(Input.ReturnUrl);
            }

            // request for a local page
            if (Url.IsLocalUrl(Input.ReturnUrl)) {
                return Redirect(Input.ReturnUrl);
            }

            if (string.IsNullOrEmpty(Input.ReturnUrl)) {
                return Redirect("~/");
            }

            // user might have clicked on a malicious link - should be logged
            throw new ArgumentException("invalid return URL");
        }

        await _events.RaiseAsync(new UserLoginFailureEvent(Input.Username, "invalid credentials", clientId: context?.Client.ClientId));
        ModelState.AddModelError(string.Empty, LoginOptions.InvalidCredentialsErrorMessage);

        return this.Page();
    }

    private async Task BuildModelAsync(string returnUrl)
    {
        Input = CreateInputModel(returnUrl);

        var context = await _interaction.GetAuthorizationContextAsync(returnUrl);

        if (await TryBuildSingleIdpViewAsync(context))
            return;

        var providers = await GetExternalProvidersAsync(context?.Client);

        View = CreateViewModel(context?.Client, providers);
    }

    private InputModel CreateInputModel(string returnUrl) =>
        new()
        {
            ReturnUrl = returnUrl
        };

    private async Task<bool> TryBuildSingleIdpViewAsync(AuthorizationRequest? context)
    {
        if (context?.IdP == null)
            return false;

        if (await _schemeProvider.GetSchemeAsync(context.IdP) == null)
            return false;

        var isLocal = context.IdP == Duende.IdentityServer.IdentityServerConstants.LocalIdentityProvider;

        View = new ViewModel
        {
            EnableLocalLogin = isLocal,
            ExternalProviders = isLocal
                ? Array.Empty<ViewModel.ExternalProvider>()
                : new[]
                {
                    new ViewModel.ExternalProvider
                    {
                        AuthenticationScheme = context.IdP
                    }
                }
        };

        Input.Username = context.LoginHint;

        return true;
    }
    private async Task<List<ViewModel.ExternalProvider>> GetExternalProvidersAsync(Client? client)
    {
        var providers = await GetStaticProvidersAsync();
        providers.AddRange(await GetDynamicProvidersAsync());

        return ApplyClientRestrictions(providers, client);
    }

    private async Task<List<ViewModel.ExternalProvider>> GetStaticProvidersAsync()
    {
        var schemes = await _schemeProvider.GetAllSchemesAsync();

        return schemes
            .Where(s => s.DisplayName != null)
            .Select(s => new ViewModel.ExternalProvider
            {
                AuthenticationScheme = s.Name,
                DisplayName = s.DisplayName!
            })
            .ToList();
    }

    private async Task<IEnumerable<ViewModel.ExternalProvider>> GetDynamicProvidersAsync()
    {
        var schemes = await _identityProviderStore.GetAllSchemeNamesAsync();

        return schemes
            .Where(s => s.Enabled)
            .Select(s => new ViewModel.ExternalProvider
            {
                AuthenticationScheme = s.Scheme,
                DisplayName = s.DisplayName
            });
    }

    private List<ViewModel.ExternalProvider> ApplyClientRestrictions(
        List<ViewModel.ExternalProvider> providers,
        Client? client)
    {
        if (client == null)
            return providers;

        Input.ClientId = client.ClientId;

        if (client.IdentityProviderRestrictions?.Any() == true)
        {
            providers = providers
                .Where(p => client.IdentityProviderRestrictions.Contains(p.AuthenticationScheme))
                .ToList();
        }

        return providers;
    }

    private ViewModel CreateViewModel(
        Client? client,
        IEnumerable<ViewModel.ExternalProvider> providers)
    {
        var allowLocal = client?.EnableLocalLogin ?? true;

        return new ViewModel
        {
            AllowRememberLogin = LoginOptions.AllowRememberLogin,
            EnableLocalLogin = allowLocal && LoginOptions.AllowLocalLogin,
            ExternalProviders = providers.ToArray()
        };
    }

}