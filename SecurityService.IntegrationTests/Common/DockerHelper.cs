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
                
        public async Task StartContainersForScenarioRun(String scenarioName){
            DockerServices dockerServices = DockerServices.SecurityService | DockerServices.SqlServer | DockerServices.MessagingService | DockerServices.EventStore;

            await base.StartContainersForScenarioRun(scenarioName, dockerServices);
                                   
            Func<String, String> securityServiceBaseAddressResolver = api => $"https://localhost:{this.SecurityServicePort}";
            HttpClient httpClient = new HttpClient();
            this.SecurityServiceClient = new SecurityServiceClient(securityServiceBaseAddressResolver,httpClient);

            ServicePointManager.SecurityProtocol = SecurityProtocolType.SystemDefault;
        }

        public override ContainerBuilder SetupSecurityServiceContainer()
        {
            this.Trace("About to Start Security Container");

            List<String> environmentVariables = this.GetCommonEnvironmentVariables();
            environmentVariables.Add($"ServiceOptions:PublicOrigin=https://{this.SecurityServiceContainerName}:{DockerPorts.SecurityServiceDockerPort}");
            environmentVariables.Add($"ServiceOptions:IssuerUrl=https://{this.SecurityServiceContainerName}:{DockerPorts.SecurityServiceDockerPort}");
            environmentVariables.Add("ASPNETCORE_ENVIRONMENT=IntegrationTest");
            environmentVariables.Add($"urls=https://*:{DockerPorts.SecurityServiceDockerPort}");

            environmentVariables.Add("ServiceOptions:PasswordOptions:RequiredLength=6");
            environmentVariables.Add("ServiceOptions:PasswordOptions:RequireDigit=false");
            environmentVariables.Add("ServiceOptions:PasswordOptions:RequireUpperCase=false");
            environmentVariables.Add("ServiceOptions:UserOptions:RequireUniqueEmail=false");
            environmentVariables.Add("ServiceOptions:SignInOptions:RequireConfirmedEmail=false");

            List<String> additionalEnvironmentVariables = this.GetAdditionalVariables(ContainerType.SecurityService);

            if (additionalEnvironmentVariables != null)
            {
                environmentVariables.AddRange(additionalEnvironmentVariables);
            }

            ContainerBuilder securityServiceContainer = new Builder().UseContainer().WithName(this.SecurityServiceContainerName)
                                                                     .WithEnvironment(environmentVariables.ToArray())
                                                                     .UseImageDetails(this.GetImageDetails(ContainerType.SecurityService))
                                                                     .MountHostFolder(this.HostTraceFolder)
                                                                     .SetDockerCredentials(this.DockerCredentials);

            Int32? hostPort = this.GetHostPort(ContainerType.SecurityService);
            if (hostPort == null)
            {
                securityServiceContainer = securityServiceContainer.ExposePort(DockerPorts.SecurityServiceDockerPort);
            }
            else
            {
                securityServiceContainer = securityServiceContainer.ExposePort(hostPort.Value, DockerPorts.SecurityServiceDockerPort);
            }

            // Now build and return the container                
            return securityServiceContainer;
        }
    }
}
