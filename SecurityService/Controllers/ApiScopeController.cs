using MediatR;
using SecurityService.BusinessLogic.RequestHandlers;

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
        private readonly IMediator Mediator;
        private readonly IModelFactory ModelFactory;

        public ApiScopeController(IMediator mediator, IModelFactory modelFactory)
        {
            Mediator = mediator;
            this.ModelFactory = modelFactory;
        }

        [HttpPost]
        [Route("")]
        [SwaggerResponse(201, type: typeof(CreateApiScopeResponse))]
        [SwaggerResponseExample(201, typeof(CreateApiScopeResponseExample))]
        public async Task<IActionResult> CreateApiScope([FromBody] CreateApiScopeRequest createApiScopeRequest,
                                                           CancellationToken cancellationToken)
        {
            BusinessLogic.RequestHandlers.CreateApiScopeRequest request = BusinessLogic.RequestHandlers.CreateApiScopeRequest.Create(createApiScopeRequest.Name,
                createApiScopeRequest.DisplayName,
                createApiScopeRequest.Description);

            await this.Mediator.Send(request, cancellationToken);

            // return the result
            return this.Created($"{ApiScopeController.ControllerRoute}/{createApiScopeRequest.Name}", new CreateApiScopeResponse
                                                                                        {
                                                                                            ApiScopeName = createApiScopeRequest.Name
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
            GetApiScopeRequest request = GetApiScopeRequest.Create(apiScopeName);

            ApiScope apiScopeModel = await this.Mediator.Send(request, cancellationToken);

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
            GetApiScopesRequest request = GetApiScopesRequest.Create();

            List<ApiScope> apiScopeList = await this.Mediator.Send(request, cancellationToken);

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