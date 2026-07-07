#nullable enable
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using Shared.Serialisation;

namespace SecurityService.IntergrationTests.Common
{
    using Client;
    using SecurityService.IntegrationTesting.Helpers;
    using Shared.IntegrationTesting;
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading.Tasks;

    public class DockerHelper : Shared.IntegrationTesting.TestContainers.DockerHelper
    {
        public ISecurityServiceClient? SecurityServiceClient;
        private IntegrationTestCertificate? _integrationTestCertificate;

        String Serialise(Object arg)
        {
            return StringSerialiser.Serialise<Object>(arg, new SerialiserOptions(SerialiserPropertyFormat.SnakeCase));
        }

        Object Deserialise(String arg, Type type)
        {
            return StringSerialiser.DeserializeObject<Object>(arg, type, new SerialiserOptions(SerialiserPropertyFormat.SnakeCase));
        }

        public override async Task StartContainersForScenarioRun(String scenarioName, DockerServices dockerServices)
        {
            this._integrationTestCertificate = IntegrationTestCertificate.Create();

            try
            {
                await base.StartContainersForScenarioRun(scenarioName, dockerServices);

                Func<String, String> securityServiceBaseAddressResolver = _ => $"https://localhost:{this.SecurityServicePort}";
                HttpClient httpClient = this.CreatePinnedHttpClient();
                this.SecurityServiceClient = new SecurityServiceClient(securityServiceBaseAddressResolver, httpClient, Serialise, Deserialise);

                ServicePointManager.SecurityProtocol = SecurityProtocolType.SystemDefault;
            }
            catch
            {
                this.DisposeIntegrationTestCertificate();
                throw;
            }
        }

        public override ContainerBuilder SetupSecurityServiceContainer()
        {
            this.Trace("About to Start Security Container");

            if (this._integrationTestCertificate is null)
            {
                throw new InvalidOperationException("Integration test certificate was not initialised before container setup.");
            }

            Dictionary<String, String> environmentVariables = this.GetCommonEnvironmentVariables();
            environmentVariables.Add("ServiceOptions:PublicOrigin", $"https://{this.SecurityServiceContainerName}:{DockerPorts.SecurityServiceDockerPort}");
            environmentVariables.Add("ServiceOptions:IssuerUrl", $"https://{this.SecurityServiceContainerName}:{DockerPorts.SecurityServiceDockerPort}");
            environmentVariables.Add("ASPNETCORE_ENVIRONMENT", "IntegrationTest");
            environmentVariables.Add("urls", $"https://*:{DockerPorts.SecurityServiceDockerPort}");
            environmentVariables.Add("ServiceOptions:KestrelOptions:Path", this._integrationTestCertificate.GetContainerCertificatePath());
            environmentVariables.Add("ServiceOptions:KestrelOptions:Password", this._integrationTestCertificate.Password);
            environmentVariables.Add("ServiceOptions:PasswordOptions:RequiredLength", "6");
            environmentVariables.Add("ServiceOptions:PasswordOptions:RequireDigit", "false");
            environmentVariables.Add("ServiceOptions:PasswordOptions:RequireUpperCase", "false");
            environmentVariables.Add("ServiceOptions:UserOptions:RequireUniqueEmail", "false");
            environmentVariables.Add("ServiceOptions:SignInOptions:RequireConfirmedEmail", "false");
            environmentVariables.Add("ConnectionStrings:PersistedGrantDbContext", this.SetConnectionString($"PersistedGrantStore-{this.TestId}", this.UseSecureSqlServerDatabase));
            environmentVariables.Add("ConnectionStrings:ConfigurationDbContext", this.SetConnectionString($"Configuration-{this.TestId}", this.UseSecureSqlServerDatabase));
            environmentVariables.Add("ConnectionStrings:AuthenticationDbContext", this.SetConnectionString($"Authentication-{this.TestId}", this.UseSecureSqlServerDatabase));

            Dictionary<String, String> additionalEnvironmentVariables = this.GetAdditionalVariables(ContainerType.SecurityService);
            if (additionalEnvironmentVariables != null)
            {
                foreach (KeyValuePair<String, String> additionalEnvironmentVariable in additionalEnvironmentVariables)
                {
                    environmentVariables.Add(additionalEnvironmentVariable.Key, additionalEnvironmentVariable.Value);
                }
            }

            SimpleResults.Result<(String imageName, Boolean useLatest)> imageDetailsResult = this.GetImageDetails(ContainerType.SecurityService);
            if (imageDetailsResult.IsFailed)
            {
                throw new Exception($"Image details not found for {ContainerType.SecurityService}");
            }

            ContainerBuilder securityServiceContainer = new ContainerBuilder(imageDetailsResult.Data.imageName)
                .WithName(this.SecurityServiceContainerName)
                .WithEnvironment(environmentVariables)
                .WithBindMount(this._integrationTestCertificate.CertificateDirectory, this._integrationTestCertificate.ContainerCertificateDirectory, AccessMode.ReadOnly);

            Int32? hostPort = this.GetHostPort(ContainerType.SecurityService);
            if (hostPort is null || hostPort <= 0)
            {
                securityServiceContainer = securityServiceContainer.WithPortBinding(DockerPorts.SecurityServiceDockerPort, true);
            }
            else
            {
                securityServiceContainer = securityServiceContainer.WithPortBinding(DockerPorts.SecurityServiceDockerPort, hostPort.Value);
            }

            return securityServiceContainer;
        }

        public override Task CreateSubscriptions()
        {
            return Task.CompletedTask;
        }

        public void DisposeIntegrationTestCertificate()
        {
            this._integrationTestCertificate?.Dispose();
            this._integrationTestCertificate = null;
        }

        private HttpClient CreatePinnedHttpClient()
        {
            if (this._integrationTestCertificate is null)
            {
                throw new InvalidOperationException("Integration test certificate was not initialised before HttpClient creation.");
            }

            HttpClientHandler handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (_, certificate, _, _) =>
                    certificate is X509Certificate2 certificate2 &&
                    String.Equals(certificate2.Thumbprint, this._integrationTestCertificate.Thumbprint, StringComparison.OrdinalIgnoreCase)
            };

            return new HttpClient(handler, disposeHandler: true);
        }
    }
}
