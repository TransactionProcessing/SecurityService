namespace SecurityService.Service
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Database.DbContexts;
    using IdentityServer4.EntityFramework.DbContexts;
    using IdentityServer4.EntityFramework.Services;
    using IdentityServer4.EntityFramework.Stores;
    using IdentityServer4.Models;
    using IdentityServer4.Services;
    using IdentityServer4.Stores;
    using Microsoft.AspNetCore.Identity;
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
            //builder.AddInMemoryUsers(IdentityUserSeedData.GetIdentityUsers(SeedingType.IntegrationTest));
            //builder.AddInMemoryRoles(RoleSeedData.GetIdentityRoles(SeedingType.IntegrationTest));
            //builder.AddInMemoryUserRoles(IdentityUserRoleSeedData.GetIdentityUserRoles(SeedingType.IntegrationTest));
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