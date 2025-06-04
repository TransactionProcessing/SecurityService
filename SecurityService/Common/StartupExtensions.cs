using Microsoft.Extensions.Logging;

namespace SecurityService
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Database.DbContexts;
    using Duende.IdentityServer.EntityFramework.DbContexts;
    using Duende.IdentityServer.Models;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Shared.General;

    [ExcludeFromCodeCoverage]
    public static class StartupExtensions
    {
        #region Methods

        public static IIdentityServerBuilder AddIdentityServerStorage(this IIdentityServerBuilder builder,
                                                                      String configurationConnectionString,
                                                                      String persistedGrantStoreConenctionString,
                                                                      String authenticationConenctionString) {
            builder.AddConfigurationStore<ConfigurationDbContext>(options => { options.ConfigureDbContext = c => c.UseSqlServer(configurationConnectionString, sqlOptions => sqlOptions.MigrationsAssembly("SecurityService.SqlServerMigrations"))
                .EnableSensitiveDataLogging()
                .LogTo(Console.WriteLine, LogLevel.Information); });

            builder.AddOperationalStore<PersistedGrantDbContext>(options => { options.ConfigureDbContext = c => c.UseSqlServer(persistedGrantStoreConenctionString, sqlOptions => sqlOptions.MigrationsAssembly("SecurityService.SqlServerMigrations"))
                .EnableSensitiveDataLogging()
                .LogTo(Console.WriteLine, LogLevel.Information); });

            builder.Services.AddDbContext<AuthenticationDbContext>(builder => builder.UseSqlServer(authenticationConenctionString, sqlOptions => sqlOptions.MigrationsAssembly("SecurityService.SqlServerMigrations")
            ).EnableSensitiveDataLogging()
            .LogTo(Console.WriteLine, LogLevel.Information));
            
            return builder;
        }

        public static IIdentityServerBuilder AddIntegrationTestConfiguration(this IIdentityServerBuilder builder)
        {
            builder.AddInMemoryClients(new List<Client>());
            builder.AddInMemoryApiResources(new List<ApiResource>());
            builder.AddInMemoryIdentityResources(new List<IdentityResource>());

            builder.AddConfigurationStore(options => { options.ConfigureDbContext = c => c.UseInMemoryDatabase("Configuration"); });
            builder.AddOperationalStore(options => { options.ConfigureDbContext = c => c.UseInMemoryDatabase("PersistedGrant"); });
            builder.Services.AddDbContext<AuthenticationDbContext>(builder => builder.UseInMemoryDatabase("Authentication"));

            builder.AddInMemoryPersistedGrants();

            return builder;
        }

        #endregion
    }
}