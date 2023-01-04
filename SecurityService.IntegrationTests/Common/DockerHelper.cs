using System;
using System.Collections.Generic;
using System.Text;

namespace SecurityService.IntergrationTests.Common
{
    using System.Data.SqlClient;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Security;
    using System.Security.Cryptography.X509Certificates;
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
    using Microsoft.EntityFrameworkCore;
    using Shared.IntegrationTesting;
    using TechTalk.SpecFlow;

    public class DockerHelper : Shared.IntegrationTesting.DockerHelper
    {
        public ISecurityServiceClient SecurityServiceClient;
                
        public async Task StartContainersForScenarioRun(String scenarioName)
        {
            await base.StartContainersForScenarioRun(scenarioName);
                                   
            Func<String, String> securityServiceBaseAddressResolver = api => $"https://localhost:{this.SecurityServicePort}";
            HttpClient httpClient = new HttpClient();
            this.SecurityServiceClient = new SecurityServiceClient(securityServiceBaseAddressResolver,httpClient);

            ServicePointManager.SecurityProtocol = SecurityProtocolType.SystemDefault;
        }

        public override async Task<IContainerService> SetupSecurityServiceContainer(List<INetworkService> networkServices,
                                                                                    List<String> additionalEnvironmentVariables = null)
        {
            this.Trace("About to Start Security Container");

            List<String> environmentVariables = this.GetCommonEnvironmentVariables(DockerPorts.SecurityServiceDockerPort);
            environmentVariables.Add($"ServiceOptions:PublicOrigin=https://{this.SecurityServiceContainerName}:{DockerPorts.SecurityServiceDockerPort}");
            environmentVariables.Add($"ServiceOptions:IssuerUrl=https://{this.SecurityServiceContainerName}:{DockerPorts.SecurityServiceDockerPort}");
            environmentVariables.Add("ASPNETCORE_ENVIRONMENT=IntegrationTest");
            environmentVariables.Add($"urls=https://*:{DockerPorts.SecurityServiceDockerPort}");

            environmentVariables.Add($"ServiceOptions:PasswordOptions:RequiredLength=6");
            environmentVariables.Add($"ServiceOptions:PasswordOptions:RequireDigit=false");
            environmentVariables.Add($"ServiceOptions:PasswordOptions:RequireUpperCase=false");
            environmentVariables.Add($"ServiceOptions:UserOptions:RequireUniqueEmail=false");
            environmentVariables.Add($"ServiceOptions:SignInOptions:RequireConfirmedEmail=false");

            if (additionalEnvironmentVariables != null)
            {
                environmentVariables.AddRange(additionalEnvironmentVariables);
            }

            ContainerBuilder securityServiceContainer = new Builder().UseContainer().WithName(this.SecurityServiceContainerName)
                                                                     .WithEnvironment(environmentVariables.ToArray())
                                                                     .UseImageDetails(this.GetImageDetails(ContainerType.SecurityService))
                                                                     .ExposePort(DockerPorts.SecurityServiceDockerPort)
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
    }
}
