﻿namespace SecurityService.Bootstrapper
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using BusinessLogic;
    using Database.DbContexts;
    using Factories;
    using Lamar;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Diagnostics.HealthChecks;
    using Microsoft.Extensions.Hosting;
    using Microsoft.OpenApi.Models;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;
    using Shared.Extensions;
    using Shared.General;
    using Swashbuckle.AspNetCore.Filters;

    [ExcludeFromCodeCoverage]
    public class MiddlewareRegistry : ServiceRegistry
    {
        public MiddlewareRegistry()
        {
            if (Startup.WebHostEnvironment.IsEnvironment("IntegrationTest")){
                this.AddHealthChecks();
            }
            else if (Startup.Configuration.GetValue<Boolean>("ServiceOptions:UseInMemoryDatabase")){
                this.AddHealthChecks().AddMessagingService();
            }
            else
            {
                this.AddHealthChecks()
                    .AddMessagingService()
                    .AddSqlServer(ConfigurationReader.GetConnectionString("PersistedGrantDbContext"),
                                  "SELECT 1;",
                                  "Persisted Grant DB",
                                  HealthStatus.Unhealthy,
                                  new string[] { "db", "sql", "sqlserver", "persistedgrant" })
                    .AddSqlServer(ConfigurationReader.GetConnectionString("ConfigurationDbContext"),
                                  "SELECT 1;",
                                  "Configuration DB",
                                  HealthStatus.Unhealthy,
                                  new string[] { "db", "sql", "sqlserver", "configuration" })
                    .AddSqlServer(ConfigurationReader.GetConnectionString("AuthenticationDbContext"),
                                  "SELECT 1;",
                                  "Authentication DB",
                                  HealthStatus.Unhealthy,
                                  new string[] { "db", "sql", "sqlserver", "authentication" });
            }

            this.AddSwaggerGen(c =>
                               {
                                   c.SwaggerDoc("v1", new OpenApiInfo
                                                      {
                                                          Title = "Authentication API",
                                                          Version = "1.0",
                                                          Description = "A REST Api to provide authentication services including management of user/client and api details.",
                                                          Contact = new OpenApiContact
                                                                    {
                                                                        Name = "Stuart Ferguson",
                                                                        Email = "golfhandicapping@btinternet.com"
                                                                    }
                                                      });
                                   // add a custom operation filter which sets default values
                                   c.OperationFilter<SwaggerDefaultValues>();
                                   c.ExampleFilters();

                                   //Locate the XML files being generated by ASP.NET...
                                   DirectoryInfo directory = new DirectoryInfo(AppContext.BaseDirectory);
                                   FileInfo[] xmlFiles = directory.GetFiles("*.xml");

                                   //... and tell Swagger to use those XML comments.
                                   foreach (FileInfo fileInfo in xmlFiles)
                                   {
                                       c.IncludeXmlComments(fileInfo.FullName);
                                   }
                               });

            this.AddSwaggerExamplesFromAssemblyOf<SwaggerJsonConverter>();
        }
    }

}
