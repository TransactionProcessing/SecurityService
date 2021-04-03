namespace SecurityService.Controllers.Account
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using IdentityModel;
    using IdentityServer4;
    using IdentityServer4.Events;
    using IdentityServer4.Services;
    using IdentityServer4.Stores;
    using IdentityServer4.Test;
    using Manager;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.Controller" />
    [Route(ExternalController.ControllerRoute)]
    [ExcludeFromCodeCoverage]
    [SecurityHeaders]
    [AllowAnonymous]
    public class ExternalController : Controller
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
        /// The logger
        /// </summary>
        private readonly ILogger<ExternalController> _logger;

        /// <summary>
        /// The security service manager
        /// </summary>
        private readonly ISecurityServiceManager _securityServiceManager;

        /// <summary>
        /// The users
        /// </summary>
        private readonly TestUserStore _users;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ExternalController"/> class.
        /// </summary>
        /// <param name="interaction">The interaction.</param>
        /// <param name="clientStore">The client store.</param>
        /// <param name="events">The events.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="securityServiceManager">The security service manager.</param>
        public ExternalController(IIdentityServerInteractionService interaction,
                                  IClientStore clientStore,
                                  IEventService events,
                                  ILogger<ExternalController> logger,
                                  ISecurityServiceManager securityServiceManager)
        {
            this._interaction = interaction;
            this._clientStore = clientStore;
            this._logger = logger;
            this._securityServiceManager = securityServiceManager;
            this._events = events;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Post processing of external authentication
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception">External authentication error</exception>
        [HttpGet]
        [Route("callback")]
        public async Task<IActionResult> Callback()
        {
            // read external identity from the temporary cookie
            var result = await this.HttpContext.AuthenticateAsync(IdentityServerConstants.ExternalCookieAuthenticationScheme);
            if (result?.Succeeded != true)
            {
                throw new Exception("External authentication error");
            }

            if (this._logger.IsEnabled(LogLevel.Debug))
            {
                var externalClaims = result.Principal.Claims.Select(c => $"{c.Type}: {c.Value}");
                this._logger.LogDebug("External claims: {@claims}", externalClaims);
            }

            // lookup our user and external provider info
            var (user, provider, providerUserId, claims) = this.FindUserFromExternalProvider(result);
            if (user == null)
            {
                // this might be where you might initiate a custom workflow for user registration
                // in this sample we don't show how that would be done, as our sample implementation
                // simply auto-provisions new external user
                user = this.AutoProvisionUser(provider, providerUserId, claims);
            }

            // this allows us to collect any additional claims or properties
            // for the specific protocols used and store them in the local auth cookie.
            // this is typically used to store data needed for signout from those protocols.
            var additionalLocalClaims = new List<Claim>();
            var localSignInProps = new AuthenticationProperties();
            this.ProcessLoginCallback(result, additionalLocalClaims, localSignInProps);

            // issue authentication cookie for user
            var isuser = new IdentityServerUser(user.SubjectId)
                         {
                             DisplayName = user.Username,
                             IdentityProvider = provider,
                             AdditionalClaims = additionalLocalClaims
                         };

            await this.HttpContext.SignInAsync(isuser, localSignInProps);

            // delete temporary cookie used during external authentication
            await this.HttpContext.SignOutAsync(IdentityServerConstants.ExternalCookieAuthenticationScheme);

            // retrieve return URL
            var returnUrl = result.Properties.Items["returnUrl"] ?? "~/";

            // check if external login is in the context of an OIDC request
            var context = await this._interaction.GetAuthorizationContextAsync(returnUrl);
            await this._events.RaiseAsync(new UserLoginSuccessEvent(provider, providerUserId, user.SubjectId, user.Username, true, context?.Client.ClientId));

            if (context != null)
            {
                if (context.IsNativeClient())
                {
                    // The client is native, so this change in how to
                    // return the response is for better UX for the end user.
                    return this.LoadingPage("Redirect", returnUrl);
                }
            }

            return this.Redirect(returnUrl);
        }

        /// <summary>
        /// initiate roundtrip to external authentication provider
        /// </summary>
        /// <param name="scheme">The scheme.</param>
        /// <param name="returnUrl">The return URL.</param>
        /// <returns></returns>
        /// <exception cref="Exception">invalid return URL</exception>
        [HttpGet]
        [Route("challenge")]
        public IActionResult Challenge(String scheme,
                                       String returnUrl)
        {
            if (string.IsNullOrEmpty(returnUrl)) returnUrl = "~/";

            // validate returnUrl - either it is a valid OIDC URL or back to a local page
            if (this.Url.IsLocalUrl(returnUrl) == false && this._interaction.IsValidReturnUrl(returnUrl) == false)
            {
                // user might have clicked on a malicious link - should be logged
                throw new Exception("invalid return URL");
            }

            // start challenge and roundtrip the return URL and scheme 
            var props = new AuthenticationProperties
                        {
                            RedirectUri = this.Url.Action(nameof(this.Callback)),
                            Items =
                            {
                                {"returnUrl", returnUrl},
                                {"scheme", scheme},
                            }
                        };

            return this.Challenge(props, scheme);
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
            var user = this._users.AutoProvisionUser(provider, providerUserId, claims.ToList());
            return user;
        }

        /// <summary>
        /// Finds the user from external provider.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <returns></returns>
        /// <exception cref="Exception">Unknown userid</exception>
        private (TestUser user, String provider, String providerUserId, IEnumerable<Claim> claims) FindUserFromExternalProvider(AuthenticateResult result)
        {
            var externalUser = result.Principal;

            // try to determine the unique id of the external user (issued by the provider)
            // the most common claim type for that are the sub claim and the NameIdentifier
            // depending on the external provider, some other claim type might be used
            var userIdClaim = externalUser.FindFirst(JwtClaimTypes.Subject) ?? externalUser.FindFirst(ClaimTypes.NameIdentifier) ?? throw new Exception("Unknown userid");

            // remove the user id claim so we don't include it as an extra claim if/when we provision the user
            var claims = externalUser.Claims.ToList();
            claims.Remove(userIdClaim);

            var provider = result.Properties.Items["scheme"];
            var providerUserId = userIdClaim.Value;

            // find external user
            var user = this._users.FindByExternalProvider(provider, providerUserId);

            return (user, provider, providerUserId, claims);
        }

        // if the external login is OIDC-based, there are certain things we need to preserve to make logout work
        // this will be different for WS-Fed, SAML2p or other protocols
        /// <summary>
        /// Processes the login callback.
        /// </summary>
        /// <param name="externalResult">The external result.</param>
        /// <param name="localClaims">The local claims.</param>
        /// <param name="localSignInProps">The local sign in props.</param>
        private void ProcessLoginCallback(AuthenticateResult externalResult,
                                          List<Claim> localClaims,
                                          AuthenticationProperties localSignInProps)
        {
            // if the external system sent a session id claim, copy it over
            // so we can use it for single sign-out
            var sid = externalResult.Principal.Claims.FirstOrDefault(x => x.Type == JwtClaimTypes.SessionId);
            if (sid != null)
            {
                localClaims.Add(new Claim(JwtClaimTypes.SessionId, sid.Value));
            }

            // if the external provider issued an id_token, we'll keep it for signout
            var idToken = externalResult.Properties.GetTokenValue("id_token");
            if (idToken != null)
            {
                localSignInProps.StoreTokens(new[]
                                             {
                                                 new AuthenticationToken
                                                 {
                                                     Name = "id_token",
                                                     Value = idToken
                                                 }
                                             });
            }
        }

        #endregion

        #region Others

        /// <summary>
        /// The controller name
        /// </summary>
        public const String ControllerName = "external";

        /// <summary>
        /// The controller route
        /// </summary>
        private const String ControllerRoute = ExternalController.ControllerName;

        #endregion
    }
}