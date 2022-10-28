namespace SecurityService
{
    using System;
    using System.IO;
    using Bootstrapper;
    using Database.DbContexts;
    using Duende.IdentityServer.EntityFramework.DbContexts;
    using HealthChecks.UI.Client;
    using Lamar;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.DataProtection;
    using Microsoft.AspNetCore.Diagnostics.HealthChecks;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.HttpOverrides;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using NLog.Extensions.Logging;
    using Shared.Extensions;
    using Shared.General;
    using Shared.Logger;
    using ILogger = Microsoft.Extensions.Logging.ILogger;

    /// <summary>
    /// 
    /// </summary>
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
        /// Gets a value indicating whether this instance is my SQL.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is my SQL; otherwise, <c>false</c>.
        /// </value>
        public static Boolean IsMySql => string.Compare(Startup.GetDatabaseEngine, "MySql", StringComparison.InvariantCultureIgnoreCase) == 0;

        /// <summary>
        /// Gets a value indicating whether this instance is SQL server.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is SQL server; otherwise, <c>false</c>.
        /// </value>
        public static Boolean IsSqlServer =>
            Startup.GetDatabaseEngine == null || string.Compare(Startup.GetDatabaseEngine, "SqlServer", StringComparison.InvariantCultureIgnoreCase) == 0;

        /// <summary>
        /// Gets or sets the hosting environment.
        /// </summary>
        /// <value>
        /// The hosting environment.
        /// </value>
        public static IWebHostEnvironment WebHostEnvironment { get; set; }

        /// <summary>
        /// Gets the get database engine.
        /// </summary>
        /// <value>
        /// The get database engine.
        /// </value>
        private static String GetDatabaseEngine => ConfigurationReader.GetValue("AppSettings", "DatabaseEngine");

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
            String nlogConfigFilename = "nlog.config";
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            loggerFactory.ConfigureNLog(Path.Combine(Startup.WebHostEnvironment.ContentRootPath, nlogConfigFilename));
            loggerFactory.AddNLog();

            ILogger logger = loggerFactory.CreateLogger("Security Service");

            Logger.Initialise(logger);

            Action<String> loggerAction = message => { Logger.LogInformation(message); };
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
                                 endpoints.MapRazorPages();
                                 endpoints.MapDefaultControllerRoute();
                                 endpoints.MapHealthChecks("health",
                                                           new HealthCheckOptions
                                                           {
                                                               Predicate = _ => true,
                                                               ResponseWriter = Shared.HealthChecks.HealthCheckMiddleware.WriteResponse
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

    public class EmailConfirmationTokenProvider<TUser> : DataProtectorTokenProvider<TUser> where TUser : class
    {
        public EmailConfirmationTokenProvider(IDataProtectionProvider dataProtectionProvider,
                                              IOptions<EmailConfirmationTokenProviderOptions> options,
                                              ILogger<DataProtectorTokenProvider<TUser>> logger)
            : base(dataProtectionProvider, options, logger)
        {
        }
    }

    public class EmailConfirmationTokenProviderOptions : DataProtectionTokenProviderOptions
    {

    }
}