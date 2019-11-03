namespace SecurityService.Service
{
    using System;
    using System.Collections.Generic;
    using IdentityServer4.EntityFramework.Services;
    using IdentityServer4.EntityFramework.Stores;
    using IdentityServer4.Services;
    using IdentityServer4.Stores;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// 
    /// </summary>
    public static class StartupExtensions
    {
        #region Methods

        public static IIdentityServerBuilder AddIdentityServerStorage(this IIdentityServerBuilder builder,
                                                              String connectionString)
        {
            builder.AddConfigurationStore(connectionString);
            builder.AddOperationalStore(connectionString);

            return builder;
        }

        /// <summary>
        /// Adds the configuration store.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="connectionString">The connection string.</param>
        /// <returns></returns>
        public static IIdentityServerBuilder AddConfigurationStore(this IIdentityServerBuilder builder,
                                                                   String connectionString)
        {
            builder.Services.AddTransient<IClientStore, ClientStore>();
            builder.Services.AddTransient<IResourceStore, ResourceStore>();
            builder.Services.AddTransient<ICorsPolicyService, CorsPolicyService>();

            return builder;
        }

        /// <summary>
        /// Adds the configuration store cache.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns></returns>
        public static IIdentityServerBuilder AddConfigurationStoreCache(this IIdentityServerBuilder builder)
        {
            builder.Services.AddMemoryCache(); 
            builder.AddInMemoryCaching();

            // these need to be registered as concrete classes in DI for
            // the caching decorators to work
            builder.Services.AddTransient<ClientStore>();
            builder.Services.AddTransient<ResourceStore>();

            // add the caching decorators
            builder.AddClientStoreCache<ClientStore>();
            builder.AddResourceStoreCache<ResourceStore>();

            builder.AddOperationalStore();

            return builder;
        }


        /// <summary>
        /// Adds the operational store.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="connectionString">The connection string.</param>
        /// <returns></returns>
        public static IIdentityServerBuilder AddOperationalStore(this IIdentityServerBuilder builder,
                                                                 String connectionString)
        {
            builder.Services.AddTransient<IPersistedGrantStore, PersistedGrantStore>();

            return builder;
        }

        #endregion
    }
}