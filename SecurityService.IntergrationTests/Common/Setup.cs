namespace SecurityService.IntergrationTests.Common
{
    using System;
    using System.Data;
    using System.Data.SqlClient;
    using System.IO;
    using System.Net;
    using System.Threading;
    using Ductus.FluentDocker.Builders;
    using Ductus.FluentDocker.Services;
    using Ductus.FluentDocker.Services.Extensions;
    using Shouldly;
    using TechTalk.SpecFlow;

    [Binding]
    public class Setup
    {
        public static IContainerService DatabaseServerContainer;
        private static String DbConnectionStringWithNoDatabase;
        public static INetworkService DatabaseServerNetwork;

        [BeforeTestRun]
        protected static void GlobalSetup()
        {
            ShouldlyConfiguration.DefaultTaskTimeout = TimeSpan.FromMinutes(1);

            // Setup a network for the DB Server
            Setup.DatabaseServerNetwork = new Builder().UseNetwork($"sharednetwork").ReuseIfExist().Build();

            // Start the Database Server here
            Setup.DbConnectionStringWithNoDatabase = Setup.StartSqlContainerWithOpenConnection();
        }

        public static String GetConnectionString(String databaseName)
        {
            return $"{Setup.DbConnectionStringWithNoDatabase} database={databaseName};";
        }

        private static String StartSqlContainerWithOpenConnection()
        {
            String containerName = $"shareddatabasesqlserver";
            Setup.DatabaseServerContainer = new Ductus.FluentDocker.Builders.Builder()
                                      .UseContainer()
                                      .WithName(containerName)
                                      .UseImage("mcr.microsoft.com/mssql/server")
                                      .WithEnvironment("ACCEPT_EULA=Y", $"SA_PASSWORD=thisisalongpassword123!")
                                      .ExposePort(1433)
                                      .UseNetwork(Setup.DatabaseServerNetwork)
                                      .KeepContainer()
                                      .KeepRunning()
                                      .ReuseIfExists()
                                      .Build()
                                      .Start()
                                      .WaitForPort("1433/tcp", 30000);

            IPEndPoint sqlServerEndpoint = Setup.DatabaseServerContainer.ToHostExposedEndpoint("1433/tcp");

            // Try opening a connection
            Int32 maxRetries = 10;
            Int32 counter = 1;

            String server = "127.0.0.1";
            String database = "master";
            String user = "sa";
            String password = "thisisalongpassword123!";
            String port = sqlServerEndpoint.Port.ToString();

            String connectionString = $"server={server},{port};user id={user}; password={password}; database={database};";

            SqlConnection connection = new SqlConnection(connectionString);

            using (StreamWriter sw = new StreamWriter("C:\\Temp\\testlog.log", true))
            {
                while (counter <= maxRetries)
                {
                    try
                    {
                        sw.WriteLine($"Attempt {counter}");
                        sw.WriteLine(DateTime.Now);
                        
                        connection.Open();

                        SqlCommand command = connection.CreateCommand();
                        command.CommandText = "SELECT * FROM spt_fallback_db";
                        command.ExecuteNonQuery();

                        sw.WriteLine("Connection Opened");

                        connection.Close();

                        break;
                    }
                    catch (SqlException ex)
                    {
                        if (connection.State == ConnectionState.Open)
                        {
                            connection.Close();
                        }

                        sw.WriteLine(ex);
                        Thread.Sleep(20000);
                    }
                    finally
                    {
                        counter++;
                    }
                }
            }

            return $"server={containerName};user id={user}; password={password};";
        }
    }
}