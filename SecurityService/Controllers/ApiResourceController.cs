using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SecurityService.Controllers
{
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using Common.Examples;
    using DataTransferObjects;
    using DataTransferObjects.Requests;
    using DataTransferObjects.Responses;
    using Factories;
    using IdentityServer4.Models;
    using Manager;
    using Microsoft.AspNetCore.Mvc;
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
        /// <summary>
        /// The manager
        /// </summary>
        private readonly ISecurityServiceManager Manager;

        /// <summary>
        /// The model factory
        /// </summary>
        private readonly IModelFactory ModelFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiResourceController"/> class.
        /// </summary>
        /// <param name="manager">The manager.</param>
        /// <param name="modelFactory">The model factory.</param>
        public ApiResourceController(ISecurityServiceManager manager, IModelFactory modelFactory)
        {
            this.Manager = manager;
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
        [ProducesResponseType(typeof(CreateApiResourceResponse), 201)]
        public async Task<IActionResult> CreateApiResource([FromBody] CreateApiResourceRequest createApiResourceRequest,
                                                     CancellationToken cancellationToken)
        {
            String apiResourceName = await this.Manager.CreateApiResource(createApiResourceRequest.Name,
                                                 createApiResourceRequest.DisplayName,
                                                 createApiResourceRequest.Description,
                                                 createApiResourceRequest.Secret,
                                                 createApiResourceRequest.Scopes,
                                                 createApiResourceRequest.UserClaims,
                                                 cancellationToken);

            // return the result
            return this.Created($"{ApiResourceController.ControllerRoute}/{apiResourceName}", new CreateApiResourceResponse
                                                                                  {
                                                                                      ApiResourceName = apiResourceName
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
        [ProducesResponseType(typeof(ApiResourceDetails), 200)]
        public async Task<IActionResult> GetApiResource([FromRoute] String apiResourceName,
                                                           CancellationToken cancellationToken)
        {
            ApiResource apiResourceModel = await this.Manager.GetApiResource(apiResourceName,cancellationToken);

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
        [ProducesResponseType(typeof(List<ApiResourceDetails>), 200)]
        public async Task<IActionResult> GetApiResources(CancellationToken cancellationToken)
        {
            List<ApiResource> apiResourceList = await this.Manager.GetApiResources(cancellationToken);

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
