using System;

namespace SecurityService.IntergrationTests.Common
{
    using Ductus.FluentDocker.Services;
    using Ductus.FluentDocker.Services.Extensions;
    using NLog;
    using Shared.IntegrationTesting;
    using Shared.Logger;
    using Shouldly;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;
    using TechTalk.SpecFlow;

    [Binding]
    public class Setup
    {
        public static IContainerService DatabaseServerContainer;
        public static INetworkService DatabaseServerNetwork;
        public static (String usename, String password) SqlCredentials = ("sa", "thisisalongpassword123!");
        public static (String url, String username, String password) DockerCredentials = ("https://www.docker.com", "stuartferguson", "Sc0tland");
        [BeforeTestRun]
        protected static void GlobalSetup()
        {
            ShouldlyConfiguration.DefaultTaskTimeout = TimeSpan.FromMinutes(1);

            DockerHelper dockerHelper = new DockerHelper();

            NlogLogger logger = new NlogLogger();
            logger.Initialise(LogManager.GetLogger("Specflow"), "Specflow");
            LogManager.AddHiddenAssembly(typeof(NlogLogger).Assembly);
            dockerHelper.Logger = logger;
            dockerHelper.SqlCredentials = Setup.SqlCredentials;
            dockerHelper.DockerCredentials = Setup.DockerCredentials;
            dockerHelper.SqlServerContainerName = "sharedsqlserver";

            try
            {
                Setup.DatabaseServerNetwork = dockerHelper.SetupTestNetwork("sharednetwork");
                Setup.DatabaseServerContainer = dockerHelper.SetupSqlServerContainer(Setup.DatabaseServerNetwork);
            }
            catch(Exception ex)
            {
                global::System.Console.WriteLine(ex.Message);
                if (ex.InnerException != null)
                {
                    global::System.Console.WriteLine(ex.InnerException.Message);
                    if (ex.InnerException.InnerException != null)
                    {
                        global::System.Console.WriteLine(ex.InnerException.InnerException.Message);
                    }
                }
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
