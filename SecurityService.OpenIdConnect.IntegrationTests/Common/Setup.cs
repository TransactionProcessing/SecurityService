using System;

namespace SecurityService.IntergrationTests.Common
{
    using System.Threading.Tasks;
    using Ductus.FluentDocker.Services;
    using Ductus.FluentDocker.Services.Extensions;
    using NLog;
    using Reqnroll;
    using Shared.IntegrationTesting;
    using Shared.Logger;
    using Shouldly;

    [Binding]
    public class Setup
    {
        public static IContainerService DatabaseServerContainer;
        public static INetworkService DatabaseServerNetwork;
        public static (String usename, String password) SqlCredentials = ("sa", "thisisalongpassword123!");
        public static (String url, String username, String password) DockerCredentials = ("https://www.docker.com", "stuartferguson", "Sc0tland");
        static object padLock = new object(); // Object to lock on

        public static async Task GlobalSetup(DockerHelper dockerHelper)
        {
            ShouldlyConfiguration.DefaultTaskTimeout = TimeSpan.FromMinutes(1);
            dockerHelper.SqlCredentials = Setup.SqlCredentials;
            dockerHelper.DockerCredentials = Setup.DockerCredentials;
            dockerHelper.SqlServerContainerName = "sharedsqlserver";

            // Only one thread can execute this block at a time
            lock (Setup.padLock)
            {
                Setup.DatabaseServerNetwork = dockerHelper.SetupTestNetwork("sharednetwork");

                dockerHelper.Logger.LogInformation("in start SetupSqlServerContainer");
                Setup.DatabaseServerContainer = dockerHelper.SetupSqlServerContainer(Setup.DatabaseServerNetwork).Result;
            }
        }

        public static String GetConnectionString(String databaseName)
        {
            return $"server={Setup.DatabaseServerContainer.Name};database={databaseName};user id={Setup.SqlCredentials.usename};password={Setup.SqlCredentials.password}";
        }

        public static String GetLocalConnectionString(String databaseName)
        {
            Int32 databaseHostPort = Setup.DatabaseServerContainer.ToHostExposedEndpoint("1433/tcp").Port;

            return $"server=localhost,{databaseHostPort};database={databaseName};user id={Setup.SqlCredentials.usename};password={Setup.SqlCredentials.password}";
        }

    }
}
