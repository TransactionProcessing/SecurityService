namespace SecurityService.Service.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using DataTransferObjects;
    using Manager;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Swashbuckle.AspNetCore.Annotations;
    using Swashbuckle.AspNetCore.Filters;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.ControllerBase" />
    [Route(UserController.ControllerRoute)]
    [ApiController]
    [ApiVersion("1.0")]
    public class UserController : ControllerBase
    {
        #region Fields

        /// <summary>
        /// The manager
        /// </summary>
        private readonly ISecurityServiceManager Manager;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="UserController" /> class.
        /// </summary>
        /// <param name="manager">The manager.</param>
        public UserController(ISecurityServiceManager manager)
        {
            this.Manager = manager;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates the user.
        /// </summary>
        /// <param name="createUserRequest">The create user request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("")]
        [SwaggerResponse(201, type: typeof(CreateUserResponse))]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest createUserRequest, CancellationToken cancellationToken)
        {
            // Create the user
            Guid userId = await this.Manager.CreateUser(createUserRequest, cancellationToken);

            // return the result
            return this.Created($"{UserController.ControllerRoute}/{userId}",
                                new CreateUserResponse
                                {
                                    UserId =  userId
                                });
        }

        /// <summary>
        /// Gets the user.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        [HttpGet]
        [Route("{userId}")]
        [SwaggerResponse(200, type: typeof(UserDetails))]
        public async Task<IActionResult> GetUser([FromRoute] Guid userId,
                                                 CancellationToken cancellationToken)
        {
            UserDetails userDetails = await this.Manager.GetUser(userId, cancellationToken);

            return this.Ok(userDetails);
        }

        /// <summary>
        /// Gets the users.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        [HttpGet]
        [Route("")]
        [SwaggerResponse(200, type: typeof(List<UserDetails>))]
        public async Task<IActionResult> GetUsers([FromQuery] String userName,
                                                 CancellationToken cancellationToken)
        {
            List<UserDetails> userList = await this.Manager.GetUsers(userName, cancellationToken);

            return this.Ok(userList);
        }

        #endregion

        #region Others

        /// <summary>
        /// The controller name
        /// </summary>
        public const String ControllerName = "users";

        /// <summary>
        /// The controller route
        /// </summary>
        private const String ControllerRoute = "api/" + UserController.ControllerName;

        #endregion
    }
}