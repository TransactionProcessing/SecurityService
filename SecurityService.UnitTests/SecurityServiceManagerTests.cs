namespace SecurityService.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using DataTransferObjects;
    using Manager;
    using Manager.Exceptions;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Moq;
    using Shared.Exceptions;
    using Shouldly;
    using Xunit;

    public partial class SecurityServiceManagerTests
    {
        #region Methods

        [Fact]
        public void SecurityServiceManager_CanBeCreated_IsCreated()
        {
            Mock<IPasswordHasher<IdentityUser>> passwordHasher = new Mock<IPasswordHasher<IdentityUser>>();
            Mock<IUserStore<IdentityUser>> userStore = new Mock<IUserStore<IdentityUser>>();
            UserManager<IdentityUser> userManager = new UserManager<IdentityUser>(userStore.Object, null, null, null, null, null, null, null, null);
            Mock<IRoleStore<IdentityRole>> roleStore = new Mock<IRoleStore<IdentityRole>>();
            RoleManager<IdentityRole> roleManager = new RoleManager<IdentityRole>(roleStore.Object, null, null, null, null);
            Mock<IHttpContextAccessor> contextAccessor = new Mock<IHttpContextAccessor>();
            Mock<IUserClaimsPrincipalFactory<IdentityUser>> claimsFactory = new Mock<IUserClaimsPrincipalFactory<IdentityUser>>();
            SignInManager<IdentityUser> signInManager = new SignInManager<IdentityUser>(userManager, contextAccessor.Object, claimsFactory.Object, null, null, null,null);

            SecurityServiceManager securityServiceManager = new SecurityServiceManager(passwordHasher.Object, userManager, roleManager, signInManager);

            securityServiceManager.ShouldNotBeNull();
        }
        
        [Theory]
        [InlineData(true, "givenname", "familyname", "emailaddress", false, false, typeof(ArgumentNullException))]
        [InlineData(true, null, "familyname", "emailaddress", false, false, typeof(ArgumentNullException))]
        [InlineData(true, "", "familyname", "emailaddress", false, false, typeof(ArgumentNullException))]
        [InlineData(true, "givenname", null, "emailaddress", false, false, typeof(ArgumentNullException))]
        [InlineData(true, "givenname", "", "emailaddress", false, false, typeof(ArgumentNullException))]
        [InlineData(false, "givenname", "familyname", null, false, false, typeof(ArgumentNullException))]
        [InlineData(false, "givenname", "familyname", "", false, false, typeof(ArgumentNullException))]
        //[InlineData(false, "givenname", "familyname", "", true, false, typeof(ArgumentNullException))]
        //[InlineData(false, "givenname", "familyname", "", false, true, typeof(ArgumentNullException))]
        public void SecurityServiceManager_CreateUser_InvalidRequest_ErrorThrown(Boolean nullRequest,
                                                                                   String givenName,
                                                                                   String familyName,
                                                                                   String emailAddress,
                                                                                   Boolean nullClaims,
                                                                                   Boolean nullRoles,
                                                                                   Type exceptionType)
        {
            SecurityServiceManager securityServiceManager = this.SetupSecurityServiceManager();

            CreateUserRequest request = null;
            if (!nullRequest)
            {
                request = new CreateUserRequest()
                          {
                              Claims = nullClaims ? null : SecurityServiceManagerTestData.Claims,
                              GivenName = givenName,
                              FamilyName = familyName,
                              EmailAddress = emailAddress,
                              Password = SecurityServiceManagerTestData.Password,
                              PhoneNumber = SecurityServiceManagerTestData.PhoneNumber,
                              Roles = nullRoles ? null : SecurityServiceManagerTestData.Roles
                          };
            }

            Should.Throw(async () => { await securityServiceManager.CreateUser(request, CancellationToken.None); }, exceptionType);
        }

        [Theory]
        [InlineData("123456")]
        //[InlineData(null)]
        //[InlineData("")]
        public async Task SecurityServiceManager_CreateUser_UserIsCreated(String password)
        {
            this.UserStore.Setup(u => u.CreateAsync(It.IsAny<IdentityUser>(), It.IsAny<CancellationToken>())).ReturnsAsync(IdentityResult.Success);
            this.PasswordHasher.Setup(ph => ph.HashPassword(It.IsAny<IdentityUser>(), It.IsAny<String>()))
                .Returns("HashedPassword");

            SecurityServiceManager securityServiceManager = this.SetupSecurityServiceManager();
            
            CreateUserRequest request = SecurityServiceManagerTestData.GetCreateUserRequest;
            request.Password = password;

            Guid userId = await securityServiceManager.CreateUser(request, CancellationToken.None);

            userId.ShouldNotBe(Guid.Empty);
        }
        
        [Fact]
        public async Task SecurityServiceManager_GetUser_UserDataReturned()
        {
            this.UserStore.Setup(u => u.FindByIdAsync(It.IsAny<String>(), CancellationToken.None)).ReturnsAsync(SecurityServiceManagerTestData.IdentityUser);
            this.UserStore.As<IUserRoleStore<IdentityUser>>()
                .Setup(us => us.GetRolesAsync(It.IsAny<IdentityUser>(), CancellationToken.None)).ReturnsAsync(new List<String>());
            this.UserStore.As<IUserClaimStore<IdentityUser>>().Setup(us => us.GetClaimsAsync(It.IsAny<IdentityUser>(), CancellationToken.None))
                .ReturnsAsync(new List<Claim>());

            SecurityServiceManager securityServiceManager = this.SetupSecurityServiceManager();
            
            UserDetails userDetails = await securityServiceManager.GetUser(Guid.Parse(SecurityServiceManagerTestData.User1Id), CancellationToken.None);

            userDetails.UserId.ShouldBe(Guid.Parse(SecurityServiceManagerTestData.User1Id));
            userDetails.UserName.ShouldBe(SecurityServiceManagerTestData.UserName);
            userDetails.Claims.ShouldBeEmpty();
            userDetails.Email.ShouldBe(SecurityServiceManagerTestData.EmailAddress);
            userDetails.PhoneNumber.ShouldBe(SecurityServiceManagerTestData.PhoneNumber);
            userDetails.Roles.ShouldBeEmpty();
        }

        [Fact]
        public async Task SecurityServiceManager_GetUser_UserNotFound_ErrorThrown()
        {
            SecurityServiceManager securityServiceManager = this.SetupSecurityServiceManager();

            Should.Throw<NotFoundException>(async () =>
                                            {
                                                await securityServiceManager.GetUser(Guid.Parse(SecurityServiceManagerTestData.User1Id), CancellationToken.None);
                                            });
        }

        [Fact]
        public async Task SecurityServiceManager_GetUser_InvalidData_ErrorThrown()
        {
            SecurityServiceManager securityServiceManager = this.SetupSecurityServiceManager();

            Should.Throw<ArgumentNullException>(async () =>
                                            {
                                                await securityServiceManager.GetUser(Guid.Empty, CancellationToken.None);
                                            });
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public async Task SecurityServiceManager_GetUsers_NoFilters_UserDataReturned(String userName)
        {
            IQueryable<IdentityUser> identityUserList = await SecurityServiceManagerTestData.IdentityUsers();

            this.UserStore.As<IQueryableUserStore<IdentityUser>>().Setup(u => u.Users).Returns(identityUserList);
            this.UserStore.As<IUserRoleStore<IdentityUser>>()
                .Setup(us => us.GetRolesAsync(It.IsAny<IdentityUser>(), CancellationToken.None)).ReturnsAsync(new List<String>());
            this.UserStore.As<IUserClaimStore<IdentityUser>>().Setup(us => us.GetClaimsAsync(It.IsAny<IdentityUser>(), CancellationToken.None))
                .ReturnsAsync(new List<Claim>());

            SecurityServiceManager securityServiceManager = this.SetupSecurityServiceManager();

            List<UserDetails> userDetails = await securityServiceManager.GetUsers(userName, CancellationToken.None);

            userDetails.ShouldNotBeEmpty();
            userDetails.ShouldNotBeNull();
            userDetails.Count.ShouldBe(identityUserList.Count());
        }

        [Fact]
        public async Task SecurityServiceManager_GetUsers_ByUserName_UserDataReturned()
        {
            IQueryable<IdentityUser> identityUserList = await SecurityServiceManagerTestData.IdentityUsers();

            this.UserStore.As<IQueryableUserStore<IdentityUser>>().Setup(u => u.Users).Returns(identityUserList);
            this.UserStore.As<IUserRoleStore<IdentityUser>>()
                .Setup(us => us.GetRolesAsync(It.IsAny<IdentityUser>(), CancellationToken.None)).ReturnsAsync(new List<String>());
            this.UserStore.As<IUserClaimStore<IdentityUser>>().Setup(us => us.GetClaimsAsync(It.IsAny<IdentityUser>(), CancellationToken.None))
                .ReturnsAsync(new List<Claim>());

            SecurityServiceManager securityServiceManager = this.SetupSecurityServiceManager();

            List<UserDetails> userDetails = await securityServiceManager.GetUsers(SecurityServiceManagerTestData.UserName1, CancellationToken.None);

            userDetails.ShouldNotBeEmpty();
            userDetails.ShouldNotBeNull();
            userDetails.Count.ShouldBe(1);
            userDetails.First().UserId.ToString().ShouldBe(SecurityServiceManagerTestData.User1Id);
            userDetails.First().UserName.ShouldBe(SecurityServiceManagerTestData.UserName1);
            userDetails.First().Email.ShouldBe(SecurityServiceManagerTestData.EmailAddress1);
            userDetails.First().PhoneNumber.ShouldBe(SecurityServiceManagerTestData.PhoneNumber);
        }

        [Fact]
        public async Task SecurityServiceManager_GetUsers_ByUserName_UserNotFound_ErrorThrown()
        {
            IQueryable<IdentityUser> identityUserList = await SecurityServiceManagerTestData.IdentityUsers();

            this.UserStore.As<IQueryableUserStore<IdentityUser>>().Setup(u => u.Users).Returns(identityUserList);
            this.UserStore.As<IUserRoleStore<IdentityUser>>()
                .Setup(us => us.GetRolesAsync(It.IsAny<IdentityUser>(), CancellationToken.None)).ReturnsAsync(new List<String>());
            this.UserStore.As<IUserClaimStore<IdentityUser>>().Setup(us => us.GetClaimsAsync(It.IsAny<IdentityUser>(), CancellationToken.None))
                .ReturnsAsync(new List<Claim>());

            SecurityServiceManager securityServiceManager = this.SetupSecurityServiceManager();

            List<UserDetails> userDetails = await securityServiceManager.GetUsers(SecurityServiceManagerTestData.UserName, CancellationToken.None);

            userDetails.ShouldNotBeNull();
            userDetails.ShouldBeEmpty();
        }

        [Fact]
        public async Task SecurityServiceManager_GetUsers_ByUserName_PartialMatch_ErrorThrown()
        {
            IQueryable<IdentityUser> identityUserList = await SecurityServiceManagerTestData.IdentityUsers();

            this.UserStore.As<IQueryableUserStore<IdentityUser>>().Setup(u => u.Users).Returns(identityUserList);
            this.UserStore.As<IUserRoleStore<IdentityUser>>().Setup(us => us.GetRolesAsync(It.IsAny<IdentityUser>(), CancellationToken.None))
                .ReturnsAsync(new List<String>());
            this.UserStore.As<IUserClaimStore<IdentityUser>>().Setup(us => us.GetClaimsAsync(It.IsAny<IdentityUser>(), CancellationToken.None))
                .ReturnsAsync(new List<Claim>());

            SecurityServiceManager securityServiceManager = this.SetupSecurityServiceManager();

            List<UserDetails> userDetails = await securityServiceManager.GetUsers("testa", CancellationToken.None);

            userDetails.ShouldNotBeNull();
            userDetails.ShouldNotBeEmpty();
            userDetails.Count.ShouldBe(2);
        }

        #endregion
    }
}