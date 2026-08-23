using ClientProxyBase;
using HealthChecks.UI.Client;
using MediatR;
using MessagingService.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NLog;
using NLog.Extensions.Logging;
using OpenIddict.Abstractions;
using SecurityService.BusinessLogic;
using SecurityService.BusinessLogic.Requests;
using SecurityService.Common;
using SecurityService.Configuration;
using SecurityService.Database.DbContexts;
using SecurityService.Database.Entities;
using SecurityService.Endpoints;
using SecurityService.HealthChecks;
using SecurityService.HostedServices;
using Sentry.Extensibility;
using Shared.EntityFramework;
using Shared.Extensions;
using Shared.General;
using Shared.Logger;
using Shared.Logger.TennantContext;
using Shared.Middleware;
using Shared.Serialisation;
using System.Reflection;
using System.Security.Cryptography;
using static OpenIddict.Abstractions.OpenIddictConstants;
using static OpenIddict.Abstractions.OpenIddictConstants.Permissions;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using Logger = Shared.Logger.Logger;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = AppContext.BaseDirectory
});
builder.Host.UseWindowsService();

builder.WebHost.ConfigureAppConfiguration((context, configBuilder) =>
{
    var env = context.HostingEnvironment;

    configBuilder.SetBasePath(env.ContentRootPath)
        .AddJsonFile("hosting.json", optional: true)
        .AddJsonFile($"hosting.{env.EnvironmentName}.json", optional: true)
        .AddJsonFile("/home/txnproc/config/appsettings.json", optional: true, reloadOnChange: true)
        .AddJsonFile($"/home/txnproc/config/appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
        .AddEnvironmentVariables();

    // Build a snapshot of configuration so we can use it immediately (e.g. for Sentry)
    var builtConfig = configBuilder.Build();

    // Keep existing static usage (if you must), and initialise the ConfigurationReader now.
    ConfigurationReader.Initialise(builder.Configuration);

    // Configure Sentry on the webBuilder using the config snapshot.
    var sentrySection = builtConfig.GetSection("SentryConfiguration");
    if (sentrySection.Exists())
    {
        // Replace the condition below if you intended to only enable Sentry in certain environments.
        if (env.IsDevelopment() == false)
        {
            builder.WebHost.UseSentry(o =>
            {
                o.Dsn = builtConfig["SentryConfiguration:Dsn"];
                o.SendDefaultPii = true;
                o.MaxRequestBodySize = RequestSize.Always;
                o.CaptureBlockingCalls = ConfigurationReader.GetValueOrDefault("SentryConfiguration", "CaptureBlockingCalls", false);
                o.IncludeActivityData = ConfigurationReader.GetValueOrDefault("SentryConfiguration", "IncludeActivityData", false);
                o.Release = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown";
            });
        }
    }
});

var hostingUrls = builder.Configuration["urls"];
if (string.IsNullOrWhiteSpace(hostingUrls) == false)
{
    builder.WebHost.UseUrls(hostingUrls);
}

builder.Services.Configure<ServiceOptions>(builder.Configuration.GetSection("ServiceOptions"));
ServiceOptions options = new ServiceOptions();
builder.Configuration.GetSection("ServiceOptions").Bind(options);
var kestrelCertificate = KestrelCertificateLoader.LoadCertificate(options.KestrelOptions, builder.Environment.ContentRootPath);
if (kestrelCertificate is not null)
{
    builder.WebHost.ConfigureKestrel(serverOptions =>
    {
        serverOptions.ConfigureHttpsDefaults(httpsOptions =>
        {
            httpsOptions.ServerCertificate = kestrelCertificate;
        });
    });
}

if (options.UseInMemoryDatabase)
{
    builder.Services.AddDbContext<SecurityServiceDbContext>(builder => {
        builder.UseInMemoryDatabase(options.InMemoryDatabaseName);
        builder.UseOpenIddict();
    });
}
else
{
    SqlServerRetryOptions retryOptions;
    try
    {
        retryOptions = ConfigurationReader.GetSection<SqlServerRetryOptions>("AppSettings:SqlServerRetry");
    }
    catch (KeyNotFoundException)
    {
        retryOptions = null;
    }

    if (retryOptions != null)
    {
        builder.Services.AddDbContext<SecurityServiceDbContext>(options => {
            
            options.UseSharedSqlServer<SecurityServiceDbContext>(ConfigurationReader.GetConnectionString("AuthenticationDbContext"), retry => {
                retry.AdditionalTransientErrorNumbers = retryOptions.AdditionalTransientErrorNumbers;
                retry.MaxRetryCount = retryOptions.MaxRetryCount;
                retry.MaxRetryDelay = retryOptions.MaxRetryDelay;
            });
            options.UseOpenIddict();
        });
    }
    else
    {
        builder.Services.AddDbContext<SecurityServiceDbContext>(options => {
            options.UseSqlServer(ConfigurationReader.GetConnectionString("AuthenticationDbContext"), retry => {
                retry.EnableRetryOnFailure();
            });
            options.UseOpenIddict();
            
        });
        
    }
}


builder.Services.AddSingleton<IMessagingServiceClient, TestMessagingServiceClient>();
builder.Services.AddSingleton<IClientJwtService, ClientJwtService>();

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(identityOptions =>
{
    identityOptions.Password.RequireDigit = options.PasswordOptions.RequireDigit;
    identityOptions.Password.RequireNonAlphanumeric = options.PasswordOptions.RequireNonAlphanumeric;
    identityOptions.Password.RequireUppercase = options.PasswordOptions.RequireUppercase;
    identityOptions.Password.RequiredLength = options.PasswordOptions.RequiredLength;
    identityOptions.Password.RequiredUniqueChars = options.PasswordOptions.RequiredUniqueChars;
    identityOptions.Password.RequireLowercase = options.PasswordOptions.RequireLowercase;

    identityOptions.User.RequireUniqueEmail = options.UserOptions.RequireUniqueEmail;
    identityOptions.SignIn.RequireConfirmedEmail = options.SignInOptions.RequireConfirmedEmail;
})
.AddEntityFrameworkStores<SecurityServiceDbContext>()
.AddDefaultTokenProviders()
.AddTokenProvider<EmailConfirmationTokenProvider<ApplicationUser>>("emailconfirmation");

builder.Services.Configure<DataProtectionTokenProviderOptions>(opt => opt.TokenLifespan =
    TimeSpan.FromHours(options.TokenOptions.PasswordResetTokenExpiryInHours));
builder.Services.Configure<EmailConfirmationTokenProviderOptions>(opt =>
    opt.TokenLifespan = TimeSpan.FromHours(options.TokenOptions.EmailConfirmationTokenExpiryInHours));

builder.Services.ConfigureApplicationCookie(cookieOptions =>
{
    cookieOptions.LoginPath = "/Account/Login";
    cookieOptions.LogoutPath = "/Account/Logout";
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<TenantContext>();

if (builder.Environment.IsEnvironment("IntegrationTest"))
{
    builder.Services.AddSingleton<IMessagingServiceClient, TestMessagingServiceClient>();
}
else
{
    builder.Services.RegisterHttpClient<IMessagingServiceClient, MessagingServiceClient>();
}

StringSerialiser.Initialise(new SystemTextJsonSerializer(SystemTextJsonSerializer.GetDefaultJsonSerializerOptions()));

builder.Services.AddSingleton<Func<Object, String>>(_ => obj => StringSerialiser.Serialise(obj));
builder.Services.AddSingleton<Func<String, Type, Object>>(_ => (str, type) => StringSerialiser.DeserializeObject<Object>(str, type));
builder.Services.AddSingleton<Func<String, String>>(container => serviceName => { return ConfigurationReader.GetBaseServerUri(serviceName).OriginalString; });

bool logRequests = ConfigurationReader.GetValueOrDefault<Boolean>("MiddlewareLogging", "LogRequests", true);
bool logResponses = ConfigurationReader.GetValueOrDefault<Boolean>("MiddlewareLogging", "LogResponses", true);
LogLevel middlewareLogLevel = ConfigurationReader.GetValueOrDefault<LogLevel>("MiddlewareLogging", "MiddlewareLogLevel", LogLevel.Warning);

RequestResponseMiddlewareLoggingConfig config =
    new RequestResponseMiddlewareLoggingConfig(middlewareLogLevel, logRequests, logResponses);

builder.Services.AddSingleton(config);


builder.Services.AddAuthorization();
builder.Services.AddMediatR(configuration =>
{
    configuration.RegisterServicesFromAssembly(typeof(SecurityServiceCommands).Assembly);
});
builder.Services.ConfigureHttpJsonOptions(jsonOptions => SecurityService.Configuration.JsonSerializerConfiguration.ConfigureMinimalApi(jsonOptions.SerializerOptions));
builder.Services.AddRazorPages();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHostedService<DatabaseInitializer>();

if (builder.Environment.IsEnvironment("IntegrationTest")) {
    builder.Services.AddHealthChecks();
}
else if (options.UseInMemoryDatabase) {
    builder.Services.AddHealthChecks().AddMessagingService();
}
else {
    builder.Services.AddHealthChecks().AddMessagingService().AddCheck<DatabaseHealthCheck>("database").AddCheck<IssuerHealthCheck>("issuer");
}


builder.Services.AddHttpLogging(loggingOptions =>
{
    loggingOptions.LoggingFields = HttpLoggingFields.RequestMethod |
                                   HttpLoggingFields.RequestPath |
                                   HttpLoggingFields.ResponseStatusCode |
                                   HttpLoggingFields.Duration;
});

var rsa = RSA.Create(2048);
var key = new RsaSecurityKey(rsa);

var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.RsaSha256);
builder.Services.AddSingleton(signingCredentials);

builder.Services.AddOpenIddict()
    .AddCore(coreOptions =>
    {
        coreOptions.UseEntityFrameworkCore().UseDbContext<SecurityServiceDbContext>();
    })
    .AddServer(serverOptions =>
    {
        serverOptions.SetIssuer(new Uri(options.IssuerUrl));
        serverOptions.SetAuthorizationEndpointUris("/connect/authorize");
        serverOptions.SetTokenEndpointUris("/connect/token");
        serverOptions.SetEndSessionEndpointUris("/connect/logout");
        serverOptions.SetUserInfoEndpointUris("/connect/userinfo");
        serverOptions.SetIntrospectionEndpointUris("/connect/introspect");
        serverOptions.SetRevocationEndpointUris("/connect/revocation");
        serverOptions.SetDeviceAuthorizationEndpointUris("/connect/device");
        serverOptions.SetEndUserVerificationEndpointUris("/connect/verify");

        serverOptions.AllowAuthorizationCodeFlow()
                     .AllowClientCredentialsFlow()
                     .AllowDeviceAuthorizationFlow()
                     .AllowHybridFlow()
                     .AllowImplicitFlow()
                     .AllowPasswordFlow()
                     .AllowRefreshTokenFlow();

        serverOptions.RegisterScopes(OpenIddictConstants.Scopes.OpenId, OpenIddictConstants.Scopes.Profile, OpenIddictConstants.Scopes.Email, OpenIddictConstants.Scopes.OfflineAccess, OpenIddictConstants.Scopes.Roles);
        serverOptions.DisableAccessTokenEncryption();
        serverOptions.IgnoreEndpointPermissions();
        serverOptions.IgnoreGrantTypePermissions();
        serverOptions.IgnoreResponseTypePermissions();
        serverOptions.IgnoreScopePermissions();
        serverOptions.AddDevelopmentEncryptionCertificate();
        serverOptions.AddDevelopmentSigningCertificate();
        serverOptions.UseAspNetCore()
                     .DisableTransportSecurityRequirement()
                     .EnableAuthorizationEndpointPassthrough()
                     .EnableEndSessionEndpointPassthrough()
                     .EnableEndUserVerificationEndpointPassthrough()
                     .EnableStatusCodePagesIntegration()
                     .EnableTokenEndpointPassthrough()
                     .EnableUserInfoEndpointPassthrough();

        serverOptions.AddSigningKey(key);
    })
    .AddValidation(validationOptions =>
    {
        validationOptions.UseLocalServer();
        validationOptions.UseAspNetCore();
    });

String contentRoot = builder.Environment.ContentRootPath;
String nlogConfigPath = Path.Combine(contentRoot, "nlog.config");

LogManager.Setup(b =>
{
    b.SetupLogFactory(setup =>
    {
        setup.AddCallSiteHiddenAssembly(typeof(NlogLogger).Assembly);
        setup.AddCallSiteHiddenAssembly(typeof(Shared.Logger.Logger).Assembly);
        setup.AddCallSiteHiddenAssembly(typeof(TenantMiddleware).Assembly);
    });
    b.LoadConfigurationFromFile(nlogConfigPath);
});

builder.Host.ConfigureLogging(loggingBuilder =>
{
    loggingBuilder.AddNLog();
});

var app = builder.Build();

if (builder.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

ILogger logger = builder.Services.BuildServiceProvider().GetRequiredService<ILoggerFactory>().CreateLogger("Security Service");

Logger.Initialise(logger);
builder.Configuration.LogConfiguration(Logger.LogWarning);
app.UseMiddleware<TenantMiddleware>();
app.AddRequestResponseLogging();
app.AddExceptionHandler();

app.UseStatusCodePagesWithReExecute("/Home/Error");
app.UseHttpLogging();
app.UseSwagger();
app.UseSwaggerUI();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Redirect("/swagger"));
app.MapRazorPages();
app.MapDeveloperEndpoints();
app.MapManagementEndpoints();
app.MapOidcEndpoints();
app.MapHealthChecks("health", new HealthCheckOptions { Predicate = _ => true, ResponseWriter = Shared.HealthChecks.HealthCheckMiddleware.WriteResponse });
app.MapHealthChecks("healthui", new HealthCheckOptions { Predicate = _ => true, ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse });

app.Run();
