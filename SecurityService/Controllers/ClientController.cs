namespace SecurityService.Service.Controllers
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
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Swashbuckle.AspNetCore.Annotations;
    using Swashbuckle.AspNetCore.Filters;

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

        private readonly IModelFactory ModelFactory;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="UserController" /> class.
        /// </summary>
        /// <param name="manager">The manager.</param>
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
                                                              cancellationToken);

            // return the result
            return this.Created($"{ClientController.ControllerRoute}/{clientId}",
                                new CreateClientResponse
                                {
                                    ClientId = clientId
                                });
        }

        [HttpGet]
        [Route("{clientId}")]
        [SwaggerResponse(200, type: typeof(ClientDetails))]
        public async Task<IActionResult> GetClient([FromRoute] String clientId,
                                                 CancellationToken cancellationToken)
        {
            Client clientModel = await this.Manager.GetClient(clientId, cancellationToken);

            return this.Ok(this.ModelFactory.ConvertFrom(clientModel));
        }

        [HttpGet]
        [Route("")]
        [SwaggerResponse(200, type: typeof(ClientDetails))]
        public async Task<IActionResult> GetClients(CancellationToken cancellationToken)
        {
            List<Client> clientList = await this.Manager.GetClients(cancellationToken);

            return this.Ok(this.ModelFactory.ConvertFrom(clientList));
        }

        ///// <summary>
        ///// Gets the users.
        ///// </summary>
        ///// <param name="userName">Name of the user.</param>
        ///// <param name="cancellationToken">The cancellation token.</param>
        ///// <returns></returns>
        //[HttpGet]
        //[Route("")]
        //[SwaggerResponse(200, type: typeof(List<UserDetails>))]
        //public async Task<IActionResult> GetUsers([FromQuery] String userName,
        //                                         CancellationToken cancellationToken)
        //{
        //    List<UserDetails> userList = await this.Manager.GetUsers(userName, cancellationToken);

        //    return this.Ok(userList);
        //}

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