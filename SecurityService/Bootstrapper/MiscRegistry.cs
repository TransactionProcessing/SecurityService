using Microsoft.Extensions.Logging;

namespace SecurityService.Bootstrapper
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using BusinessLogic;
    using ClientProxyBase;
    using Factories;
    using Lamar;
    using MessagingService.Client;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Shared.General;
    using Shared.Middleware;

    [ExcludeFromCodeCoverage]
    public class MiscRegistry : ServiceRegistry
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MiscRegistry"/> class.
        /// </summary>
        public MiscRegistry()
        {
            this.AddHttpContextAccessor();

            this.AddSingleton<IModelFactory, ModelFactory>();

            if (Startup.WebHostEnvironment.IsEnvironment("IntegrationTest")) {
                this.AddSingleton<IMessagingServiceClient, TestMessagingServiceClient>();
            }
            else {
                this.RegisterHttpClient<IMessagingServiceClient, MessagingServiceClient>();
            }

            this.AddSingleton<Func<String, String>>(container => serviceName => { return ConfigurationReader.GetBaseServerUri(serviceName).OriginalString; });

            bool logRequests = ConfigurationReaderExtensions.GetValueOrDefault<Boolean>("MiddlewareLogging", "LogRequests", true);
            bool logResponses = ConfigurationReaderExtensions.GetValueOrDefault<Boolean>("MiddlewareLogging", "LogResponses", true);
            LogLevel middlewareLogLevel = ConfigurationReaderExtensions.GetValueOrDefault<LogLevel>("MiddlewareLogging", "MiddlewareLogLevel", LogLevel.Warning);

            RequestResponseMiddlewareLoggingConfig config =
                new RequestResponseMiddlewareLoggingConfig(middlewareLogLevel, logRequests, logResponses);

            this.AddSingleton(config);
        }

        #endregion
    }

    public static class ConfigurationReaderExtensions
    {
        public static T GetValueOrDefault<T>(String sectionName, String keyName, T defaultValue)
        {
            try
            {
                var value = ConfigurationReader.GetValue(sectionName, keyName);

                if (String.IsNullOrEmpty(value))
                {
                    return defaultValue;
                }

                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch (KeyNotFoundException kex)
            {
                return defaultValue;
            }
        }
    }
}