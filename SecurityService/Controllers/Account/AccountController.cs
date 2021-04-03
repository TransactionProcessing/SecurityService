namespace SecurityService.Controllers.Account
{
    using System;
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
    [Route(AccountController.ControllerRoute)]
    [ExcludeFromCodeCoverage]
    [SecurityHeaders]
    [AllowAnonymous]
    public class AccountController : Controller
    {
        #region Fields

        /// <summary>
        /// The client store
        /// </summary>
        private readonly IClientStore _clientStore;

        /// <summary>
        /// The events
        /// </summary>
        private readonly IEventService _events;

        /// <summary>
        /// The interaction
        /// </summary>
        private readonly IIdentityServerInteractionService _interaction;

        /// <summary>
        /// The scheme provider
        /// </summary>
        private readonly IAuthenticationSchemeProvider _schemeProvider;

        /// <summary>
        /// The security service manager
        /// </summary>
        private readonly ISecurityServiceManager _securityServiceManager;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountController"/> class.
        /// </summary>
        /// <param name="interaction">The interaction.</param>
        /// <param name="clientStore">The client store.</param>
        /// <param name="schemeProvider">The scheme provider.</param>
        /// <param name="events">The events.</param>
        /// <param name="securityServiceManager">The security service manager.</param>
        public AccountController(IIdentityServerInteractionService interaction,
                                 IClientStore clientStore,
                                 IAuthenticationSchemeProvider schemeProvider,
                                 IEventService events,
                                 ISecurityServiceManager securityServiceManager)
        {
            this._interaction = interaction;
            this._clientStore = clientStore;
            this._schemeProvider = schemeProvider;
            this._events = events;
            this._securityServiceManager = securityServiceManager;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Accesses the denied.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("accessdenied")]
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
        [Route("login")]
        public async Task<IActionResult> Login(String returnUrl)
        {
            // build a model so we know what to show on the login page
            var vm = await this.BuildLoginViewModelAsync(returnUrl);

            if (vm.IsExternalLoginOnly)
            {
                // we only have one option for logging in and it's an external provider
                return this.RedirectToAction("Challenge",
                                             "External",
                                             new
                                             {
                                                 scheme = vm.ExternalLoginScheme,
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
        /// <exception cref="NotFoundException">No user found with Username [{model.Username}]</exception>
        /// <exception cref="Exception">invalid return URL</exception>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("login")]
        public async Task<IActionResult> Login(LoginInputModel model,
                                               String button,
                                               CancellationToken cancellationToken)
        {
            // check if we are in the context of an authorization request
            var context = await this._interaction.GetAuthorizationContextAsync(model.ReturnUrl);

            // the user clicked the "cancel" button
            if (button != "login")
            {
                if (context != null)
                {
                    // if the user cancels, send a result back into IdentityServer as if they 
                    // denied the consent (even if this client does not require consent).
                    // this will send back an access denied OIDC error response to the client.
                    await this._interaction.DenyAuthorizationAsync(context, AuthorizationError.AccessDenied);

                    // we can trust model.ReturnUrl since GetAuthorizationContextAsync returned non-null
                    if (context.IsNativeClient())
                    {
                        // The client is native, so this change in how to
                        // return the response is for better UX for the end user.
                        return this.LoadingPage("Redirect", model.ReturnUrl);
                    }

                    return this.Redirect(model.ReturnUrl);
                }

                // since we don't have a valid context, then we just go back to the home page
                return this.Redirect("~/");
            }

            if (this.ModelState.IsValid)
            {
                // validate username/password against in-memory store
                if (await this._securityServiceManager.ValidateCredentials(model.Username, model.Password, cancellationToken))
                {
                    var userList = await this._securityServiceManager.GetUsers(model.Username, cancellationToken);

                    var user = userList.SingleOrDefault();

                    if (user == null)
                    {
                        throw new NotFoundException($"No user found with Username [{model.Username}]");
                    }

                    await this._events.RaiseAsync(new UserLoginSuccessEvent(user.Username, user.SubjectId, user.Username, clientId:context?.Client.ClientId));

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
                    var isuser = new IdentityServerUser(user.SubjectId)
                                 {
                                     DisplayName = user.Username
                                 };

                    await this.HttpContext.SignInAsync(isuser, props);

                    if (context != null)
                    {
                        if (context.IsNativeClient())
                        {
                            // The client is native, so this change in how to
                            // return the response is for better UX for the end user.
                            return this.LoadingPage("Redirect", model.ReturnUrl);
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

                await this._events.RaiseAsync(new UserLoginFailureEvent(model.Username, "invalid credentials", clientId:context?.Client.ClientId));
                this.ModelState.AddModelError(string.Empty, AccountOptions.InvalidCredentialsErrorMessage);
            }

            // something went wrong, show form with error
            var vm = await this.BuildLoginViewModelAsync(model);
            return this.View(vm);
        }

        /// <summary>
        /// Show logout page
        /// </summary>
        /// <param name="logoutId">The logout identifier.</param>
        /// <returns></returns>
        [HttpGet]
        [Route("logout")]
        public async Task<IActionResult> Logout(String logoutId)
        {
            // build a model so the logout page knows what to display
            var vm = await this.BuildLogoutViewModelAsync(logoutId);

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
        [Route("logout")]
        public async Task<IActionResult> Logout(LogoutInputModel model)
        {
            // build a model so the logged out page knows what to display
            var vm = await this.BuildLoggedOutViewModelAsync(model.LogoutId);

            if (this.User?.Identity.IsAuthenticated == true)
            {
                // delete local authentication cookie
                await this.HttpContext.SignOutAsync();

                // raise the logout event
                await this._events.RaiseAsync(new UserLogoutSuccessEvent(this.User.GetSubjectId(), this.User.GetDisplayName()));
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
            var logout = await this._interaction.GetLogoutContextAsync(logoutId);

            var vm = new LoggedOutViewModel
                     {
                         AutomaticRedirectAfterSignOut = AccountOptions.AutomaticRedirectAfterSignOut,
                         PostLogoutRedirectUri = logout?.PostLogoutRedirectUri,
                         ClientName = string.IsNullOrEmpty(logout?.ClientName) ? logout?.ClientId : logout?.ClientName,
                         SignOutIframeUrl = logout?.SignOutIFrameUrl,
                         LogoutId = logoutId
                     };

            if (this.User?.Identity.IsAuthenticated == true)
            {
                var idp = this.User.FindFirst(JwtClaimTypes.IdentityProvider)?.Value;
                if (idp != null && idp != IdentityServerConstants.LocalIdentityProvider)
                {
                    var providerSupportsSignout = await this.HttpContext.GetSchemeSupportsSignOutAsync(idp);
                    if (providerSupportsSignout)
                    {
                        if (vm.LogoutId == null)
                        {
                            // if there's no current logout context, we need to create one
                            // this captures necessary info from the current logged in user
                            // before we signout and redirect away to the external IdP for signout
                            vm.LogoutId = await this._interaction.CreateLogoutContextAsync();
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
            var context = await this._interaction.GetAuthorizationContextAsync(returnUrl);
            if (context?.IdP != null && await this._schemeProvider.GetSchemeAsync(context.IdP) != null)
            {
                var local = context.IdP == IdentityServerConstants.LocalIdentityProvider;

                // this is meant to short circuit the UI and only trigger the one external IdP
                var vm = new LoginViewModel
                         {
                             EnableLocalLogin = local,
                             ReturnUrl = returnUrl,
                             Username = context?.LoginHint,
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

            var schemes = await this._schemeProvider.GetAllSchemesAsync();

            var providers = schemes.Where(x => x.DisplayName != null).Select(x => new ExternalProvider
                                                                                  {
                                                                                      DisplayName = x.DisplayName ?? x.Name,
                                                                                      AuthenticationScheme = x.Name
                                                                                  }).ToList();

            var allowLocal = true;
            if (context?.Client.ClientId != null)
            {
                var client = await this._clientStore.FindEnabledClientByIdAsync(context.Client.ClientId);
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
            var vm = await this.BuildLoginViewModelAsync(model.ReturnUrl);
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
            var vm = new LogoutViewModel
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

            var context = await this._interaction.GetLogoutContextAsync(logoutId);
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

        #region Others

        /// <summary>
        /// The controller name
        /// </summary>
        public const String ControllerName = "account";

        /// <summary>
        /// The controller route
        /// </summary>
        private const String ControllerRoute = AccountController.ControllerName;

        #endregion
    }
}