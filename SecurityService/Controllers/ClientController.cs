/*using Microsoft.AspNetCore.Http;
using Shared.Results.Web;
using SimpleResults;

namespace SecurityService.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;
    using Azure.Core;
    using BusinessLogic;
    using BusinessLogic.Requests;
    using Common.Examples;
    using DataTransferObjects;
    using DataTransferObjects.Responses;
    using Duende.IdentityServer.EntityFramework.Entities;
    using Duende.IdentityServer.Models;
    using Factories;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;
    using Shared.Results;
    using Swashbuckle.AspNetCore.Annotations;
    using Swashbuckle.AspNetCore.Filters;
    using CreateClientRequest = DataTransferObjects.Requests.CreateClientRequest;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.ControllerBase" />
    [Route(ClientController.ControllerRoute)]
    [ApiController]
    [ExcludeFromCodeCoverage]
    public class ClientController : ControllerBase
    {
        #region Fields
        
        private readonly IMediator Mediator;

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
        public ClientController(IMediator mediator, IModelFactory modelFactory)
        {
            this.Mediator = mediator;
            this.ModelFactory = modelFactory;
        }

        #endregion

        #region Methods
        
        /// <summary>
        /// Creates the client.
        /// </summary>
        /// <param name="createClientRequest">The create client request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("")]
        [SwaggerResponse(201, type: typeof(CreateClientResponse))]
        [SwaggerResponseExample(201, typeof(CreateClientResponseExample))]
        public async Task<IResult> CreateClient([FromBody] CreateClientRequest createClientRequest, CancellationToken cancellationToken)
        {
            SecurityServiceCommands.CreateClientCommand command = new(createClientRequest.ClientId,
                                                                                                                   createClientRequest.Secret,
                                                                                                                   createClientRequest.ClientName,
                                                                                                                   createClientRequest.ClientDescription,
                                                                                                                   createClientRequest.AllowedScopes,
                                                                                                                   createClientRequest.AllowedGrantTypes,
                                                                                                                   createClientRequest.ClientUri,
                                                                                                                   createClientRequest.ClientRedirectUris,
                                                                                                                   createClientRequest.ClientPostLogoutRedirectUris,
                                                                                                                   createClientRequest.RequireConsent,
                                                                                                                   createClientRequest.AllowOfflineAccess);
            
            // Create the client
            Result result = await this.Mediator.Send(command, cancellationToken);

            //if (result.IsFailed)
            //    return result.ToActionResultX();

            //// return the result
            //return this.Created($"{ClientController.ControllerRoute}/{createClientRequest.ClientId}",
            //                    new CreateClientResponse
            //                    {
            //                        ClientId = createClientRequest.ClientId
            //                    });
            return ResponseFactory.FromResult(result);
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
        [SwaggerResponseExample(200, typeof(ClientDetailsResponseExample))]
        public async Task<IResult> GetClient([FromRoute] String clientId,
                                                 CancellationToken cancellationToken)
        {
            SecurityServiceQueries.GetClientQuery query = new(clientId);

            Result<Duende.IdentityServer.Models.Client> result= await this.Mediator.Send(query, cancellationToken);

            return ResponseFactory.FromResult(result, this.ModelFactory.ConvertFrom);
        }

        /// <summary>
        /// Gets the clients.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        [HttpGet]
        [Route("")]
        [SwaggerResponse(200, type: typeof(List<ClientDetails>))]
        [SwaggerResponseExample(200, typeof(ClientDetailsListResponseExample))]
        public async Task<IResult> GetClients(CancellationToken cancellationToken)
        {
            SecurityServiceQueries.GetClientsQuery query = new();

            Result<List<Duende.IdentityServer.Models.Client>> result = await this.Mediator.Send(query, cancellationToken);

            return ResponseFactory.FromResult(result, this.ModelFactory.ConvertFrom);
        }

        #endregion

        #region Others

        /// <summary>
        /// The controller name
        /// </summary>
        private const String ControllerName = "clients";

        /// <summary>
        /// The controller route
        /// </summary>
        private const String ControllerRoute = "api/" + ClientController.ControllerName;

        #endregion
    }
}*/