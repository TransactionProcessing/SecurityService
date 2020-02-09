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
    using IdentityServer4.Stores;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Models;
    using ViewModels;

    /// <summary>
    /// This controller processes the consent UI
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.Controller" />
    [SecurityHeaders]
    [Authorize]
    [ExcludeFromCodeCoverage]
    public class ConsentController : Controller
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
        private readonly ILogger<ConsentController> Logger;

        /// <summary>
        /// The resource store
        /// </summary>
        private readonly IResourceStore ResourceStore;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsentController"/> class.
        /// </summary>
        /// <param name="identityServerInteractionService">The identity server interaction service.</param>
        /// <param name="clientStore">The client store.</param>
        /// <param name="resourceStore">The resource store.</param>
        /// <param name="events">The events.</param>
        /// <param name="logger">The logger.</param>
        public ConsentController(IIdentityServerInteractionService identityServerInteractionService,
                                 IClientStore clientStore,
                                 IResourceStore resourceStore,
                                 IEventService events,
                                 ILogger<ConsentController> logger)
        {
            this.IdentityServerInteractionService = identityServerInteractionService;
            this.ClientStore = clientStore;
            this.ResourceStore = resourceStore;
            this.Events = events;
            this.Logger = logger;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates the scope view model.
        /// </summary>
        /// <param name="scope">The scope.</param>
        /// <param name="check">if set to <c>true</c> [check].</param>
        /// <returns></returns>
        public ScopeViewModel CreateScopeViewModel(Scope scope,
                                                   Boolean check)
        {
            return new ScopeViewModel
                   {
                       Name = scope.Name,
                       DisplayName = scope.DisplayName,
                       Description = scope.Description,
                       Emphasize = scope.Emphasize,
                       Required = scope.Required,
                       Checked = check || scope.Required
                   };
        }

        /// <summary>
        /// Shows the consent screen
        /// </summary>
        /// <param name="returnUrl">The return URL.</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Index(String returnUrl)
        {
            ConsentViewModel vm = await this.BuildViewModelAsync(returnUrl);
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
        public async Task<IActionResult> Index(ConsentInputModel model)
        {
            ProcessConsentResult result = await this.ProcessConsent(model);

            if (result.IsRedirect)
            {
                if (await this.ClientStore.IsPkceClientAsync(result.ClientId))
                {
                    // if the client is PKCE then we assume it's native, so this change in how to
                    // return the response is for better UX for the end user.
                    return this.View("Redirect",
                                     new RedirectViewModel
                                     {
                                         RedirectUrl = result.RedirectUri
                                     });
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
            AuthorizationRequest request = await this.IdentityServerInteractionService.GetAuthorizationContextAsync(returnUrl);
            if (request != null)
            {
                Client client = await this.ClientStore.FindEnabledClientByIdAsync(request.ClientId);
                if (client != null)
                {
                    Resources resources = await this.ResourceStore.FindEnabledResourcesByScopeAsync(request.ScopesRequested);
                    if (resources != null && (resources.IdentityResources.Any() || resources.ApiResources.Any()))
                    {
                        return this.CreateConsentViewModel(model, returnUrl, request, client, resources);
                    }

                    this.Logger.LogError("No scopes matching: {0}",
                                         request.ScopesRequested.Aggregate((x,
                                                                            y) => x + ", " + y));
                }
                else
                {
                    this.Logger.LogError("Invalid client id: {0}", request.ClientId);
                }
            }
            else
            {
                this.Logger.LogError("No consent request matching request: {0}", returnUrl);
            }

            return null;
        }

        /// <summary>
        /// Creates the consent view model.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="returnUrl">The return URL.</param>
        /// <param name="request">The request.</param>
        /// <param name="client">The client.</param>
        /// <param name="resources">The resources.</param>
        /// <returns></returns>
        private ConsentViewModel CreateConsentViewModel(ConsentInputModel model,
                                                        String returnUrl,
                                                        AuthorizationRequest request,
                                                        Client client,
                                                        Resources resources)
        {
            ConsentViewModel vm = new ConsentViewModel
                                  {
                                      RememberConsent = model?.RememberConsent ?? true,
                                      ScopesConsented = model?.ScopesConsented ?? Enumerable.Empty<String>(),

                                      ReturnUrl = returnUrl,

                                      ClientName = client.ClientName ?? client.ClientId,
                                      ClientUrl = client.ClientUri,
                                      ClientLogoUrl = client.LogoUri,
                                      AllowRememberConsent = client.AllowRememberConsent
                                  };

            vm.IdentityScopes = resources.IdentityResources.Select(x => this.CreateScopeViewModel(x, vm.ScopesConsented.Contains(x.Name) || model == null)).ToArray();
            vm.ResourceScopes = resources.ApiResources.SelectMany(x => x.Scopes)
                                         .Select(x => this.CreateScopeViewModel(x, vm.ScopesConsented.Contains(x.Name) || model == null)).ToArray();
            if (ConsentOptions.EnableOfflineAccess && resources.OfflineAccess)
            {
                vm.ResourceScopes = vm.ResourceScopes.Union(new[]
                                                            {
                                                                this.GetOfflineAccessScope(vm.ScopesConsented.Contains(IdentityServerConstants
                                                                                                                       .StandardScopes.OfflineAccess) || model == null)
                                                            });
            }

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
                       Name = identity.Name,
                       DisplayName = identity.DisplayName,
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
                       Name = IdentityServerConstants.StandardScopes.OfflineAccess,
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
            ProcessConsentResult result = new ProcessConsentResult();

            // validate return url is still valid
            AuthorizationRequest request = await this.IdentityServerInteractionService.GetAuthorizationContextAsync(model.ReturnUrl);
            if (request == null) return result;

            ConsentResponse grantedConsent = null;

            // user clicked 'no' - send back the standard 'access_denied' response
            if (model?.Button == "no")
            {
                grantedConsent = ConsentResponse.Denied;

                // emit event
                await this.Events.RaiseAsync(new ConsentDeniedEvent(this.User.GetSubjectId(), request.ClientId, request.ScopesRequested));
            }
            // user clicked 'yes' - validate the data
            else if (model?.Button == "yes")
            {
                // if the user consented to some scope, build the response model
                if (model.ScopesConsented != null && model.ScopesConsented.Any())
                {
                    IEnumerable<String> scopes = model.ScopesConsented;
                    if (ConsentOptions.EnableOfflineAccess == false)
                    {
                        scopes = scopes.Where(x => x != IdentityServerConstants.StandardScopes.OfflineAccess);
                    }

                    grantedConsent = new ConsentResponse
                                     {
                                         RememberConsent = model.RememberConsent,
                                         ScopesConsented = scopes.ToArray()
                                     };

                    // emit event
                    await this.Events.RaiseAsync(new ConsentGrantedEvent(this.User.GetSubjectId(),
                                                                         request.ClientId,
                                                                         request.ScopesRequested,
                                                                         grantedConsent.ScopesConsented,
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
                await this.IdentityServerInteractionService.GrantConsentAsync(request, grantedConsent);

                // indicate that's it ok to redirect back to authorization endpoint
                result.RedirectUri = model.ReturnUrl;
                result.ClientId = request.ClientId;
            }
            else
            {
                // we need to redisplay the consent UI
                result.ViewModel = await this.BuildViewModelAsync(model.ReturnUrl, model);
            }

            return result;
        }

        #endregion
    }
}