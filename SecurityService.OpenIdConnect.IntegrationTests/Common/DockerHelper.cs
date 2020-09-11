﻿using System;
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
    using Ductus.FluentDocker;
    using Ductus.FluentDocker.Builders;
    using Ductus.FluentDocker.Commands;
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
                                                                      String hostFolder,
                                                                      Int32 dockerPort,
                                                                      (String URL, String UserName, String Password)? dockerCredentials,
                                                                      Boolean forceLatestImage = false,
                                                                      List<String> additionalEnvironmentVariables = null)
        {
            logger.LogInformation("About to Start Security Container");

            List<String> environmentVariables = new List<String>();
            environmentVariables.Add($"ServiceOptions:PublicOrigin=http://{containerName}:{dockerPort}");
            environmentVariables.Add($"ServiceOptions:IssuerUrl=http://{containerName}:{dockerPort}");
            environmentVariables.Add("ASPNETCORE_ENVIRONMENT=IntegrationTest");
            environmentVariables.Add($"urls=http://*:5551");

            if (additionalEnvironmentVariables != null)
            {
                environmentVariables.AddRange(additionalEnvironmentVariables);
            }

            String containerFolder = FdOs.IsLinux() ? "/home/txnproc/trace" : "C:\\home\\txnproc\\trace";
            ContainerBuilder securityServiceContainer = new Builder().UseContainer().WithName(containerName)
                                                                     .WithEnvironment(environmentVariables.ToArray()).UseImage(imageName, forceLatestImage)
                                                                     .ExposePort(dockerPort, 5551).UseNetwork(new List<INetworkService>
                                                                                                            {
                                                                                                                networkService
                                                                                                            }.ToArray());//.Mount(hostFolder,containerFolder,MountType.ReadWrite);

            if (dockerCredentials.HasValue)
            {
                securityServiceContainer.WithCredential(dockerCredentials.Value.URL, dockerCredentials.Value.UserName, dockerCredentials.Value.Password);
            }

            // Now build and return the container                
            IContainerService builtContainer = securityServiceContainer.Build().Start().WaitForPort($"5551/tcp", 30000);
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
                                                                            .UseImage("securityservicetestwebclient").ExposePort(5004)
                                                                            .UseNetwork(new List<INetworkService>
                                                                                        {
                                                                                            networkService
                                                                                        }.ToArray())
                                                                            .Build().Start().WaitForPort("5004/tcp", 30000);

            return securityServiceTestUIContainer;
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

        public override async Task StartContainersForScenarioRun(String scenarioName)
        {
            String traceFolder = FdOs.IsWindows()
                ? $"C:\\home\\txnproc\\trace\\{scenarioName}"
                : $"/home/txnproc/trace/{scenarioName}";

            Logging.Enabled();

            Guid testGuid = Guid.NewGuid();
            this.TestId = testGuid;

            (String, String, String) dockerCredentials = ("https://www.docker.com", "stuartferguson", "Sc0tland");

            // Setup the container names
            this.SecurityServiceContainerName = $"securityservice{testGuid:N}";
            this.SecurityServiceTestUIContainerName = $"securityservicetestui{testGuid:N}";

            INetworkService testNetwork = this.SetupTestNetwork(); 
            this.TestNetworks.Add(testNetwork);

            IContainerService securityServiceContainer = SetupSecurityServiceContainer(this.SecurityServiceContainerName,
                                                                                                    this.Logger,
                                                                                                    "securityservice",
                                                                                                    testNetwork,
                                                                                                    traceFolder,
                                                                                                    5551,
                                                                                                    dockerCredentials);

            this.SecurityServicePort = securityServiceContainer.ToHostExposedEndpoint($"5551/tcp").Port;

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
            options.AddArguments("--disable-gpu");
            options.AddArguments("--no-sandbox");
            options.AddArguments("--disable-dev-shm-usage");
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
