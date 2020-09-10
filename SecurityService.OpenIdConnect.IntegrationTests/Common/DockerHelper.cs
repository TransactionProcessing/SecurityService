using System;
using System.Collections.Generic;
using System.Text;

namespace SecurityService.IntergrationTests.Common
{
    using System.Data.SqlClient;
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
            IList<IHostService> hosts = new Hosts().Discover();
            IHostService docker = hosts.FirstOrDefault(x => x.IsNative) ?? hosts.FirstOrDefault(x => x.Name == "default");

            if (docker.Host.IsWindowsEngine())
            {
                this.TestNetwork = Fd.UseNetwork($"testnetwork{this.TestId:N}").UseDriver("nat").Build();
            }
            else
            {
                // Build a network
                this.TestNetwork = new Ductus.FluentDocker.Builders.Builder().UseNetwork($"testnetwork{this.TestId}").Build();
            }
        }

        public async Task StartContainersForScenarioRun(String scenarioName)
        {
            String traceFolder = FdOs.IsWindows()
                ? $"C:\\home\\txnproc\\trace\\{scenarioName}"
                : $"/home/txnproc/trace/{scenarioName}";

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
                                                                          $"ClientId=estateUIClient{this.TestId:N}",
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
            options.AddArguments("--disable-gpu");
            options.AddArguments("--no-sandbox");
            options.AddArguments("--disable-dev-shm-usage");
            List<String> experimentalFlags = new List<string>();
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
