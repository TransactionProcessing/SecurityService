using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SecurityService.UnitTests {
    using BusinessLogic;
    using BusinessLogic.RequestHandlers;
    using Duende.IdentityServer;
    using Duende.IdentityServer.Configuration;
    using Duende.IdentityServer.EntityFramework.DbContexts;
    using Duende.IdentityServer.EntityFramework.Options;
    using Duende.IdentityServer.Services;
    using Lamar;
    using MessagingService.Client;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Diagnostics;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging.Abstractions;
    using Microsoft.Extensions.Options;
    using Moq;
    using SecurityService.Database.DbContexts;

    //public class TestRoleValidator : IRoleValidator<IdentityRole>{
    //    public async Task<IdentityResult> ValidateAsync(RoleManager<IdentityRole> manager, IdentityRole role){
    //        if (role.Name == "CreateFailed"){
    //            List<IdentityError> errors = new List<IdentityError>();
    //            errors.Add(new IdentityError{
    //                                            Description = "Error",
    //                                            Code = "1"
    //                                        });
    //            return IdentityResult.Failed(errors.ToArray());
    //        }
    //        return IdentityResult.Success;
    //    }
    //}

    //public class CreateUserValidator : IUserValidator<IdentityUser>
    //{
    //    public async Task<IdentityResult> ValidateAsync(UserManager<IdentityUser> manager, IdentityUser user){
    //        if (user.UserName == "CreateFailed")
    //        {
    //            List<IdentityError> errors = new List<IdentityError>();
    //            errors.Add(new IdentityError
    //                       {
    //                           Description = "Error",
    //                           Code = "1"
    //                       });
    //            return IdentityResult.Failed(errors.ToArray());
    //        }
    //        return IdentityResult.Success;
    //    }
    //}

    //public class AddClaimsToUserUserValidator : IUserValidator<IdentityUser>
    //{
    //    public async Task<IdentityResult> ValidateAsync(UserManager<IdentityUser> manager, IdentityUser user)
    //    {
    //        if (user.UserName == "ClaimsAddFailed")
    //        {
    //            List<IdentityError> errors = new List<IdentityError>();
    //            errors.Add(new IdentityError
    //                       {
    //                           Description = "Error",
    //                           Code = "1"
    //                       });
    //            return IdentityResult.Failed(errors.ToArray());
    //        }
    //        return IdentityResult.Success;
    //    }
    //}

    public class SetupRequestHandlers{

        public Mock<IPasswordHasher<ApplicationUser>> PasswordHasher;
        public IUserStore<ApplicationUser> UserStore;
        public IRoleStore<IdentityRole> RoleStore;
        public List<IUserValidator<ApplicationUser>> UserValidators;
        public List<IRoleValidator<IdentityRole>> RoleValidators;
        public List<IPasswordValidator<ApplicationUser>> PasswordValidators;
        IdentityErrorDescriber ErrorDescriber = new IdentityErrorDescriber();
        public Mock<IServiceProvider> ServiceProvider;
        public Mock<IMessagingServiceClient> MessagingServiceClient;
        public ServiceOptions ServiceOptions;
        public UserManager<ApplicationUser> UserManager;
        public IdentityServerTools IdentityServerTools;
        public Mock<IOptions<IdentityOptions>> Options;

        public Mock<IRoleValidator<IdentityRole>> RoleValidator;
        public Mock<IUserValidator<ApplicationUser>> UserValidator;
        public Mock<IPasswordValidator<ApplicationUser>> PasswordValidator;


        ILookupNormalizer KeyNormalizer = new UpperInvariantLookupNormalizer();
        public SetupRequestHandlers(){
            IdentityOptions identityOptions = new IdentityOptions();
            identityOptions.Tokens.ProviderMap.Add("Default", new TokenProviderDescriptor(typeof(IUserTwoFactorTokenProvider<ApplicationUser>)));
            
            this.Options = new Mock<IOptions<IdentityOptions>>();
            this.Options.Setup(o => o.Value).Returns(identityOptions);

            this.PasswordHasher = new Mock<IPasswordHasher<ApplicationUser>>();
            this.RoleValidator = new Mock<IRoleValidator<IdentityRole>>();
            this.UserValidator = new Mock<IUserValidator<ApplicationUser>>();
            this.UserValidators = new List<IUserValidator<ApplicationUser>>();
            this.UserValidators.Add(this.UserValidator.Object);
            this.RoleValidators = new List<IRoleValidator<IdentityRole>>();
            this.RoleValidators.Add(this.RoleValidator.Object);
            this.PasswordValidator = new Mock<IPasswordValidator<ApplicationUser>>();
            this.PasswordValidators = new List<IPasswordValidator<ApplicationUser>>();
            this.PasswordValidators.Add(this.PasswordValidator.Object);

            this.ServiceProvider = new Mock<IServiceProvider>();
            this.ServiceOptions = new ServiceOptions();
            this.IdentityServerTools = this.SetupIdentityServerTools();
            this.MessagingServiceClient = new Mock<IMessagingServiceClient>();

            this.SetupServiceProvider();

        }

        private void SetupServiceProvider()
        {
            Mock<IUserTwoFactorTokenProvider<ApplicationUser>> tokenProvider =
                new Mock<IUserTwoFactorTokenProvider<ApplicationUser>>();
            tokenProvider.Setup(tp => tp.GenerateAsync(It.IsAny<String>(), It.IsAny<UserManager<ApplicationUser>>(),
                                                       It.IsAny<ApplicationUser>())).ReturnsAsync("token");

            tokenProvider.Setup(tp => tp.ValidateAsync(It.IsAny<String>(), It.IsAny<String>(),
                                                       It.IsAny<UserManager<ApplicationUser>>(),
                                                       It.IsAny<ApplicationUser>())).ReturnsAsync(true);
            this.ServiceProvider.Setup(sp => sp.GetService(It.IsAny<Type>())).Returns(tokenProvider.Object);
        }

        private IdentityServerTools SetupIdentityServerTools()
        {
            Mock<IServiceProvider> serviceProvider = new Mock<IServiceProvider>();
            Mock<IIssuerNameService> issuerNameService = new Mock<IIssuerNameService>();
            Mock<ITokenCreationService> tokenCreationService = new Mock<ITokenCreationService>();
            Mock<IClock> systemClock = new Mock<IClock>();
            systemClock.Setup(c => c.UtcNow).Returns(DateTimeOffset.UtcNow);
            
            serviceProvider.Setup(m => m.GetService(typeof(IdentityServerOptions))).Returns(new IdentityServerOptions());

            issuerNameService.Setup(i => i.GetCurrentAsync()).ReturnsAsync("Test Issuer");

            IdentityServerTools identityServerTools = new IdentityServerTools(issuerNameService.Object, tokenCreationService.Object,
                                                                              systemClock.Object, new IdentityServerOptions());

            return identityServerTools;
        }

        public static ConfigurationDbContext GetConfigurationDbContext(){
            String databaseName = Guid.NewGuid().ToString();
            DbContextOptionsBuilder<ConfigurationDbContext> builder = new DbContextOptionsBuilder<ConfigurationDbContext>().UseInMemoryDatabase(databaseName)
                                                                                                                           .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning));
            ConfigurationDbContext context = new ConfigurationDbContext(builder.Options);
            context.StoreOptions = new ConfigurationStoreOptions();

            return context;
        }

        public static AuthenticationDbContext GetAuthenticationDbContext()
        {
            String databaseName = Guid.NewGuid().ToString();
            DbContextOptionsBuilder<AuthenticationDbContext> builder = new DbContextOptionsBuilder<AuthenticationDbContext>().UseInMemoryDatabase(databaseName)
                                                                                                                             .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning));
            AuthenticationDbContext context = new AuthenticationDbContext(builder.Options);

            return context;
        }

        public UserRequestHandler SetUserRequestHandler(ConfigurationDbContext configurationDbContext, AuthenticationDbContext authenticationDbContext)
        {
            this.UserStore = new UserStore<ApplicationUser>(authenticationDbContext,this.ErrorDescriber);
            this.RoleStore = new RoleStore<IdentityRole>(authenticationDbContext, this.ErrorDescriber);
            this.UserManager =
                new UserManager<ApplicationUser>(this.UserStore, this.Options.Object, this.PasswordHasher.Object,
                                              this.UserValidators, this.PasswordValidators,
                                              this.KeyNormalizer, this.ErrorDescriber, this.ServiceProvider.Object, 
                                              new NullLogger<UserManager<ApplicationUser>>());

            this.PasswordHasher.Setup(p => p.HashPassword(It.IsAny<ApplicationUser>(), It.IsAny<String>())).Returns("passwordhash");
            this.PasswordHasher.Setup(p => p.VerifyHashedPassword(It.IsAny<ApplicationUser>(), It.IsAny<String>(), It.IsAny<String>())).Returns(PasswordVerificationResult.Success);

            this.ServiceOptions.ClientId = "clientId";

            return new UserRequestHandler(this.PasswordHasher.Object,
                                          this.UserManager,
                                          this.ServiceOptions,
                                          this.MessagingServiceClient.Object,
                                          this.IdentityServerTools,
                                          configurationDbContext);
        }

        public ApiResourceRequestHandler SetupApiResourceRequestHandler(ConfigurationDbContext configurationDbContext)
        {
            return new ApiResourceRequestHandler(configurationDbContext);
        }

        public ApiScopeRequestHandler SetupApiScopeRequestHandler(ConfigurationDbContext configurationDbContext)
        {
            return new ApiScopeRequestHandler(configurationDbContext);
        }

        public ClientRequestHandler SetupClientRequestHandler(ConfigurationDbContext configurationDbContext)
        {
            return new ClientRequestHandler(configurationDbContext);
        }

        public IdentityResourceRequestHandler SetupIdentityResourceRequestHandler(ConfigurationDbContext configurationDbContext)
        {
            return new IdentityResourceRequestHandler(configurationDbContext);
        }

        public RoleRequestHandler SetupRoleRequestHandler(ConfigurationDbContext configurationDbContext, AuthenticationDbContext authenticationDbContext){
            this.RoleStore = new RoleStore<IdentityRole>(authenticationDbContext, this.ErrorDescriber);
            RoleManager<IdentityRole> roleManager = new RoleManager<IdentityRole>(this.RoleStore,
                                                                                  this.RoleValidators,
                                                                                  this.KeyNormalizer,
                                                                                  this.ErrorDescriber,
                                                                                  new NullLogger<RoleManager<IdentityRole>>());
            return new RoleRequestHandler(roleManager);
        }
    }
}
