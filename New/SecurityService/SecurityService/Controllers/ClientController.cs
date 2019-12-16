using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SecurityService.Manager;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SecurityService.Controllers
{
    using System.Diagnostics.CodeAnalysis;
    using SecurityService.DataTransferObjects.Requests;
    using System.Threading;
    using DataTransferObjects.Responses;

    [Route(ClientController.ControllerRoute)]
    [ApiController]
    [ExcludeFromCodeCoverage]
    public class ClientController : ControllerBase
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
        /// <param name="modelFactory">The model factory.</param>
        public ClientController(ISecurityServiceManager manager)
        {
            this.Manager = manager;
        }

        #endregion

        [HttpPost]
        [Route("")]
        public async Task<IActionResult> CreateClient([FromBody] CreateClientRequest createClientRequest, CancellationToken cancellationToken)
        {
            // Create the client
            String clientId = await this.Manager.CreateClient(createClientRequest.ClientId,
                                                              createClientRequest.Secret,
                                                              createClientRequest.ClientName,
                                                              createClientRequest.ClientDescription,
                                                              createClientRequest.AllowedScopes,
                                                              createClientRequest.AllowedGrantTypes,
                                                              cancellationToken);

            // return the result
            return this.Created($"{ClientController.ControllerRoute}/{clientId}",
                                new CreateClientResponse
                                {
                                    ClientId = clientId
                                });
        }

        #region Others

        /// <summary>
        /// The controller name
        /// </summary>
        public const String ControllerName = "clients";

        /// <summary>
        /// The controller route
        /// </summary>
        private const String ControllerRoute = "api/" + ClientController.ControllerName;

        #endregion
    }

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
        /// Initializes a new instance of the <see cref="ApiResourceController"/> class.
        /// </summary>
        /// <param name="manager">The manager.</param>
        /// <param name="modelFactory">The model factory.</param>
        public ApiResourceController(ISecurityServiceManager manager)
        {
            this.Manager = manager;
        }

        /// <summary>
        /// Creates the API resource.
        /// </summary>
        /// <param name="createApiResourceRequest">The create API resource request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("")]
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
            return this.Created($"{ApiResourceController.ControllerRoute}/{apiResourceName}",
                                new 
                                {
                                    ApiResourceName = apiResourceName
                                });
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
