namespace SecurityService.IntergrationTests.Common
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Runtime.CompilerServices;
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
    using Shared.IntegrationTesting;
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
        /// The security service test UI container name
        /// </summary>
        public String SecurityServiceTestUIContainerName;

        /// <summary>
        /// The security service test UI port
        /// </summary>
        public Int32 SecurityServiceTestUIPort;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DockerHelper"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public DockerHelper() : base()
        {
            
        }

        #endregion

        #region Methods

        public Func<String, String> securityServiceBaseAddressResolver;
        public HttpClient httpClient = new HttpClient();

        public override void SetupContainerNames()
        {
            base.SetupContainerNames();

            // Setup the container names
            this.SecurityServiceContainerName = $"identity-server";
            this.SecurityServiceTestUIContainerName = $"securityservicetestui{this.TestId:N}";
        }

        /// <summary>
        /// Starts the containers for scenario run.
        /// </summary>
        /// <param name="scenarioName">Name of the scenario.</param>
        public override async Task StartContainersForScenarioRun(String scenarioName)
        {
            await base.StartContainersForScenarioRun(scenarioName);

            securityServiceBaseAddressResolver = api => $"https://localhost:{this.SecurityServicePort}";

            IContainerService securityServiceTestUIContainer = SetupSecurityServiceTestUIContainer(this.SecurityServiceTestUIContainerName,
                                                                                                                this.SecurityServiceContainerName,
                                                                                                                this.SecurityServicePort,
                                                                                                                this.TestNetworks,
                                                                                                                ("estateUIClient", "Secret1"));

            this.SecurityServiceTestUIPort = securityServiceTestUIContainer.ToHostExposedEndpoint("5004/tcp").Port;

            this.SecurityServiceClient = new SecurityServiceClient(securityServiceBaseAddressResolver, httpClient);

            this.Containers.AddRange(new List<IContainerService>
                                     {
                                         securityServiceTestUIContainer
                                     });

            DockerHelper.AddEntryToHostsFile("127.0.0.1", SecurityServiceContainerName);
            DockerHelper.AddEntryToHostsFile("localhost", SecurityServiceContainerName);
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
            else if (BaseDockerHelper.GetDockerEnginePlatform() == DockerEnginePlatform.Linux)
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
        /// Setups the security service test UI container.
        /// </summary>
        /// <param name="containerName">Name of the container.</param>
        /// <param name="securityServiceContainerName">Name of the security service container.</param>
        /// <param name="securityServiceContainerPort">The security service container port.</param>
        /// <param name="networkService">The network service.</param>
        /// <param name="clientDetails">The client details.</param>
        /// <returns></returns>
        private IContainerService SetupSecurityServiceTestUIContainer(String containerName,
                                                                             String securityServiceContainerName,
                                                                             Int32 securityServiceContainerPort,
                                                                             List<INetworkService> networkServices,
                                                                             (String clientId, String clientSecret) clientDetails)
        {
            // Management API Container
            IContainerService securityServiceTestUIContainer = new Builder().UseContainer().WithName(containerName)
                                                                            .WithEnvironment($"AppSettings:Authority=https://identity-server:{securityServiceContainerPort}",
                                                                                             $"AppSettings:ClientId={clientDetails.clientId}",
                                                                                             $"AppSettings:ClientSecret={clientDetails.clientSecret}",
                                                                                             "urls=https://*:5004")
                                                                            .UseImage("securityservicetestui").ExposePort(5004)
                                                                            .Build().Start().WaitForPort("5004/tcp", 30000);

            foreach (INetworkService networkService in networkServices) {
                networkService.Attach(securityServiceTestUIContainer, false);
            }

            return securityServiceTestUIContainer;
        }

        public override async Task<IContainerService> SetupSecurityServiceContainer(List<INetworkService> networkServices)
        {
            this.Trace("About to Start Security Container");

            List<String> environmentVariables = this.GetCommonEnvironmentVariables();
            environmentVariables.Add($"ServiceOptions:PublicOrigin=https://{this.SecurityServiceContainerName}:{DockerPorts.SecurityServiceDockerPort}");
            environmentVariables.Add($"ServiceOptions:IssuerUrl=https://{this.SecurityServiceContainerName}:{DockerPorts.SecurityServiceDockerPort}");
            environmentVariables.Add("ASPNETCORE_ENVIRONMENT=IntegrationTest");
            environmentVariables.Add($"urls=https://*:{DockerPorts.SecurityServiceDockerPort}");

            environmentVariables.Add($"ServiceOptions:PasswordOptions:RequiredLength=6");
            environmentVariables.Add($"ServiceOptions:PasswordOptions:RequireDigit=false");
            environmentVariables.Add($"ServiceOptions:PasswordOptions:RequireUpperCase=false");
            environmentVariables.Add($"ServiceOptions:UserOptions:RequireUniqueEmail=false");
            environmentVariables.Add($"ServiceOptions:SignInOptions:RequireConfirmedEmail=false");


            List<String> additionalEnvironmentVariables = this.GetAdditionalVariables(ContainerType.SecurityService);

            if (additionalEnvironmentVariables != null)
            {
                environmentVariables.AddRange(additionalEnvironmentVariables);
            }

            ContainerBuilder securityServiceContainer = new Builder().UseContainer().WithName(this.SecurityServiceContainerName)
                                                                     .WithEnvironment(environmentVariables.ToArray())
                                                                     .UseImageDetails(this.GetImageDetails(ContainerType.SecurityService))
                                                                     .ExposePort(DockerPorts.SecurityServiceDockerPort, DockerPorts.SecurityServiceDockerPort)
                                                                     .MountHostFolder(this.HostTraceFolder)
                                                                     .SetDockerCredentials(this.DockerCredentials);

            // Now build and return the container                
            IContainerService builtContainer = securityServiceContainer.Build().Start().WaitForPort($"{DockerPorts.SecurityServiceDockerPort}/tcp", 30000);

            foreach (INetworkService networkService in networkServices)
            {
                networkService.Attach(builtContainer, false);
            }

            this.Trace("Security Service Container Started");
            this.Containers.Add(builtContainer);

            //  Do a health check here
            this.SecurityServicePort = builtContainer.ToHostExposedEndpoint($"{DockerPorts.SecurityServiceDockerPort}/tcp").Port;
            await this.DoHealthCheck(ContainerType.SecurityService);

            return builtContainer;
        }

        #endregion
    }
}