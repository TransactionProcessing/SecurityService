namespace SecurityService
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Database.DbContexts;
    using Duende.IdentityServer.Models;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;

    [ExcludeFromCodeCoverage]
    public static class StartupExtensions
    {
        #region Methods

        public static IIdentityServerBuilder AddIdentityServerStorage(this IIdentityServerBuilder builder,
                                                              String configurationConnectionString,
                                                              String persistedGrantStoreConenctionString,
                                                              String authenticationConenctionString,
                                                              String migrationsAssembly)
        {
            builder.AddConfigurationStore(options =>
                                          {
                                              options.ConfigureDbContext =
                                                  c => c.UseSqlServer(configurationConnectionString, sqlOptions => sqlOptions.MigrationsAssembly(migrationsAssembly));
                                          });

            builder.AddOperationalStore(options =>
                                          {
                                              options.ConfigureDbContext =
                                                  c => c.UseSqlServer(persistedGrantStoreConenctionString, sqlOptions => sqlOptions.MigrationsAssembly(migrationsAssembly));
                                          });

            builder.Services.AddDbContext<AuthenticationDbContext>(builder => builder.UseSqlServer(authenticationConenctionString, sqlOptions => sqlOptions.MigrationsAssembly(migrationsAssembly)));

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