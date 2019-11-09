using System;
using System.Collections.Generic;
using System.Text;

namespace SecurityService.IntergrationTests.Common
{
    using System.Data.SqlClient;
    using System.Net;
    using System.Threading.Tasks;
    using Ductus.FluentDocker.Builders;
    using Ductus.FluentDocker.Model.Builders;
    using Ductus.FluentDocker.Services;
    using Ductus.FluentDocker.Services.Extensions;
    using IdentityServer4.EntityFramework.DbContexts;
    using IdentityServer4.EntityFramework.Options;
    using Microsoft.EntityFrameworkCore;

    public class DockerHelper
    {
        protected INetworkService TestNetwork;

        protected IContainerService SecurityServiceContainer;

        protected String SecurityServiceContainerName;

        protected Guid TestId;

        private void SetupTestNetwork()
        {
            // Build a network
            this.TestNetwork = new Ductus.FluentDocker.Builders.Builder().UseNetwork($"testnetwork{this.TestId}").Build();
        }

        public async Task StartContainersForScenarioRun(String scenarioName)
        {
            String traceFolder = $"/home/txnproc/estatemanagement/trace/{scenarioName}/";

            Logging.Enabled();

            Guid testGuid = Guid.NewGuid();
            this.TestId = testGuid;

            // Setup the container names
            this.SecurityServiceContainerName = $"securityservice{testGuid:N}";

            this.SetupTestNetwork();
            this.SetupSecurityServiceContainer(traceFolder);

            this.SecurityServicePort = this.SecurityServiceContainer.ToHostExposedEndpoint("5001/tcp").Port;
        }

        protected Int32 SecurityServicePort;

        private void SetupSecurityServiceContainer(String traceFolder)
        {
            String persistedGrantDbContextConnectionString =
                $"\"ConnectionStrings:PersistedGrantDbContext={Setup.GetConnectionString($"PersistedGrantStore{this.TestId:N}")}\"";
            String configurationDbContextConnectionString = $"\"ConnectionStrings:ConfigurationDbContext={Setup.GetConnectionString($"Configuration{this.TestId:N}")}\"";
            String authenticationDbContextConnectionString =
                $"\"ConnectionStrings:AuthenticationDbContext={Setup.GetConnectionString($"Authentication{this.TestId:N}")}\"";

            // Management API Container
            this.SecurityServiceContainer = new Builder().UseContainer().WithName(this.SecurityServiceContainerName)
                                                         .WithEnvironment(persistedGrantDbContextConnectionString,
                                                                          configurationDbContextConnectionString,
                                                                          authenticationDbContextConnectionString,
                                                                          $"ServiceOptions:PublicOrigin=http://{this.SecurityServiceContainerName}:5001",
                                                                          $"ServiceOptions:IssuerUrl=http://{this.SecurityServiceContainerName}:5001")
                                                         .UseImage("securityservice").ExposePort(5001).UseNetwork(new List<INetworkService>
                                                                                                                  {
                                                                                                                      this.TestNetwork,
                                                                                                                      Setup.DatabaseServerNetwork
                                                                                                                  }.ToArray())
                                                         .Mount(traceFolder, "/home", MountType.ReadWrite).Build().Start().WaitForPort("5001/tcp", 30000);
        }

        public async Task StopContainersForScenarioRun()
        {
            DeleteDatabase($"PersistedGrantStore{this.TestId:N}");
            DeleteDatabase($"Configuration{this.TestId:N}");
            DeleteDatabase($"Authentication{this.TestId:N}");

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

        private void DeleteDatabase(String database)
        {
            IPEndPoint sqlEndpoint = Setup.DatabaseServerContainer.ToHostExposedEndpoint("1433/tcp");

            String server = "127.0.0.1";
            String user = "sa";
            String password = "thisisalongpassword123!";
            String port = sqlEndpoint.Port.ToString();

            String connectionString = $"server={server},{port};user id={user}; password={password}; database={database};";

            SqlConnection connection = new SqlConnection(connectionString);

            connection.Open();

            SqlCommand command = connection.CreateCommand();
            command.CommandText = $"DROP DATABASE {database}";
            command.ExecuteNonQuery();
            connection.Close();
        }
    }
}
