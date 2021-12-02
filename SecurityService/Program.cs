// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.


using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using System;

namespace SecurityService
{
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Security.Authentication;
    using System.Security.Cryptography.X509Certificates;
    using Microsoft.AspNetCore.Server.Kestrel.Core;
    using Microsoft.AspNetCore.Server.Kestrel.Https;
    using Microsoft.Extensions.Configuration;

    public class Program
    {
        public static IHostBuilder CreateHostBuilder(String[] args)
        {
            Console.Title = "Security Service";

            //At this stage, we only need our hosting file for ip and ports
            IConfigurationRoot config = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("hosting.json", optional: true)
                                                                  .AddJsonFile("hosting.development.json", optional: true).AddEnvironmentVariables().Build();

            

            IHostBuilder hostBuilder = Host.CreateDefaultBuilder(args);
            hostBuilder.ConfigureWebHostDefaults(webBuilder =>
                                                 {
                                                     webBuilder.UseStartup<Startup>();
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
                                                                                                  listenOptions.UseHttps(Program.LoadCertificate());
                                                                                              });
                                                                           });
                                                     
                                                 });
            return hostBuilder;
        }

        private static X509Certificate2 LoadCertificate()
        {
            //just to ensure that we are picking the right file! little bit of ugly code:
            var files = Directory.GetFiles(Directory.GetCurrentDirectory());
            var certificateFile = files.First(name => name.Contains("pfx"));
            Console.WriteLine(certificateFile);

            return new X509Certificate2(certificateFile, "password");
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