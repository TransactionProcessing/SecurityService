namespace SecurityService.Controllers.Grants
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading.Tasks;
    using IdentityServer4.Events;
    using IdentityServer4.Extensions;
    using IdentityServer4.Services;
    using IdentityServer4.Stores;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using ViewModels;

    /// <summary>
    /// This sample controller allows a user to revoke grants given to clients
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.Controller" />
    [Route(GrantsController.ControllerRoute)]
    [ExcludeFromCodeCoverage]
    [SecurityHeaders]
    [Authorize]
    public class GrantsController : Controller
    {
        #region Fields

        /// <summary>
        /// The clients
        /// </summary>
        private readonly IClientStore _clients;

        /// <summary>
        /// The events
        /// </summary>
        private readonly IEventService _events;

        /// <summary>
        /// The interaction
        /// </summary>
        private readonly IIdentityServerInteractionService _interaction;

        /// <summary>
        /// The resources
        /// </summary>
        private readonly IResourceStore _resources;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="GrantsController"/> class.
        /// </summary>
        /// <param name="interaction">The interaction.</param>
        /// <param name="clients">The clients.</param>
        /// <param name="resources">The resources.</param>
        /// <param name="events">The events.</param>
        public GrantsController(IIdentityServerInteractionService interaction,
                                IClientStore clients,
                                IResourceStore resources,
                                IEventService events)
        {
            this._interaction = interaction;
            this._clients = clients;
            this._resources = resources;
            this._events = events;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Show list of grants
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("index")]
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
        [Route("revoke")]
        public async Task<IActionResult> Revoke(String clientId)
        {
            await this._interaction.RevokeUserConsentAsync(clientId);
            await this._events.RaiseAsync(new GrantsRevokedEvent(this.User.GetSubjectId(), clientId));

            return this.RedirectToAction("Index");
        }

        /// <summary>
        /// Builds the view model asynchronous.
        /// </summary>
        /// <returns></returns>
        private async Task<GrantsViewModel> BuildViewModelAsync()
        {
            var grants = await this._interaction.GetAllUserGrantsAsync();

            var list = new List<GrantViewModel>();
            foreach (var grant in grants)
            {
                var client = await this._clients.FindClientByIdAsync(grant.ClientId);
                if (client != null)
                {
                    var resources = await this._resources.FindResourcesByScopeAsync(grant.Scopes);

                    var item = new GrantViewModel
                               {
                                   ClientId = client.ClientId,
                                   ClientName = client.ClientName ?? client.ClientId,
                                   ClientLogoUrl = client.LogoUri,
                                   ClientUrl = client.ClientUri,
                                   Description = grant.Description,
                                   Created = grant.CreationTime,
                                   Expires = grant.Expiration,
                                   IdentityGrantNames = resources.IdentityResources.Select(x => x.DisplayName ?? x.Name).ToArray(),
                                   ApiGrantNames = resources.ApiScopes.Select(x => x.DisplayName ?? x.Name).ToArray()
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

        #region Others

        /// <summary>
        /// The controller name
        /// </summary>
        public const String ControllerName = "grants";

        /// <summary>
        /// The controller route
        /// </summary>
        private const String ControllerRoute = GrantsController.ControllerName;

        #endregion
    }
}