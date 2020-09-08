using System;
using System.Collections.Generic;
using System.Text;

namespace SecurityService.IntergrationTests.Common
{
    using System.Data.SqlClient;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using BoDi;
    using Client;
    using Ductus.FluentDocker.Builders;
    using Ductus.FluentDocker.Common;
    using Ductus.FluentDocker.Model.Builders;
    using Ductus.FluentDocker.Services;
    using Ductus.FluentDocker.Services.Extensions;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Chrome;
    using Shared.Logger;
    using TechTalk.SpecFlow;

    public class DockerHelper : Shared.IntegrationTesting.DockerHelper
    {
        private static void AddEntryToHostsFile(String ipaddress, String hostname)
        {
            if (FdOs.IsWindows())
            {
                using (StreamWriter w = File.AppendText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), @"drivers\etc\hosts")))
                {
                    w.WriteLine($"{ipaddress} {hostname}");
                }
            }
            else if (FdOs.IsLinux())
            {
                ExecuteBashCommand($"echo {ipaddress} {hostname} | sudo tee -a /etc/hosts");
            }
        }

        static string ExecuteBashCommand(string command)
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

            return proc.StandardOutput.ReadToEnd();
        }

        private static IContainerService SetupSecurityServiceContainer(String containerName,
                                                                      ILogger logger,
                                                                      String imageName,
                                                                      INetworkService networkService,
                                                                      Int32 port,
                                                                      String hostFolder,
                                                                      (String URL, String UserName, String Password)? dockerCredentials,
                                                                      Boolean forceLatestImage = false,
                                                                      List<String> additionalEnvironmentVariables = null)
        {
            logger.LogInformation("About to Start Security Container");

            List<String> environmentVariables = new List<String>();
            environmentVariables.Add($"ServiceOptions:PublicOrigin=http://{containerName}:{port}");
            environmentVariables.Add($"ServiceOptions:IssuerUrl=http://{containerName}:{port}");
            environmentVariables.Add("ASPNETCORE_ENVIRONMENT=IntegrationTest");
            environmentVariables.Add($"urls=http://*:{port}");

            if (additionalEnvironmentVariables != null)
            {
                environmentVariables.AddRange(additionalEnvironmentVariables);
            }

            ContainerBuilder securityServiceContainer = new Builder().UseContainer().WithName(containerName)
                                                                     .WithEnvironment(environmentVariables.ToArray()).UseImage(imageName, forceLatestImage)
                                                                     .ExposePort(port, port).UseNetwork(new List<INetworkService>
                                                                                                                                    {
                                                                                                                                        networkService
                                                                                                                                    }.ToArray()).Mount(hostFolder,
                                                                                                                                                       "/home/txnproc/trace",
                                                                                                                                                       MountType
                                                                                                                                                           .ReadWrite);

            if (dockerCredentials.HasValue)
            {
                securityServiceContainer.WithCredential(dockerCredentials.Value.URL, dockerCredentials.Value.UserName, dockerCredentials.Value.Password);
            }

            // Now build and return the container                
            IContainerService builtContainer = securityServiceContainer.Build().Start().WaitForPort($"{port}/tcp", 30000);
            Thread.Sleep(20000); // This hack is in till health checks implemented :|

            AddEntryToHostsFile("127.0.0.1", containerName);

            logger.LogInformation("Security Service Container Started");

            return builtContainer;
        }

        private static IContainerService SetupSecurityServiceTestUIContainer(String containerName, 
                                                                String securityServiceContainerName, 
                                                                Int32 securityServiceContainerPort,
                                                                INetworkService networkService,
                                                                (String clientId, String clientSecret) clientDetails)
        {
            // Management API Container
            IContainerService securityServiceTestUIContainer = new Builder().UseContainer().WithName(containerName)
                                                                            .WithEnvironment($"Authority=http://{securityServiceContainerName}:{securityServiceContainerPort}",
                                                                                             $"ClientId={clientDetails.clientId}",
                                                                                             $"ClientSecret={clientDetails.clientSecret}")//,
                                                                                             //$"MetadataAddress=http://{securityServiceContainerName}:{securityServiceContainerPort}/.well-known/openid-configuration")
                                                                            .UseImage("securityservicetestwebclient").ExposePort(5004)
                                                                            .UseNetwork(new List<INetworkService>
                                                                                        {
                                                                                            networkService
                                                                                        }.ToArray())
                                                                            .Build().Start().WaitForPort("5004/tcp", 30000);

            return securityServiceTestUIContainer;

            //Console.Out.WriteLine("Started Security Service");
        }

        private readonly NlogLogger Logger;
        protected List<INetworkService> TestNetworks;
        protected List<IContainerService> Containers;
        public Guid TestId;
        public String SecurityServiceContainerName;
        public String SecurityServiceTestUIContainerName;
        protected Int32 SecurityServicePort;
        public Int32 SecurityServiceTestUIPort;
        public ISecurityServiceClient SecurityServiceClient;

        public DockerHelper(NlogLogger logger)
        {
            this.Logger = logger;
            this.Containers = new List<IContainerService>();
            this.TestNetworks = new List<INetworkService>();
        }

        public override async Task StartContainersForScenarioRun(String scenarioName)
        {
            String traceFolder = $"/home/txnproc/trace/{scenarioName}/";

            Logging.Enabled();

            Guid testGuid = Guid.NewGuid();
            this.TestId = testGuid;

            (String, String, String) dockerCredentials = ("https://www.docker.com", "stuartferguson", "Sc0tland");

            // Setup the container names
            this.SecurityServiceContainerName = $"securityservice{testGuid:N}";
            this.SecurityServiceTestUIContainerName = $"securityservicetestui{testGuid:N}";

            INetworkService testNetwork = DockerHelper.SetupTestNetwork();
            this.TestNetworks.Add(testNetwork);

            this.SecurityServicePort = 5551;
            IContainerService securityServiceContainer = SetupSecurityServiceContainer(this.SecurityServiceContainerName,
                                                                                                    this.Logger,
                                                                                                    "stuartferguson/securityservice",
                                                                                                    testNetwork,
                                                                                                    this.SecurityServicePort,
                                                                                                    traceFolder,
                                                                                                    dockerCredentials);

            this.SecurityServicePort = securityServiceContainer.ToHostExposedEndpoint($"{this.SecurityServicePort}/tcp").Port;

            IContainerService securityServiceTestUIContainer = SetupSecurityServiceTestUIContainer(this.SecurityServiceTestUIContainerName,
                                                                                                   this.SecurityServiceContainerName,
                                                                                                   this.SecurityServicePort,
                                                                                                   testNetwork,
                                                                                                   ("estateUIClient", "Secret1"));

            this.SecurityServiceTestUIPort = securityServiceTestUIContainer.ToHostExposedEndpoint("5004/tcp").Port;

            Func <String, String> securityServiceBaseAddressResolver = api => $"http://127.0.0.1:{this.SecurityServicePort}";
            HttpClient httpClient = new HttpClient();
            this.SecurityServiceClient = new SecurityServiceClient(securityServiceBaseAddressResolver, httpClient);

            this.Containers.AddRange(new List<IContainerService>
                                     {
                                         securityServiceContainer,
                                         securityServiceTestUIContainer
                                     });

            //    Console.Out.WriteLine($"Security Service Test UI Port is [{this.SecurityServiceTestUIPort}]");

            //    await Task.Delay(30000).ConfigureAwait(false);
        }

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
    }

    [Binding]
    public class Hooks
    {
        private readonly IObjectContainer ObjectContainer;
        private IWebDriver WebDriver;

        public Hooks(IObjectContainer objectContainer)
        {
            this.ObjectContainer = objectContainer;
        }

        [BeforeScenario(Order = 0)]
        public async Task BeforeScenario()
        {
            ChromeOptions options = new ChromeOptions();
            options.AddArguments("--window-size=1920,1080");
            options.AddArguments("--start-maximized");
            //options.AddArguments("--headless");
            //options.A "same-site-by-default-cookies", "2");
            //options.AddAdditionalCapability("cookies-without-same-site-must-be-secure", "2");
            var experimentalFlags = new List<string>();
            experimentalFlags.Add("same-site-by-default-cookies@2");
            experimentalFlags.Add("cookies-without-same-site-must-be-secure@2");
            options.AddLocalStatePreference("browser.enabled_labs_experiments", experimentalFlags);

            this.WebDriver = new ChromeDriver(options);
            this.ObjectContainer.RegisterInstanceAs(this.WebDriver);
        }

        [AfterScenario(Order = 0)]
        public void AfterScenario()
        {
            this.WebDriver.Dispose();
        }
    }
}
