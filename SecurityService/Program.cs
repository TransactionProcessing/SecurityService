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
    using System.Security.Cryptography.X509Certificates;
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
                                                     webBuilder.UseKestrel();
                                                 });
            return hostBuilder;
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