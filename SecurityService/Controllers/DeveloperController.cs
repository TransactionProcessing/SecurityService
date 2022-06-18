using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SecurityService.Controllers
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;
    using Bootstrapper;
    using MessagingService.Client;
    using Microsoft.Extensions.Hosting;

    [Route(DeveloperController.ControllerRoute)]
    [ApiController]
    [ExcludeFromCodeCoverage]
    public class DeveloperController : ControllerBase
    {
        private readonly IMessagingServiceClient MessagingServiceClient;

        public DeveloperController(IMessagingServiceClient messagingServiceClient) {
            this.MessagingServiceClient = messagingServiceClient;
        }

        [HttpGet]
        [Route("lastemail")]
        public async Task<IActionResult> GetLastEmailMessage(CancellationToken cancellationToken) {
            if (Startup.WebHostEnvironment.IsEnvironment("IntegrationTest") && this.MessagingServiceClient.GetType() == typeof(TestMessagingServiceClient)) {
                var lastEmailRequest = ((TestMessagingServiceClient)this.MessagingServiceClient).LastEmailRequest;

                return Ok(lastEmailRequest);
            }

            return this.NotFound();
        }

        [HttpGet]
        [Route("lastsms")]
        public async Task<IActionResult> GetLastSMSMessage(CancellationToken cancellationToken)
        {
            if (Startup.WebHostEnvironment.IsEnvironment("IntegrationTest") && this.MessagingServiceClient.GetType() == typeof(TestMessagingServiceClient))
            {
                var lastSmsRequest = ((TestMessagingServiceClient)this.MessagingServiceClient).LastSMSRequest;

                return Ok(lastSmsRequest);
            }

            return this.NotFound();
        }

        #region Others

        /// <summary>
        /// The controller name
        /// </summary>
        public const String ControllerName = "developer";

        /// <summary>
        /// The controller route
        /// </summary>
        private const String ControllerRoute = "api/" + DeveloperController.ControllerName;

        #endregion
    }

}
