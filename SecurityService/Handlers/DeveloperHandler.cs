using Microsoft.Extensions.Hosting;
using SecurityService.BusinessLogic;
using System.Threading;
using System.Threading.Tasks;
using MessagingService.Client;
using Microsoft.AspNetCore.Http;

namespace SecurityService.Handlers
{
    using Bootstrapper;
    using Microsoft.AspNetCore.Mvc;

    public static class DeveloperHandler
    {
        public static Task<IResult> GetLastEmailMessage(IMessagingServiceClient messagingServiceClient,
                                                        CancellationToken cancellationToken)
        {
            if (Startup.WebHostEnvironment.IsEnvironment("IntegrationTest")
                && messagingServiceClient.GetType() == typeof(TestMessagingServiceClient))
            {
                var lastEmailRequest = ((TestMessagingServiceClient)messagingServiceClient).LastEmailRequest;
                return Task.FromResult(Results.Ok(lastEmailRequest) as IResult);
            }

            return Task.FromResult(Results.NotFound() as IResult);
        }

        public static Task<IResult> GetLastSMSMessage(IMessagingServiceClient messagingServiceClient,
                                                      CancellationToken cancellationToken)
        {
            if (Startup.WebHostEnvironment.IsEnvironment("IntegrationTest")
                && messagingServiceClient.GetType() == typeof(TestMessagingServiceClient))
            {
                var lastSmsRequest = ((TestMessagingServiceClient)messagingServiceClient).LastSMSRequest;
                return Task.FromResult(Results.Ok(lastSmsRequest) as IResult);
            }

            return Task.FromResult(Results.NotFound() as IResult);
        }
    }
}