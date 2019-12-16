using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace SecurityService
{
    using System.ComponentModel;
    using IdentityServer4.EntityFramework.DbContexts;
    using IdentityServer4.EntityFramework.Interfaces;
    using IdentityServer4.EntityFramework.Options;
    using IdentityServer4.EntityFramework.Services;
    using IdentityServer4.EntityFramework.Storage;
    using IdentityServer4.EntityFramework.Stores;
    using IdentityServer4.Models;
    using IdentityServer4.Services;
    using IdentityServer4.Stores;
    using Manager;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;
    using SecurityService.Database.DbContexts;

    public class Startup
    {
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

        public Startup(IWebHostEnvironment webHostEnvironment)
        {
            IConfigurationBuilder builder = new ConfigurationBuilder().SetBasePath(webHostEnvironment.ContentRootPath)
                                                                      .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                                                                      .AddJsonFile($"appsettings.{webHostEnvironment.EnvironmentName}.json", optional: true)
                                                                      .AddEnvironmentVariables();

            Startup.Configuration = builder.Build();
            Startup.WebHostEnvironment = webHostEnvironment;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<ISecurityServiceManager, SecurityServiceManager>();

            this.ConfigureMiddlewareServices(services);
        }

        private void ConfigureMiddlewareServices(IServiceCollection services)
        {
            services.AddControllers().AddNewtonsoftJson(options =>
                                                        {
                                                            options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                                                            options.SerializerSettings.TypeNameHandling = TypeNameHandling.Auto;
                                                            options.SerializerSettings.Formatting = Formatting.Indented;
                                                            options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
                                                            options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                                                        });

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

            services.AddDbContext<AuthenticationDbContext>(builder => builder.UseInMemoryDatabase("Authentication")).AddTransient<AuthenticationDbContext>();

            services.AddIdentityServer(options =>
                                       {
                                           options.Events.RaiseErrorEvents = true;
                                           options.Events.RaiseFailureEvents = true;
                                           options.Events.RaiseInformationEvents = true;
                                           options.Events.RaiseSuccessEvents = true;
                                       }).AddIntegrationTestConfiguration().AddDeveloperSigningCredential(persistKey:false).AddAspNetIdentity<IdentityUser>()
                    .AddConfigurationStore(options => { options.ConfigureDbContext = c => c.UseInMemoryDatabase("Configuration"); });
        }
        
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseIdentityServer();

            app.UseRouting();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }

    public static class StartupExtensions
    {
        public static IIdentityServerBuilder AddIntegrationTestConfiguration(this IIdentityServerBuilder builder)
        {
            builder.AddInMemoryClients(new List<Client>());
            builder.AddInMemoryApiResources(new List<ApiResource>());
            //builder.AddInMemoryUsers(IdentityUserSeedData.GetIdentityUsers(SeedingType.IntegrationTest));
            //builder.AddInMemoryRoles(RoleSeedData.GetIdentityRoles(SeedingType.IntegrationTest));
            //builder.AddInMemoryUserRoles(IdentityUserRoleSeedData.GetIdentityUserRoles(SeedingType.IntegrationTest));
            builder.AddInMemoryIdentityResources(new List<IdentityResource>());

            builder.AddInMemoryPersistedGrants();

            return builder;
        }
    }
}
