// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.


using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using NLog.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using Shared.Logger;
using Shared.Middleware;
using System;
using Sentry.Extensibility;

namespace SecurityService
{
    using Lamar.Microsoft.DependencyInjection;
    using Microsoft.AspNetCore.Server.Kestrel.Core;
    using Microsoft.AspNetCore.Server.Kestrel.Https;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;
    using NLog;
    using Shared.General;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Reflection;
    using System.Security.Authentication;
    using System.Security.Cryptography.X509Certificates;

    [ExcludeFromCodeCoverage]
    public class Program
    {
        public static IHostBuilder CreateHostBuilder(String[] args)
        {
            //At this stage, we only need our hosting file for ip and ports
            String contentRoot = Directory.GetCurrentDirectory();
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

            IHostBuilder hostBuilder = Host.CreateDefaultBuilder(args);
            hostBuilder.UseWindowsService();
            hostBuilder.UseLamar();
            hostBuilder.ConfigureLogging(logging => {
                logging.AddConsole();
                logging.AddNLog();
            });

            ConfigureWeb(hostBuilder);
            
            return hostBuilder;
        }

        private static void ConfigureWeb(IHostBuilder hostBuilder) {
            FileInfo fi = new FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location);
            IConfigurationRoot config = new ConfigurationBuilder().SetBasePath(fi.Directory.FullName).AddJsonFile("hosting.json", optional: false)
                .AddJsonFile("hosting.development.json", optional: true).AddEnvironmentVariables().Build();

            hostBuilder.ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.ConfigureAppConfiguration((context, configBuilder) =>
                {
                    var env = context.HostingEnvironment;

                    configBuilder.SetBasePath(fi.Directory.FullName)
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
                    Startup.Configuration = builtConfig;
                    ConfigurationReader.Initialise(Startup.Configuration);

                    // Configure Sentry on the webBuilder using the config snapshot.
                    var sentrySection = builtConfig.GetSection("SentryConfiguration");
                    if (sentrySection.Exists())
                    {
                        // Replace the condition below if you intended to only enable Sentry in certain environments.
                        if (env.IsDevelopment() == false)
                        {
                            webBuilder.UseSentry(o =>
                            {
                                o.Dsn = builtConfig["SentryConfiguration:Dsn"];
                                o.SendDefaultPii = true;
                                o.MaxRequestBodySize = RequestSize.Always;
                                o.CaptureBlockingCalls = true;
                                o.Release = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown";
                            });
                        }
                    }
                });

                webBuilder.UseStartup<Startup>();
                webBuilder.ConfigureServices(services =>
                {
                    services.AddRazorPages();
                    
                    // This is important, the call to AddControllers()
                    // cannot be made before the usage of ConfigureWebHostDefaults
                    services.AddControllers()
                        .AddNewtonsoftJson(options =>
                    {
                        options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                        options.SerializerSettings.TypeNameHandling = TypeNameHandling.Auto;
                        options.SerializerSettings.Formatting = Formatting.Indented;
                        options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
                        options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                    }); ;
                });
                webBuilder.UseConfiguration(config);
                webBuilder.UseKestrel(options =>
                {
                    var urls = config.GetSection("urls").Value.Split(":");
                    var port = Int32.Parse(urls[2]);

                    options.Listen(IPAddress.Any,
                                   port,
                                   listenOptions =>
                                   {
                                       // Enable support for HTTP1 and HTTP2 (required if you want to host gRPC endpoints)
                                       listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
                                       // Configure Kestrel to use a certificate from a local .PFX file for hosting HTTPS
                                       listenOptions.UseHttps(Program.LoadCertificate(fi.Directory.FullName));
                                   });
                });

            });
        }

        private static X509Certificate2 LoadCertificate(String path)
        {
            //just to ensure that we are picking the right file! little bit of ugly code:
            var files = Directory.GetFiles(path);
            var certificateFile = files.First(name => name.Contains("pfx"));
            
            var cert = new X509Certificate2(certificateFile, "password");
            return cert;
        }

        public static void Main(String[] args)
        {
            Log.Logger = new LoggerConfiguration()
                         .MinimumLevel.Verbose()
                         .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                         .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
                         .MinimumLevel.Override("System", LogEventLevel.Warning)
                         .MinimumLevel.Override("Microsoft.AspNetCore.Authentication", LogEventLevel.Information)
                         .Enrich.FromLogContext()
                         // uncomment to write to Azure diagnostics stream
                         //.WriteTo.File(
                         //    @"D:\home\LogFiles\Application\identityserver.txt",
                         //    fileSizeLimitBytes: 1_000_000,
                         //    rollOnFileSizeLimit: true,
                         //    shared: true,
                         //    flushToDiskInterval: TimeSpan.FromSeconds(1))
                         .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}", theme: AnsiConsoleTheme.Code)
                         .CreateLogger();

            Program.CreateHostBuilder(args).Build().Run();
        }
    }
}