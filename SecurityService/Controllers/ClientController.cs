namespace SecurityService.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;
    using DataTransferObjects;
    using DataTransferObjects.Requests;
    using DataTransferObjects.Responses;
    using Factories;
    using IdentityServer4.Models;
    using Manager;
    using Microsoft.AspNetCore.Mvc;
    using Swashbuckle.AspNetCore.Annotations;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.ControllerBase" />
    [Route(ClientController.ControllerRoute)]
    [ApiController]
    [ApiVersion("1.0")]
    [ExcludeFromCodeCoverage]
    public class ClientController : ControllerBase
    {
        #region Fields

        /// <summary>
        /// The manager
        /// </summary>
        private readonly ISecurityServiceManager Manager;

        /// <summary>
        /// The model factory
        /// </summary>
        private readonly IModelFactory ModelFactory;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="UserController" /> class.
        /// </summary>
        /// <param name="manager">The manager.</param>
        /// <param name="modelFactory">The model factory.</param>
        public ClientController(ISecurityServiceManager manager, IModelFactory modelFactory)
        {
            this.Manager = manager;
            this.ModelFactory = modelFactory;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates the user.
        /// </summary>
        /// <param name="createClientRequest">The create client request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("")]
        [SwaggerResponse(201, type: typeof(CreateClientResponse))]
        public async Task<IActionResult> CreateClient([FromBody] CreateClientRequest createClientRequest, CancellationToken cancellationToken)
        {
            // Create the client
            String clientId = await this.Manager.CreateClient(createClientRequest.ClientId,
                                                              createClientRequest.Secret,
                                                              createClientRequest.ClientName,
                                                              createClientRequest.ClientDescription,
                                                              createClientRequest.AllowedScopes,
                                                              createClientRequest.AllowedGrantTypes,
                                                              createClientRequest.ClientRedirectUris,
                                                              createClientRequest.ClientPostLogoutRedirectUris,
                                                              createClientRequest.RequireConsent,
                                                              cancellationToken);

            // return the result
            return this.Created($"{ClientController.ControllerRoute}/{clientId}",
                                new CreateClientResponse
                                {
                                    ClientId = clientId
                                });
        }

        /// <summary>
        /// Gets the client.
        /// </summary>
        /// <param name="clientId">The client identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        [HttpGet]
        [Route("{clientId}")]
        [SwaggerResponse(200, type: typeof(ClientDetails))]
        public async Task<IActionResult> GetClient([FromRoute] String clientId,
                                                 CancellationToken cancellationToken)
        {
            Client clientModel = await this.Manager.GetClient(clientId, cancellationToken);

            return this.Ok(this.ModelFactory.ConvertFrom(clientModel));
        }

        /// <summary>
        /// Gets the clients.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        [HttpGet]
        [Route("")]
        [SwaggerResponse(200, type: typeof(List<ClientDetails>))]
        public async Task<IActionResult> GetClients(CancellationToken cancellationToken)
        {
            List<Client> clientList = await this.Manager.GetClients(cancellationToken);

            return this.Ok(this.ModelFactory.ConvertFrom(clientList));
        }

        #endregion

        #region Others

        /// <summary>
        /// The controller name
        /// </summary>
        public const String ControllerName = "clients";

        /// <summary>
        /// The controller route
        /// </summary>
        private const String ControllerRoute = "api/" + ClientController.ControllerName;

        #endregion
    }
}