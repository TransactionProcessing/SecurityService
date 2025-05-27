using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Shared.Results;
using Shared.Results.Web;
using SimpleResults;

namespace SecurityService.Controllers
{
    using System.Threading;
    using Azure.Core;
    using BusinessLogic;
    using BusinessLogic.Requests;
    using Common.Examples;
    using DataTransferObjects.Responses;
    using Factories;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;
    using Swashbuckle.AspNetCore.Annotations;
    using Swashbuckle.AspNetCore.Filters;
    using CreateRoleRequest = DataTransferObjects.Requests.CreateRoleRequest;

    [Route(RoleController.ControllerRoute)]
    [ApiController]
    [ExcludeFromCodeCoverage]
    public class RoleController : ControllerBase
    {
        #region Fields

        private readonly IMediator Mediator;

        private readonly IModelFactory ModelFactory;

        #endregion

        #region Constructors

        public RoleController(IMediator mediator, IModelFactory modelFactory)
        {
            this.Mediator = mediator;
            this.ModelFactory = modelFactory;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates the role.
        /// </summary>
        /// <param name="createRoleRequest">The create role request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("")]
        [SwaggerResponse(201, type: typeof(CreateRoleResponse))]
        [SwaggerResponseExample(statusCode: 201, typeof(CreateRoleResponseExample))]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequest createRoleRequest, CancellationToken cancellationToken)
        {
            Guid roleId = Guid.NewGuid();
            SecurityServiceCommands.CreateRoleCommand command  = new(roleId, createRoleRequest.RoleName);

            Result result = await this.Mediator.Send(command, cancellationToken);
            if (result.IsFailed)
                return result.ToActionResultX();
            
            // return the result
            return result.ToActionResultX();
        }

        /// <summary>
        /// Gets the role.
        /// </summary>
        /// <param name="roleId">The role identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        [HttpGet]
        [Route("{roleId}")]
        [SwaggerResponse(200, type: typeof(RoleDetails))]
        [SwaggerResponseExample(statusCode: 200, typeof(RoleDetailsResponseExample))]
        public async Task<IActionResult> GetRole([FromRoute] Guid roleId,
                                                 CancellationToken cancellationToken)
        {
            SecurityServiceQueries.GetRoleQuery query = new(roleId);

            Result<Models.RoleDetails> result = await this.Mediator.Send(query, cancellationToken);

            if (result.IsFailed)
                return result.ToActionResultX();

            RoleDetails model = this.ModelFactory.ConvertFrom(result.Data);

            return Result.Success(model).ToActionResultX();
        }

        /// <summary>
        /// Gets the roles.
        /// </summary>
        /// <param name="roleId">The role identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        [HttpGet]
        [Route("")]
        [SwaggerResponse(200, type: typeof(List<RoleDetails>))]
        [SwaggerResponseExample(statusCode: 200, typeof(RoleDetailsListResponseExample))]
        public async Task<IActionResult> GetRoles(CancellationToken cancellationToken)
        {
            SecurityServiceQueries.GetRolesQuery query = new();

            Result<List<Models.RoleDetails>> result = await this.Mediator.Send(query, cancellationToken);

            if (result.IsFailed)
                return result.ToActionResultX();

            List<RoleDetails> model = this.ModelFactory.ConvertFrom(result.Data);

            return Result.Success(model).ToActionResultX();
        }

        #endregion

        #region Others

        /// <summary>
        /// The controller name
        /// </summary>
        public const String ControllerName = "roles";

        /// <summary>
        /// The controller route
        /// </summary>
        private const String ControllerRoute = "api/" + RoleController.ControllerName;

        #endregion
    }
}
