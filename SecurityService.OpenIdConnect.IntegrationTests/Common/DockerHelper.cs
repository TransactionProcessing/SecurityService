namespace SecurityService.IntergrationTests.Common
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Client;
    using Ductus.FluentDocker;
    using Ductus.FluentDocker.Builders;
    using Ductus.FluentDocker.Commands;
    using Ductus.FluentDocker.Common;
    using Ductus.FluentDocker.Model.Builders;
    using Ductus.FluentDocker.Services;
    using Ductus.FluentDocker.Services.Extensions;
    using Shared.Logger;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Shared.IntegrationTesting.DockerHelper" />
    public class DockerHelper : Shared.IntegrationTesting.DockerHelper
    {
        #region Fields

        /// <summary>
        /// The security service client
        /// </summary>
        public ISecurityServiceClient SecurityServiceClient;

        /// <summary>
        /// The security service container name
        /// </summary>
        public String SecurityServiceContainerName;

        /// <summary>
        /// The security service test UI container name
        /// </summary>
        public String SecurityServiceTestUIContainerName;

        /// <summary>
        /// The security service test UI port
        /// </summary>
        public Int32 SecurityServiceTestUIPort;

        /// <summary>
        /// The test identifier
        /// </summary>
        public Guid TestId;

        /// <summary>
        /// The containers
        /// </summary>
        protected List<IContainerService> Containers;

        /// <summary>
        /// The security service port
        /// </summary>
        protected Int32 SecurityServicePort;

        /// <summary>
        /// The test networks
        /// </summary>
        protected List<INetworkService> TestNetworks;

        /// <summary>
        /// The logger
        /// </summary>
        private readonly NlogLogger Logger;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DockerHelper"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public DockerHelper(NlogLogger logger)
        {
            this.Logger = logger;
            this.Containers = new List<IContainerService>();
            this.TestNetworks = new List<INetworkService>();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Starts the containers for scenario run.
        /// </summary>
        /// <param name="scenarioName">Name of the scenario.</param>
        public override async Task StartContainersForScenarioRun(String scenarioName)
        {
            String traceFolder = FdOs.IsWindows() ? $"C:\\home\\txnproc\\trace\\{scenarioName}" : $"/home/txnproc/trace/{scenarioName}";

            Logging.Enabled();

            Guid testGuid = Guid.NewGuid();
            this.TestId = testGuid;

            (String, String, String) dockerCredentials = ("https://www.docker.com", "stuartferguson", "Sc0tland");

            // Setup the container names
            this.SecurityServiceContainerName = $"identity-server";
            this.SecurityServiceTestUIContainerName = $"securityservicetestui{testGuid:N}";

            INetworkService testNetwork = this.SetupTestNetwork();
            this.TestNetworks.Add(testNetwork);

            IContainerService securityServiceContainer = SetupSecurityServiceContainer(this.SecurityServiceContainerName,
                                                                                                    this.Logger,
                                                                                                    "securityservice",
                                                                                                    testNetwork,
                                                                                                    5001,
                                                                                                    dockerCredentials,
                                                                                                    traceFolder);

            this.SecurityServicePort = securityServiceContainer.ToHostExposedEndpoint("5001/tcp").Port;

            IContainerService securityServiceTestUIContainer = DockerHelper.SetupSecurityServiceTestUIContainer(this.SecurityServiceTestUIContainerName,
                                                                                                                this.SecurityServiceContainerName,
                                                                                                                this.SecurityServicePort,
                                                                                                                testNetwork,
                                                                                                                ("estateUIClient", "Secret1"));

            this.SecurityServiceTestUIPort = securityServiceTestUIContainer.ToHostExposedEndpoint("5004/tcp").Port;

            Func<String, String> securityServiceBaseAddressResolver = api => $"https://localhost:{this.SecurityServicePort}";
            HttpClient httpClient = new HttpClient();
            this.SecurityServiceClient = new SecurityServiceClient(securityServiceBaseAddressResolver, httpClient);

            this.Containers.AddRange(new List<IContainerService>
                                     {
                                         securityServiceContainer,
                                         securityServiceTestUIContainer
                                     });
        }

        /// <summary>
        /// Stops the containers for scenario run.
        /// </summary>
        public override async Task StopContainersForScenarioRun()
        {
            if (this.Containers.Any())
            {
                foreach (IContainerService containerService in this.Containers)
                {
                    containerService.StopOnDispose = true;
                    containerService.RemoveOnDispose = true;
                    containerService.Dispose();
                }
            }

            if (this.TestNetworks.Any())
            {
                foreach (INetworkService networkService in this.TestNetworks)
                {
                    networkService.Stop();
                    networkService.Remove(true);
                }
            }
        }

        /// <summary>
        /// Adds the entry to hosts file.
        /// </summary>
        /// <param name="ipaddress">The ipaddress.</param>
        /// <param name="hostname">The hostname.</param>
        private static void AddEntryToHostsFile(String ipaddress,
                                                String hostname)
        {
            if (FdOs.IsWindows())
            {
                using(StreamWriter w = File.AppendText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), @"drivers\etc\hosts")))
                {
                    w.WriteLine($"{ipaddress} {hostname}");
                }
            }
            else if (FdOs.IsLinux())
            {
                DockerHelper.ExecuteBashCommand($"echo {ipaddress} {hostname} | sudo tee -a /etc/hosts");
            }
        }

        /// <summary>
        /// Executes the bash command.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <returns></returns>
        private static void ExecuteBashCommand(String command)
        {
            // according to: https://stackoverflow.com/a/15262019/637142
            // thans to this we will pass everything as one command
            command = command.Replace("\"", "\"\"");

            var proc = new Process
                       {
                           StartInfo = new ProcessStartInfo
                                       {
                                           FileName = "/bin/bash",
                                           Arguments = "-c \"" + command + "\"",
                                           UseShellExecute = false,
                                           RedirectStandardOutput = true,
                                           CreateNoWindow = true
                                       }
                       };
            Console.WriteLine(proc.StartInfo.Arguments);

            proc.Start();
            proc.WaitForExit();
        }

        /// <summary>
        /// Setups the security service container.
        /// </summary>
        /// <param name="containerName">Name of the container.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="imageName">Name of the image.</param>
        /// <param name="networkService">The network service.</param>
        /// <param name="hostFolder">The host folder.</param>
        /// <param name="dockerPort">The docker port.</param>
        /// <param name="dockerCredentials">The docker credentials.</param>
        /// <param name="forceLatestImage">if set to <c>true</c> [force latest image].</param>
        /// <param name="additionalEnvironmentVariables">The additional environment variables.</param>
        /// <returns></returns>
        private static IContainerService SetupSecurityServiceContainer(String containerName,
                                                                       ILogger logger,
                                                                       String imageName,
                                                                       INetworkService networkService,
                                                                       Int32 dockerPort,
                                                                       (String URL, String UserName, String Password)? dockerCredentials,
                                                                       String traceFolder,
                                                                       Boolean forceLatestImage = false,
                                                                       List<String> additionalEnvironmentVariables = null)
        {
            logger.LogInformation("About to Start Security Container");

            List<String> environmentVariables = new List<String>();
            environmentVariables.Add($"ServiceOptions:PublicOrigin=https://identity-server:{dockerPort}");
            environmentVariables.Add($"ServiceOptions:IssuerUrl=https://identity-server:{dockerPort}");
            environmentVariables.Add("ASPNETCORE_ENVIRONMENT=IntegrationTest");
            environmentVariables.Add("urls=https://*:5001");

            if (additionalEnvironmentVariables != null)
            {
                environmentVariables.AddRange(additionalEnvironmentVariables);
            }

            ContainerBuilder securityServiceContainer = new Builder().UseContainer().WithName(containerName)
                                                                     .WithEnvironment(environmentVariables.ToArray()).UseImage(imageName, forceLatestImage)
                                                                     .ExposePort(dockerPort, 5001).UseNetwork(new List<INetworkService>
                                                                                                              {
                                                                                                                  networkService
                                                                                                              }.ToArray());

            if (dockerCredentials.HasValue)
            {
                securityServiceContainer.WithCredential(dockerCredentials.Value.URL, dockerCredentials.Value.UserName, dockerCredentials.Value.Password);
            }

            // Now build and return the container                
            IContainerService builtContainer = securityServiceContainer.Build().Start().WaitForPort("5001/tcp", 30000);
            Thread.Sleep(20000); // This hack is in till health checks implemented :|

            //DockerHelper.AddEntryToHostsFile("127.0.0.1", containerName);
            DockerHelper.AddEntryToHostsFile("localhost", containerName);

            logger.LogInformation("Security Service Container Started");

            return builtContainer;
        }

        /// <summary>
        /// Setups the security service test UI container.
        /// </summary>
        /// <param name="containerName">Name of the container.</param>
        /// <param name="securityServiceContainerName">Name of the security service container.</param>
        /// <param name="securityServiceContainerPort">The security service container port.</param>
        /// <param name="networkService">The network service.</param>
        /// <param name="clientDetails">The client details.</param>
        /// <returns></returns>
        private static IContainerService SetupSecurityServiceTestUIContainer(String containerName,
                                                                             String securityServiceContainerName,
                                                                             Int32 securityServiceContainerPort,
                                                                             INetworkService networkService,
                                                                             (String clientId, String clientSecret) clientDetails)
        {
            // Management API Container
            IContainerService securityServiceTestUIContainer = new Builder().UseContainer().WithName(containerName)
                                                                            .WithEnvironment($"Authority=https://identity-server:{securityServiceContainerPort}",
                                                                                             $"ClientId={clientDetails.clientId}",
                                                                                             $"ClientSecret={clientDetails.clientSecret}",
                                                                                             "urls=https://*:5004")
                                                                            .UseImage("securityservicetestui").ExposePort(5004)
                                                                            .UseNetwork(new List<INetworkService>
                                                                                        {
                                                                                            networkService
                                                                                        }.ToArray()).Build().Start().WaitForPort("5004/tcp", 30000);

            return securityServiceTestUIContainer;
        }

        /// <summary>
        /// Setups the test network.
        /// </summary>
        /// <returns></returns>
        private INetworkService SetupTestNetwork()
        {
            IList<IHostService> hosts = new Hosts().Discover();
            IHostService docker = hosts.FirstOrDefault(x => x.IsNative) ?? hosts.FirstOrDefault(x => x.Name == "default");

            if (docker.Host.IsWindowsEngine())
            {
                return Fd.UseNetwork($"testnetwork{this.TestId:N}").UseDriver("nat").Build();
            }

            if (docker.Host.IsLinuxEngine())
            {
                return Shared.IntegrationTesting.DockerHelper.SetupTestNetwork();
            }

            return null;
        }

        #endregion
    }
}