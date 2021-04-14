namespace SecurityService.Controllers.Consent
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading.Tasks;
    using IdentityServer4;
    using IdentityServer4.Events;
    using IdentityServer4.Extensions;
    using IdentityServer4.Models;
    using IdentityServer4.Services;
    using IdentityServer4.Validation;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Models;
    using ViewModels;

    /// <summary>
    /// This controller processes the consent UI
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.Controller" />
    [Route(ConsentController.ControllerRoute)]
    [ExcludeFromCodeCoverage]
    [SecurityHeaders]
    [Authorize]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ConsentController : Controller
    {
        #region Fields

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
        private readonly ILogger<ConsentController> _logger;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsentController"/> class.
        /// </summary>
        /// <param name="interaction">The interaction.</param>
        /// <param name="events">The events.</param>
        /// <param name="logger">The logger.</param>
        public ConsentController(IIdentityServerInteractionService interaction,
                                 IEventService events,
                                 ILogger<ConsentController> logger)
        {
            this._interaction = interaction;
            this._events = events;
            this._logger = logger;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates the scope view model.
        /// </summary>
        /// <param name="parsedScopeValue">The parsed scope value.</param>
        /// <param name="apiScope">The API scope.</param>
        /// <param name="check">if set to <c>true</c> [check].</param>
        /// <returns></returns>
        public ScopeViewModel CreateScopeViewModel(ParsedScopeValue parsedScopeValue,
                                                   ApiScope apiScope,
                                                   Boolean check)
        {
            var displayName = apiScope.DisplayName ?? apiScope.Name;
            if (!string.IsNullOrWhiteSpace(parsedScopeValue.ParsedParameter))
            {
                displayName += ":" + parsedScopeValue.ParsedParameter;
            }

            return new ScopeViewModel
                   {
                       Value = parsedScopeValue.RawValue,
                       DisplayName = displayName,
                       Description = apiScope.Description,
                       Emphasize = apiScope.Emphasize,
                       Required = apiScope.Required,
                       Checked = check || apiScope.Required
                   };
        }

        /// <summary>
        /// Shows the consent screen
        /// </summary>
        /// <param name="returnUrl">The return URL.</param>
        /// <returns></returns>
        [HttpGet]
        [Route("index")]
        public async Task<IActionResult> Index(String returnUrl)
        {
            var vm = await this.BuildViewModelAsync(returnUrl);
            if (vm != null)
            {
                return this.View("Index", vm);
            }

            return this.View("Error");
        }

        /// <summary>
        /// Handles the consent screen postback
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("index")]
        public async Task<IActionResult> Index(ConsentInputModel model)
        {
            var result = await this.ProcessConsent(model);

            if (result.IsRedirect)
            {
                var context = await this._interaction.GetAuthorizationContextAsync(model.ReturnUrl);
                if (context?.IsNativeClient() == true)
                {
                    // The client is native, so this change in how to
                    // return the response is for better UX for the end user.
                    return this.LoadingPage("Redirect", result.RedirectUri);
                }

                return this.Redirect(result.RedirectUri);
            }

            if (result.HasValidationError)
            {
                this.ModelState.AddModelError(string.Empty, result.ValidationError);
            }

            if (result.ShowView)
            {
                return this.View("Index", result.ViewModel);
            }

            return this.View("Error");
        }

        /// <summary>
        /// Builds the view model asynchronous.
        /// </summary>
        /// <param name="returnUrl">The return URL.</param>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        private async Task<ConsentViewModel> BuildViewModelAsync(String returnUrl,
                                                                 ConsentInputModel model = null)
        {
            var request = await this._interaction.GetAuthorizationContextAsync(returnUrl);
            if (request != null)
            {
                return this.CreateConsentViewModel(model, returnUrl, request);
            }

            this._logger.LogError("No consent request matching request: {0}", returnUrl);

            return null;
        }

        /// <summary>
        /// Creates the consent view model.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="returnUrl">The return URL.</param>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        private ConsentViewModel CreateConsentViewModel(ConsentInputModel model,
                                                        String returnUrl,
                                                        AuthorizationRequest request)
        {
            var vm = new ConsentViewModel
                     {
                         RememberConsent = model?.RememberConsent ?? true,
                         ScopesConsented = model?.ScopesConsented ?? Enumerable.Empty<String>(),
                         Description = model?.Description,

                         ReturnUrl = returnUrl,

                         ClientName = request.Client.ClientName ?? request.Client.ClientId,
                         ClientUrl = request.Client.ClientUri,
                         ClientLogoUrl = request.Client.LogoUri,
                         AllowRememberConsent = request.Client.AllowRememberConsent
                     };

            vm.IdentityScopes = request.ValidatedResources.Resources.IdentityResources
                                       .Select(x => this.CreateScopeViewModel(x, vm.ScopesConsented.Contains(x.Name) || model == null)).ToArray();

            var apiScopes = new List<ScopeViewModel>();
            foreach (var parsedScope in request.ValidatedResources.ParsedScopes)
            {
                var apiScope = request.ValidatedResources.Resources.FindApiScope(parsedScope.ParsedName);
                if (apiScope != null)
                {
                    var scopeVm = this.CreateScopeViewModel(parsedScope, apiScope, vm.ScopesConsented.Contains(parsedScope.RawValue) || model == null);
                    apiScopes.Add(scopeVm);
                }
            }

            if (ConsentOptions.EnableOfflineAccess && request.ValidatedResources.Resources.OfflineAccess)
            {
                apiScopes.Add(this.GetOfflineAccessScope(vm.ScopesConsented.Contains(IdentityServerConstants.StandardScopes.OfflineAccess) || model == null));
            }

            vm.ApiScopes = apiScopes;

            return vm;
        }

        /// <summary>
        /// Creates the scope view model.
        /// </summary>
        /// <param name="identity">The identity.</param>
        /// <param name="check">if set to <c>true</c> [check].</param>
        /// <returns></returns>
        private ScopeViewModel CreateScopeViewModel(IdentityResource identity,
                                                    Boolean check)
        {
            return new ScopeViewModel
                   {
                       Value = identity.Name,
                       DisplayName = identity.DisplayName ?? identity.Name,
                       Description = identity.Description,
                       Emphasize = identity.Emphasize,
                       Required = identity.Required,
                       Checked = check || identity.Required
                   };
        }

        /// <summary>
        /// Gets the offline access scope.
        /// </summary>
        /// <param name="check">if set to <c>true</c> [check].</param>
        /// <returns></returns>
        private ScopeViewModel GetOfflineAccessScope(Boolean check)
        {
            return new ScopeViewModel
                   {
                       Value = IdentityServerConstants.StandardScopes.OfflineAccess,
                       DisplayName = ConsentOptions.OfflineAccessDisplayName,
                       Description = ConsentOptions.OfflineAccessDescription,
                       Emphasize = true,
                       Checked = check
                   };
        }

        /*****************************************/
        /* helper APIs for the ConsentController */
        /*****************************************/
        /// <summary>
        /// Processes the consent.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        private async Task<ProcessConsentResult> ProcessConsent(ConsentInputModel model)
        {
            var result = new ProcessConsentResult();

            // validate return url is still valid
            var request = await this._interaction.GetAuthorizationContextAsync(model.ReturnUrl);
            if (request == null) return result;

            ConsentResponse grantedConsent = null;

            // user clicked 'no' - send back the standard 'access_denied' response
            if (model?.Button == "no")
            {
                grantedConsent = new ConsentResponse
                                 {
                                     Error = AuthorizationError.AccessDenied
                                 };

                // emit event
                await this._events.RaiseAsync(new ConsentDeniedEvent(this.User.GetSubjectId(), request.Client.ClientId, request.ValidatedResources.RawScopeValues));
            }
            // user clicked 'yes' - validate the data
            else if (model?.Button == "yes")
            {
                // if the user consented to some scope, build the response model
                if (model.ScopesConsented != null && model.ScopesConsented.Any())
                {
                    var scopes = model.ScopesConsented;
                    if (ConsentOptions.EnableOfflineAccess == false)
                    {
                        scopes = scopes.Where(x => x != IdentityServerConstants.StandardScopes.OfflineAccess);
                    }

                    grantedConsent = new ConsentResponse
                                     {
                                         RememberConsent = model.RememberConsent,
                                         ScopesValuesConsented = scopes.ToArray(),
                                         Description = model.Description
                                     };

                    // emit event
                    await this._events.RaiseAsync(new ConsentGrantedEvent(this.User.GetSubjectId(),
                                                                          request.Client.ClientId,
                                                                          request.ValidatedResources.RawScopeValues,
                                                                          grantedConsent.ScopesValuesConsented,
                                                                          grantedConsent.RememberConsent));
                }
                else
                {
                    result.ValidationError = ConsentOptions.MustChooseOneErrorMessage;
                }
            }
            else
            {
                result.ValidationError = ConsentOptions.InvalidSelectionErrorMessage;
            }

            if (grantedConsent != null)
            {
                // communicate outcome of consent back to identityserver
                await this._interaction.GrantConsentAsync(request, grantedConsent);

                // indicate that's it ok to redirect back to authorization endpoint
                result.RedirectUri = model.ReturnUrl;
                result.Client = request.Client;
            }
            else
            {
                // we need to redisplay the consent UI
                result.ViewModel = await this.BuildViewModelAsync(model.ReturnUrl, model);
            }

            return result;
        }

        #endregion

        #region Others

        /// <summary>
        /// The controller name
        /// </summary>
        public const String ControllerName = "consent";

        /// <summary>
        /// The controller route
        /// </summary>
        private const String ControllerRoute = ConsentController.ControllerName;

        #endregion
    }
}