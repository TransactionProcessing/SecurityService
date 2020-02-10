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
    using TechTalk.SpecFlow;

    public class DockerHelper
    {
        public INetworkService TestNetwork;

        public IContainerService SecurityServiceTestUIContainer;

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
            this.SecurityServiceTestUIContainerName = $"securityservicetestui{testGuid:N}";

            this.SetupTestNetwork();
            
            this.SetupSecurityServiceTestUIContainer(traceFolder);
            this.SecurityServiceTestUIPort = this.SecurityServiceTestUIContainer.ToHostExposedEndpoint("5004/tcp").Port;

            Func<String, String> securityServiceBaseAddressResolver = api => $"http://sferguson.ddns.net:55001";
            HttpClient httpClient = new HttpClient();
            this.SecurityServiceClient = new SecurityServiceClient(securityServiceBaseAddressResolver,httpClient);

            Console.Out.WriteLine($"Security Service Test UI Port is [{this.SecurityServiceTestUIPort}]");

            await Task.Delay(30000).ConfigureAwait(false);
        }

        public Int32 SecurityServiceTestUIPort;
        

        private void SetupSecurityServiceTestUIContainer(String traceFolder)
        {
            // Management API Container
            this.SecurityServiceTestUIContainer = new Builder().UseContainer().WithName(this.SecurityServiceTestUIContainerName)
                                                         .WithEnvironment($"Authority=http://sferguson.ddns.net:55001",
                                                                          $"ClientId=estateUIClient{this.TestId.ToString("N")}",
                                                                          "ClientSecret=Secret1")
                                                         .UseImage("securityservicetestwebclient").ExposePort(5004)
                                                         .UseNetwork(new List<INetworkService>
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
