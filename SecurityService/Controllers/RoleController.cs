using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace SecurityService.Controllers
{
    using System.Threading;
    using DataTransferObjects.Requests;
    using DataTransferObjects.Responses;
    using Factories;
    using Manager;
    using Microsoft.AspNetCore.Mvc;
    using Service.Controllers;
    using Swashbuckle.AspNetCore.Annotations;

    [Route(RoleController.ControllerRoute)]
    [ApiController]
    [ApiVersion("1.0")]
    [ExcludeFromCodeCoverage]
    public class RoleController : ControllerBase
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
        /// Initializes a new instance of the <see cref="RoleController" /> class.
        /// </summary>
        /// <param name="manager">The manager.</param>
        /// <param name="modelFactory">The model factory.</param>
        public RoleController(ISecurityServiceManager manager, IModelFactory modelFactory)
        {
            this.Manager = manager;
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
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequest createRoleRequest, CancellationToken cancellationToken)
        {
            // Create the role
            Guid roleId = await this.Manager.CreateRole(createRoleRequest.RoleName,
                                                        cancellationToken);

            // return the result
            return this.Created($"{RoleController.ControllerRoute}/{roleId}",
                                new CreateRoleResponse
                                {
                                    RoleId = roleId
                                });
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
        public async Task<IActionResult> GetRole([FromRoute] Guid roleId,
                                                 CancellationToken cancellationToken)
        {
            Models.RoleDetails roleDetailsModel = await this.Manager.GetRole(roleId, cancellationToken);

            return this.Ok(this.ModelFactory.ConvertFrom(roleDetailsModel));
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
        public async Task<IActionResult> GetRoles(CancellationToken cancellationToken)
        {
            List<Models.RoleDetails> roleDetailsModel = await this.Manager.GetRoles(cancellationToken);

            return this.Ok(this.ModelFactory.ConvertFrom(roleDetailsModel));
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
