using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SecurityService.BusinessLogic.RequestHandlers;

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
    using Swashbuckle.AspNetCore.Annotations;
    using Swashbuckle.AspNetCore.Filters;

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
        public async Task<IActionResult> CreateApiResource([FromBody] CreateApiResourceRequest createApiResourceRequest,
                                                     CancellationToken cancellationToken)
        {
            BusinessLogic.RequestHandlers.CreateApiResourceRequest request = BusinessLogic.RequestHandlers.CreateApiResourceRequest.Create(createApiResourceRequest.Name,
                createApiResourceRequest.DisplayName,
                createApiResourceRequest.Description,
                createApiResourceRequest.Secret,
                createApiResourceRequest.Scopes,
                createApiResourceRequest.UserClaims);

            await this.Mediator.Send(request, cancellationToken);
            
            // return the result
            return this.Created($"{ApiResourceController.ControllerRoute}/{createApiResourceRequest.Name}", new CreateApiResourceResponse
                                                                                  {
                                                                                      ApiResourceName = createApiResourceRequest.Name
            });
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
        public async Task<IActionResult> GetApiResource([FromRoute] String apiResourceName,
                                                           CancellationToken cancellationToken)
        {
            BusinessLogic.RequestHandlers.GetApiResourceRequest request = GetApiResourceRequest.Create(apiResourceName);

            ApiResource apiResourceModel = await this.Mediator.Send(request, cancellationToken);

            // return the result
            return this.Ok(this.ModelFactory.ConvertFrom(apiResourceModel));
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
        public async Task<IActionResult> GetApiResources(CancellationToken cancellationToken)
        {
            BusinessLogic.RequestHandlers.GetApiResourcesRequest request = GetApiResourcesRequest.Create();

            List<ApiResource> apiResourceList = await this.Mediator.Send(request, cancellationToken);

            return this.Ok(this.ModelFactory.ConvertFrom(apiResourceList));
        }

        #region Others

        /// <summary>
        /// The controller name
        /// </summary>
        public const String ControllerName = "apiresources";

        /// <summary>
        /// The controller route
        /// </summary>
        private const String ControllerRoute = "api/" + ApiResourceController.ControllerName;

        #endregion
    }
}
