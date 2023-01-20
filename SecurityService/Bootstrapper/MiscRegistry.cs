namespace SecurityService.Bootstrapper
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using BusinessLogic;
    using Factories;
    using Lamar;
    using MessagingService.Client;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Shared.General;

    [ExcludeFromCodeCoverage]
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
}