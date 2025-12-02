/*using MediatR;
using Microsoft.AspNetCore.Http;
using SecurityService.BusinessLogic.RequestHandlers;
using SecurityService.BusinessLogic.Requests;
using Shared.Results.Web;
using SimpleResults;

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
    using Shared.Results;
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
            this.Mediator = mediator;
            this.ModelFactory = modelFactory;
        }

        [HttpPost]
        [Route("")]
        [SwaggerResponse(201, type: typeof(CreateApiScopeResponse))]
        [SwaggerResponseExample(201, typeof(CreateApiScopeResponseExample))]
        public async Task<IResult> CreateApiScope([FromBody] CreateApiScopeRequest createApiScopeRequest,
                                                           CancellationToken cancellationToken)
        {
            SecurityServiceCommands.CreateApiScopeCommand command = new(createApiScopeRequest.Name,
                createApiScopeRequest.DisplayName,
                createApiScopeRequest.Description);

            Result result = await this.Mediator.Send(command, cancellationToken);

            return ResponseFactory.FromResult(result);
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
        public async Task<IResult> GetApiScope([FromRoute] String apiScopeName,
                                                     CancellationToken cancellationToken)
        {
            SecurityServiceQueries.GetApiScopeQuery query = new(apiScopeName);

            var result = await this.Mediator.Send(query, cancellationToken);

            return ResponseFactory.FromResult(result, this.ModelFactory.ConvertFrom);
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
        public async Task<IResult> GetApiScopes(CancellationToken cancellationToken)
        {
            SecurityServiceQueries.GetApiScopesQuery query = new();

            Result<List<ApiScope>> result = await this.Mediator.Send(query, cancellationToken);

            return ResponseFactory.FromResult(result, this.ModelFactory.ConvertFrom);
        }

        #region Others

        /// <summary>
        /// The controller name
        /// </summary>
        private const String ControllerName = "apiscopes";

        /// <summary>
        /// The controller route
        /// </summary>
        private const String ControllerRoute = "api/" + ApiScopeController.ControllerName;

        #endregion
    }
}*/