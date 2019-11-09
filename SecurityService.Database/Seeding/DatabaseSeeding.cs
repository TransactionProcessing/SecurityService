namespace SecurityService.Database.Seeding
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using DbContexts;
    using IdentityServer4.EntityFramework.DbContexts;
    using IdentityServer4.EntityFramework.Entities;
    using Microsoft.EntityFrameworkCore;

    public class DatabaseSeeding
    {
        #region Methods

        /// <summary>
        /// Initialises the authentication database.
        /// </summary>
        /// <param name="authenticationDbContext">The authentication database context.</param>
        /// <param name="seedingType">Type of the seeding.</param>
        public static void InitialiseAuthenticationDatabase(AuthenticationDbContext authenticationDbContext,
                                                            SeedingType seedingType)
        {
            Boolean isDbInitialised = false;
            Int32 retryCounter = 0;
            while (retryCounter < 20 && !isDbInitialised)
            {
                try
                {
                    if (authenticationDbContext.Database.IsSqlServer())
                    {
                        authenticationDbContext.Database.Migrate();
                    }
                    
                    isDbInitialised = true;
                    break;
                }
                catch (Exception ex)
                {
                    retryCounter++;
                    Thread.Sleep(10000);
                }
            }

            if (!isDbInitialised)
            {
                String connString = authenticationDbContext.Database.GetDbConnection().ConnectionString;

                Exception newException = new Exception($"Error initialising Db with Connection String [{connString}]");
                throw newException;
            }
        }

        /// <summary>
        /// Initialises the configuration database.
        /// </summary>
        /// <param name="configurationDbContext">The configuration database context.</param>
        /// <param name="seedingType">Type of the seeding.</param>
        public static void InitialiseConfigurationDatabase(ConfigurationDbContext configurationDbContext,
                                                           SeedingType seedingType)
        {
            Boolean isDbInitialised = false;
            Int32 retryCounter = 0;
            while (retryCounter < 20 && !isDbInitialised)
            {
                try
                {
                    if (configurationDbContext.Database.IsSqlServer())
                    {
                        configurationDbContext.Database.Migrate();
                    }

                    DatabaseSeeding.AddIdentityResources(configurationDbContext, seedingType);

                    configurationDbContext.SaveChanges();

                    isDbInitialised = true;
                    break;
                }
                catch (Exception ex)
                {
                    retryCounter++;
                    Thread.Sleep(10000);
                }
            }

            if (!isDbInitialised)
            {
                String connString = configurationDbContext.Database.GetDbConnection().ConnectionString;

                Exception newException = new Exception($"Error initialising Db with Connection String [{connString}]");
                throw newException;
            }
        }

        /// <summary>
        /// Initialises the persisted grant database.
        /// </summary>
        /// <param name="persistedGrantDbContext">The persisted grant database context.</param>
        /// <param name="seedingType">Type of the seeding.</param>
        public static void InitialisePersistedGrantDatabase(PersistedGrantDbContext persistedGrantDbContext,
                                                            SeedingType seedingType)
        {
            Boolean isDbInitialised = false;
            Int32 retryCounter = 0;
            while (retryCounter < 20 && !isDbInitialised)
            {
                try
                {
                    if (persistedGrantDbContext.Database.IsSqlServer())
                    {
                        persistedGrantDbContext.Database.Migrate();
                    }

                    isDbInitialised = true;
                    break;
                }
                catch (Exception ex)
                {
                    retryCounter++;
                    Thread.Sleep(10000);
                }
            }

            if (!isDbInitialised)
            {
                String connString = persistedGrantDbContext.Database.GetDbConnection().ConnectionString;

                Exception newException = new Exception($"Error initialising Db with Connection String [{connString}]");
                throw newException;
            }
        }

        /// <summary>
        /// Adds the identity resources.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="seedingType">Type of the seeding.</param>
        private static void AddIdentityResources(ConfigurationDbContext context,
                                                 SeedingType seedingType)
        {
            List<IdentityResource> identityResources = IdentityResourceSeedData.GetIdentityResources(seedingType);

            foreach (IdentityResource identityResource in identityResources)
            {
                Boolean foundResource = context.IdentityResources.Any(a => a.Name == identityResource.Name);

                if (!foundResource)
                {
                    context.IdentityResources.Add(identityResource);
                    context.SaveChanges();
                }
            }
        }

        #endregion
    }
}
