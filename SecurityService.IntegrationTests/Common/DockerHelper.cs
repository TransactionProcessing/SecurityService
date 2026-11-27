#nullable enable
using TransactionProcessing.UnionResults;
using SecurityService.DataTransferObjects;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using TransactionProcessing.Shared.Serialisation;

namespace SecurityService.IntergrationTests.Common
{
    using Client;
    using NLog.Config;
    using SecurityService.IntegrationTesting.Helpers;
    using TransactionProcessing.Shared.IntegrationTesting;
    using TransactionProcessing.Shared.IntegrationTesting.TestContainers;
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    public class DockerHelper : TransactionProcessing.Shared.IntegrationTesting.TestContainers.DockerHelper
    {
        public ISecurityServiceClient? SecurityServiceClient;

        String Serialise(Object arg)
        {
            return StringSerialiser.Serialise<Object>(arg, new SerialiserOptions(SerialiserPropertyFormat.SnakeCase));
        }

        Object Deserialise(String arg, Type type)
        {
            return StringSerialiser.DeserializeObject<Object>(arg, type, new SerialiserOptions(SerialiserPropertyFormat.SnakeCase));
        }

        public String AccessToken;

        public override async Task StartContainersForScenarioRun(String scenarioName, DockerServices dockerServices, CancellationToken cancellationToken)
        {
            try
            {
                await base.StartContainersForScenarioRun(scenarioName, dockerServices, cancellationToken).ConfigureAwait(false);

                Func<String, String> securityServiceBaseAddressResolver = _ => $"https://localhost:{this.SecurityServicePort}";
                HttpClient httpClient = this.CreateHttpClient();
                this.SecurityServiceClient = new SecurityServiceClient(securityServiceBaseAddressResolver, httpClient, Serialise, Deserialise);

                TransactionProcessing.UnionResults.Result<SecurityService.DataTransferObjects.TokenResponse> bootstrapToken =
                    await this.SecurityServiceClient
                        .GetToken("management-bootstrap", "management-bootstrap-secret", CancellationToken.None)
                        .ConfigureAwait(false);

                if (bootstrapToken.Value is not Success<TokenResponse> { Value.AccessToken: { } accessToken } || String.IsNullOrWhiteSpace(accessToken))
                {
                    throw new InvalidOperationException($"Unable to obtain the integration-test management bootstrap token: {bootstrapToken.GetErrorMessage() ?? "No error details were returned."}");
                }

                this.AccessToken = accessToken;
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

            if (this.IntegrationTestCertificate is null)
            {
                throw new InvalidOperationException("Integration test certificate was not initialised before container setup.");
            }

            Dictionary<String, String> environmentVariables = this.GetCommonEnvironmentVariables();
            environmentVariables.Add("ServiceOptions:PublicOrigin", $"https://{this.SecurityServiceContainerName}:{DockerPorts.SecurityServiceDockerPort}");
            environmentVariables.Add("ServiceOptions:IssuerUrl", $"https://{this.SecurityServiceContainerName}:{DockerPorts.SecurityServiceDockerPort}");
            environmentVariables.Add("ASPNETCORE_ENVIRONMENT", "IntegrationTest");
            environmentVariables.Add("urls", $"https://*:{DockerPorts.SecurityServiceDockerPort}");
            environmentVariables.Add("ServiceOptions:KestrelOptions:Path", this.IntegrationTestCertificate.GetContainerCertificatePath());
            environmentVariables.Add("ServiceOptions:KestrelOptions:Password", this.IntegrationTestCertificate.Password);
            environmentVariables.Add("ServiceOptions:PasswordOptions:RequiredLength", "6");
            environmentVariables.Add("ServiceOptions:PasswordOptions:RequireDigit", "false");
            environmentVariables.Add("ServiceOptions:PasswordOptions:RequireUpperCase", "false");
            environmentVariables.Add("ServiceOptions:UserOptions:RequireUniqueEmail", "false");
            environmentVariables.Add("ServiceOptions:UserOptions:UseProvidedPasswordForEmailUsers", "true");
            environmentVariables.Add("ServiceOptions:SignInOptions:RequireConfirmedEmail", "false");
            environmentVariables.Add("ServiceOptions:ManagementBootstrap:Enabled", "true");
            environmentVariables.Add("ServiceOptions:ManagementBootstrap:ClientId", "management-bootstrap");
            environmentVariables.Add("ServiceOptions:ManagementBootstrap:ClientSecret", "management-bootstrap-secret");
            environmentVariables.Add("ServiceOptions:OAuth:LegacyGrantTypeClients:password:0", "merchantClient");
            environmentVariables.Add("ServiceOptions:OAuth:LegacyGrantTypeClients:hybrid:0", "testclient2");
            environmentVariables.Add("ServiceOptions:OAuth:EnableLegacyGrantTypes", "true");
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

            TransactionProcessing.UnionResults.Result<(String imageName, Boolean useLatest)> imageDetailsResult = this.GetImageDetails(ContainerType.SecurityService);
            if (imageDetailsResult.Value is not Success<(String imageName, Boolean useLatest)> imageDetails)
            {
                throw new Exception($"Image details not found for {ContainerType.SecurityService}");
            }

            ContainerBuilder securityServiceContainer = new ContainerBuilder(imageDetails.Value.imageName)
                .WithName(this.SecurityServiceContainerName)
                .WithEnvironment(environmentVariables)
                .MountHostFolder(this.HostTraceFolder)
                .WithResourceMapping(this.IntegrationTestCertificate.CertificateDirectory, this.IntegrationTestCertificate.ContainerCertificateDirectory);

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

        public override Task CreateSubscriptions(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private HttpClient CreateHttpClient()
        {
            if (this.IntegrationTestCertificate is null)
            {
                throw new InvalidOperationException("Integration test certificate was not initialised before HttpClient creation.");
            }

            return new HttpClient();
        }
    }
}
