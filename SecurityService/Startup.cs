﻿// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace SecurityService
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Reflection;
    using System.Security.Claims;
    using BusinessLogic;
    using Duende.IdentityServer.EntityFramework.DbContexts;
    using Duende.IdentityServer.EntityFramework.Mappers;
    using Duende.IdentityServer.Models;
    using Factories;
    using HealthChecks.UI.Client;
    using IdentityModel;
    using Microsoft.AspNetCore.Diagnostics.HealthChecks;
    using Microsoft.AspNetCore.HttpOverrides;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Diagnostics.HealthChecks;
    using Microsoft.Extensions.Logging;
    using Microsoft.OpenApi.Models;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;
    using NLog.Extensions.Logging;
    using SecurityService.Database.DbContexts;
    using Shared.Extensions;
    using Shared.General;
    using Shared.Logger;
    using Swashbuckle.AspNetCore.Filters;
    using ILogger = Microsoft.Extensions.Logging.ILogger;

    public class Startup
    {
        #region Fields

        /// <summary>
        /// The authentication conenction string
        /// </summary>
        private readonly String AuthenticationConenctionString;

        /// <summary>
        /// The configuration connection string
        /// </summary>
        private readonly String ConfigurationConnectionString;

        /// <summary>
        /// The persisted grant store conenction string
        /// </summary>
        private readonly String PersistedGrantStoreConenctionString;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the configuration.
        /// </summary>
        /// <value>
        /// The configuration.
        /// </value>
        public static IConfigurationRoot Configuration { get; set; }

        /// <summary>
        /// Gets or sets the hosting environment.
        /// </summary>
        /// <value>
        /// The hosting environment.
        /// </value>
        public static IWebHostEnvironment WebHostEnvironment { get; set; }

        #endregion

        private static String GetDatabaseEngine => ConfigurationReader.GetValue("AppSettings", "DatabaseEngine");

        public static Boolean IsSqlServer => GetDatabaseEngine == null || String.Compare(GetDatabaseEngine, "SqlServer", StringComparison.InvariantCultureIgnoreCase) == 0;

        public static Boolean IsMySql => String.Compare(GetDatabaseEngine, "MySql", StringComparison.InvariantCultureIgnoreCase) == 0;
        
        public Startup(IWebHostEnvironment webHostEnvironment)
        {
            IConfigurationBuilder builder = new ConfigurationBuilder().SetBasePath(webHostEnvironment.ContentRootPath)
                                                                      .AddJsonFile("/home/txnproc/config/appsettings.json", true, true)
                                                                      .AddJsonFile($"/home/txnproc/config/appsettings.{webHostEnvironment.EnvironmentName}.json", optional: true)
                                                                      .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                                                                      .AddJsonFile($"appsettings.{webHostEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true)
                                                                      .AddEnvironmentVariables();

            Startup.Configuration = builder.Build();
            Startup.WebHostEnvironment = webHostEnvironment;

            // Get the DB Connection Strings
            this.PersistedGrantStoreConenctionString = Startup.Configuration.GetConnectionString("PersistedGrantDbContext");
            this.ConfigurationConnectionString = Startup.Configuration.GetConnectionString("ConfigurationDbContext");
            this.AuthenticationConenctionString = Startup.Configuration.GetConnectionString("AuthenticationDbContext");
        }

        public void ConfigureServices(IServiceCollection services)
        {
            ConfigurationReader.Initialise(Startup.Configuration);
            
            services.AddScoped<ISecurityServiceManager, SecurityServiceManager>();
            services.AddSingleton<IModelFactory, ModelFactory>();

            this.ConfigureIdentityServer(services);
            this.ConfigureMVC(services);
            this.ConfigureSwagger(services);
            this.ConfigureHealthChecks(services);

        }

        private void ConfigureMVC(IServiceCollection services)
        {
            services.AddControllersWithViews().AddNewtonsoftJson(options =>
                                                                 {
                                                                     options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                                                                     options.SerializerSettings.TypeNameHandling = TypeNameHandling.Auto;
                                                                     options.SerializerSettings.Formatting = Formatting.Indented;
                                                                     options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
                                                                     options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                                                                 });
        }

        private void ConfigureHealthChecks(IServiceCollection services)
        {
            services.AddHealthChecks()
                    .AddSqlServer(ConfigurationReader.GetConnectionString("PersistedGrantDbContext"),
                                  "SELECT 1;",
                                  "Persisted Grant DB",
                                  HealthStatus.Unhealthy,
                                  new string[] {"db", "sql", "sqlserver", "persistedgrant"})
                    .AddSqlServer(ConfigurationReader.GetConnectionString("ConfigurationDbContext"),
                                  "SELECT 1;",
                                  "Configuration DB",
                                  HealthStatus.Unhealthy,
                                  new string[] {"db", "sql", "sqlserver", "configuration"})
                    .AddSqlServer(ConfigurationReader.GetConnectionString("AuthenticationDbContext"),
                                  "SELECT 1;",
                                  "Authentication DB",
                                  HealthStatus.Unhealthy,
                                  new string[] {"db", "sql", "sqlserver", "authentication"})
                    .AddMessagingService();
        }

        private void ConfigureSwagger(IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
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

            services.AddSwaggerExamplesFromAssemblyOf<SwaggerJsonConverter>();
        }

        private void ConfigureIdentityServer(IServiceCollection services)
        {
            services.AddIdentity<IdentityUser, IdentityRole>().AddEntityFrameworkStores<AuthenticationDbContext>().AddDefaultTokenProviders();

            IIdentityServerBuilder identityServerBuilder = services.AddIdentityServer(options =>
                                                                                      {
                                                                                          // https://docs.duendesoftware.com/identityserver/v5/fundamentals/resources/
                                                                                          options.EmitStaticAudienceClaim = true;

                                                                                          options.Events.RaiseSuccessEvents = true;
                                                                                          options.Events.RaiseFailureEvents = true;
                                                                                          options.Events.RaiseErrorEvents = true;

                                                                                          options.IssuerUri = Startup.Configuration.GetValue<String>("ServiceOptions:IssuerUrl");
                                                                                      });

            identityServerBuilder.AddAspNetIdentity<IdentityUser>();

            if (Startup.WebHostEnvironment.IsEnvironment("IntegrationTest") || Startup.Configuration.GetValue<Boolean>("ServiceOptions:UseInMemoryDatabase") == true)
            {
                identityServerBuilder.AddIntegrationTestConfiguration();
            }
            else
            {
                identityServerBuilder.AddIdentityServerStorage(this.ConfigurationConnectionString,
                                                               this.PersistedGrantStoreConenctionString,
                                                               this.AuthenticationConenctionString);
            }
        }

        public void Configure(IApplicationBuilder app,
                              IWebHostEnvironment env,
                              ILoggerFactory loggerFactory)
        {
            String nlogConfigFilename = "nlog.config";
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            loggerFactory.ConfigureNLog(Path.Combine(Startup.WebHostEnvironment.ContentRootPath, nlogConfigFilename));
            loggerFactory.AddNLog();

            ILogger logger = loggerFactory.CreateLogger("Security Service");

            Logger.Initialise(logger);

            Action<String> loggerAction = message =>
                                          {
                                              Logger.LogInformation(message);
                                          };
            Startup.Configuration.LogConfiguration(loggerAction);

            app.AddRequestLogging();
            app.AddResponseLogging();
            app.AddExceptionHandler();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            
            app.UseRouting();

            app.UseIdentityServer();

            app.UseAuthorization();
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedProto
            });

            app.UseEndpoints(endpoints =>
                             {
                                 endpoints.MapDefaultControllerRoute();
                                 endpoints.MapHealthChecks("health", new HealthCheckOptions()
                                 {
                                     Predicate = _ => true,
                                     ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                                 });
                             });

            app.UseSwagger();

            app.UseSwaggerUI();

            // this will do the initial DB population
            this.InitializeDatabase(app);
        }

        private void InitializeDatabase(IApplicationBuilder app)
        {
            using (IServiceScope serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                var persistedGrantDbContext = serviceScope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>();
                var configurationDbContext = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
                var authenticationContext = serviceScope.ServiceProvider.GetRequiredService<AuthenticationDbContext>();

                if (persistedGrantDbContext != null && persistedGrantDbContext.Database.IsRelational())
                {
                    persistedGrantDbContext.Database.Migrate();
                }

                if (configurationDbContext != null && configurationDbContext.Database.IsRelational())
                {
                    configurationDbContext.Database.Migrate();
                }

                if (authenticationContext != null && authenticationContext.Database.IsRelational())
                {
                    authenticationContext.Database.Migrate();
                }
            }
        }
    }
}
