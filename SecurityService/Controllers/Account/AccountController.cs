namespace SecurityService.Controllers.Account
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using IdentityModel;
    using IdentityServer4;
    using IdentityServer4.Events;
    using IdentityServer4.Extensions;
    using IdentityServer4.Models;
    using IdentityServer4.Services;
    using IdentityServer4.Stores;
    using Manager;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Models;
    using Shared.Exceptions;
    using ViewModels;

    /// <summary>
    /// This sample controller implements a typical login/logout/provision workflow for local and external accounts.
    /// The login service encapsulates the interactions with the user data store. This data store is in-memory only and cannot be used for production!
    /// The interaction service provides a way for the UI to communicate with identityserver for validation and context retrieval
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.Controller" />
    [SecurityHeaders]
    [AllowAnonymous]
    [ExcludeFromCodeCoverage]
    public class AccountController : Controller
    {
        #region Fields

        /// <summary>
        /// The client store
        /// </summary>
        private readonly IClientStore ClientStore;

        /// <summary>
        /// The events
        /// </summary>
        private readonly IEventService Events;

        /// <summary>
        /// The interaction
        /// </summary>
        private readonly IIdentityServerInteractionService InteractionService;

        /// <summary>
        /// The scheme provider
        /// </summary>
        private readonly IAuthenticationSchemeProvider SchemeProvider;

        /// <summary>
        /// The manager
        /// </summary>
        private readonly ISecurityServiceManager Manager;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountController" /> class.
        /// </summary>
        /// <param name="interactionService">The interaction service.</param>
        /// <param name="clientStore">The client store.</param>
        /// <param name="schemeProvider">The scheme provider.</param>
        /// <param name="events">The events.</param>
        /// <param name="manager">The manager.</param>
        public AccountController(IIdentityServerInteractionService interactionService,
                                 IClientStore clientStore,
                                 IAuthenticationSchemeProvider schemeProvider,
                                 IEventService events,
                                 ISecurityServiceManager manager)
        {
            this.InteractionService = interactionService;
            this.ClientStore = clientStore;
            this.SchemeProvider = schemeProvider;
            this.Events = events;
            this.Manager = manager;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Accesses the denied.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return this.View();
        }

        /// <summary>
        /// Entry point into the login workflow
        /// </summary>
        /// <param name="returnUrl">The return URL.</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Login(String returnUrl)
        {
            // build a model so we know what to show on the login page
            LoginViewModel vm = await this.BuildLoginViewModelAsync(returnUrl);

            if (vm.IsExternalLoginOnly)
            {
                // we only have one option for logging in and it's an external provider
                return this.RedirectToAction("Challenge",
                                             "External",
                                             new
                                             {
                                                 provider = vm.ExternalLoginScheme,
                                                 returnUrl
                                             });
            }

            return this.View(vm);
        }

        /// <summary>
        /// Handle postback from username/password login
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="button">The button.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="Exception">invalid return URL</exception>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginInputModel model,
                                               String button,
                                               CancellationToken cancellationToken)
        {
            // check if we are in the context of an authorization request
            AuthorizationRequest context = await this.InteractionService.GetAuthorizationContextAsync(model.ReturnUrl);

            // the user clicked the "cancel" button
            if (button != "login")
            {
                if (context != null)
                {
                    // if the user cancels, send a result back into IdentityServer as if they 
                    // denied the consent (even if this client does not require consent).
                    // this will send back an access denied OIDC error response to the client.
                    await this.InteractionService.GrantConsentAsync(context, ConsentResponse.Denied);

                    // we can trust model.ReturnUrl since GetAuthorizationContextAsync returned non-null
                    if (await this.ClientStore.IsPkceClientAsync(context.ClientId))
                    {
                        // if the client is PKCE then we assume it's native, so this change in how to
                        // return the response is for better UX for the end user.
                        return this.View("Redirect",
                                         new RedirectViewModel
                                         {
                                             RedirectUrl = model.ReturnUrl
                                         });
                    }

                    return this.Redirect(model.ReturnUrl);
                }

                // since we don't have a valid context, then we just go back to the home page
                return this.Redirect("~/");
            }

            if (this.ModelState.IsValid)
            {
                // validate username/password against in-memory store
                if (await this.Manager.ValidateCredentials(model.Username, model.Password, cancellationToken))
                {
                    List<UserDetails> userList = await this.Manager.GetUsers(model.Username, cancellationToken);

                    if (userList == null || userList.Any() == false || userList.Count != 1)
                    {
                        throw new NotFoundException($"User not found with User Name [{model.Username}]");
                    }

                    UserDetails user = userList.Single();

                    await this.Events.RaiseAsync(new UserLoginSuccessEvent(user.UserName, user.UserId.ToString(), user.UserName, clientId:context?.ClientId));

                    // only set explicit expiration here if user chooses "remember me". 
                    // otherwise we rely upon expiration configured in cookie middleware.
                    AuthenticationProperties props = null;
                    if (AccountOptions.AllowRememberLogin && model.RememberLogin)
                    {
                        props = new AuthenticationProperties
                                {
                                    IsPersistent = true,
                                    ExpiresUtc = DateTimeOffset.UtcNow.Add(AccountOptions.RememberMeLoginDuration)
                                };
                    }

                    ;

                    // issue authentication cookie with subject ID and username
                    await this.HttpContext.SignInAsync(user.UserId.ToString(), user.UserName, props);

                    if (context != null)
                    {
                        if (await this.ClientStore.IsPkceClientAsync(context.ClientId))
                        {
                            // if the client is PKCE then we assume it's native, so this change in how to
                            // return the response is for better UX for the end user.
                            return this.View("Redirect",
                                             new RedirectViewModel
                                             {
                                                 RedirectUrl = model.ReturnUrl
                                             });
                        }

                        // we can trust model.ReturnUrl since GetAuthorizationContextAsync returned non-null
                        return this.Redirect(model.ReturnUrl);
                    }

                    // request for a local page
                    if (this.Url.IsLocalUrl(model.ReturnUrl))
                    {
                        return this.Redirect(model.ReturnUrl);
                    }

                    if (string.IsNullOrEmpty(model.ReturnUrl))
                    {
                        return this.Redirect("~/");
                    }

                    // user might have clicked on a malicious link - should be logged
                    throw new Exception("invalid return URL");
                }

                await this.Events.RaiseAsync(new UserLoginFailureEvent(model.Username, "invalid credentials", clientId:context?.ClientId));
                this.ModelState.AddModelError(string.Empty, AccountOptions.InvalidCredentialsErrorMessage);
            }

            // something went wrong, show form with error
            LoginViewModel vm = await this.BuildLoginViewModelAsync(model);
            return this.View(vm);
        }

        /// <summary>
        /// Show logout page
        /// </summary>
        /// <param name="logoutId">The logout identifier.</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Logout(String logoutId)
        {
            // build a model so the logout page knows what to display
            LogoutViewModel vm = await this.BuildLogoutViewModelAsync(logoutId);

            if (vm.ShowLogoutPrompt == false)
            {
                // if the request for logout was properly authenticated from IdentityServer, then
                // we don't need to show the prompt and can just log the user out directly.
                return await this.Logout(vm);
            }

            return this.View(vm);
        }

        /// <summary>
        /// Handle logout page postback
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout(LogoutInputModel model)
        {
            // build a model so the logged out page knows what to display
            LoggedOutViewModel vm = await this.BuildLoggedOutViewModelAsync(model.LogoutId);

            if (this.User?.Identity.IsAuthenticated == true)
            {
                // delete local authentication cookie
                await this.Manager.Signout();

                // raise the logout event
                await this.Events.RaiseAsync(new UserLogoutSuccessEvent(this.User.GetSubjectId(), this.User.GetDisplayName()));
            }

            // check if we need to trigger sign-out at an upstream identity provider
            if (vm.TriggerExternalSignout)
            {
                // build a return URL so the upstream provider will redirect back
                // to us after the user has logged out. this allows us to then
                // complete our single sign-out processing.
                String url = this.Url.Action("Logout",
                                             new
                                             {
                                                 logoutId = vm.LogoutId
                                             });

                // this triggers a redirect to the external provider for sign-out
                return this.SignOut(new AuthenticationProperties
                                    {
                                        RedirectUri = url
                                    },
                                    vm.ExternalAuthenticationScheme);
            }

            return this.View("LoggedOut", vm);
        }

        /// <summary>
        /// Builds the logged out view model asynchronous.
        /// </summary>
        /// <param name="logoutId">The logout identifier.</param>
        /// <returns></returns>
        private async Task<LoggedOutViewModel> BuildLoggedOutViewModelAsync(String logoutId)
        {
            // get context information (client name, post logout redirect URI and iframe for federated signout)
            LogoutRequest logout = await this.InteractionService.GetLogoutContextAsync(logoutId);

            LoggedOutViewModel vm = new LoggedOutViewModel
                                    {
                                        AutomaticRedirectAfterSignOut = AccountOptions.AutomaticRedirectAfterSignOut,
                                        PostLogoutRedirectUri = logout?.PostLogoutRedirectUri,
                                        ClientName = string.IsNullOrEmpty(logout?.ClientName) ? logout?.ClientId : logout?.ClientName,
                                        SignOutIframeUrl = logout?.SignOutIFrameUrl,
                                        LogoutId = logoutId
                                    };

            if (this.User?.Identity.IsAuthenticated == true)
            {
                String idp = this.User.FindFirst(JwtClaimTypes.IdentityProvider)?.Value;
                if (idp != null && idp != IdentityServerConstants.LocalIdentityProvider)
                {
                    Boolean providerSupportsSignout = await this.HttpContext.GetSchemeSupportsSignOutAsync(idp);
                    if (providerSupportsSignout)
                    {
                        if (vm.LogoutId == null)
                        {
                            // if there's no current logout context, we need to create one
                            // this captures necessary info from the current logged in user
                            // before we signout and redirect away to the external IdP for signout
                            vm.LogoutId = await this.InteractionService.CreateLogoutContextAsync();
                        }

                        vm.ExternalAuthenticationScheme = idp;
                    }
                }
            }

            return vm;
        }

        /*****************************************/
        /* helper APIs for the AccountController */
        /*****************************************/
        /// <summary>
        /// Builds the login view model asynchronous.
        /// </summary>
        /// <param name="returnUrl">The return URL.</param>
        /// <returns></returns>
        private async Task<LoginViewModel> BuildLoginViewModelAsync(String returnUrl)
        {
            AuthorizationRequest context = await this.InteractionService.GetAuthorizationContextAsync(returnUrl);
            if (context?.IdP != null && await this.SchemeProvider.GetSchemeAsync(context.IdP) != null)
            {
                Boolean local = context.IdP == IdentityServerConstants.LocalIdentityProvider;

                // this is meant to short circuit the UI and only trigger the one external IdP
                LoginViewModel vm = new LoginViewModel
                                    {
                                        EnableLocalLogin = local,
                                        ReturnUrl = returnUrl,
                                        Username = context?.LoginHint
                                    };

                if (!local)
                {
                    vm.ExternalProviders = new[]
                                           {
                                               new ExternalProvider
                                               {
                                                   AuthenticationScheme = context.IdP
                                               }
                                           };
                }

                return vm;
            }

            IEnumerable<AuthenticationScheme> schemes = await this.SchemeProvider.GetAllSchemesAsync();

            List<ExternalProvider> providers = schemes
                                               .Where(x => x.DisplayName != null ||
                                                           (x.Name.Equals(AccountOptions.WindowsAuthenticationSchemeName, StringComparison.OrdinalIgnoreCase)))
                                               .Select(x => new ExternalProvider
                                                            {
                                                                DisplayName = x.DisplayName,
                                                                AuthenticationScheme = x.Name
                                                            }).ToList();

            Boolean allowLocal = true;
            if (context?.ClientId != null)
            {
                Client client = await this.ClientStore.FindEnabledClientByIdAsync(context.ClientId);
                if (client != null)
                {
                    allowLocal = client.EnableLocalLogin;

                    if (client.IdentityProviderRestrictions != null && client.IdentityProviderRestrictions.Any())
                    {
                        providers = providers.Where(provider => client.IdentityProviderRestrictions.Contains(provider.AuthenticationScheme)).ToList();
                    }
                }
            }

            return new LoginViewModel
                   {
                       AllowRememberLogin = AccountOptions.AllowRememberLogin,
                       EnableLocalLogin = allowLocal && AccountOptions.AllowLocalLogin,
                       ReturnUrl = returnUrl,
                       Username = context?.LoginHint,
                       ExternalProviders = providers.ToArray()
                   };
        }

        /// <summary>
        /// Builds the login view model asynchronous.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        private async Task<LoginViewModel> BuildLoginViewModelAsync(LoginInputModel model)
        {
            LoginViewModel vm = await this.BuildLoginViewModelAsync(model.ReturnUrl);
            vm.Username = model.Username;
            vm.RememberLogin = model.RememberLogin;
            return vm;
        }

        /// <summary>
        /// Builds the logout view model asynchronous.
        /// </summary>
        /// <param name="logoutId">The logout identifier.</param>
        /// <returns></returns>
        private async Task<LogoutViewModel> BuildLogoutViewModelAsync(String logoutId)
        {
            LogoutViewModel vm = new LogoutViewModel
                                 {
                                     LogoutId = logoutId,
                                     ShowLogoutPrompt = AccountOptions.ShowLogoutPrompt
                                 };

            if (this.User?.Identity.IsAuthenticated != true)
            {
                // if the user is not authenticated, then just show logged out page
                vm.ShowLogoutPrompt = false;
                return vm;
            }

            LogoutRequest context = await this.InteractionService.GetLogoutContextAsync(logoutId);
            if (context?.ShowSignoutPrompt == false)
            {
                // it's safe to automatically sign-out
                vm.ShowLogoutPrompt = false;
                return vm;
            }

            // show the logout prompt. this prevents attacks where the user
            // is automatically signed out by another malicious web page.
            return vm;
        }

        #endregion
    }
}