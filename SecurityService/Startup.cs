using SecurityService.Endpoints;

namespace SecurityService
{
    using Bootstrapper;
    using Database.DbContexts;
    using Duende.IdentityServer.EntityFramework.DbContexts;
    using HealthChecks.UI.Client;
    using Lamar;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Diagnostics.HealthChecks;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.HttpOverrides;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using NLog.Extensions.Logging;
    using Shared.Extensions;
    using Shared.General;
    using Shared.Logger;
    using Shared.Middleware;
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using ILogger = Microsoft.Extensions.Logging.ILogger;

    [ExcludeFromCodeCoverage]
    public class Startup
    {
        #region Fields

        /// <summary>
        /// The container
        /// </summary>
        public static Container Container;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="webHostEnvironment">The web host environment.</param>
        public Startup(IWebHostEnvironment webHostEnvironment)
        {
            IConfigurationBuilder builder = new ConfigurationBuilder().SetBasePath(webHostEnvironment.ContentRootPath)
                                                                      .AddJsonFile("/home/txnproc/config/appsettings.json", true, true)
                                                                      .AddJsonFile($"/home/txnproc/config/appsettings.{webHostEnvironment.EnvironmentName}.json",
                                                                                   optional:true).AddJsonFile("appsettings.json", optional:true, reloadOnChange:true)
                                                                      .AddJsonFile($"appsettings.{webHostEnvironment.EnvironmentName}.json",
                                                                                   optional:true,
                                                                                   reloadOnChange:true).AddEnvironmentVariables();

            Startup.Configuration = builder.Build();
            Startup.WebHostEnvironment = webHostEnvironment;
        }

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

        #region Methods

        /// <summary>
        /// Configures the specified application.
        /// </summary>
        /// <param name="app">The application.</param>
        /// <param name="env">The env.</param>
        /// <param name="loggerFactory">The logger factory.</param>
        public void Configure(IApplicationBuilder app,
                              IWebHostEnvironment env,
                              ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            
            ILogger logger = loggerFactory.CreateLogger("Security Service");

            Logger.Initialise(logger);
            Startup.Configuration.LogConfiguration(Logger.LogWarning);
            app.UseMiddleware<TenantMiddleware>();
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
                                 endpoints.MapRazorPages();
                                 endpoints.MapDefaultControllerRoute();
                                 
                                 endpoints.MapApiResourceEndpoints();
                                 endpoints.MapApiScopeEndpoints();
                                 endpoints.MapIdentityResourceEndpoints();
                                 endpoints.MapUserEndpoints();
                                 endpoints.MapRoleEndpoints();
                                 endpoints.MapClientEndpoints();
                                 endpoints.MapDeveloperEndpoints();

                                 endpoints.MapHealthChecks("health",
                                                           new HealthCheckOptions
                                                           {
                                                               Predicate = _ => true,
                                                               ResponseWriter = Shared.HealthChecks.HealthCheckMiddleware.WriteResponse
                                                           });
                                 endpoints.MapHealthChecks("healthui",
                                                           new HealthCheckOptions
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

        /// <summary>
        /// Configures the container.
        /// </summary>
        /// <param name="services">The services.</param>
        public void ConfigureContainer(ServiceRegistry services)
        {
            ConfigurationReader.Initialise(Startup.Configuration);

            services.IncludeRegistry<IdentityServerRegistry>();
            services.IncludeRegistry<MiddlewareRegistry>();
            services.IncludeRegistry<MediatorRegistry>();
            services.IncludeRegistry<MiscRegistry>();

            Startup.Container = new Container(services);
        }

        /// <summary>
        /// Initializes the database.
        /// </summary>
        /// <param name="app">The application.</param>
        private void InitializeDatabase(IApplicationBuilder app)
        {
            using(IServiceScope serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                PersistedGrantDbContext persistedGrantDbContext = serviceScope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>();
                ConfigurationDbContext configurationDbContext = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
                AuthenticationDbContext authenticationContext = serviceScope.ServiceProvider.GetRequiredService<AuthenticationDbContext>();

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

        #endregion
    }
}