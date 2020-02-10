namespace SecurityService.Controllers.Account
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Security.Claims;
    using System.Security.Principal;
    using System.Threading.Tasks;
    using IdentityModel;
    using IdentityServer4;
    using IdentityServer4.Events;
    using IdentityServer4.Models;
    using IdentityServer4.Services;
    using IdentityServer4.Stores;
    using IdentityServer4.Test;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Models;
    using ViewModels;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.Controller" />
    [SecurityHeaders]
    [AllowAnonymous]
    [ExcludeFromCodeCoverage]
    public class ExternalController : Controller
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
        /// The identity server interaction service
        /// </summary>
        private readonly IIdentityServerInteractionService IdentityServerInteractionService;

        /// <summary>
        /// The logger
        /// </summary>
        private readonly ILogger<ExternalController> Logger;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ExternalController"/> class.
        /// </summary>
        /// <param name="identityServerInteractionService">The identity server interaction service.</param>
        /// <param name="clientStore">The client store.</param>
        /// <param name="events">The events.</param>
        /// <param name="logger">The logger.</param>
        public ExternalController(IIdentityServerInteractionService identityServerInteractionService,
                                  IClientStore clientStore,
                                  IEventService events,
                                  ILogger<ExternalController> logger)

        {
            this.IdentityServerInteractionService = identityServerInteractionService;
            this.ClientStore = clientStore;
            this.Logger = logger;
            this.Events = events;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Post processing of external authentication
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception">External authentication error</exception>
        [HttpGet]
        public async Task<IActionResult> Callback()
        {
            // read external identity from the temporary cookie
            AuthenticateResult result = await this.HttpContext.AuthenticateAsync(IdentityServerConstants.ExternalCookieAuthenticationScheme);
            if (result?.Succeeded != true)
            {
                throw new Exception("External authentication error");
            }

            if (this.Logger.IsEnabled(LogLevel.Debug))
            {
                IEnumerable<String> externalClaims = result.Principal.Claims.Select(c => $"{c.Type}: {c.Value}");
                this.Logger.LogDebug("External claims: {@claims}", externalClaims);
            }

            // lookup our user and external provider info
            (TestUser user, String provider, String providerUserId, IEnumerable<Claim> claims) = this.FindUserFromExternalProvider(result);
            if (user == null)
            {
                // this might be where you might initiate a custom workflow for user registration
                // in this sample we don't show how that would be done, as our sample implementation
                // simply auto-provisions new external user
                user = this.AutoProvisionUser(provider, providerUserId, claims);
            }

            // this allows us to collect any additonal claims or properties
            // for the specific prtotocols used and store them in the local auth cookie.
            // this is typically used to store data needed for signout from those protocols.
            List<Claim> additionalLocalClaims = new List<Claim>();
            AuthenticationProperties localSignInProps = new AuthenticationProperties();
            this.ProcessLoginCallbackForOidc(result, additionalLocalClaims, localSignInProps);
            this.ProcessLoginCallbackForWsFed(result, additionalLocalClaims, localSignInProps);
            this.ProcessLoginCallbackForSaml2p(result, additionalLocalClaims, localSignInProps);

            // issue authentication cookie for user
            await this.HttpContext.SignInAsync(user.SubjectId, user.Username, provider, localSignInProps, additionalLocalClaims.ToArray());

            // delete temporary cookie used during external authentication
            await this.HttpContext.SignOutAsync(IdentityServerConstants.ExternalCookieAuthenticationScheme);

            // retrieve return URL
            String returnUrl = result.Properties.Items["returnUrl"] ?? "~/";

            // check if external login is in the context of an OIDC request
            AuthorizationRequest context = await this.IdentityServerInteractionService.GetAuthorizationContextAsync(returnUrl);
            await this.Events.RaiseAsync(new UserLoginSuccessEvent(provider, providerUserId, user.SubjectId, user.Username, true, context?.ClientId));

            if (context != null)
            {
                if (await this.ClientStore.IsPkceClientAsync(context.ClientId))
                {
                    // if the client is PKCE then we assume it's native, so this change in how to
                    // return the response is for better UX for the end user.
                    return this.View("Redirect",
                                     new RedirectViewModel
                                     {
                                         RedirectUrl = returnUrl
                                     });
                }
            }

            return this.Redirect(returnUrl);
        }

        /// <summary>
        /// initiate roundtrip to external authentication provider
        /// </summary>
        /// <param name="provider">The provider.</param>
        /// <param name="returnUrl">The return URL.</param>
        /// <returns></returns>
        /// <exception cref="Exception">invalid return URL</exception>
        [HttpGet]
        public async Task<IActionResult> Challenge(String provider,
                                                   String returnUrl)
        {
            if (string.IsNullOrEmpty(returnUrl)) returnUrl = "~/";

            // validate returnUrl - either it is a valid OIDC URL or back to a local page
            if (this.Url.IsLocalUrl(returnUrl) == false && this.IdentityServerInteractionService.IsValidReturnUrl(returnUrl) == false)
            {
                // user might have clicked on a malicious link - should be logged
                throw new Exception("invalid return URL");
            }

            if (AccountOptions.WindowsAuthenticationSchemeName == provider)
            {
                // windows authentication needs special handling
                return await this.ProcessWindowsLoginAsync(returnUrl);
            }

            // start challenge and roundtrip the return URL and scheme 
            AuthenticationProperties props = new AuthenticationProperties
                                             {
                                                 RedirectUri = this.Url.Action(nameof(ExternalController.Callback)),
                                                 Items =
                                                 {
                                                     {"returnUrl", returnUrl},
                                                     {"scheme", provider}
                                                 }
                                             };

            return this.Challenge(props, provider);
        }

        /// <summary>
        /// Automatics the provision user.
        /// </summary>
        /// <param name="provider">The provider.</param>
        /// <param name="providerUserId">The provider user identifier.</param>
        /// <param name="claims">The claims.</param>
        /// <returns></returns>
        private TestUser AutoProvisionUser(String provider,
                                           String providerUserId,
                                           IEnumerable<Claim> claims)
        {
            // TODO: AutoProvisionUser
            //TestUser user = _users.AutoProvisionUser(provider, providerUserId, claims.ToList());
            //return user;
            return null;
        }

        /// <summary>
        /// Finds the user from external provider.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <returns></returns>
        /// <exception cref="Exception">Unknown userid</exception>
        private (TestUser user, String provider, String providerUserId, IEnumerable<Claim> claims) FindUserFromExternalProvider(AuthenticateResult result)
        {
            ClaimsPrincipal externalUser = result.Principal;

            // try to determine the unique id of the external user (issued by the provider)
            // the most common claim type for that are the sub claim and the NameIdentifier
            // depending on the external provider, some other claim type might be used
            Claim userIdClaim = externalUser.FindFirst(JwtClaimTypes.Subject) ??
                                externalUser.FindFirst(ClaimTypes.NameIdentifier) ?? throw new Exception("Unknown userid");

            // remove the user id claim so we don't include it as an extra claim if/when we provision the user
            List<Claim> claims = externalUser.Claims.ToList();
            claims.Remove(userIdClaim);

            String provider = result.Properties.Items["scheme"];
            String providerUserId = userIdClaim.Value;

            // find external user
            //TestUser user = _users.FindByExternalProvider(provider, providerUserId);
            // TODO: External provider
            TestUser user = null;
            return (user, provider, providerUserId, claims);
        }

        /// <summary>
        /// Processes the login callback for oidc.
        /// </summary>
        /// <param name="externalResult">The external result.</param>
        /// <param name="localClaims">The local claims.</param>
        /// <param name="localSignInProps">The local sign in props.</param>
        private void ProcessLoginCallbackForOidc(AuthenticateResult externalResult,
                                                 List<Claim> localClaims,
                                                 AuthenticationProperties localSignInProps)
        {
            // if the external system sent a session id claim, copy it over
            // so we can use it for single sign-out
            Claim sid = externalResult.Principal.Claims.FirstOrDefault(x => x.Type == JwtClaimTypes.SessionId);
            if (sid != null)
            {
                localClaims.Add(new Claim(JwtClaimTypes.SessionId, sid.Value));
            }

            // if the external provider issued an id_token, we'll keep it for signout
            String id_token = externalResult.Properties.GetTokenValue("id_token");
            if (id_token != null)
            {
                localSignInProps.StoreTokens(new[]
                                             {
                                                 new AuthenticationToken
                                                 {
                                                     Name = "id_token",
                                                     Value = id_token
                                                 }
                                             });
            }
        }

        /// <summary>
        /// Processes the login callback for saml2p.
        /// </summary>
        /// <param name="externalResult">The external result.</param>
        /// <param name="localClaims">The local claims.</param>
        /// <param name="localSignInProps">The local sign in props.</param>
        private void ProcessLoginCallbackForSaml2p(AuthenticateResult externalResult,
                                                   List<Claim> localClaims,
                                                   AuthenticationProperties localSignInProps)
        {
        }

        /// <summary>
        /// Processes the login callback for ws fed.
        /// </summary>
        /// <param name="externalResult">The external result.</param>
        /// <param name="localClaims">The local claims.</param>
        /// <param name="localSignInProps">The local sign in props.</param>
        private void ProcessLoginCallbackForWsFed(AuthenticateResult externalResult,
                                                  List<Claim> localClaims,
                                                  AuthenticationProperties localSignInProps)
        {
        }

        /// <summary>
        /// Processes the windows login asynchronous.
        /// </summary>
        /// <param name="returnUrl">The return URL.</param>
        /// <returns></returns>
        private async Task<IActionResult> ProcessWindowsLoginAsync(String returnUrl)
        {
            // see if windows auth has already been requested and succeeded
            AuthenticateResult result = await this.HttpContext.AuthenticateAsync(AccountOptions.WindowsAuthenticationSchemeName);
            if (result?.Principal is WindowsPrincipal wp)
            {
                // we will issue the external cookie and then redirect the
                // user back to the external callback, in essence, treating windows
                // auth the same as any other external authentication mechanism
                AuthenticationProperties props = new AuthenticationProperties
                                                 {
                                                     RedirectUri = this.Url.Action("Callback"),
                                                     Items =
                                                     {
                                                         {"returnUrl", returnUrl},
                                                         {"scheme", AccountOptions.WindowsAuthenticationSchemeName}
                                                     }
                                                 };

                ClaimsIdentity id = new ClaimsIdentity(AccountOptions.WindowsAuthenticationSchemeName);
                id.AddClaim(new Claim(JwtClaimTypes.Subject, wp.FindFirst(ClaimTypes.PrimarySid).Value));
                id.AddClaim(new Claim(JwtClaimTypes.Name, wp.Identity.Name));

                // add the groups as claims -- be careful if the number of groups is too large
                if (AccountOptions.IncludeWindowsGroups)
                {
                    WindowsIdentity wi = wp.Identity as WindowsIdentity;
                    IdentityReferenceCollection groups = wi.Groups.Translate(typeof(NTAccount));
                    IEnumerable<Claim> roles = groups.Select(x => new Claim(JwtClaimTypes.Role, x.Value));
                    id.AddClaims(roles);
                }

                await this.HttpContext.SignInAsync(IdentityServerConstants.ExternalCookieAuthenticationScheme, new ClaimsPrincipal(id), props);
                return this.Redirect(props.RedirectUri);
            }

            // trigger windows auth
            // since windows auth don't support the redirect uri,
            // this URL is re-triggered when we call challenge
            return this.Challenge(AccountOptions.WindowsAuthenticationSchemeName);
        }

        #endregion
    }
}