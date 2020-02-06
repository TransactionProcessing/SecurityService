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
    using Client;
    using Ductus.FluentDocker.Builders;
    using Ductus.FluentDocker.Model.Builders;
    using Ductus.FluentDocker.Services;
    using Ductus.FluentDocker.Services.Extensions;
    using IdentityServer4.EntityFramework.DbContexts;
    using IdentityServer4.EntityFramework.Options;
    using Microsoft.EntityFrameworkCore;

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
            this.SetupSecurityServiceTestUIContainer(traceFolder);

            this.SecurityServicePort = this.SecurityServiceContainer.ToHostExposedEndpoint("5001/tcp").Port;
            this.SecurityServiceTestUIPort = this.SecurityServiceTestUIContainer.ToHostExposedEndpoint("5004/tcp").Port;

            Func<String, String> securityServiceBaseAddressResolver = api => $"http://127.0.0.1:{this.SecurityServicePort}";
            HttpClient httpClient = new HttpClient();
            this.SecurityServiceClient = new SecurityServiceClient(securityServiceBaseAddressResolver,httpClient);

            Console.Out.WriteLine($"Security Service Port is [{this.SecurityServicePort}]");

            await Task.Delay(30000).ConfigureAwait(false);
        }

        public Int32 SecurityServicePort;
        public Int32 SecurityServiceTestUIPort;

        private void SetupSecurityServiceContainer(String traceFolder)
        {
            // Management API Container
            this.SecurityServiceContainer = new Builder().UseContainer().WithName(this.SecurityServiceContainerName)
                                                         .WithEnvironment("ASPNETCORE_ENVIRONMENT=IntegrationTest",
                                                                          $"ServiceOptions:PublicOrigin=http://{this.SecurityServiceContainerName}:5001",
                                                                          $"ServiceOptions:IssuerUrl=http://{this.SecurityServiceContainerName}:5001")
                                                         .UseImage("securityservice").ExposePort(5001).UseNetwork(new List<INetworkService>
                                                                                                                  {
                                                                                                                      this.TestNetwork
                                                                                                                  }.ToArray())
                                                         .Mount(traceFolder, "/home/txnproc/trace", MountType.ReadWrite).Build().Start().WaitForPort("5001/tcp", 30000);

            Console.Out.WriteLine("Started Security Service");
        }

        private void SetupSecurityServiceTestUIContainer(String traceFolder)
        {
            // Management API Container
            this.SecurityServiceContainer = new Builder().UseContainer().WithName(this.SecurityServiceTestUIContainerName)
                                                         .WithEnvironment($"Authority=http://{this.SecurityServiceContainerName}:5001",
                                                                          $"ClientId=estateUIClient",
                                                                          "ClientSecret=Secret1")
                                                         .UseImage("securityservicetestwebclient").ExposePort(5004).UseNetwork(new List<INetworkService>
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

                //DeleteDatabase($"PersistedGrantStore{this.TestId:N}");
                //DeleteDatabase($"Configuration{this.TestId:N}");
                //DeleteDatabase($"Authentication{this.TestId:N}");

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

        //private void DeleteDatabase(String database)
        //{
        //    IPEndPoint sqlEndpoint = Setup.DatabaseServerContainer.ToHostExposedEndpoint("1433/tcp");

        //    String server = "127.0.0.1";
        //    String user = "sa";
        //    String password = "thisisalongpassword123!";
        //    String port = sqlEndpoint.Port.ToString();

        //    String connectionString = $"server={server},{port};user id={user}; password={password}; database={"master"};";

        //    SqlConnection connection = new SqlConnection(connectionString);

        //    connection.Open();

        //    SqlCommand setSingleUserCommand = connection.CreateCommand();
        //    setSingleUserCommand.CommandText = $"alter database {database} set single_user with rollback immediate";
        //    setSingleUserCommand.ExecuteNonQuery();

        //    SqlCommand dropDatabaseCommand = connection.CreateCommand();
        //    dropDatabaseCommand.CommandText = $"DROP DATABASE {database}";
        //    dropDatabaseCommand.ExecuteNonQuery();
            
        //    connection.Close();
        //}
    }
}
