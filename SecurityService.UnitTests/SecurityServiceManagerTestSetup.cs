namespace SecurityService.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using IdentityServer4.EntityFramework.Interfaces;
    using Manager;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.Logging.Abstractions;
    using Microsoft.Extensions.Options;
    using MockQueryable.Moq;
    using Moq;

    public partial class SecurityServiceManagerTests
    {
        private Mock<IPasswordHasher<IdentityUser>> PasswordHasher = new Mock<IPasswordHasher<IdentityUser>>();
        private Mock<IUserStore<IdentityUser>> UserStore = new Mock<IUserStore<IdentityUser>>();

        private List<IPasswordValidator<IdentityUser>>
            PasswordValidators = new List<IPasswordValidator<IdentityUser>>();

        private List<IUserValidator<IdentityUser>> UserValidators = new List<IUserValidator<IdentityUser>>();
        private Mock<IRoleStore<IdentityRole>> RoleStore = new Mock<IRoleStore<IdentityRole>>();
        private Mock<IdentityErrorDescriber> ErrorDescriber = new Mock<IdentityErrorDescriber>();
        private Mock<IServiceProvider> ServiceProvider = new Mock<IServiceProvider>();
        private Mock<IHttpContextAccessor> ContextAccessor = new Mock<IHttpContextAccessor>();
        private Mock<IUserClaimsPrincipalFactory<IdentityUser>> ClaimsFactory = new Mock<IUserClaimsPrincipalFactory<IdentityUser>>();

        public enum TestScenario
        {

            CreateUserSuccess,

            GetUserSuccess,

            GetUser_UserNotFound,

            GetUser_InvalidId,
            
            GetUsers,//_ByUserName_Success,

            GetUsers_ByUserName_NotFound,
            
            CreateUser_InvalidData
        }

        private void SetupErrorDescriber()
        {
            this.ErrorDescriber.Setup(e => e.PasswordMismatch()).Returns(new IdentityError
            {
                Code = "1",
                Description = "Password Mismatch"
            });
            this.ErrorDescriber.Setup(e => e.InvalidToken()).Returns(new IdentityError
            {
                Code = "2",
                Description = "Invalid Token"
            });
            this.ErrorDescriber.Setup(e => e.LoginAlreadyAssociated()).Returns(new IdentityError
            {
                Code = "3",
                Description = "Login Already Associated"
            });
            this.ErrorDescriber.Setup(e => e.UserAlreadyInRole(It.IsAny<String>())).Returns(new IdentityError
            {
                Code = "4",
                Description = "User Already In Role"
            });
            this.ErrorDescriber.Setup(e => e.UserNotInRole(It.IsAny<String>())).Returns(new IdentityError
            {
                Code = "5",
                Description = "User Not In Role"
            });
            this.ErrorDescriber.Setup(e => e.UserLockoutNotEnabled()).Returns(new IdentityError
            {
                Code = "6",
                Description = "User Lockout Not Enabled"
            });
            this.ErrorDescriber.Setup(e => e.RecoveryCodeRedemptionFailed()).Returns(new IdentityError
            {
                Code = "7",
                Description = "Recovery Code Redemption Failed"
            });
        }

        private void SetupServiceProvider()
        {
            Mock<IUserTwoFactorTokenProvider<IdentityUser>> tokenProvider =
                new Mock<IUserTwoFactorTokenProvider<IdentityUser>>();
            tokenProvider.Setup(tp => tp.GenerateAsync(It.IsAny<String>(), It.IsAny<UserManager<IdentityUser>>(),
                                                       It.IsAny<IdentityUser>())).ReturnsAsync("token");

            tokenProvider.Setup(tp => tp.ValidateAsync(It.IsAny<String>(), It.IsAny<String>(),
                                                           It.IsAny<UserManager<IdentityUser>>(),
                                                           It.IsAny<IdentityUser>())).ReturnsAsync(true);
            this.ServiceProvider.Setup(sp => sp.GetService(It.IsAny<Type>())).Returns(tokenProvider.Object);
        }

        private SecurityServiceManager SetupSecurityServiceManager()
        {
            this.SetupServiceProvider();
            this.SetupErrorDescriber();

            IdentityOptions identityOptions = new IdentityOptions();
            identityOptions.Tokens.ProviderMap.Add("Default",
                new TokenProviderDescriptor(typeof(IUserTwoFactorTokenProvider<IdentityUser>)));
            Mock<IOptions<IdentityOptions>> options = new Mock<IOptions<IdentityOptions>>();
            options.Setup(o => o.Value).Returns(identityOptions);

            UserManager<IdentityUser> userManager =
                new UserManager<IdentityUser>(this.UserStore.Object, options.Object, this.PasswordHasher.Object,
                    this.UserValidators, this.PasswordValidators,
                    null, this.ErrorDescriber.Object, this.ServiceProvider.Object, new NullLogger<UserManager<IdentityUser>>());

            RoleManager<IdentityRole> roleManager =
                new RoleManager<IdentityRole>(this.RoleStore.Object, null, null, null, null);

            Mock<Func<IConfigurationDbContext>> configurationDbContextResolver =
                new Mock<Func<IConfigurationDbContext>>();

            SignInManager<IdentityUser> signInManager = new SignInManager<IdentityUser>(userManager, this.ContextAccessor.Object, this.ClaimsFactory.Object, null, null, null, null);
            //Mock<IOptions<ServiceOptions>> serviceOptions = new Mock<IOptions<ServiceOptions>>();
            //serviceOptions.Setup(so => so.Value).Returns(new ServiceOptions
            //{
            //    PublicOrigin = "http://localhost"
            //});
            //Mock<IMessagingService> messagingService = new Mock<IMessagingService>();
            
            SecurityServiceManager securityServiceManager =
                new SecurityServiceManager(this.PasswordHasher.Object, userManager, roleManager, signInManager);

            return securityServiceManager;
        }


    }
}

