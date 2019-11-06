namespace SecurityService.Service.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;
    using DataTransferObjects;
    using DataTransferObjects.Responses;
    using Factories;
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
    [ExcludeFromCodeCoverage]
    public class UserController : ControllerBase
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
        /// Initializes a new instance of the <see cref="UserController" /> class.
        /// </summary>
        /// <param name="manager">The manager.</param>
        /// <param name="modelFactory">The model factory.</param>
        public UserController(ISecurityServiceManager manager, IModelFactory modelFactory)
        {
            this.Manager = manager;
            this.ModelFactory = modelFactory;
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
            Guid userId = await this.Manager.CreateUser(createUserRequest.GivenName,
                                                        createUserRequest.MiddleName,
                                                        createUserRequest.FamilyName,
                                                        createUserRequest.EmailAddress,
                                                        createUserRequest.Password,
                                                        createUserRequest.EmailAddress,
                                                        createUserRequest.PhoneNumber,
                                                        createUserRequest.Claims,
                                                        createUserRequest.Roles,
                                                        cancellationToken);

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
            Models.UserDetails userDetailsModel = await this.Manager.GetUser(userId, cancellationToken);

            return this.Ok(this.ModelFactory.ConvertFrom(userDetailsModel));
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
            List<Models.UserDetails> userModelList = await this.Manager.GetUsers(userName, cancellationToken);
            
            return this.Ok(this.ModelFactory.ConvertFrom(userModelList));
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