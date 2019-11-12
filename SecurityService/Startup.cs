namespace SecurityService.Service
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Reflection;
    using System.Threading.Tasks;
    using Database.DbContexts;
    using Database.Seeding;
    using Factories;
    using IdentityServer4.EntityFramework.DbContexts;
    using IdentityServer4.EntityFramework.Interfaces;
    using Lamar;
    using Manager;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.HttpOverrides;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.ApiExplorer;
    using Microsoft.AspNetCore.Mvc.Versioning;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;
    using NLog.Extensions.Logging;
    using Shared.Extensions;
    using Shared.General;
    using Swashbuckle.AspNetCore.Filters;
    using Swashbuckle.AspNetCore.SwaggerGen;

    [ExcludeFromCodeCoverage]
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

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="env">The env.</param>
        public Startup(IWebHostEnvironment webHostEnvironment)
        {
            IConfigurationBuilder builder = new ConfigurationBuilder().SetBasePath(webHostEnvironment.ContentRootPath)
                                                                      .AddJsonFile("appsettings.json", optional:true, reloadOnChange:true)
                                                                      .AddJsonFile($"appsettings.{webHostEnvironment.EnvironmentName}.json", optional:true)
                                                                      .AddEnvironmentVariables();

            Startup.Configuration = builder.Build();
            Startup.WebHostEnvironment = webHostEnvironment;

            // Get the DB Connection Strings
            Startup.PersistedGrantStoreConenctionString = Startup.Configuration.GetConnectionString(nameof(PersistedGrantDbContext));
            Startup.ConfigurationConnectionString = Startup.Configuration.GetConnectionString(nameof(ConfigurationDbContext));
            Startup.AuthenticationConenctionString = Startup.Configuration.GetConnectionString(nameof(AuthenticationDbContext));
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

        public static IContainer Container { get; set; }

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
        /// <param name="provider">The provider.</param>
        public void Configure(IApplicationBuilder app,
                              IWebHostEnvironment env,
                              ILoggerFactory loggerFactory,
                              IApiVersionDescriptionProvider provider)
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

            ConfigurationReader.Initialise(Startup.Configuration);

            app.AddRequestLogging();
            app.AddResponseLogging();
            app.AddExceptionHandler();

            app.UseRouting();

            app.UseStaticFiles();

            app.UseForwardedHeaders(new ForwardedHeadersOptions
                                    {
                                        ForwardedHeaders = ForwardedHeaders.XForwardedProto
                                    });

            app.UseIdentityServer();

            // Setup the database
            this.InitialiseDatabase(app).Wait();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });

            app.UseSwagger();

            app.UseSwaggerUI(options =>
                             {
                                 // build a swagger endpoint for each discovered API version
                                 foreach (ApiVersionDescription description in provider.ApiVersionDescriptions)
                                 {
                                     options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
                                 }
                             });
        }

        public void ConfigureContainer(ServiceRegistry services)
        {
            this.ConfigureMiddlewareServices(services);

            services.AddControllers().AddNewtonsoftJson(options =>
                                                        {
                                                            options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                                                            options.SerializerSettings.TypeNameHandling = TypeNameHandling.Auto;
                                                            options.SerializerSettings.Formatting = Formatting.Indented;
                                                            options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
                                                            options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                                                        });

            Startup.GetConfiguredContainer(services, Startup.WebHostEnvironment);
        }

        public static IContainer GetConfiguredContainer(ServiceRegistry services,
                                                        IWebHostEnvironment webHostEnvironment)
        {
            Container container = new Container(services);

            // TODO: Build a registry file
            //services.IncludeRegistry<CommonRegistry>();
            services.AddSingleton<ISecurityServiceManager, SecurityServiceManager>();
            services.AddSingleton<IModelFactory, ModelFactory>();

            container.Configure(services);

            Startup.Container = container;

            return container;
        }

        private void ConfigureMiddlewareServices(IServiceCollection services)
        {
            services.AddApiVersioning(options =>
                                      {
                                          // reporting api versions will return the headers "api-supported-versions" and "api-deprecated-versions"
                                          options.ReportApiVersions = true;
                                          options.DefaultApiVersion = new ApiVersion(1, 0);
                                          options.AssumeDefaultVersionWhenUnspecified = true;
                                          options.ApiVersionReader = new HeaderApiVersionReader("api-version");
                                      });

            services.AddVersionedApiExplorer(options =>
                                             {
                                                 // add the versioned api explorer, which also adds IApiVersionDescriptionProvider service
                                                 // note: the specified format code will format the version as "'v'major[.minor][-status]"
                                                 options.GroupNameFormat = "'v'VVV";

                                                 // note: this option is only necessary when versioning by url segment. the SubstitutionFormat
                                                 // can also be used to control the format of the API version in route templates
                                                 options.SubstituteApiVersionInUrl = true;
                                             });

            services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();

            services.AddSwaggerGen(c =>
                                   {
                                       // add a custom operation filter which sets default values
                                       c.OperationFilter<SwaggerDefaultValues>();
                                       c.ExampleFilters();
                                   });

            services.AddSwaggerExamplesFromAssemblyOf<SwaggerJsonConverter>();

            services.AddIdentity<IdentityUser, IdentityRole>(o =>
                                                             {
                                                                 o.Password.RequireDigit =
                                                                     Startup.Configuration.GetValue<Boolean>("IdentityOptions:PasswordOptions:RequireDigit");
                                                                 o.Password.RequireLowercase =
                                                                     Startup.Configuration.GetValue<Boolean>("IdentityOptions:PasswordOptions:RequireLowercase");
                                                                 o.Password.RequireUppercase =
                                                                     Startup.Configuration.GetValue<Boolean>("IdentityOptions:PasswordOptions:RequireUppercase");
                                                                 o.Password.RequireNonAlphanumeric =
                                                                     Startup.Configuration.GetValue<Boolean>("IdentityOptions:PasswordOptions:RequireNonAlphanumeric");
                                                                 o.Password.RequiredLength =
                                                                     Startup.Configuration.GetValue<Int32>("IdentityOptions:PasswordOptions:RequiredLength");
                                                             }).AddEntityFrameworkStores<AuthenticationDbContext>().AddDefaultTokenProviders();

            if (Startup.WebHostEnvironment.IsEnvironment("IntegrationTest"))
            {
                services.AddIdentityServer(options =>
                                           {
                                               options.Events.RaiseSuccessEvents = true;
                                               options.Events.RaiseFailureEvents = true;
                                               options.Events.RaiseErrorEvents = true;
                                               options.PublicOrigin = Startup.Configuration.GetValue<String>("ServiceOptions:PublicOrigin");
                                               options.IssuerUri = Startup.Configuration.GetValue<String>("ServiceOptions:PublicOrigin");
                                           }).AddDeveloperSigningCredential().AddAspNetIdentity<IdentityUser>().AddIntegrationTestConfiguration()
                        .AddJwtBearerClientAuthentication();

                services.AddDbContext<AuthenticationDbContext>(builder => builder.UseInMemoryDatabase("Authentication")).AddTransient<AuthenticationDbContext>();

                services.AddDbContext<ConfigurationDbContext>(builder => builder.UseInMemoryDatabase("Configuration")).AddTransient<IConfigurationDbContext, ConfigurationDbContext>();

                services.AddDbContext<PersistedGrantDbContext>(builder => builder.UseInMemoryDatabase("PersistedGrantStore")).AddTransient<IPersistedGrantDbContext, PersistedGrantDbContext>();
            }
            else
            {
                String migrationsAssembly = typeof(AuthenticationDbContext).GetTypeInfo().Assembly.GetName().Name;
                services.AddDbContext<ConfigurationDbContext>(builder => builder.UseSqlServer(Startup.ConfigurationConnectionString,
                                                                                              sqlOptions => sqlOptions.MigrationsAssembly(migrationsAssembly)))
                        .AddTransient<IConfigurationDbContext, ConfigurationDbContext>();

                services.AddDbContext<PersistedGrantDbContext>(builder => builder.UseSqlServer(Startup.PersistedGrantStoreConenctionString,
                                                                                               sqlOptions => sqlOptions.MigrationsAssembly(migrationsAssembly)))
                        .AddTransient<IPersistedGrantDbContext, PersistedGrantDbContext>();

                services.AddDbContext<AuthenticationDbContext>(builder => builder.UseSqlServer(Startup.AuthenticationConenctionString,
                                                                                               sqlOptions => sqlOptions.MigrationsAssembly(migrationsAssembly)))
                        .AddTransient<AuthenticationDbContext>();

                services.AddIdentityServer(options =>
                                           {
                                               options.Events.RaiseSuccessEvents = true;
                                               options.Events.RaiseFailureEvents = true;
                                               options.Events.RaiseErrorEvents = true;
                                               options.PublicOrigin = Startup.Configuration.GetValue<String>("ServiceOptions:PublicOrigin");
                                               options.IssuerUri = Startup.Configuration.GetValue<String>("ServiceOptions:PublicOrigin");
                                           }).AddConfigurationStore().AddOperationalStore().AddDeveloperSigningCredential()
                        .AddIdentityServerStorage(Startup.ConfigurationConnectionString).AddAspNetIdentity<IdentityUser>().AddJwtBearerClientAuthentication();
            }

            services.AddCors();
        }

        private async Task InitialiseDatabase(IApplicationBuilder app)
        {
            using(IServiceScope scope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                PersistedGrantDbContext persistedGrantDbContext = scope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>();
                ConfigurationDbContext configurationDbContext = scope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
                AuthenticationDbContext authenticationDbContext = scope.ServiceProvider.GetRequiredService<AuthenticationDbContext>();

                SeedingType seedingType = Startup.Configuration.GetValue<SeedingType>("SeedingType");

                DatabaseSeeding.InitialiseAuthenticationDatabase(authenticationDbContext, seedingType);
                DatabaseSeeding.InitialiseConfigurationDatabase(configurationDbContext, seedingType);
                DatabaseSeeding.InitialisePersistedGrantDatabase(persistedGrantDbContext, seedingType);
            }
        }

        #endregion
    }
}