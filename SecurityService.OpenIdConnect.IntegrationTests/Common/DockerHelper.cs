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
    using Ductus.FluentDocker.Executors;
    using Ductus.FluentDocker.Model.Builders;
    using Ductus.FluentDocker.Services;
    using Ductus.FluentDocker.Services.Extensions;
    using Newtonsoft.Json;
    using Shared.HealthChecks;
    using Shared.IntegrationTesting;
    using Shared.Logger;
    using Shouldly;

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
        public override async Task StartContainersForScenarioRun(String scenarioName, DockerServices dockerServices)
        {
            this.Trace($"Test Id is {this.TestId} and Scenrio {scenarioName}");

            await base.StartContainersForScenarioRun(scenarioName,dockerServices);

            await StartContainer(SetupSecurityServiceTestUIContainer, this.TestNetworks);

            securityServiceBaseAddressResolver = api => $"https://localhost:{this.SecurityServicePort}";



            //IContainerService securityServiceTestUIContainer = SetupSecurityServiceTestUIContainer(this.SecurityServiceTestUIContainerName,
            //                                                                                                    this.SecurityServiceContainerName,
            //                                                                                                    this.SecurityServicePort,
            //                                                                                                    this.TestNetworks,
            //                                                                                                    ("estateUIClient", "Secret1"));

            //

            this.SecurityServiceClient = new SecurityServiceClient(securityServiceBaseAddressResolver, httpClient);

            //this.Containers.AddRange(new List<IContainerService>
            //                         {
            //                             securityServiceTestUIContainer
            //                         });

            DockerHelper.AddEntryToHostsFile("127.0.0.1", SecurityServiceContainerName);
            DockerHelper.AddEntryToHostsFile("localhost", SecurityServiceContainerName);
        }

        protected async Task DoTestUIHealthCheck()
        {
            await Retry.For(async () => {
                                this.Trace($"About to do health check for Test UI");

                                String healthCheck =
                                    await this.HealthCheckClient.PerformHealthCheck("https", "127.0.0.1", this.SecurityServiceTestUIPort, CancellationToken.None);

                                HealthCheckResult result = JsonConvert.DeserializeObject<HealthCheckResult>(healthCheck);

                                this.Trace($"health check complete for Test UI result is [{healthCheck}]");

                                result.Status.ShouldBe(HealthCheckStatus.Healthy.ToString(), $"Service Type: Test UI Details {healthCheck}");
                                this.Trace($"health check complete for Test UI");
                            },
                            TimeSpan.FromMinutes(3),
                            TimeSpan.FromSeconds(20));
        }

        protected async Task<IContainerService> StartContainer(Func<ContainerBuilder> buildContainerFunc, List<INetworkService> networkServices)
        {
            ConsoleStream<String> consoleLogs = null;
            try
            {
                var containerBuilder = buildContainerFunc();

                IContainerService builtContainer = containerBuilder.Build();
                consoleLogs = builtContainer.Logs(true);
                var startedContainer = builtContainer.Start();
                foreach (INetworkService networkService in networkServices)
                {
                    networkService.Attach(startedContainer, false);
                }

                this.Trace($"Test UI Container Started");
                this.Containers.Add(startedContainer);

                this.SecurityServiceTestUIPort = startedContainer.ToHostExposedEndpoint("5004/tcp").Port;

                await this.DoTestUIHealthCheck();
                
                return startedContainer;
            }
            catch (Exception ex)
            {
                if (consoleLogs != null){
                    while (consoleLogs.IsFinished == false){
                        var s = consoleLogs.TryRead(10000);
                        this.Trace(s);
                    }
                }

                this.Error($"Error starting container [{buildContainerFunc.Method.Name}]", ex);
                throw;
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
        
        private const String SetupSecurityServiceTestUIContainerName = "";

        private ContainerBuilder SetupSecurityServiceTestUIContainer(){

            var clientDetails = ("estateUIClient", "Secret1");

            ContainerBuilder securityServiceTestUIContainer = new Builder().UseContainer().WithName(this.SecurityServiceTestUIContainerName)
                                                                           .WithEnvironment($"AppSettings:Authority=https://identity-server:{this.SecurityServicePort}",
                                                                                            $"AppSettings:ClientId={clientDetails.Item1}",
                                                                                            $"AppSettings:ClientSecret={clientDetails.Item2}",
                                                                                            "urls=https://*:5004",
                                                                                            "Logging:LogLevel:Microsoft=Information",
                                                                                            "Logging:LogLevel:Default=Information",
                                                                                            "Logging:EventLog:LogLevel:Default=None")
                                                                           .UseImage("securityservicetestui").ExposePort(5004);

            return securityServiceTestUIContainer;
        }
        
        public override ContainerBuilder SetupSecurityServiceContainer()
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

            return securityServiceContainer;
        }

        #endregion
    }
}