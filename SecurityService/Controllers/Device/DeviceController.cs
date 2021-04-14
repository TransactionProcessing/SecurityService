namespace SecurityService.Controllers.Device
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading.Tasks;
    using IdentityServer4;
    using IdentityServer4.Configuration;
    using IdentityServer4.Events;
    using IdentityServer4.Extensions;
    using IdentityServer4.Models;
    using IdentityServer4.Services;
    using IdentityServer4.Validation;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Models;
    using ViewModels;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.Controller" />
    [Route(DeviceController.ControllerRoute)]
    [ExcludeFromCodeCoverage]
    [Authorize]
    [SecurityHeaders]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class DeviceController : Controller
    {
        #region Fields

        /// <summary>
        /// The events
        /// </summary>
        private readonly IEventService _events;

        /// <summary>
        /// The interaction
        /// </summary>
        private readonly IDeviceFlowInteractionService _interaction;

        /// <summary>
        /// The logger
        /// </summary>
        private readonly ILogger<DeviceController> _logger;

        /// <summary>
        /// The options
        /// </summary>
        private readonly IOptions<IdentityServerOptions> _options;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceController"/> class.
        /// </summary>
        /// <param name="interaction">The interaction.</param>
        /// <param name="eventService">The event service.</param>
        /// <param name="options">The options.</param>
        /// <param name="logger">The logger.</param>
        public DeviceController(IDeviceFlowInteractionService interaction,
                                IEventService eventService,
                                IOptions<IdentityServerOptions> options,
                                ILogger<DeviceController> logger)
        {
            this._interaction = interaction;
            this._events = eventService;
            this._options = options;
            this._logger = logger;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Callbacks the specified model.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">model</exception>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("callback")]
        public async Task<IActionResult> Callback(DeviceAuthorizationInputModel model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            var result = await this.ProcessConsent(model);
            if (result.HasValidationError) return this.View("Error");

            return this.View("Success");
        }

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
            return new ScopeViewModel
                   {
                       Value = parsedScopeValue.RawValue,
                       // todo: use the parsed scope value in the display?
                       DisplayName = apiScope.DisplayName ?? apiScope.Name,
                       Description = apiScope.Description,
                       Emphasize = apiScope.Emphasize,
                       Required = apiScope.Required,
                       Checked = check || apiScope.Required
                   };
        }

        /// <summary>
        /// Indexes this instance.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("index")]
        public async Task<IActionResult> Index()
        {
            String userCodeParamName = this._options.Value.UserInteraction.DeviceVerificationUserCodeParameter;
            String userCode = this.Request.Query[userCodeParamName];
            if (string.IsNullOrWhiteSpace(userCode)) return this.View("UserCodeCapture");

            var vm = await this.BuildViewModelAsync(userCode);
            if (vm == null) return this.View("Error");

            vm.ConfirmUserCode = true;
            return this.View("UserCodeConfirmation", vm);
        }

        /// <summary>
        /// Users the code capture.
        /// </summary>
        /// <param name="userCode">The user code.</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("usercodecapture")]
        public async Task<IActionResult> UserCodeCapture(String userCode)
        {
            var vm = await this.BuildViewModelAsync(userCode);
            if (vm == null) return this.View("Error");

            return this.View("UserCodeConfirmation", vm);
        }

        /// <summary>
        /// Builds the view model asynchronous.
        /// </summary>
        /// <param name="userCode">The user code.</param>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        private async Task<DeviceAuthorizationViewModel> BuildViewModelAsync(String userCode,
                                                                             DeviceAuthorizationInputModel model = null)
        {
            var request = await this._interaction.GetAuthorizationContextAsync(userCode);
            if (request != null)
            {
                return this.CreateConsentViewModel(userCode, model, request);
            }

            return null;
        }

        /// <summary>
        /// Creates the consent view model.
        /// </summary>
        /// <param name="userCode">The user code.</param>
        /// <param name="model">The model.</param>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        private DeviceAuthorizationViewModel CreateConsentViewModel(String userCode,
                                                                    DeviceAuthorizationInputModel model,
                                                                    DeviceFlowAuthorizationRequest request)
        {
            var vm = new DeviceAuthorizationViewModel
                     {
                         UserCode = userCode,
                         Description = model?.Description,

                         RememberConsent = model?.RememberConsent ?? true,
                         ScopesConsented = model?.ScopesConsented ?? Enumerable.Empty<String>(),

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

        /// <summary>
        /// Processes the consent.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        private async Task<ProcessConsentResult> ProcessConsent(DeviceAuthorizationInputModel model)
        {
            var result = new ProcessConsentResult();

            var request = await this._interaction.GetAuthorizationContextAsync(model.UserCode);
            if (request == null) return result;

            ConsentResponse grantedConsent = null;

            // user clicked 'no' - send back the standard 'access_denied' response
            if (model.Button == "no")
            {
                grantedConsent = new ConsentResponse
                                 {
                                     Error = AuthorizationError.AccessDenied
                                 };

                // emit event
                await this._events.RaiseAsync(new ConsentDeniedEvent(this.User.GetSubjectId(), request.Client.ClientId, request.ValidatedResources.RawScopeValues));
            }
            // user clicked 'yes' - validate the data
            else if (model.Button == "yes")
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
                await this._interaction.HandleRequestAsync(model.UserCode, grantedConsent);

                // indicate that's it ok to redirect back to authorization endpoint
                result.RedirectUri = model.ReturnUrl;
                result.Client = request.Client;
            }
            else
            {
                // we need to redisplay the consent UI
                result.ViewModel = await this.BuildViewModelAsync(model.UserCode, model);
            }

            return result;
        }

        #endregion

        #region Others

        /// <summary>
        /// The controller name
        /// </summary>
        public const String ControllerName = "device";

        /// <summary>
        /// The controller route
        /// </summary>
        private const String ControllerRoute = DeviceController.ControllerName;

        #endregion
    }
}