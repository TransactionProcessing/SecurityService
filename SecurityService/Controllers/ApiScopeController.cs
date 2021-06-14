namespace SecurityService.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessLogic;
    using Common.Examples;
    using DataTransferObjects.Requests;
    using DataTransferObjects.Responses;
    using Duende.IdentityServer.Models;
    using Factories;
    using Microsoft.AspNetCore.Mvc;
    using Swashbuckle.AspNetCore.Annotations;
    using Swashbuckle.AspNetCore.Filters;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.ControllerBase" />
    [Route(ApiScopeController.ControllerRoute)]
    [ApiController]
    [ExcludeFromCodeCoverage]
    public class ApiScopeController : ControllerBase
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
        /// Initializes a new instance of the <see cref="ApiResourceController" /> class.
        /// </summary>
        /// <param name="manager">The manager.</param>
        /// <param name="modelFactory">The model factory.</param>
        public ApiScopeController(ISecurityServiceManager manager, IModelFactory modelFactory)
        {
            this.Manager = manager;
            this.ModelFactory = modelFactory;
        }

        [HttpPost]
        [Route("")]
        [SwaggerResponse(201, type: typeof(CreateApiScopeResponse))]
        [SwaggerResponseExample(201, typeof(CreateApiScopeResponseExample))]
        public async Task<IActionResult> CreateApiScope([FromBody] CreateApiScopeRequest createApiScopeRequest,
                                                           CancellationToken cancellationToken)
        {
            String apiScopeName = await this.Manager.CreateApiScope(createApiScopeRequest.Name,
                                                                       createApiScopeRequest.DisplayName,
                                                                       createApiScopeRequest.Description,
                                                                       cancellationToken);

            // return the result
            return this.Created($"{ApiScopeController.ControllerRoute}/{apiScopeName}", new CreateApiScopeResponse
                                                                                        {
                                                                                            ApiScopeName = apiScopeName
                                                                                        });
        }

        /// <summary>
        /// Gets the API scope.
        /// </summary>
        /// <param name="apiScopeName">Name of the API scope.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        [HttpGet]
        [Route("{apiScopeName}")]
        [SwaggerResponse(200, type: typeof(ApiScopeDetails))]
        [SwaggerResponseExample(200, typeof(ApiScopeDetailsResponseExample))]
        public async Task<IActionResult> GetApiScope([FromRoute] String apiScopeName,
                                                     CancellationToken cancellationToken)
        {
            ApiScope apiScopeModel = await this.Manager.GetApiScope(apiScopeName, cancellationToken);

            // return the result
            return this.Ok(this.ModelFactory.ConvertFrom(apiScopeModel));
        }

        /// <summary>
        /// Gets the api scopes.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        [HttpGet]
        [Route("")]
        [SwaggerResponse(200, type: typeof(List<ApiScopeDetails>))]
        [SwaggerResponseExample(200, typeof(ApiScopeDetailsListResponseExample))]
        public async Task<IActionResult> GetApiScopes(CancellationToken cancellationToken)
        {
            List<ApiScope> apiScopeList = await this.Manager.GetApiScopes(cancellationToken);

            return this.Ok(this.ModelFactory.ConvertFrom(apiScopeList));
        }

        #region Others

        /// <summary>
        /// The controller name
        /// </summary>
        public const String ControllerName = "apiscopes";

        /// <summary>
        /// The controller route
        /// </summary>
        private const String ControllerRoute = "api/" + ApiScopeController.ControllerName;

        #endregion
    }
}