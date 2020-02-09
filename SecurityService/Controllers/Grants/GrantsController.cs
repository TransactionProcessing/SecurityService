namespace SecurityService.Controllers.Grants
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading.Tasks;
    using IdentityServer4.Events;
    using IdentityServer4.Extensions;
    using IdentityServer4.Models;
    using IdentityServer4.Services;
    using IdentityServer4.Stores;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using ViewModels;

    /// <summary>
    /// This sample controller allows a user to revoke grants given to clients
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.Controller" />
    [SecurityHeaders]
    [Authorize]
    [ExcludeFromCodeCoverage]
    public class GrantsController : Controller
    {
        #region Fields

        /// <summary>
        /// The client store
        /// </summary>
        private readonly IClientStore ClientStore;

        /// <summary>
        /// The event service
        /// </summary>
        private readonly IEventService EventService;

        /// <summary>
        /// The identity server interaction service
        /// </summary>
        private readonly IIdentityServerInteractionService IdentityServerInteractionService;

        /// <summary>
        /// The resource store
        /// </summary>
        private readonly IResourceStore ResourceStore;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="GrantsController"/> class.
        /// </summary>
        /// <param name="identityServerInteractionService">The identity server interaction service.</param>
        /// <param name="clientStore">The client store.</param>
        /// <param name="resourceStore">The resource store.</param>
        /// <param name="eventService">The event service.</param>
        public GrantsController(IIdentityServerInteractionService identityServerInteractionService,
                                IClientStore clientStore,
                                IResourceStore resourceStore,
                                IEventService eventService)
        {
            this.IdentityServerInteractionService = identityServerInteractionService;
            this.ClientStore = clientStore;
            this.ResourceStore = resourceStore;
            this.EventService = eventService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Show list of grants
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            return this.View("Index", await this.BuildViewModelAsync());
        }

        /// <summary>
        /// Handle postback to revoke a client
        /// </summary>
        /// <param name="clientId">The client identifier.</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Revoke(String clientId)
        {
            await this.IdentityServerInteractionService.RevokeUserConsentAsync(clientId);
            await this.EventService.RaiseAsync(new GrantsRevokedEvent(this.User.GetSubjectId(), clientId));

            return this.RedirectToAction("Index");
        }

        /// <summary>
        /// Builds the view model asynchronous.
        /// </summary>
        /// <returns></returns>
        private async Task<GrantsViewModel> BuildViewModelAsync()
        {
            IEnumerable<Consent> grants = await this.IdentityServerInteractionService.GetAllUserConsentsAsync();

            List<GrantViewModel> list = new List<GrantViewModel>();
            foreach (Consent grant in grants)
            {
                Client client = await this.ClientStore.FindClientByIdAsync(grant.ClientId);
                if (client != null)
                {
                    Resources resources = await this.ResourceStore.FindResourcesByScopeAsync(grant.Scopes);

                    GrantViewModel item = new GrantViewModel
                                          {
                                              ClientId = client.ClientId,
                                              ClientName = client.ClientName ?? client.ClientId,
                                              ClientLogoUrl = client.LogoUri,
                                              ClientUrl = client.ClientUri,
                                              Created = grant.CreationTime,
                                              Expires = grant.Expiration,
                                              IdentityGrantNames = resources.IdentityResources.Select(x => x.DisplayName ?? x.Name).ToArray(),
                                              ApiGrantNames = resources.ApiResources.Select(x => x.DisplayName ?? x.Name).ToArray()
                                          };

                    list.Add(item);
                }
            }

            return new GrantsViewModel
                   {
                       Grants = list
                   };
        }

        #endregion
    }
}