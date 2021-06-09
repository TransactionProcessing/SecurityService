namespace SecurityService.UnitTests
{
    using System;
    using System.Collections.Generic;
    using Duende.IdentityServer.Services;
    using Duende.IdentityServer.Stores;
    using Lamar;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.DataProtection;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Moq;
    using Xunit;

    public class BootstrapperTests
    {
        [Fact]
        public void VerifyBootstrapperIsValid()
        {
            //ServiceRegistry serviceRegistry = new ServiceRegistry();
            //Mock<IWebHostEnvironment> hostingEnvironment = new Mock<IWebHostEnvironment>();

            //Startup.Configuration = this.SetupMemoryConfiguration();

            //serviceRegistry.AddRange(this.AddTestRegistrations());

            //IContainer container = Startup.GetConfiguredContainer(serviceRegistry, hostingEnvironment.Object);
            
            //container.AssertConfigurationIsValid();
        }
    
        private IConfigurationRoot SetupMemoryConfiguration()
        {
            Dictionary<String, String> configuration = new Dictionary<String, String>();
            
            IConfigurationBuilder builder = new ConfigurationBuilder();
            builder.AddInMemoryCollection(configuration);

            return builder.Build();
        }

        private ServiceRegistry AddTestRegistrations()
        {
            Mock<IIdentityServerInteractionService> service = new Mock<IIdentityServerInteractionService>();

            ServiceRegistry serviceRegistry = new ServiceRegistry();

            serviceRegistry.For<IIdentityServerInteractionService>().Use(service.Object);
            serviceRegistry.For<IClientStore>().Use<InMemoryClientStore>();
            serviceRegistry.For<IUserStore<IdentityUser>>().Use<UserStore<IdentityUser>>();
            serviceRegistry.For<IResourceStore>().Use<InMemoryResourcesStore>();
            serviceRegistry.For<ILoggerFactory>().Use<LoggerFactory>();
            serviceRegistry.For<IAuthenticationSchemeProvider>().Use<AuthenticationSchemeProvider>();
            serviceRegistry.For<IHttpContextAccessor>().Use<HttpContextAccessor>();
            serviceRegistry.For<DbContextOptions>().Use<DbContextOptions<DbContext>>();
            serviceRegistry.For<ILookupNormalizer>().Use<UpperInvariantLookupNormalizer>();
            serviceRegistry.For<ILogger<UserManager<IdentityUser>>>().Use<Logger<UserManager<IdentityUser>>>();
            serviceRegistry.For<IRoleStore<IdentityRole>>().Use<RoleStore<IdentityRole>>();
            serviceRegistry.For<ILogger<RoleManager<IdentityRole>>>().Use<Logger<RoleManager<IdentityRole>>>();
            serviceRegistry.For<IDataProtectionProvider>().Use<EphemeralDataProtectionProvider>();
            serviceRegistry.For<ILogger<SignInManager<IdentityUser>>>().Use<Logger<SignInManager<IdentityUser>>>();
            serviceRegistry.For<IUserConfirmation<IdentityUser>>().Use<DefaultUserConfirmation<IdentityUser>>();
            serviceRegistry.For<IUserClaimsPrincipalFactory<IdentityUser>>().Use<UserClaimsPrincipalFactory<IdentityUser>>();
            serviceRegistry.For<SignInManager<IdentityUser>>().Use<SignInManager<IdentityUser>>();
            serviceRegistry.For<IOptions<PasswordHasherOptions>>().Use<OptionsManager<PasswordHasherOptions>>();
            serviceRegistry.For<IOptionsFactory<PasswordHasherOptions>>().Use<OptionsFactory<PasswordHasherOptions>>();
            serviceRegistry.For<IOptions<AuthenticationOptions>>().Use<OptionsManager<AuthenticationOptions>>();
            serviceRegistry.For<IOptionsFactory<AuthenticationOptions>>().Use<OptionsFactory<AuthenticationOptions>>();
            serviceRegistry.For<IOptions<IdentityOptions>>().Use<OptionsManager<IdentityOptions>>();
            serviceRegistry.For<IOptionsFactory<IdentityOptions>>().Use<OptionsFactory<IdentityOptions>>();
            serviceRegistry.For<IOptions<CookieTempDataProviderOptions>>().Use<OptionsManager<CookieTempDataProviderOptions>>();
            serviceRegistry.For<IOptionsFactory<CookieTempDataProviderOptions>>().Use<OptionsFactory<CookieTempDataProviderOptions>>();
            //serviceRegistry.For<IOptions<EmailOptions>>().Use<OptionsManager<EmailOptions>>();
            //serviceRegistry.For<IOptionsFactory<EmailOptions>>().Use<OptionsFactory<EmailOptions>>();
            //serviceRegistry.For<IOptions<ServiceOptions>>().Use<OptionsManager<ServiceOptions>>();
            //serviceRegistry.For<IOptionsFactory<ServiceOptions>>().Use<OptionsFactory<ServiceOptions>>();

            serviceRegistry.For<IPasswordHasher<IdentityUser>>().Use<PasswordHasher<IdentityUser>>();

            return serviceRegistry;
        }
    }
}