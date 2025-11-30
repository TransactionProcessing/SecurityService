using System.Runtime.InteropServices;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Networks;
using Shared.IntegrationTesting.TestContainers;

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
    using Newtonsoft.Json;
    using Shared.HealthChecks;
    using Shared.IntegrationTesting;
    using Shared.Logger;
    using Shouldly;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Shared.IntegrationTesting.DockerHelper" />
    public class DockerHelper : Shared.IntegrationTesting.TestContainers.DockerHelper
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

        public override async Task CreateSubscriptions(){
            // Nothing to set up here
        }

        /// <summary>
        /// Starts the containers for scenario run.
        /// </summary>
        /// <param name="scenarioName">Name of the scenario.</param>
        public override async Task StartContainersForScenarioRun(String scenarioName, DockerServices dockerServices)
        {
            this.Trace($"Test Id is {this.TestId} and Scenario {scenarioName}");

            await base.StartContainersForScenarioRun(scenarioName,dockerServices);

            await StartContainer(SetupSecurityServiceTestUIContainer, this.TestNetworks);

            securityServiceBaseAddressResolver = api => $"https://localhost:{this.SecurityServicePort}";

            this.SecurityServiceClient = new SecurityServiceClient(securityServiceBaseAddressResolver, httpClient);

            DockerHelper.AddEntryToHostsFile("127.0.0.1", SecurityServiceContainerName);
            DockerHelper.AddEntryToHostsFile("localhost", SecurityServiceContainerName);
        }

        protected async Task DoTestUIHealthCheck()
        {
            await Retry.For(async () => {
                                this.Trace($"About to do health check for Test UI");

                                SimpleResults.Result<String> healthCheck =
                                    await this.HealthCheckClient.PerformHealthCheck("https", "127.0.0.1", this.SecurityServiceTestUIPort, CancellationToken.None);

                                healthCheck.IsSuccess.ShouldBeTrue($"Health check for Test UI failed with [{healthCheck.Message}]");
                                HealthCheckResult result = JsonConvert.DeserializeObject<HealthCheckResult>(healthCheck.Data);

                                this.Trace($"health check complete for Test UI result is [{healthCheck}]");

                                result.Status.ShouldBe(HealthCheckStatus.Healthy.ToString(), $"Service Type: Test UI Details {healthCheck}");
                                this.Trace($"health check complete for Test UI");
                            },
                            TimeSpan.FromMinutes(3),
                            TimeSpan.FromSeconds(20));
        }

        protected async Task StartContainer(Func<ContainerBuilder> buildContainerFunc, List<INetwork> networkServices)
        {
           try
            {
                var containerBuilder = buildContainerFunc();

                foreach (INetwork networkService in networkServices) {
                    containerBuilder = containerBuilder.WithNetwork(networkService);
                }
                IContainer builtContainer = containerBuilder.Build();
                await builtContainer.StartAsync();
                this.Trace($"Test UI Container Started");

                this.SecurityServiceTestUIPort = builtContainer.GetMappedPublicPort(5004);

                this.Containers.AddRange(new List<(DockerServices, IContainer)> {
                    (DockerServices.None,builtContainer)
                });

                await this.DoTestUIHealthCheck();
            }
            catch (Exception ex)
            {
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
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                using(StreamWriter w = File.AppendText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), @"drivers\etc\hosts")))
                {
                    w.WriteLine($"{ipaddress} {hostname}");
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
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
        
        private ContainerBuilder SetupSecurityServiceTestUIContainer(){

            var clientDetails = ("estateUIClient", "Secret1");
            Dictionary<String, String> environmentVariables = new Dictionary<String, String>();
            environmentVariables.Add($"AppSettings:Authority",$"https://identity-server:{this.SecurityServicePort}");
            environmentVariables.Add($"AppSettings:ClientId",clientDetails.Item1);
            environmentVariables.Add($"AppSettings:ClientSecret",clientDetails.Item2);
            //environmentVariables.Add("urls","https://*:5004");
            environmentVariables.Add("Logging:LogLevel:Microsoft","Information");
            environmentVariables.Add("Logging:LogLevel:Default","Information");
            environmentVariables.Add("Logging:EventLog:LogLevel:Default","None");



            ContainerBuilder securityServiceTestUIContainer = new ContainerBuilder().WithName(this.SecurityServiceTestUIContainerName)
                .WithEnvironment(environmentVariables).WithImage("securityservicetestui").WithPortBinding(5004, true);

            return securityServiceTestUIContainer;
        }
        
        public override ContainerBuilder SetupSecurityServiceContainer()
        {
            this.Trace("About to Start Security Container");

            var environmentVariables = this.GetCommonEnvironmentVariables();
            environmentVariables.Add($"ServiceOptions:PublicOrigin",$"https://{this.SecurityServiceContainerName}:{DockerPorts.SecurityServiceDockerPort}");
            environmentVariables.Add($"ServiceOptions:IssuerUrl",$"https://{this.SecurityServiceContainerName}:{DockerPorts.SecurityServiceDockerPort}");
            environmentVariables.Add("ASPNETCORE_ENVIRONMENT","IntegrationTest");
            environmentVariables.Add($"urls",$"https://*:{DockerPorts.SecurityServiceDockerPort}");

            environmentVariables.Add($"ServiceOptions:PasswordOptions:RequiredLength","6");
            environmentVariables.Add($"ServiceOptions:PasswordOptions:RequireDigit","false");
            environmentVariables.Add($"ServiceOptions:PasswordOptions:RequireUpperCase","false");
            environmentVariables.Add($"ServiceOptions:UserOptions:RequireUniqueEmail","false");
            environmentVariables.Add($"ServiceOptions:SignInOptions:RequireConfirmedEmail","false");

            environmentVariables.Add("ConnectionStrings:PersistedGrantDbContext",this.SetConnectionString($"PersistedGrantStore-{this.TestId}", this.UseSecureSqlServerDatabase));
            environmentVariables.Add("ConnectionStrings:ConfigurationDbContext", this.SetConnectionString( $"Configuration-{this.TestId}", this.UseSecureSqlServerDatabase));
            environmentVariables.Add("ConnectionStrings:AuthenticationDbContext", this.SetConnectionString($"Authentication-{this.TestId}", this.UseSecureSqlServerDatabase));

            Dictionary<String, String> additionalEnvironmentVariables = this.GetAdditionalVariables(ContainerType.SecurityService);

            if (additionalEnvironmentVariables != null)
            {
                foreach (KeyValuePair<String, String> additionalEnvironmentVariable in additionalEnvironmentVariables) {
                    environmentVariables.Add(additionalEnvironmentVariable.Key, additionalEnvironmentVariable.Value);
                }
            }

            var imageDetailsResult = this.GetImageDetails(ContainerType.SecurityService);
            if (imageDetailsResult.IsFailed)
                throw new Exception($"Image details not found for {ContainerType.SecurityService}");

            ContainerBuilder securityServiceContainer = new ContainerBuilder().WithName(this.SecurityServiceContainerName).WithEnvironment(environmentVariables).WithImage(imageDetailsResult.Data.imageName).WithPortBinding(DockerPorts.SecurityServiceDockerPort, DockerPorts.SecurityServiceDockerPort).MountHostFolder(this.DockerPlatform, this.HostTraceFolder);

            return securityServiceContainer;
        }

        #endregion
    }
}