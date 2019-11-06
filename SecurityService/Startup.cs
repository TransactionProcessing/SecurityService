namespace SecurityService.Service
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Database.DbContexts;
    using Factories;
    using IdentityServer4.EntityFramework.DbContexts;
    using Lamar;
    using Manager;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.HttpOverrides;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.ApiExplorer;
    using Microsoft.AspNetCore.Mvc.Versioning;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.OpenApi.Models;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Newtonsoft.Json.Serialization;
    using NLog.Extensions.Logging;
    using Shared.Extensions;
    using Shared.General;
    using Swashbuckle.AspNetCore.Filters;
    using Swashbuckle.AspNetCore.Swagger;
    using Swashbuckle.AspNetCore.SwaggerGen;
    using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

    [ExcludeFromCodeCoverage]
    public class Startup
    {
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
            IConfigurationBuilder builder =
                new ConfigurationBuilder().SetBasePath(webHostEnvironment.ContentRootPath)
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

        #region Public Methods

        #region public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)        

        /// <summary>
        /// Configures the specified application.
        /// </summary>
        /// <param name="app">The application.</param>
        /// <param name="env">The env.</param>
        /// <param name="loggerFactory">The logger factory.</param>
        /// <param name="provider">The provider.</param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory,
                              IApiVersionDescriptionProvider provider)
        {
            String nlogConfigFilename = $"nlog.config";
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

            app.AddExceptionHandler();
            app.AddRequestLogging();
            app.AddResponseLogging();

            app.UseRouting();

            app.UseStaticFiles();            

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedProto
            });

            app.UseIdentityServer();
            
            // Setup the database
            //this.InitialiseDatabase(app, env).Wait();

            app.UseEndpoints(endpoints =>
                             {
                                 endpoints.MapControllers();
                             });

            app.UseSwagger();

            app.UseSwaggerUI(
                             options =>
                             {
                                 // build a swagger endpoint for each discovered API version
                                 foreach (ApiVersionDescription description in provider.ApiVersionDescriptions)
                                 {
                                     options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
                                 }
                             });
        }
        #endregion

        #endregion

        //#region public IServiceProvider ConfigureServices(IServiceCollection services)        
        ///// <summary>
        ///// Configures the services.
        ///// </summary>
        ///// <param name="services">The services.</param>
        ///// <returns></returns>
        //public void ConfigureContainer(ServiceRegistry services)
        //{
        //    Startup.ConfigureMiddlewareServices(services);

        //    Startup.GetConfiguredContainer(services, Startup.WebHostEnvironment);
        //}
        //#endregion

        //#region public static IContainer GetConfiguredContainer(IServiceCollection services, IHostingEnvironment hostingEnvironment)        
        ///// <summary>
        ///// Gets the configured container.
        ///// </summary>
        ///// <param name="services">The services.</param>
        ///// <param name="hostingEnvironment">The hosting environment.</param>
        ///// <returns></returns>
        //public static IContainer GetConfiguredContainer(ServiceRegistry services,
        //                                                IWebHostEnvironment webHostEnvironment)
        //{
        //    Startup.ConfigureCommonServices(services);

        //    Container container = new Container(services);

        //    return container;
        //}
        //#endregion

        //#region Private Methods

        //#region private static void ConfigureMiddlewareServices(IServiceCollection services)        
        ///// <summary>
        ///// Configures the middleware services.
        ///// </summary>
        ///// <param name="services">The services.</param>
        //private static void ConfigureMiddlewareServices(IServiceCollection services)
        //{            
        //    services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);;

        //    services.AddIdentity<IdentityUser, IdentityRole>(o =>
        //    {
        //        o.Password.RequireDigit = Startup.Configuration.GetValue<Boolean>("IdentityOptions:PasswordOptions:RequireDigit");
        //        o.Password.RequireLowercase =
        //            Startup.Configuration.GetValue<Boolean>("IdentityOptions:PasswordOptions:RequireLowercase");
        //        o.Password.RequireUppercase =
        //            Startup.Configuration.GetValue<Boolean>("IdentityOptions:PasswordOptions:RequireUppercase");
        //        o.Password.RequireNonAlphanumeric =
        //            Startup.Configuration.GetValue<Boolean>("IdentityOptions:PasswordOptions:RequireNonAlphanumeric");
        //        o.Password.RequiredLength = Startup.Configuration.GetValue<Int32>("IdentityOptions:PasswordOptions:RequiredLength");
        //    }).AddEntityFrameworkStores<AuthenticationDbContext>().AddDefaultTokenProviders();

        //        String migrationsAssembly = typeof(AuthenticationDbContext).GetTypeInfo().Assembly.GetName().Name;

        //        services.AddDbContext<ConfigurationDbContext>(builder =>
        //                builder.UseSqlServer(Startup.ConfigurationConnectionString, sqlOptions => sqlOptions.MigrationsAssembly(migrationsAssembly)))
        //            .AddTransient<ConfigurationDbContext>();

        //        services.AddDbContext<PersistedGrantDbContext>(builder =>
        //                builder.UseSqlServer(Startup.PersistedGrantStoreConenctionString, sqlOptions => sqlOptions.MigrationsAssembly(migrationsAssembly)))
        //            .AddTransient<PersistedGrantDbContext>();

        //        services.AddDbContext<AuthenticationDbContext>(builder =>
        //                builder.UseSqlServer(Startup.AuthenticationConenctionString, sqlOptions => sqlOptions.MigrationsAssembly(migrationsAssembly)))
        //            .AddTransient<AuthenticationDbContext>();

        //        services.AddIdentityServer(options =>
        //                {
        //                    options.Events.RaiseSuccessEvents = true;
        //                    options.Events.RaiseFailureEvents = true;
        //                    options.Events.RaiseErrorEvents = true;
        //                    options.PublicOrigin = Startup.Configuration.GetValue<String>("ServiceOptions:PublicOrigin");
        //                    options.IssuerUri = Startup.Configuration.GetValue<String>("ServiceOptions:PublicOrigin");
        //                })
        //            .AddConfigurationStore()
        //            .AddOperationalStore()
        //            .AddDeveloperSigningCredential()
        //            .AddIdentityServerStorage(Startup.ConfigurationConnectionString)
        //            .AddAspNetIdentity<IdentityUser>()
        //            .AddJwtBearerClientAuthentication();

        //        services.AddCors();

        //    // Read the authentication configuration
        //    //var securityConfig = new SecurityServiceConfiguration();
        //    //Configuration.GetSection("SecurityConfiguration").Bind(securityConfig);

        //    //services.AddAuthentication("Bearer")
        //    //    .AddIdentityServerAuthentication("token", options =>
        //    //    {
        //    //        options.Authority = securityConfig.SecurityService;
        //    //        options.RequireHttpsMetadata = false;
        //    //        options.ApiName = securityConfig.ApiName;
        //    //    });

        //    services.AddSwaggerGen(c =>
        //    {
        //        c.SwaggerDoc("v1", new Info { Title = "Security Service", Version = "v1" });
        //    });
        //}
        //#endregion

        //#region private static void ConfigureCommonServices(IServiceCollection services)        
        ///// <summary>
        ///// Configures the common services.
        ///// </summary>
        ///// <param name="services">The services.</param>
        //private static void ConfigureCommonServices(IServiceCollection services)
        //{
        //    services.AddSingleton<ISecurityServiceManager, SecurityServiceManager>();
        //    services.AddSingleton<IPasswordHasher<IdentityUser>, PasswordHasher<IdentityUser>>();
        //    services.AddSingleton<IUserClaimsPrincipalFactory<IdentityUser>, UserClaimsPrincipalFactory<IdentityUser>>();
        //    services.AddSingleton<ILogger<SignInManager<IdentityUser>>, Logger<SignInManager<IdentityUser>>>();

        //    Boolean useDummyMessagingService = Startup.Configuration.GetValue<Boolean>("ServiceOptions:UseDummyMessagingService");
        //    if (useDummyMessagingService)
        //    {
        //        services.AddSingleton<IMessagingService, DummyMessagingService>();
        //    }
        //    else
        //    {
        //        //services.AddSingleton<IMessagingService, MessagingService>();
        //    }            
        //}
        //#endregion

        //#region private async Task InitialiseDatabase(IApplicationBuilder app, IHostingEnvironment environment)
        ///// <summary>
        ///// Initialises the database.
        ///// </summary>
        ///// <param name="app">The application.</param>
        ///// <param name="environment">The environment.</param>
        //private async Task InitialiseDatabase(IApplicationBuilder app, IHostingEnvironment environment)
        //{
        //    using(IServiceScope scope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
        //    {
        //        PersistedGrantDbContext persistedGrantDbContext = scope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>();
        //        ConfigurationDbContext configurationDbContext = scope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
        //        AuthenticationDbContext authenticationDbContext = scope.ServiceProvider.GetRequiredService<AuthenticationDbContext>();

        //        DatabaseSeeding.InitialisePersistedGrantDatabase(persistedGrantDbContext);
        //        DatabaseSeeding.InitialiseConfigurationDatabase(configurationDbContext);
        //        DatabaseSeeding.InitialiseAuthenticationDatabase(authenticationDbContext);
        //    }
        //}

        //#endregion

        //#endregion

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

        public static IContainer Container { get; set; }

        private void ConfigureMiddlewareServices(IServiceCollection services)
        {
            services.AddApiVersioning(
                                      options =>
                                      {
                                          // reporting api versions will return the headers "api-supported-versions" and "api-deprecated-versions"
                                          options.ReportApiVersions = true;
                                          options.DefaultApiVersion = new ApiVersion(1, 0);
                                          options.AssumeDefaultVersionWhenUnspecified = true;
                                          options.ApiVersionReader = new HeaderApiVersionReader("api-version");
                                      });

            services.AddVersionedApiExplorer(
                                             options =>
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
                o.Password.RequireDigit = Startup.Configuration.GetValue<Boolean>("IdentityOptions:PasswordOptions:RequireDigit");
                o.Password.RequireLowercase =
                    Startup.Configuration.GetValue<Boolean>("IdentityOptions:PasswordOptions:RequireLowercase");
                o.Password.RequireUppercase =
                    Startup.Configuration.GetValue<Boolean>("IdentityOptions:PasswordOptions:RequireUppercase");
                o.Password.RequireNonAlphanumeric =
                    Startup.Configuration.GetValue<Boolean>("IdentityOptions:PasswordOptions:RequireNonAlphanumeric");
                o.Password.RequiredLength = Startup.Configuration.GetValue<Int32>("IdentityOptions:PasswordOptions:RequiredLength");
            }).AddEntityFrameworkStores<AuthenticationDbContext>().AddDefaultTokenProviders();

            String migrationsAssembly = typeof(AuthenticationDbContext).GetTypeInfo().Assembly.GetName().Name;
            services.AddDbContext<ConfigurationDbContext>(builder =>
                        builder.UseSqlServer(Startup.ConfigurationConnectionString, sqlOptions => sqlOptions.MigrationsAssembly(migrationsAssembly)))
                    .AddTransient<ConfigurationDbContext>();

            services.AddDbContext<PersistedGrantDbContext>(builder =>
                    builder.UseSqlServer(Startup.PersistedGrantStoreConenctionString, sqlOptions => sqlOptions.MigrationsAssembly(migrationsAssembly)))
                .AddTransient<PersistedGrantDbContext>();

            services.AddDbContext<AuthenticationDbContext>(builder =>
                    builder.UseSqlServer(Startup.AuthenticationConenctionString, sqlOptions => sqlOptions.MigrationsAssembly(migrationsAssembly)))
                .AddTransient<AuthenticationDbContext>();

            services.AddIdentityServer(options =>
                    {
                        options.Events.RaiseSuccessEvents = true;
                        options.Events.RaiseFailureEvents = true;
                        options.Events.RaiseErrorEvents = true;
                        options.PublicOrigin = Startup.Configuration.GetValue<String>("ServiceOptions:PublicOrigin");
                        options.IssuerUri = Startup.Configuration.GetValue<String>("ServiceOptions:PublicOrigin");
                    })
                .AddConfigurationStore()
                .AddOperationalStore()
                .AddDeveloperSigningCredential()
                .AddIdentityServerStorage(Startup.ConfigurationConnectionString)
                .AddAspNetIdentity<IdentityUser>()
                .AddJwtBearerClientAuthentication();

            services.AddCors();
        }

        private async Task InitialiseDatabase(IApplicationBuilder app)
        {
            using (IServiceScope scope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                PersistedGrantDbContext persistedGrantDbContext = scope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>();
                ConfigurationDbContext configurationDbContext = scope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
                AuthenticationDbContext authenticationDbContext = scope.ServiceProvider.GetRequiredService<AuthenticationDbContext>();

                if (persistedGrantDbContext.Database.IsSqlServer())
                {
                    persistedGrantDbContext.Database.Migrate();
                }

                if (configurationDbContext.Database.IsSqlServer())
                {
                    configurationDbContext.Database.Migrate();
                }

                if (authenticationDbContext.Database.IsSqlServer())
                {
                    authenticationDbContext.Database.Migrate();
                }
            }
        }
    }

    [ExcludeFromCodeCoverage]
    public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
    {
        #region Fields

        /// <summary>
        /// The provider
        /// </summary>
        private readonly IApiVersionDescriptionProvider provider;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigureSwaggerOptions"/> class.
        /// </summary>
        /// <param name="provider">The <see cref="IApiVersionDescriptionProvider">provider</see> used to generate Swagger documents.</param>
        public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider) => this.provider = provider;

        #endregion

        #region Methods

        /// <inheritdoc />
        public void Configure(SwaggerGenOptions options)
        {
            // add a swagger document for each discovered API version
            // note: you might choose to skip or document deprecated API versions differently
            foreach (ApiVersionDescription description in this.provider.ApiVersionDescriptions)
            {
                options.SwaggerDoc(description.GroupName, ConfigureSwaggerOptions.CreateInfoForApiVersion(description));
            }
        }

        /// <summary>
        /// Creates the information for API version.
        /// </summary>
        /// <param name="description">The description.</param>
        /// <returns></returns>
        private static OpenApiInfo CreateInfoForApiVersion(ApiVersionDescription description)
        {
            OpenApiInfo info = new OpenApiInfo
            {
                Title = "Golf Handicapping API",
                Version = description.ApiVersion.ToString(),
                Description = "A REST Api to manage the golf club handicapping system.",
                Contact = new OpenApiContact
                {
                    Name = "Stuart Ferguson",
                    Email = "golfhandicapping@btinternet.com"
                },
                License = new OpenApiLicense
                {
                    Name = "TODO"
                }
            };

            if (description.IsDeprecated)
            {
                info.Description += " This API version has been deprecated.";
            }

            return info;
        }
        
        #endregion
    }

    [ExcludeFromCodeCoverage]
    public class SwaggerDefaultValues : IOperationFilter
    {
        /// <summary>
        /// Applies the filter to the specified operation using the given context.
        /// </summary>
        /// <param name="operation">The operation to apply the filter to.</param>
        /// <param name="context">The current operation filter context.</param>
        public void Apply(OpenApiOperation operation,
                          OperationFilterContext context)
        {
            ApiDescription apiDescription = context.ApiDescription;
            ApiVersion apiVersion = apiDescription.GetApiVersion();
            ApiVersionModel model = apiDescription.ActionDescriptor.GetApiVersionModel(ApiVersionMapping.Explicit | ApiVersionMapping.Implicit);

            operation.Deprecated = model.DeprecatedApiVersions.Contains(apiVersion);

            if (operation.Parameters == null)
            {
                return;
            }

            foreach (OpenApiParameter parameter in operation.Parameters)
            {
                ApiParameterDescription description = apiDescription.ParameterDescriptions.First(p => p.Name == parameter.Name);

                if (parameter.Description == null)
                {
                    parameter.Description = description.ModelMetadata?.Description;
                }

                parameter.Required |= description.IsRequired;
            }
        }
    }

    [ExcludeFromCodeCoverage]
    public class SwaggerJsonConverter : JsonConverter
    {
        #region Properties

        /// <summary>
        /// Gets a value indicating whether this <see cref="T:Newtonsoft.Json.JsonConverter" /> can read JSON.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this <see cref="T:Newtonsoft.Json.JsonConverter" /> can read JSON; otherwise, <c>false</c>.
        /// </value>
        public override Boolean CanRead => false;

        #endregion

        #region Methods

        /// <summary>
        /// Determines whether this instance can convert the specified object type.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <returns>
        ///   <c>true</c> if this instance can convert the specified object type; otherwise, <c>false</c>.
        /// </returns>
        public override Boolean CanConvert(Type objectType)
        {
            return true;
        }

        /// <summary>
        /// Reads the JSON representation of the object.
        /// </summary>
        /// <param name="reader">The <see cref="T:Newtonsoft.Json.JsonReader" /> to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing value of object being read.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <returns>
        /// The object value.
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        /// <exception cref="NotImplementedException"></exception>
        public override Object ReadJson(JsonReader reader,
                                        Type objectType,
                                        Object existingValue,
                                        JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Writes the JSON representation of the object.
        /// </summary>
        /// <param name="writer">The <see cref="T:Newtonsoft.Json.JsonWriter" /> to write to.</param>
        /// <param name="value">The value.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override void WriteJson(JsonWriter writer,
                                       Object value,
                                       JsonSerializer serializer)
        {
            // Disable sending the $type in the serialized json
            serializer.TypeNameHandling = TypeNameHandling.None;

            JToken t = JToken.FromObject(value);
            t.WriteTo(writer);
        }

        #endregion
    }
}