using System;
using System.Collections.Generic;
using System.Text;

namespace SecurityService.IntergrationTests.Common
{
    using NLog;
    using Shared.IntegrationTesting;
    using Shared.Logger;
    using System.Threading.Tasks;
    using Ductus.FluentDocker.Services;
    using TechTalk.SpecFlow;
    using Ductus.FluentDocker.Model.Builders;
    using Ductus.FluentDocker.Services.Extensions;
    using System.IO;
    using Ductus.FluentDocker.Builders;
    using Ductus.FluentDocker.Common;

    public class LocalDockerHelper : DockerHelper
    {
        public override async Task<IContainerService> SetupEventStoreContainer(INetworkService networkService,
                                                                         Boolean isSecure = false) {

            if (FdOs.IsWindows() == true) {
                this.HostTraceFolder = this.HostTraceFolder.Replace("C:\\home\\txnproc\\trace\\", "C:\\actions-runner\\_work\\trace\\"); ;
            }

            this.Trace($"{this.HostTraceFolder}");
            this.Trace("About to Start Event Store Container");

            List<String> environmentVariables = new() {
                                                      "EVENTSTORE_RUN_PROJECTIONS=all",
                                                      "EVENTSTORE_START_STANDARD_PROJECTIONS=true",
                                                      "EVENTSTORE_ENABLE_ATOM_PUB_OVER_HTTP=true",
                                                      "EVENTSTORE_ENABLE_EXTERNAL_TCP=true"
                                                  };

            String logfolder = "/var/log/eventstore";
            DockerEnginePlatform enginePlatform = DockerHelper.GetDockerEnginePlatform();
            if (enginePlatform == DockerEnginePlatform.Windows) {
                logfolder = "C:\\Logs";
            }

            ContainerBuilder eventStoreContainerBuilder = new Builder().UseContainer().UseImageDetails(this.GetImageDetails(ContainerType.EventStore))
                                                                       .ExposePort(DockerPorts.EventStoreHttpDockerPort).ExposePort(DockerPorts.EventStoreTcpDockerPort)
                                                                       .WithName(this.EventStoreContainerName).UseNetwork(networkService)
                                                                       .MountHostFolder(this.HostTraceFolder, logfolder);

            if (isSecure == false)
            {
                environmentVariables.Add("EVENTSTORE_INSECURE=true");
            }
            else
            {
                // Copy these to the container
                String path = Path.Combine(Directory.GetCurrentDirectory(), "certs");

                eventStoreContainerBuilder = eventStoreContainerBuilder.Mount(path, "/etc/eventstore/certs", MountType.ReadWrite);

                // Certificates configuration
                environmentVariables.Add("EVENTSTORE_CertificateFile=/etc/eventstore/certs/node1/node.crt");
                environmentVariables.Add("EVENTSTORE_CertificatePrivateKeyFile=/etc/eventstore/certs/node1/node.key");
                environmentVariables.Add("EVENTSTORE_TrustedRootCertificatesPath=/etc/eventstore/certs/ca");
            }

            eventStoreContainerBuilder = eventStoreContainerBuilder.WithEnvironment(environmentVariables.ToArray());

            IContainerService eventStoreContainer = eventStoreContainerBuilder.Build().Start();
            await Retry.For(async () => { eventStoreContainer = eventStoreContainer.WaitForPort($"{DockerPorts.EventStoreHttpDockerPort}/tcp"); });

            this.EventStoreHttpPort = eventStoreContainer.ToHostExposedEndpoint($"{DockerPorts.EventStoreHttpDockerPort}/tcp").Port;
            this.Trace($"EventStore Http Port: [{this.EventStoreHttpPort}]");

            this.Trace("Event Store Container Started");

            this.Containers.Add(eventStoreContainer);
            return eventStoreContainer;
        }
    }

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

            this.TestingContext.DockerHelper = new LocalDockerHelper();
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
