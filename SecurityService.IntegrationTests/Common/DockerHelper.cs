using System;
using System.Collections.Generic;
using System.Text;

namespace SecurityService.IntergrationTests.Common
{
    using System.Data.SqlClient;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using BoDi;
    using Client;
    using Coypu;
    using Ductus.FluentDocker.Builders;
    using Ductus.FluentDocker.Model.Builders;
    using Ductus.FluentDocker.Services;
    using Ductus.FluentDocker.Services.Extensions;
    using IdentityServer4.EntityFramework.DbContexts;
    using IdentityServer4.EntityFramework.Options;
    using Microsoft.EntityFrameworkCore;
    using TechTalk.SpecFlow;

    public class DockerHelper
    {
        public INetworkService TestNetwork;

        public IContainerService SecurityServiceContainer;

        public IContainerService SecurityServiceTestUIContainer;

        public String SecurityServiceContainerName;

        public String SecurityServiceTestUIContainerName;

        public ISecurityServiceClient SecurityServiceClient;
        
        public Guid TestId;
        private void SetupTestNetwork()
        {
            // Build a network
            this.TestNetwork = new Ductus.FluentDocker.Builders.Builder().UseNetwork($"testnetwork{this.TestId}").Build();
        }

        public async Task StartContainersForScenarioRun(String scenarioName)
        {
            String traceFolder = $"/home/txnproc/trace/{scenarioName}/";

            Logging.Enabled();

            Guid testGuid = Guid.NewGuid();
            this.TestId = testGuid;

            // Setup the container names
            this.SecurityServiceContainerName = $"securityservice{testGuid:N}";
            this.SecurityServiceTestUIContainerName = $"securityservicetestui{testGuid:N}";

            this.SetupTestNetwork();
            
            this.SetupSecurityServiceContainer(traceFolder);
            this.SecurityServicePort = this.SecurityServiceContainer.ToHostExposedEndpoint("5001/tcp").Port;
            
            this.SetupSecurityServiceTestUIContainer(traceFolder);
            this.SecurityServiceTestUIPort = this.SecurityServiceTestUIContainer.ToHostExposedEndpoint("5004/tcp").Port;

            Func<String, String> securityServiceBaseAddressResolver = api => $"http://127.0.0.1:{this.SecurityServicePort}";
            HttpClient httpClient = new HttpClient();
            this.SecurityServiceClient = new SecurityServiceClient(securityServiceBaseAddressResolver,httpClient);

            Console.Out.WriteLine($"Security Service Port is [{this.SecurityServicePort}]");
            Console.Out.WriteLine($"Security Service Test UI Port is [{this.SecurityServiceTestUIPort}]");

            await Task.Delay(30000).ConfigureAwait(false);
        }

        public Int32 SecurityServicePort;
        public Int32 SecurityServiceTestUIPort;

        private void SetupSecurityServiceContainer(String traceFolder)
        {


            // Management API Container
            this.SecurityServiceContainer = new Builder().UseContainer().WithName(this.SecurityServiceContainerName)
                                                         .WithEnvironment("ASPNETCORE_ENVIRONMENT=IntegrationTest",
                                                                          $"ServiceOptions:PublicOrigin=http://127.0.0.1:5001",
                                                                          $"ServiceOptions:IssuerUrl=http://127.0.0.1:5001")
                                                         .UseImage("securityservice").ExposePort(5001,5001).UseNetwork(new List<INetworkService>
                                                                                                                  {
                                                                                                                      this.TestNetwork
                                                                                                                  }.ToArray())
                                                         .Mount(traceFolder, "/home/txnproc/trace", MountType.ReadWrite)
                                                         .
                                                         
                                                         .Build().Start().WaitForPort("5001/tcp", 30000);

            Console.Out.WriteLine("Started Security Service");
        }

        private void SetupSecurityServiceTestUIContainer(String traceFolder)
        {
            // Management API Container
            this.SecurityServiceTestUIContainer = new Builder().UseContainer().WithName(this.SecurityServiceTestUIContainerName)
                                                         .WithEnvironment($"Authority=http://127.0.0.1:{this.SecurityServicePort}",
                                                                          $"ClientId=estateUIClient",
                                                                          "ClientSecret=Secret1")
                                                         .UseImage("securityservicetestwebclient").ExposePort(5004,5004).UseNetwork(new List<INetworkService>
                                                                                                                  {
                                                                                                                      this.TestNetwork
                                                                                                                  }.ToArray())
                                                         .Build().Start().WaitForPort("5004/tcp", 30000);

            Console.Out.WriteLine("Started Security Service");
        }

        public async Task StopContainersForScenarioRun()
        {
            try
            {
                if (this.SecurityServiceContainer != null)
                {
                    this.SecurityServiceContainer.StopOnDispose = true;
                    this.SecurityServiceContainer.RemoveOnDispose = true;
                    this.SecurityServiceContainer.Dispose();
                }

                if (this.SecurityServiceTestUIContainer != null)
                {
                    this.SecurityServiceTestUIContainer.StopOnDispose = true;
                    this.SecurityServiceTestUIContainer.RemoveOnDispose = true;
                    this.SecurityServiceTestUIContainer.Dispose();
                }

                if (this.TestNetwork != null)
                {
                    this.TestNetwork.Stop();
                    this.TestNetwork.Remove(true);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }

    [Binding]
    public class Hooks
    {
        private readonly IObjectContainer ObjectContainer;
        private BrowserSession BrowserSession;

        public Hooks(IObjectContainer objectContainer)
        {
            this.ObjectContainer = objectContainer;
        }

        [BeforeScenario(Order = 0)]
        public async Task BeforeScenario()
        {
            SessionConfiguration sessionConfiguration = new SessionConfiguration
                                                        {
                                                            AppHost = "localhost",
                                                            SSL = false,
                                                        };

            sessionConfiguration.Driver = Type.GetType("Coypu.Drivers.Selenium.SeleniumWebDriver, Coypu");
            sessionConfiguration.Browser = Coypu.Drivers.Browser.Parse("chrome");

            this.BrowserSession = new BrowserSession(sessionConfiguration);
            this.ObjectContainer.RegisterInstanceAs(this.BrowserSession);
        }

        [AfterScenario(Order = 0)]
        public void AfterScenario()
        {
            this.BrowserSession.Dispose();
        }
    }
}
