namespace SecurityService
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Net.Http;
    using System.Reflection;
    using System.Threading.Tasks;
    using Database.DbContexts;
    using Database.Seeding;
    using Factories;
    using HealthChecks.UI.Client;
    using IdentityServer4.EntityFramework.DbContexts;
    using IdentityServer4.Extensions;
    using Manager;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Diagnostics.HealthChecks;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.HttpOverrides;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.ApiExplorer;
    using Microsoft.AspNetCore.Mvc.Versioning;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Diagnostics.HealthChecks;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;
    using NLog.Extensions.Logging;
    using NuGet.Versioning;
    using Shared.Extensions;
    using Shared.General;
    using Shared.Logger;
    using Swashbuckle.AspNetCore.Filters;
    using Swashbuckle.AspNetCore.SwaggerGen;
    using ILogger = Microsoft.Extensions.Logging.ILogger;

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
            app.Use(async (ctx, next) =>
                    {
                        var gimp = Configuration.GetValue<String>("ServiceOptions:PublicOrigin");
                        ctx.SetIdentityServerOrigin(gimp);
                        await next();
                    });

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

            app.UseRouting();
            // Block 4:
            //  UseIdentityServer include a call to UseAuthentication
            app.UseIdentityServer();
            app.UseAuthorization();

            app.UseStaticFiles();

            app.UseForwardedHeaders(new ForwardedHeadersOptions
                                    {
                                        ForwardedHeaders = ForwardedHeaders.XForwardedProto
                                    });

            app.UseIdentityServer();

            // Setup the database
            this.InitialiseDatabase(app).Wait();

            app.UseEndpoints(endpoints =>
                             {
                                 endpoints.MapControllers();
                                 endpoints.MapHealthChecks("health", new HealthCheckOptions()
                                                                     {
                                                                         Predicate = _ => true,
                                                                         ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                                                                     });
                             });

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

        public void ConfigureServices(IServiceCollection services)
        {
            ConfigurationReader.Initialise(Startup.Configuration);

            this.ConfigureMiddlewareServices(services);

            services.AddControllersWithViews().AddNewtonsoftJson(options =>
                                                                 {
                                                                     options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                                                                     options.SerializerSettings.TypeNameHandling = TypeNameHandling.Auto;
                                                                     options.SerializerSettings.Formatting = Formatting.Indented;
                                                                     options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
                                                                     options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                                                                 });

            services.AddScoped<ISecurityServiceManager, SecurityServiceManager>();
            services.AddSingleton<IModelFactory, ModelFactory>();
        }
        
        private void ConfigureMiddlewareServices(IServiceCollection services)
        {
            services.AddHealthChecks()
                    .AddSqlServer(connectionString:ConfigurationReader.GetConnectionString("PersistedGrantDbContext"),
                                  healthQuery:"SELECT 1;",
                                  name:"Persisted Grant DB",
                                  failureStatus:HealthStatus.Unhealthy,
                                  tags:new string[] {"db", "sql", "sqlserver", "persistedgrant"})
                    .AddSqlServer(connectionString:ConfigurationReader.GetConnectionString("ConfigurationDbContext"),
                                  healthQuery:"SELECT 1;",
                                  name:"Configuration DB",
                                  failureStatus:HealthStatus.Unhealthy,
                                  tags:new string[] {"db", "sql", "sqlserver", "configuration"})
                    .AddSqlServer(connectionString:ConfigurationReader.GetConnectionString("AuthenticationDbContext"),
                                  healthQuery:"SELECT 1;",
                                  name:"Authentication DB",
                                  failureStatus:HealthStatus.Unhealthy,
                                  tags:new string[] {"db", "sql", "sqlserver", "authentication"})
                    .AddUrlGroup(new Uri($"{ConfigurationReader.GetValue("ServiceAddresses", "MessagingService")}/health"),
                                 name: "Messaging Service",
                                 httpMethod: HttpMethod.Get,
                                 failureStatus: HealthStatus.Unhealthy,
                                 tags: new string[] { "application", "messaging" });

            var version = ConfigurationReader.GetValue("ServiceOptions", "ApiVersion");
            var v = NuGetVersion.Parse(version);
            services.AddApiVersioning(options =>
                                      {
                                          // reporting api versions will return the headers "api-supported-versions" and "api-deprecated-versions"
                                          options.ReportApiVersions = true;
                                          options.DefaultApiVersion = new ApiVersion(v.Major, v.Minor, $"Patch{v.Patch}");
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

            IIdentityServerBuilder identityServerBuilder = services.AddIdentityServer(options =>
                                                                                      {
                                                                                          options.Events.RaiseSuccessEvents = true;
                                                                                          options.Events.RaiseFailureEvents = true;
                                                                                          options.Events.RaiseErrorEvents = true;
                                                                                          // TODO: Investigate - https://github.com/IdentityServer/IdentityServer4/issues/4631
                                                                                          //options.PublicOrigin = Startup.Configuration.GetValue<String>("ServiceOptions:PublicOrigin");
                                                                                          options.IssuerUri =
                                                                                              Startup.Configuration.GetValue<String>("ServiceOptions:IssuerUrl");
                                                                                      })
                                                                   .AddAspNetIdentity<IdentityUser>()
                                                                   .AddJwtBearerClientAuthentication()
                                                                   .AddDeveloperSigningCredential();

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