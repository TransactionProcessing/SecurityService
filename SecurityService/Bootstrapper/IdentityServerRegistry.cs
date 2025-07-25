﻿namespace SecurityService.Bootstrapper
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using BusinessLogic;
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
            String persistedGrantStoreConnectionString = Startup.Configuration.GetConnectionString("PersistedGrantDbContext");
            String configurationConnectionString = Startup.Configuration.GetConnectionString("ConfigurationDbContext");
            String authenticationConnectionString = Startup.Configuration.GetConnectionString("AuthenticationDbContext");

            ServiceOptions serviceOptions = new ServiceOptions();
            IConfigurationSection serviceOptionsSection = Startup.Configuration.GetSection("ServiceOptions");
            serviceOptionsSection.Bind(serviceOptions);

            this.AddSingleton<ServiceOptions>(serviceOptions);
            this.AddIdentity<ApplicationUser, IdentityRole>(opt => {
                                                             opt.Tokens.EmailConfirmationTokenProvider = "emailconfirmation";
                                                             opt.Password.RequireDigit = serviceOptions.PasswordOptions.RequireDigit;
                                                             opt.Password.RequireUppercase = serviceOptions.PasswordOptions.RequireUpperCase;
                                                             opt.Password.RequiredLength = serviceOptions.PasswordOptions.RequiredLength;
                                                             opt.SignIn.RequireConfirmedEmail = serviceOptions.SignInOptions.RequireConfirmedEmail;
                                                             opt.User.RequireUniqueEmail = serviceOptions.UserOptions.RequireUniqueEmail;
                                                         }).AddEntityFrameworkStores<AuthenticationDbContext>().AddDefaultTokenProviders()
                .AddTokenProvider<EmailConfirmationTokenProvider<ApplicationUser>>("emailconfirmation");

            this.Configure<DataProtectionTokenProviderOptions>(opt => opt.TokenLifespan =
                                                                   TimeSpan.FromHours(serviceOptions.TokenOptions.PasswordResetTokenExpiryInHours));
            this.Configure<EmailConfirmationTokenProviderOptions>(opt =>
                                                                          opt.TokenLifespan = TimeSpan.FromHours(serviceOptions.TokenOptions.EmailConfirmationTokenExpiryInHours));


            IIdentityServerBuilder identityServerBuilder = this.AddIdentityServer(options =>
                                                                                  {
                                                                                      // https://docs.duendesoftware.com/identityserver/v5/fundamentals/resources/
                                                                                      options.EmitStaticAudienceClaim = true;

                                                                                      options.Events.RaiseSuccessEvents = true;
                                                                                      options.Events.RaiseFailureEvents = true;
                                                                                      options.Events.RaiseErrorEvents = true;

                                                                                      options.IssuerUri = serviceOptions.IssuerUrl;
                                                                                  });

            identityServerBuilder.AddAspNetIdentity<ApplicationUser>();

            if (serviceOptions.UseInMemoryDatabase)
            {
                identityServerBuilder.AddIntegrationTestConfiguration();
            }
            else
            {
                identityServerBuilder.AddIdentityServerStorage(configurationConnectionString, persistedGrantStoreConnectionString, authenticationConnectionString);
            }
        }

        #endregion
    }
}