// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace SecurityService
{
    using System;
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
        private static String AuthenticationConenctionString;

        /// <summary>
        /// The configuration connection string
        /// </summary>
        private static String ConfigurationConnectionString;

        /// <summary>
        /// The persisted grant store conenction string
        /// </summary>
        private static String PersistedGrantStoreConenctionString;

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

        public Startup(IWebHostEnvironment webHostEnvironment)
        {
            IConfigurationBuilder builder = new ConfigurationBuilder().SetBasePath(webHostEnvironment.ContentRootPath)
                                                                      .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                                                                      .AddJsonFile($"appsettings.{webHostEnvironment.EnvironmentName}.json", optional: true)
                                                                      .AddEnvironmentVariables();

            Startup.Configuration = builder.Build();
            Startup.WebHostEnvironment = webHostEnvironment;

            // Get the DB Connection Strings
            Startup.PersistedGrantStoreConenctionString = Startup.Configuration.GetConnectionString(nameof(PersistedGrantDbContext));
            Startup.ConfigurationConnectionString = Startup.Configuration.GetConnectionString(nameof(ConfigurationDbContext));
            Startup.AuthenticationConenctionString = Startup.Configuration.GetConnectionString(nameof(AuthenticationDbContext));
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
                    .AddSqlServer(connectionString: ConfigurationReader.GetConnectionString("PersistedGrantDbContext"),
                                  healthQuery: "SELECT 1;",
                                  name: "Persisted Grant DB",
                                  failureStatus: HealthStatus.Unhealthy,
                                  tags: new string[] { "db", "sql", "sqlserver", "persistedgrant" })
                    .AddSqlServer(connectionString: ConfigurationReader.GetConnectionString("ConfigurationDbContext"),
                                  healthQuery: "SELECT 1;",
                                  name: "Configuration DB",
                                  failureStatus: HealthStatus.Unhealthy,
                                  tags: new string[] { "db", "sql", "sqlserver", "configuration" })
                    .AddSqlServer(connectionString: ConfigurationReader.GetConnectionString("AuthenticationDbContext"),
                                  healthQuery: "SELECT 1;",
                                  name: "Authentication DB",
                                  failureStatus: HealthStatus.Unhealthy,
                                  tags: new string[] { "db", "sql", "sqlserver", "authentication" })
                    .AddUrlGroup(new Uri($"{ConfigurationReader.GetValue("ServiceAddresses", "MessagingService")}/health"),
                                 name: "Messaging Service",
                                 httpMethod: HttpMethod.Get,
                                 failureStatus: HealthStatus.Unhealthy,
                                 tags: new string[] { "application", "messaging" });
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
            services.AddIdentity<IdentityUser, IdentityRole>()
                    .AddEntityFrameworkStores<AuthenticationDbContext>()
                    .AddDefaultTokenProviders();

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
                String migrationsAssembly = typeof(AuthenticationDbContext).GetTypeInfo().Assembly.GetName().Name;
                identityServerBuilder.AddIdentityServerStorage(Startup.ConfigurationConnectionString,
                                                               Startup.PersistedGrantStoreConenctionString,
                                                               Startup.AuthenticationConenctionString,
                                                               migrationsAssembly);
            }
        }

        public void Configure(IApplicationBuilder app,
                              IWebHostEnvironment env,
                              ILoggerFactory loggerFactory)
        {
            String nlogConfigFilename = "nlog.config";
            if (env.IsDevelopment())
            {
                nlogConfigFilename = $"nlog.{env.EnvironmentName}.config";
                app.UseDeveloperExceptionPage();
            }

            loggerFactory.ConfigureNLog(Path.Combine(Startup.WebHostEnvironment.ContentRootPath, nlogConfigFilename));
            loggerFactory.AddNLog();

            ILogger logger = loggerFactory.CreateLogger("Security Service");

            Logger.Initialise(logger);

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
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                var persistedGrantDbContext = serviceScope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>();
                if (persistedGrantDbContext.Database.IsRelational())
                {
                    persistedGrantDbContext.Database.Migrate();
                }
                var configurationDbContext = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
                if (configurationDbContext.Database.IsRelational())
                {
                    configurationDbContext.Database.Migrate();
                }

                var authenticationContext = serviceScope.ServiceProvider.GetRequiredService<AuthenticationDbContext>();
                if (authenticationContext.Database.IsRelational())
                {
                    authenticationContext.Database.Migrate();
                }
            }
        }
    }
}
