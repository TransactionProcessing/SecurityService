using System;
using System.Collections.Generic;
using System.Text;

namespace SecurityService.IntergrationTests.Common
{
    using NLog;
    using Shared.IntegrationTesting;
    using Shared.Logger;
    using System.Threading.Tasks;
    using TechTalk.SpecFlow;

    [Binding]
    [Scope(Tag = "base")]
    public class GenericSteps
    {
        private readonly ScenarioContext ScenarioContext;

        private readonly TestingContext TestingContext;

        public GenericSteps(ScenarioContext scenarioContext,
                            TestingContext testingContext)
        {
            this.ScenarioContext = scenarioContext;
            this.TestingContext = testingContext;
        }

        [BeforeScenario]
        public async Task StartSystem()
        {
            // Initialise a logger
            String scenarioName = this.ScenarioContext.ScenarioInfo.Title.Replace(" ", "");
            NlogLogger logger = new NlogLogger();
            logger.Initialise(LogManager.GetLogger(scenarioName), scenarioName);
            LogManager.AddHiddenAssembly(typeof(NlogLogger).Assembly);

            this.TestingContext.DockerHelper = new DockerHelper();
            this.TestingContext.DockerHelper.Logger = logger;
            this.TestingContext.DockerHelper.SqlServerContainer = Setup.DatabaseServerContainer;
            this.TestingContext.DockerHelper.SqlServerNetwork = Setup.DatabaseServerNetwork;
            this.TestingContext.DockerHelper.DockerCredentials = Setup.DockerCredentials;
            this.TestingContext.DockerHelper.SqlCredentials = Setup.SqlCredentials;
            this.TestingContext.DockerHelper.SqlServerContainerName = "sharedsqlserver";

            this.TestingContext.DockerHelper.SetImageDetails(ContainerType.SecurityService, ("securityservice", false));
            DockerEnginePlatform enginePlatform = DockerHelper.GetDockerEnginePlatform();
            if (enginePlatform == DockerEnginePlatform.Windows)
            {
                this.TestingContext.DockerHelper.SetImageDetails(Shared.IntegrationTesting.ContainerType.SqlServer, ("stuartferguson/sqlserverwindows:2019-CU18", true));
                this.TestingContext.DockerHelper.SetImageDetails(Shared.IntegrationTesting.ContainerType.EventStore, ("stuartferguson/eventstore_windows:21.10.0", true));
                this.TestingContext.DockerHelper.SetImageDetails(Shared.IntegrationTesting.ContainerType.MessagingService, ("stuartferguson/messagingservicewindows:master", true));
                this.TestingContext.DockerHelper.SetImageDetails(Shared.IntegrationTesting.ContainerType.TestHost, ("stuartferguson/testhostswindows:master", true));
                this.TestingContext.DockerHelper.SetImageDetails(Shared.IntegrationTesting.ContainerType.CallbackHandler, ("stuartferguson/callbackhandlerwindows:master", true));
                this.TestingContext.DockerHelper.SetImageDetails(Shared.IntegrationTesting.ContainerType.EstateManagement, ("stuartferguson/estatemanagementwindows:master", true));
                this.TestingContext.DockerHelper.SetImageDetails(Shared.IntegrationTesting.ContainerType.EstateReporting, ("stuartferguson/estatereportingwindows:master", true));
                this.TestingContext.DockerHelper.SetImageDetails(Shared.IntegrationTesting.ContainerType.FileProcessor, ("stuartferguson/fileprocessorwindows:master", true));
                this.TestingContext.DockerHelper.SetImageDetails(Shared.IntegrationTesting.ContainerType.VoucherManagement, ("stuartferguson/vouchermanagementwindows:master", true));
                this.TestingContext.DockerHelper.SetImageDetails(Shared.IntegrationTesting.ContainerType.VoucherManagementAcl, ("stuartferguson/vouchermanagementaclwindows:master", true));
                this.TestingContext.DockerHelper.SetImageDetails(Shared.IntegrationTesting.ContainerType.TransactionProcessor, ("stuartferguson/transactionprocessorwindows:master", true));
                this.TestingContext.DockerHelper.SetImageDetails(Shared.IntegrationTesting.ContainerType.TransactionProcessorAcl, ("stuartferguson/transactionprocessoraclwindows:master", true));
            }
            this.TestingContext.Logger = logger;
            this.TestingContext.Logger.LogInformation("About to Start Containers for Scenario Run");
            await this.TestingContext.DockerHelper.StartContainersForScenarioRun(scenarioName).ConfigureAwait(false);
            this.TestingContext.Logger.LogInformation("Containers for Scenario Run Started");
        }

        [AfterScenario]
        public async Task StopSystem()
        {
            this.TestingContext.Logger.LogInformation("About to Stop Containers for Scenario Run");
            await this.TestingContext.DockerHelper.StopContainersForScenarioRun().ConfigureAwait(false);
            this.TestingContext.Logger.LogInformation("Containers for Scenario Run Stopped");
        }
    }
}
