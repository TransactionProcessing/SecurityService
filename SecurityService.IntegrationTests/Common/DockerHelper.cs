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
    using IdentityServer4.EntityFramework.DbContexts;
    using IdentityServer4.EntityFramework.Options;
    using Microsoft.EntityFrameworkCore;
    using TechTalk.SpecFlow;

    public class DockerHelper
    {
        public INetworkService TestNetwork;

        public IContainerService SecurityServiceContainer;

        public String SecurityServiceContainerName;

        public ISecurityServiceClient SecurityServiceClient;
        
        public Guid TestId;
        private void SetupTestNetwork()
        {
            // Build a network
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
            this.SecurityServiceContainerName = $"securityservice{testGuid:N}";
            
            this.SetupTestNetwork();
            
            this.SetupSecurityServiceContainer(traceFolder);
            this.SecurityServicePort = this.SecurityServiceContainer.ToHostExposedEndpoint("5001/tcp").Port;
            
            Func<String, String> securityServiceBaseAddressResolver = api => $"http://127.0.0.1:{this.SecurityServicePort}";
            HttpClient httpClient = new HttpClient();
            this.SecurityServiceClient = new SecurityServiceClient(securityServiceBaseAddressResolver,httpClient);

            Console.Out.WriteLine($"Security Service Port is [{this.SecurityServicePort}]");

            await Task.Delay(30000).ConfigureAwait(false);
        }

        public Int32 SecurityServicePort;

        private void SetupSecurityServiceContainer(String traceFolder)
        {
            String containerFolder = FdOs.IsLinux() ? "/home/txnproc/trace" : "C:\\home\\txnproc\\trace";

            // Management API Container
            this.SecurityServiceContainer = new Builder().UseContainer().WithName(this.SecurityServiceContainerName)
                                                         .WithEnvironment("ASPNETCORE_ENVIRONMENT=IntegrationTest",
                                                                          $"ServiceOptions:PublicOrigin=http://127.0.0.1:5001",
                                                                          $"ServiceOptions:IssuerUrl=http://127.0.0.1:5001")
                                                         .UseImage("securityservice").ExposePort(5001).UseNetwork(new List<INetworkService>
                                                                                                                  {
                                                                                                                      this.TestNetwork
                                                                                                                  }.ToArray())
                                                         .Mount(traceFolder, containerFolder, MountType.ReadWrite)
                                                         .Build().Start().WaitForPort("5001/tcp", 30000);

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
}
