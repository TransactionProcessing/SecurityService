﻿namespace SecurityService.Controllers
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
    using DataTransferObjects.Responses;
    using Duende.IdentityServer.Models;
    using Factories;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;
    using Swashbuckle.AspNetCore.Annotations;
    using Swashbuckle.AspNetCore.Filters;
    using CreateIdentityResourceRequest = DataTransferObjects.Requests.CreateIdentityResourceRequest;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.ControllerBase" />
    [Route(IdentityResourceController.ControllerRoute)]
    [ApiController]
    [ExcludeFromCodeCoverage]
    public class IdentityResourceController : ControllerBase
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
        /// Initializes a new instance of the <see cref="ApiResourceController" /> class.
        /// </summary>
        /// <param name="manager">The manager.</param>
        /// <param name="modelFactory">The model factory.</param>
        public IdentityResourceController(IMediator mediator,
                                          IModelFactory modelFactory)
        {
            this.Mediator = mediator;
            this.ModelFactory = modelFactory;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates the identity resource.
        /// </summary>
        /// <param name="createIdentityResourceRequest">The create identity resource request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("")]
        [SwaggerResponse(201, type: typeof(CreateIdentityResourceResponse))]
        [SwaggerResponseExample(201, typeof(CreateIdentityResourceResponseExample))]
        public async Task<IActionResult> CreateIdentityResource([FromBody] CreateIdentityResourceRequest createIdentityResourceRequest,
                                                                CancellationToken cancellationToken)
        {
            BusinessLogic.Requests.CreateIdentityResourceRequest request = BusinessLogic.Requests.CreateIdentityResourceRequest.Create(createIdentityResourceRequest.Name,
                                                                                                                                       createIdentityResourceRequest.DisplayName,
                                                                                                                                       createIdentityResourceRequest.Description,
                                                                                                                                       createIdentityResourceRequest.Required,
                                                                                                                                       createIdentityResourceRequest.Emphasize,
                                                                                                                                       createIdentityResourceRequest.ShowInDiscoveryDocument,
                                                                                                                                       createIdentityResourceRequest.Claims);

            await this.Mediator.Send(request, cancellationToken);

            // return the result
            return this.Created($"{IdentityResourceController.ControllerRoute}/{createIdentityResourceRequest.Name}",
                                new CreateIdentityResourceResponse
                                {
                                    IdentityResourceName = createIdentityResourceRequest.Name
                                });
        }

        /// <summary>
        /// Gets the identity resource.
        /// </summary>
        /// <param name="identityResourceName">Name of the identity resource.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        [HttpGet]
        [Route("{identityResourceName}")]
        [SwaggerResponse(200, type: typeof(IdentityResourceDetails))]
        [SwaggerResponseExample(200, typeof(IdentityResourceDetailsResponseExample))]
        public async Task<IActionResult> GetIdentityResource([FromRoute] String identityResourceName,
                                                             CancellationToken cancellationToken)
        {
            GetIdentityResourceRequest request = GetIdentityResourceRequest.Create(identityResourceName);

            IdentityResource identityResourceModel = await this.Mediator.Send(request, cancellationToken);

            // return the result
            return this.Ok(this.ModelFactory.ConvertFrom(identityResourceModel));
        }

        /// <summary>
        /// Gets the identity resources.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        [HttpGet]
        [Route("")]
        [SwaggerResponse(200, type: typeof(List<IdentityResourceDetails>))]
        [SwaggerResponseExample(200, typeof(IdentityResourceDetailsListResponseExample))]
        public async Task<IActionResult> GetIdentityResources(CancellationToken cancellationToken)
        {
            GetIdentityResourcesRequest request = GetIdentityResourcesRequest.Create();

            List<IdentityResource> identityResourceList = await this.Mediator.Send(request, cancellationToken);

            return this.Ok(this.ModelFactory.ConvertFrom(identityResourceList));
        }

        #endregion

        #region Others

        /// <summary>
        /// The controller name
        /// </summary>
        public const String ControllerName = "identityresources";

        /// <summary>
        /// The controller route
        /// </summary>
        private const String ControllerRoute = "api/" + IdentityResourceController.ControllerName;

        #endregion
    }
}