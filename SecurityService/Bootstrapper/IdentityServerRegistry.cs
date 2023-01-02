﻿namespace SecurityService.Bootstrapper
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Common;
    using Database.DbContexts;
    using Lamar;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    [ExcludeFromCodeCoverage]
    public class IdentityServerRegistry : ServiceRegistry
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="IdentityServerRegistry"/> class.
        /// </summary>
        public IdentityServerRegistry()
        {
            // Get the DB Connection Strings
            String persistedGrantStoreConenctionString = Startup.Configuration.GetConnectionString("PersistedGrantDbContext");
            String configurationConnectionString = Startup.Configuration.GetConnectionString("ConfigurationDbContext");
            String authenticationConenctionString = Startup.Configuration.GetConnectionString("AuthenticationDbContext");

            this.AddIdentity<IdentityUser, IdentityRole>(opt => {
                                                             opt.Tokens.EmailConfirmationTokenProvider = "emailconfirmation";
                                                         }).AddEntityFrameworkStores<AuthenticationDbContext>()
                .AddDefaultTokenProviders()                                            
                .AddTokenProvider<EmailConfirmationTokenProvider<IdentityUser>>("emailconfirmation");

            this.Configure<DataProtectionTokenProviderOptions>(opt =>
                                                                       opt.TokenLifespan = TimeSpan.FromHours(2));
            this.Configure<EmailConfirmationTokenProviderOptions>(opt =>
                                                                          opt.TokenLifespan = TimeSpan.FromDays(3));


            IIdentityServerBuilder identityServerBuilder = this.AddIdentityServer(options =>
                                                                                  {
                                                                                      // https://docs.duendesoftware.com/identityserver/v5/fundamentals/resources/
                                                                                      options.EmitStaticAudienceClaim = true;

                                                                                      options.Events.RaiseSuccessEvents = true;
                                                                                      options.Events.RaiseFailureEvents = true;
                                                                                      options.Events.RaiseErrorEvents = true;

                                                                                      options.IssuerUri =
                                                                                          Startup.Configuration.GetValue<String>("ServiceOptions:IssuerUrl");
                                                                                  });

            identityServerBuilder.AddAspNetIdentity<IdentityUser>();

            if (Startup.WebHostEnvironment.IsEnvironment("IntegrationTest") || Startup.Configuration.GetValue<Boolean>("ServiceOptions:UseInMemoryDatabase"))
            {
                identityServerBuilder.AddIntegrationTestConfiguration();
            }
            else
            {
                identityServerBuilder.AddIdentityServerStorage(configurationConnectionString, persistedGrantStoreConenctionString, authenticationConenctionString);
            }
        }

        #endregion
    }
}