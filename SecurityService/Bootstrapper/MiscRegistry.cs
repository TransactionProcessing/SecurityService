namespace SecurityService.Bootstrapper
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessLogic;
    using Factories;
    using Lamar;
    using MessagingService.Client;
    using MessagingService.DataTransferObjects;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Shared.General;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Lamar.ServiceRegistry" />
    public class MiscRegistry : ServiceRegistry
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MiscRegistry"/> class.
        /// </summary>
        public MiscRegistry()
        {
            this.AddScoped<ISecurityServiceManager, SecurityServiceManager>();
            this.AddSingleton<IModelFactory, ModelFactory>();

            if (Startup.WebHostEnvironment.IsEnvironment("IntegrationTest")) {
                this.AddSingleton<IMessagingServiceClient, TestMessagingServiceClient>();
            }
            else {
                this.AddSingleton<IMessagingServiceClient, MessagingServiceClient>();
            }

            this.AddSingleton<Func<String, String>>(container => serviceName => { return ConfigurationReader.GetBaseServerUri(serviceName).OriginalString; });
        }

        #endregion
    }

    public class TestMessagingServiceClient : IMessagingServiceClient
    {
        public SendEmailRequest LastEmailRequest { get; private set; } 
        public SendSMSRequest LastSMSRequest { get; private set; }

        public async Task<SendEmailResponse> SendEmail(String accessToken,
                                                 SendEmailRequest request,
                                                 CancellationToken cancellationToken) {
            this.LastEmailRequest = request;
            return new SendEmailResponse {
                                             MessageId = Guid.NewGuid()
                                         };
        }

        public async Task<SendSMSResponse> SendSMS(String accessToken,
                                             SendSMSRequest request,
                                             CancellationToken cancellationToken) {
            this.LastSMSRequest = request;
            return new SendSMSResponse
            {
                       MessageId = Guid.NewGuid()
                   };
        }
    }
}