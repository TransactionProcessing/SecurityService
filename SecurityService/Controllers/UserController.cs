using Shared.Results;
using Shared.Results.Web;
using SimpleResults;

namespace SecurityService.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;
    using Azure.Core;
    using BusinessLogic;
    using BusinessLogic.Requests;
    using Common.Examples;
    using DataTransferObjects;
    using DataTransferObjects.Responses;
    using Factories;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;
    using Swashbuckle.AspNetCore.Annotations;
    using Swashbuckle.AspNetCore.Filters;
    using CreateUserRequest = DataTransferObjects.CreateUserRequest;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.ControllerBase" />
    [Route(UserController.ControllerRoute)]
    [ApiController]
    [ExcludeFromCodeCoverage]
    public class UserController : ControllerBase
    {
        #region Fields
        
        private readonly IMediator Mediator;

        private readonly IModelFactory ModelFactory;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="UserController" /> class.
        /// </summary>
        /// <param name="manager">The manager.</param>
        /// <param name="modelFactory">The model factory.</param>
        public UserController(IMediator mediator, IModelFactory modelFactory){
            this.Mediator = mediator;
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
        [SwaggerResponseExample(statusCode: 201, typeof(CreateUserResponseExample))]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest createUserRequest, CancellationToken cancellationToken)
        {
            Guid userId = Guid.NewGuid();

            SecurityServiceCommands.CreateUserCommand command =new(userId,
                                                                                                               createUserRequest.GivenName,
                                                                                                               createUserRequest.MiddleName,
                                                                                                               createUserRequest.FamilyName,
                                                                                                               createUserRequest.EmailAddress,
                                                                                                               createUserRequest.Password,
                                                                                                               createUserRequest.EmailAddress,
                                                                                                               createUserRequest.PhoneNumber,
                                                                                                               createUserRequest.Claims,
                                                                                                               createUserRequest.Roles);

            // Create the user
            Result result = await this.Mediator.Send(command, cancellationToken);
            if (result.IsFailed)
                return result.ToActionResultX();

            // return the result
            return result.ToActionResultX();
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
        [SwaggerResponseExample(statusCode: 200, typeof(UserDetailsResponseExample))]
        public async Task<IActionResult> GetUser([FromRoute] Guid userId,
                                                 CancellationToken cancellationToken)
        {
            SecurityServiceQueries.GetUserQuery query = new(userId);

            Result<Models.UserDetails> result = await this.Mediator.Send(query, cancellationToken);

            if (result.IsFailed)
                return result.ToActionResultX();

            UserDetails model = this.ModelFactory.ConvertFrom(result.Data);

            return Result.Success(model).ToActionResultX();
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
        [SwaggerResponseExample(statusCode: 200, typeof(UserDetailsListResponseExample))]

        public async Task<IActionResult> GetUsers([FromQuery] String userName,
                                                 CancellationToken cancellationToken)
        {
            SecurityServiceQueries.GetUsersQuery query = new(userName);
            
            Result<List<Models.UserDetails>> result= await this.Mediator.Send(query, cancellationToken);

            if (result.IsFailed)
                return result.ToActionResultX();

            List<UserDetails> model = this.ModelFactory.ConvertFrom(result.Data);

            return Result.Success(model).ToActionResultX();
        }

        #endregion

        #region Others

        /// <summary>
        /// The controller name
        /// </summary>
        private const String ControllerName = "users";

        /// <summary>
        /// The controller route
        /// </summary>
        private const String ControllerRoute = "api/" + UserController.ControllerName;

        #endregion
    }
}