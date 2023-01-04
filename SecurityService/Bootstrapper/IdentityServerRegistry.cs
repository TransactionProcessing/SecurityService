namespace SecurityService.Bootstrapper
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
            this.AddIdentity<IdentityUser, IdentityRole>(opt => {
                                                             opt.Tokens.EmailConfirmationTokenProvider = "emailconfirmation";
                                                             opt.Password.RequireDigit = serviceOptions.PasswordOptions.RequireDigit;
                                                             opt.Password.RequireUppercase = serviceOptions.PasswordOptions.RequireUpperCase;
                                                             opt.Password.RequiredLength = serviceOptions.PasswordOptions.RequiredLength;
                                                             opt.SignIn.RequireConfirmedEmail = serviceOptions.SignInOptions.RequireConfirmedEmail;
                                                             opt.User.RequireUniqueEmail = serviceOptions.UserOptions.RequireUniqueEmail;
                                                         }).AddEntityFrameworkStores<AuthenticationDbContext>().AddDefaultTokenProviders()
                .AddTokenProvider<EmailConfirmationTokenProvider<IdentityUser>>("emailconfirmation");

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

            identityServerBuilder.AddAspNetIdentity<IdentityUser>();

            if (Startup.WebHostEnvironment.IsEnvironment("IntegrationTest") || Startup.Configuration.GetValue<Boolean>("ServiceOptions:UseInMemoryDatabase"))
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

    public class TokenOptions
    {
        public Int32 EmailConfirmationTokenExpiryInHours { get; set; }
        public Int32 PasswordResetTokenExpiryInHours { get; set; }
    }

    public class PasswordOptions
    {
        public Int32 RequiredLength { get; set; }

        public Boolean RequireDigit { get; set; }

        public Boolean RequireUpperCase { get; set; }
    }

    public class UserOptions
    {
        public Boolean RequireUniqueEmail { get; set; }
    }

    public class SignInOptions
    {
        public Boolean RequireConfirmedEmail { get; set; }
    }
}