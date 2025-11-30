using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Shared.Results;
using Shared.Results.Web;
using SimpleResults;

namespace SecurityService.Controllers
{
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using Azure.Core;
    using Common.Examples;
    using DataTransferObjects;
    using DataTransferObjects.Requests;
    using DataTransferObjects.Responses;
    using Duende.IdentityServer.Models;
    using Factories;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;
    using SecurityService.BusinessLogic;
    using SecurityService.BusinessLogic.Requests;
    using Swashbuckle.AspNetCore.Annotations;
    using Swashbuckle.AspNetCore.Filters;
    using CreateApiResourceRequest = DataTransferObjects.Requests.CreateApiResourceRequest;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.ControllerBase" />
    [Route(ApiResourceController.ControllerRoute)]
    [ApiController]
    [ExcludeFromCodeCoverage]
    public class ApiResourceController : ControllerBase
    {
        private readonly IMediator Mediator;
        private readonly IModelFactory ModelFactory;

        public ApiResourceController(IMediator mediator, IModelFactory modelFactory)
        {
            this.Mediator = mediator;
            this.ModelFactory = modelFactory;
        }

        /// <summary>
        /// Creates the API resource.
        /// </summary>
        /// <param name="createApiResourceRequest">The create API resource request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("")]
        [SwaggerResponse(201, type: typeof(CreateApiResourceResponse))]
        [SwaggerResponseExample(201, typeof(CreateApiResourceResponseExample))]
        public async Task<IResult> CreateApiResource([FromBody] CreateApiResourceRequest createApiResourceRequest,
                                                     CancellationToken cancellationToken)
        {
            SecurityServiceCommands.CreateApiResourceCommand command = new(createApiResourceRequest.Name,
                createApiResourceRequest.DisplayName,
                createApiResourceRequest.Description,
                createApiResourceRequest.Secret,
                createApiResourceRequest.Scopes,
                createApiResourceRequest.UserClaims);

            Result result = await this.Mediator.Send(command, cancellationToken);

            return ResponseFactory.FromResult(result);
        }

        /// <summary>
        /// Gets the API resource.
        /// </summary>
        /// <param name="apiResourceName">Name of the API resource.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        [HttpGet]
        [Route("{apiResourceName}")]
        [SwaggerResponse(201, type: typeof(ApiResourceDetails))]
        [SwaggerResponseExample(201, typeof(ApiResourceDetailsResponseExample))]
        public async Task<IResult> GetApiResource([FromRoute] String apiResourceName,
                                                           CancellationToken cancellationToken)
        {
            SecurityServiceQueries.GetApiResourceQuery query = new(apiResourceName);

            Result<ApiResource> result= await this.Mediator.Send(query, cancellationToken);
            
            return ResponseFactory.FromResult(result, this.ModelFactory.ConvertFrom);

        }

        /// <summary>
        /// Gets the API resources.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        [HttpGet]
        [Route("")]
        [SwaggerResponse(200, type: typeof(List<ApiResourceDetails>))]
        [SwaggerResponseExample(201, typeof(ApiResourceDetailsListResponseExample))]
        public async Task<IResult> GetApiResources(CancellationToken cancellationToken) {
            SecurityServiceQueries.GetApiResourcesQuery query = new SecurityServiceQueries.GetApiResourcesQuery();

            Result<List<ApiResource>> result = await this.Mediator.Send(query, cancellationToken);

            return ResponseFactory.FromResult(result, this.ModelFactory.ConvertFrom);
        }

        #region Others

        /// <summary>
        /// The controller name
        /// </summary>
        private const String ControllerName = "apiresources";

        /// <summary>
        /// The controller route
        /// </summary>
        private const String ControllerRoute = "api/" + ApiResourceController.ControllerName;

        #endregion
    }
}
