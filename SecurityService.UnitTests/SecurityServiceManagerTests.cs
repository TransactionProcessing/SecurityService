namespace SecurityService.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using DataTransferObjects;
    using IdentityModel;
    using IdentityServer4.EntityFramework.Entities;
    using IdentityServer4.EntityFramework.Interfaces;
    using Manager;
    using Manager.Exceptions;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Models;
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
            SignInManager<IdentityUser> signInManager =
                new SignInManager<IdentityUser>(userManager, contextAccessor.Object, claimsFactory.Object, null, null, null, null);
            Mock<Func<IConfigurationDbContext>> configurationDbContextResolver = new Mock<Func<IConfigurationDbContext>>();
            SecurityServiceManager securityServiceManager =
                new SecurityServiceManager(passwordHasher.Object, userManager, roleManager, signInManager, configurationDbContextResolver.Object);

            securityServiceManager.ShouldNotBeNull();
        }

        [Theory]
        [InlineData("123456", true, false, false, false)]
        [InlineData("123456", false, true, false, false)]
        [InlineData("123456", false, false, true, false)]
        [InlineData("123456", false, false, false, true)]
        //[InlineData(null)]
        //[InlineData("")]
        public async Task SecurityServiceManager_CreateUser_UserIsCreated(String password,
                                                                          Boolean nullRoles,
                                                                          Boolean emptyRoles,
                                                                          Boolean nullClaims,
                                                                          Boolean emptyClaims)
        {
            this.UserStore.Setup(u => u.CreateAsync(It.IsAny<IdentityUser>(), It.IsAny<CancellationToken>())).ReturnsAsync(IdentityResult.Success);
            this.UserStore.Setup(u => u.FindByIdAsync(It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(new IdentityUser());
            this.UserStore.As<IUserRoleStore<IdentityUser>>().Setup(us => us.IsInRoleAsync(It.IsAny<IdentityUser>(), It.IsAny<String>(), CancellationToken.None))
                .ReturnsAsync(false);
            this.UserStore.As<IUserRoleStore<IdentityUser>>().Setup(us => us.AddToRoleAsync(It.IsAny<IdentityUser>(), It.IsAny<String>(), CancellationToken.None))
                .Returns(Task.CompletedTask);
            this.UserStore.As<IUserRoleStore<IdentityUser>>().Setup(us => us.GetRolesAsync(It.IsAny<IdentityUser>(), CancellationToken.None))
                .ReturnsAsync(new List<String>());
            this.UserStore.As<IUserClaimStore<IdentityUser>>()
                .Setup(us => us.AddClaimsAsync(It.IsAny<IdentityUser>(), It.IsAny<IEnumerable<Claim>>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            this.PasswordHasher.Setup(ph => ph.HashPassword(It.IsAny<IdentityUser>(), It.IsAny<String>())).Returns("HashedPassword");
            this.UserStore.Setup(u => u.UpdateAsync(It.IsAny<IdentityUser>(), It.IsAny<CancellationToken>())).ReturnsAsync(IdentityResult.Success);
            SecurityServiceManager securityServiceManager = this.SetupSecurityServiceManager();

            Dictionary<String, String> claims = SecurityServiceManagerTestData.NullClaims;

            if (nullClaims == false)
            {
                if (emptyClaims == false)
                {
                    claims = SecurityServiceManagerTestData.Claims;
                }
                else
                {
                    claims = SecurityServiceManagerTestData.EmptyClaims;
                }
            }

            List<String> roles = SecurityServiceManagerTestData.NullRoles;

            if (nullRoles == false)
            {
                if (emptyRoles == false)
                {
                    roles = SecurityServiceManagerTestData.Roles;
                }
                else
                {
                    roles = SecurityServiceManagerTestData.EmptyRoles;
                }
            }

            Guid userId = await securityServiceManager.CreateUser(SecurityServiceManagerTestData.GivenName,
                                                                  SecurityServiceManagerTestData.MiddleName,
                                                                  SecurityServiceManagerTestData.FamilyName,
                                                                  SecurityServiceManagerTestData.UserName,
                                                                  password,
                                                                  SecurityServiceManagerTestData.EmailAddress,
                                                                  SecurityServiceManagerTestData.PhoneNumber,
                                                                  claims,
                                                                  roles,
                                                                  CancellationToken.None);

            userId.ShouldNotBe(Guid.Empty);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task SecurityServiceManager_CreateUser_NullPasswordHashGenerated_ErrorThrown(String passwordHash)
        {
            this.UserStore.Setup(u => u.CreateAsync(It.IsAny<IdentityUser>(), It.IsAny<CancellationToken>())).ReturnsAsync(IdentityResult.Success);
            this.PasswordHasher.Setup(ph => ph.HashPassword(It.IsAny<IdentityUser>(), It.IsAny<String>())).Returns(passwordHash);

            SecurityServiceManager securityServiceManager = this.SetupSecurityServiceManager();

            await Should.ThrowAsync<NullReferenceException>(async () =>
                                                            {
                                                                await securityServiceManager.CreateUser(SecurityServiceManagerTestData.GivenName,
                                                                                                        SecurityServiceManagerTestData.MiddleName,
                                                                                                        SecurityServiceManagerTestData.FamilyName,
                                                                                                        SecurityServiceManagerTestData.UserName,
                                                                                                        SecurityServiceManagerTestData.Password,
                                                                                                        SecurityServiceManagerTestData.EmailAddress,
                                                                                                        SecurityServiceManagerTestData.PhoneNumber,
                                                                                                        SecurityServiceManagerTestData.Claims,
                                                                                                        SecurityServiceManagerTestData.Roles,
                                                                                                        CancellationToken.None);
                                                            });

        }

        [Fact]
        public async Task SecurityServiceManager_CreateUser_UserManagerCreateFailed_ErrorThrown()
        {
            this.UserStore.Setup(u => u.CreateAsync(It.IsAny<IdentityUser>(), It.IsAny<CancellationToken>())).ReturnsAsync(IdentityResult.Failed());
            this.PasswordHasher.Setup(ph => ph.HashPassword(It.IsAny<IdentityUser>(), It.IsAny<String>())).Returns("HashedPassword");

            SecurityServiceManager securityServiceManager = this.SetupSecurityServiceManager();

            IdentityResultException exception = await Should.ThrowAsync<IdentityResultException>(async () =>
                                                                                                 {
                                                                                                     await
                                                                                                         securityServiceManager.CreateUser(SecurityServiceManagerTestData
                                                                                                                                               .GivenName,
                                                                                                                                           SecurityServiceManagerTestData
                                                                                                                                               .MiddleName,
                                                                                                                                           SecurityServiceManagerTestData
                                                                                                                                               .FamilyName,
                                                                                                                                           SecurityServiceManagerTestData
                                                                                                                                               .UserName,
                                                                                                                                           SecurityServiceManagerTestData
                                                                                                                                               .Password,
                                                                                                                                           SecurityServiceManagerTestData
                                                                                                                                               .EmailAddress,
                                                                                                                                           SecurityServiceManagerTestData
                                                                                                                                               .PhoneNumber,
                                                                                                                                           SecurityServiceManagerTestData
                                                                                                                                               .Claims,
                                                                                                                                           SecurityServiceManagerTestData
                                                                                                                                               .Roles,
                                                                                                                                           CancellationToken.None);
                                                                                                 });

            exception.Message.ShouldContain("Error creating user");
        }

        [Fact]
        public async Task SecurityServiceManager_CreateUser_AddToRolesFailed_ErrorThrown()
        {
            this.UserStore.Setup(u => u.CreateAsync(It.IsAny<IdentityUser>(), It.IsAny<CancellationToken>())).ReturnsAsync(IdentityResult.Success);
            this.UserStore.Setup(u => u.FindByIdAsync(It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(new IdentityUser());
            this.UserStore.As<IUserRoleStore<IdentityUser>>().Setup(us => us.IsInRoleAsync(It.IsAny<IdentityUser>(), It.IsAny<String>(), CancellationToken.None))
                .ReturnsAsync(false);
            this.UserStore.As<IUserRoleStore<IdentityUser>>().Setup(us => us.AddToRoleAsync(It.IsAny<IdentityUser>(), It.IsAny<String>(), CancellationToken.None))
                .Returns(Task.CompletedTask);
            this.UserStore.Setup(u => u.UpdateAsync(It.IsAny<IdentityUser>(), It.IsAny<CancellationToken>())).ReturnsAsync(IdentityResult.Failed());
            this.UserStore.As<IUserRoleStore<IdentityUser>>().Setup(us => us.GetRolesAsync(It.IsAny<IdentityUser>(), CancellationToken.None))
                .ReturnsAsync(new List<String>());
            this.PasswordHasher.Setup(ph => ph.HashPassword(It.IsAny<IdentityUser>(), It.IsAny<String>())).Returns("HashedPassword");
            this.UserStore.Setup(u => u.DeleteAsync(It.IsAny<IdentityUser>(), It.IsAny<CancellationToken>())).ReturnsAsync(IdentityResult.Success);

            SecurityServiceManager securityServiceManager = this.SetupSecurityServiceManager();

            IdentityResultException exception = await Should.ThrowAsync<IdentityResultException>(async () =>
                                                                                                 {
                                                                                                     await
                                                                                                         securityServiceManager.CreateUser(SecurityServiceManagerTestData
                                                                                                                                               .GivenName,
                                                                                                                                           SecurityServiceManagerTestData
                                                                                                                                               .MiddleName,
                                                                                                                                           SecurityServiceManagerTestData
                                                                                                                                               .FamilyName,
                                                                                                                                           SecurityServiceManagerTestData
                                                                                                                                               .UserName,
                                                                                                                                           SecurityServiceManagerTestData
                                                                                                                                               .Password,
                                                                                                                                           SecurityServiceManagerTestData
                                                                                                                                               .EmailAddress,
                                                                                                                                           SecurityServiceManagerTestData
                                                                                                                                               .PhoneNumber,
                                                                                                                                           SecurityServiceManagerTestData
                                                                                                                                               .Claims,
                                                                                                                                           SecurityServiceManagerTestData
                                                                                                                                               .Roles,
                                                                                                                                           CancellationToken.None);
                                                                                                 });

            exception.Message.ShouldContain("Error adding roles");
        }

        [Fact]
        public async Task SecurityServiceManager_CreateUser_CleanUpFailed_ErrorThrown()
        {
            this.UserStore.Setup(u => u.CreateAsync(It.IsAny<IdentityUser>(), It.IsAny<CancellationToken>())).ReturnsAsync(IdentityResult.Success);
            this.UserStore.Setup(u => u.FindByIdAsync(It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(new IdentityUser());
            this.UserStore.As<IUserRoleStore<IdentityUser>>().Setup(us => us.IsInRoleAsync(It.IsAny<IdentityUser>(), It.IsAny<String>(), CancellationToken.None))
                .ReturnsAsync(false);
            this.UserStore.As<IUserRoleStore<IdentityUser>>().Setup(us => us.AddToRoleAsync(It.IsAny<IdentityUser>(), It.IsAny<String>(), CancellationToken.None))
                .Returns(Task.CompletedTask);
            this.UserStore.Setup(u => u.UpdateAsync(It.IsAny<IdentityUser>(), It.IsAny<CancellationToken>())).ReturnsAsync(IdentityResult.Failed());
            this.UserStore.As<IUserRoleStore<IdentityUser>>().Setup(us => us.GetRolesAsync(It.IsAny<IdentityUser>(), CancellationToken.None))
                .ReturnsAsync(new List<String>());
            this.PasswordHasher.Setup(ph => ph.HashPassword(It.IsAny<IdentityUser>(), It.IsAny<String>())).Returns("HashedPassword");
            this.UserStore.Setup(u => u.DeleteAsync(It.IsAny<IdentityUser>(), It.IsAny<CancellationToken>())).ReturnsAsync(IdentityResult.Failed());

            SecurityServiceManager securityServiceManager = this.SetupSecurityServiceManager();

            IdentityResultException exception = await Should.ThrowAsync<IdentityResultException>(async () =>
                                                                                                 {
                                                                                                     await
                                                                                                         securityServiceManager.CreateUser(SecurityServiceManagerTestData
                                                                                                                                               .GivenName,
                                                                                                                                           SecurityServiceManagerTestData
                                                                                                                                               .MiddleName,
                                                                                                                                           SecurityServiceManagerTestData
                                                                                                                                               .FamilyName,
                                                                                                                                           SecurityServiceManagerTestData
                                                                                                                                               .UserName,
                                                                                                                                           SecurityServiceManagerTestData
                                                                                                                                               .Password,
                                                                                                                                           SecurityServiceManagerTestData
                                                                                                                                               .EmailAddress,
                                                                                                                                           SecurityServiceManagerTestData
                                                                                                                                               .PhoneNumber,
                                                                                                                                           SecurityServiceManagerTestData
                                                                                                                                               .Claims,
                                                                                                                                           SecurityServiceManagerTestData
                                                                                                                                               .Roles,
                                                                                                                                           CancellationToken.None);
                                                                                                 });

            exception.Message.ShouldContain("Error deleting user");
        }

        [Fact]
        public async Task SecurityServiceManager_CreateUser_AddClaimsFailed_ErrorThrown()
        {
            this.UserStore.Setup(u => u.CreateAsync(It.IsAny<IdentityUser>(), It.IsAny<CancellationToken>())).ReturnsAsync(IdentityResult.Success);
            this.UserStore.Setup(u => u.FindByIdAsync(It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(new IdentityUser());
            this.UserStore.As<IUserRoleStore<IdentityUser>>().Setup(us => us.IsInRoleAsync(It.IsAny<IdentityUser>(), It.IsAny<String>(), CancellationToken.None))
                .ReturnsAsync(false);
            this.UserStore.As<IUserRoleStore<IdentityUser>>().Setup(us => us.AddToRoleAsync(It.IsAny<IdentityUser>(), It.IsAny<String>(), CancellationToken.None))
                .Returns(Task.CompletedTask);
            this.UserStore.SetupSequence(u => u.UpdateAsync(It.IsAny<IdentityUser>(), It.IsAny<CancellationToken>())).ReturnsAsync(IdentityResult.Success)
                .ReturnsAsync(IdentityResult.Failed());
            this.UserStore.As<IUserRoleStore<IdentityUser>>().Setup(us => us.GetRolesAsync(It.IsAny<IdentityUser>(), CancellationToken.None))
                .ReturnsAsync(new List<String>());
            this.PasswordHasher.Setup(ph => ph.HashPassword(It.IsAny<IdentityUser>(), It.IsAny<String>())).Returns("HashedPassword");
            this.UserStore.As<IUserClaimStore<IdentityUser>>()
                .Setup(u => u.AddClaimsAsync(It.IsAny<IdentityUser>(), It.IsAny<IEnumerable<Claim>>(), It.IsAny<CancellationToken>()));
            this.UserStore.Setup(u => u.DeleteAsync(It.IsAny<IdentityUser>(), It.IsAny<CancellationToken>())).ReturnsAsync(IdentityResult.Success);

            SecurityServiceManager securityServiceManager = this.SetupSecurityServiceManager();

            IdentityResultException exception = await Should.ThrowAsync<IdentityResultException>(async () =>
                                                                                                 {
                                                                                                     await
                                                                                                         securityServiceManager.CreateUser(SecurityServiceManagerTestData
                                                                                                                                               .GivenName,
                                                                                                                                           SecurityServiceManagerTestData
                                                                                                                                               .MiddleName,
                                                                                                                                           SecurityServiceManagerTestData
                                                                                                                                               .FamilyName,
                                                                                                                                           SecurityServiceManagerTestData
                                                                                                                                               .UserName,
                                                                                                                                           SecurityServiceManagerTestData
                                                                                                                                               .Password,
                                                                                                                                           SecurityServiceManagerTestData
                                                                                                                                               .EmailAddress,
                                                                                                                                           SecurityServiceManagerTestData
                                                                                                                                               .PhoneNumber,
                                                                                                                                           SecurityServiceManagerTestData
                                                                                                                                               .Claims,
                                                                                                                                           SecurityServiceManagerTestData
                                                                                                                                               .Roles,
                                                                                                                                           CancellationToken.None);
                                                                                                 });

            exception.Message.ShouldContain("Error adding claims");
        }

        [Fact]
        public async Task SecurityServiceManager_GetUser_UserDataReturned()
        {
            this.UserStore.Setup(u => u.FindByIdAsync(It.IsAny<String>(), CancellationToken.None)).ReturnsAsync(SecurityServiceManagerTestData.IdentityUser);
            this.UserStore.As<IUserRoleStore<IdentityUser>>().Setup(us => us.GetRolesAsync(It.IsAny<IdentityUser>(), CancellationToken.None))
                .ReturnsAsync(new List<String>());
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

            Should.Throw<ArgumentNullException>(async () => { await securityServiceManager.GetUser(Guid.Empty, CancellationToken.None); });
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public async Task SecurityServiceManager_GetUsers_NoFilters_UserDataReturned(String userName)
        {
            IQueryable<IdentityUser> identityUserList = await SecurityServiceManagerTestData.IdentityUsers();

            this.UserStore.As<IQueryableUserStore<IdentityUser>>().Setup(u => u.Users).Returns(identityUserList);
            this.UserStore.As<IUserRoleStore<IdentityUser>>().Setup(us => us.GetRolesAsync(It.IsAny<IdentityUser>(), CancellationToken.None))
                .ReturnsAsync(new List<String>());
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
            this.UserStore.As<IUserRoleStore<IdentityUser>>().Setup(us => us.GetRolesAsync(It.IsAny<IdentityUser>(), CancellationToken.None))
                .ReturnsAsync(new List<String>());
            this.UserStore.As<IUserClaimStore<IdentityUser>>().Setup(us => us.GetClaimsAsync(It.IsAny<IdentityUser>(), CancellationToken.None))
                .ReturnsAsync(new List<Claim>
                              {
                                  new Claim("claimType", "claimValue")
                              });

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
            this.UserStore.As<IUserRoleStore<IdentityUser>>().Setup(us => us.GetRolesAsync(It.IsAny<IdentityUser>(), CancellationToken.None))
                .ReturnsAsync(new List<String>());
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


        [Fact]
        public async Task SecurityServiceManager_CreateClient_ClientIsCreated()
        {
            String databaseName = Guid.NewGuid().ToString("N");
            IConfigurationDbContext context = this.GetConfigurationDbContext(databaseName);

            SecurityServiceManager securityServiceManager = this.SetupSecurityServiceManager(context);

            String clientId = await securityServiceManager.CreateClient(SecurityServiceManagerTestData.ClientId,
                                                                        SecurityServiceManagerTestData.ClientSecret,
                                                                        SecurityServiceManagerTestData.ClientName,
                                                                        SecurityServiceManagerTestData.ClientDescription,
                                                                        SecurityServiceManagerTestData.AllowedScopes,
                                                                        SecurityServiceManagerTestData.AllowedGrantTypes,
                                                                        CancellationToken.None);

            clientId.ShouldBe(SecurityServiceManagerTestData.ClientId);
        }

        [Fact]
        public async Task SecurityServiceManager_CreateClient_InvalidAllowedGrantType_ClientIsCreated()
        {
            String databaseName = Guid.NewGuid().ToString("N");
            IConfigurationDbContext context = this.GetConfigurationDbContext(databaseName);

            SecurityServiceManager securityServiceManager = this.SetupSecurityServiceManager(context);

            List<String> allowedGrantTypes = new List<String>
                                             {
                                                 "InvalidGrant"
                                             };

            var exception = await Should.ThrowAsync<ArgumentException>(async () =>
                                                 {
                                                     await securityServiceManager.CreateClient(SecurityServiceManagerTestData.ClientId,
                                                                                               SecurityServiceManagerTestData.ClientSecret,
                                                                                               SecurityServiceManagerTestData.ClientName,
                                                                                               SecurityServiceManagerTestData.ClientDescription,
                                                                                               SecurityServiceManagerTestData.AllowedScopes,
                                                                                               allowedGrantTypes,
                                                                                               CancellationToken.None);
                                                 });

            exception.Message.ShouldContain($"The grant types [{allowedGrantTypes.First()}] are not valid to create a new client");
        }

        [Fact]
        public async Task SecurityServiceManager_GetClient_ClientIsReturned()
        {
            String databaseName = Guid.NewGuid().ToString("N");
            IConfigurationDbContext context = this.GetConfigurationDbContext(databaseName);
            await context.Clients.AddAsync(new Client
                                           {
                                               Id = 1,
                                               ClientId = SecurityServiceManagerTestData.ClientId,
                                               ClientName = SecurityServiceManagerTestData.ClientName,
                                               Description = SecurityServiceManagerTestData.ClientDescription,
                                               AllowedGrantTypes = new List<ClientGrantType>
                                                                   {
                                                                       new ClientGrantType
                                                                       {
                                                                           Id = 1,
                                                                           GrantType = SecurityServiceManagerTestData.AllowedGrantTypes.First()
                                                                       }
                                                                   },
                                               AllowedScopes = new List<ClientScope>
                                                               {
                                                                   new ClientScope
                                                                   {
                                                                       Id = 1,
                                                                       Scope = SecurityServiceManagerTestData.AllowedScopes.First()
                                                                   }
                                                               }
                                           });
            await context.SaveChangesAsync();

            SecurityServiceManager securityServiceManager = this.SetupSecurityServiceManager(context);

            IdentityServer4.Models.Client client = await securityServiceManager.GetClient(SecurityServiceManagerTestData.ClientId, CancellationToken.None);

            client.ClientId.ShouldBe(SecurityServiceManagerTestData.ClientId);
            client.ClientName.ShouldBe(SecurityServiceManagerTestData.ClientName);
            client.Description.ShouldBe(SecurityServiceManagerTestData.ClientDescription);
            client.AllowedGrantTypes.ShouldNotBeEmpty();
            client.AllowedGrantTypes.ShouldNotBeNull();
            client.AllowedGrantTypes.Count.ShouldBe(1);
            client.AllowedGrantTypes.First().ShouldBe(SecurityServiceManagerTestData.AllowedGrantTypes.First());
            client.AllowedScopes.ShouldNotBeEmpty();
            client.AllowedScopes.ShouldNotBeNull();
            client.AllowedScopes.Count.ShouldBe(1);
            client.AllowedScopes.First().ShouldBe(SecurityServiceManagerTestData.AllowedScopes.First());
        }

        [Fact]
        public async Task SecurityServiceManager_GetClient_ClientNotFound_ErrorThrown()
        {
            String databaseName = Guid.NewGuid().ToString("N");
            IConfigurationDbContext context = this.GetConfigurationDbContext(databaseName);

            SecurityServiceManager securityServiceManager = this.SetupSecurityServiceManager(context);

            await Should.ThrowAsync<NotFoundException>(async () =>
                                                       {
                                                           await securityServiceManager.GetClient(SecurityServiceManagerTestData.ClientId, CancellationToken.None);
                                                       });
        }

        [Fact]
        public async Task SecurityServiceManager_GetClients_ClientsAreReturned()
        {
            String databaseName = Guid.NewGuid().ToString("N");
            IConfigurationDbContext context = this.GetConfigurationDbContext(databaseName);
            await context.Clients.AddAsync(new Client
            {
                Id = 1,
                ClientId = SecurityServiceManagerTestData.ClientId,
                ClientName = SecurityServiceManagerTestData.ClientName,
                Description = SecurityServiceManagerTestData.ClientDescription,
                AllowedGrantTypes = new List<ClientGrantType>
                                                                   {
                                                                       new ClientGrantType
                                                                       {
                                                                           Id = 1,
                                                                           GrantType = SecurityServiceManagerTestData.AllowedGrantTypes.First()
                                                                       }
                                                                   },
                AllowedScopes = new List<ClientScope>
                                                               {
                                                                   new ClientScope
                                                                   {
                                                                       Id = 1,
                                                                       Scope = SecurityServiceManagerTestData.AllowedScopes.First()
                                                                   }
                                                               }
            });
            await context.SaveChangesAsync();

            SecurityServiceManager securityServiceManager = this.SetupSecurityServiceManager(context);

            List<IdentityServer4.Models.Client> clients = await securityServiceManager.GetClients(CancellationToken.None);

            clients.ShouldNotBeNull();
            clients.ShouldNotBeEmpty();
            clients.Count.ShouldBe(1);
            clients.First().ClientId.ShouldBe(SecurityServiceManagerTestData.ClientId);
        }

        [Theory]
        [InlineData(true, false)]
        [InlineData(false, false)]
        public async Task SecurityServiceManager_CreateApiResource_ApiResourceIsCreated(Boolean nullScopes, Boolean emptyScopes)
        {
            String databaseName = Guid.NewGuid().ToString("N");
            IConfigurationDbContext context = this.GetConfigurationDbContext(databaseName);

            SecurityServiceManager securityServiceManager = this.SetupSecurityServiceManager(context);

            List<String> scopes = null;

            if (nullScopes == false)
            {
                if (emptyScopes == false)
                {
                    scopes = SecurityServiceManagerTestData.ApiResourceScopes;
                }
                else
                {
                    scopes = SecurityServiceManagerTestData.EmptyApiResourceScopes;
                }
            }

            String apiResourceName = await securityServiceManager.CreateApiResource(SecurityServiceManagerTestData.ApiResourceName,
                                                                        SecurityServiceManagerTestData.ApiResourceDisplayName,
                                                                        SecurityServiceManagerTestData.ApiResourceDescription,
                                                                        SecurityServiceManagerTestData.ApiResourceSecret,
                                                                        scopes,
                                                                        SecurityServiceManagerTestData.ApiResourceUserClaims,
                                                                        CancellationToken.None);

            apiResourceName.ShouldBe(SecurityServiceManagerTestData.ApiResourceName);
        }

        [Fact]
        public async Task SecurityServiceManager_GetApiResource_ApiResourceIsReturned()
        {
            String databaseName = Guid.NewGuid().ToString("N");
            IConfigurationDbContext context = this.GetConfigurationDbContext(databaseName);
            await context.ApiResources.AddAsync(new ApiResource
            {
                Id = 1,
                Name = SecurityServiceManagerTestData.ApiResourceName,
                DisplayName = SecurityServiceManagerTestData.ApiResourceDisplayName,
                Description = SecurityServiceManagerTestData.ApiResourceDescription,
                Scopes = new List<ApiScope>
                                       {
                                           new ApiScope
                                           {
                                               Id = 1,
                                               Name = SecurityServiceManagerTestData.ApiResourceScopes.First()
                                           }
                                       },
                Secrets = new List<ApiSecret>
                          {
                              new ApiSecret
                              {
                                  Value = SecurityServiceManagerTestData.ApiResourceSecret.ToSha256()
                              }
                          },
                UserClaims = new List<ApiResourceClaim>
                             {
                                 new ApiResourceClaim
                                 {
                                     Type = SecurityServiceManagerTestData.ApiResourceUserClaims.First()
                                 }
                             }

            });
            await context.SaveChangesAsync();

            SecurityServiceManager securityServiceManager = this.SetupSecurityServiceManager(context);

            IdentityServer4.Models.ApiResource apiResource = await securityServiceManager.GetApiResource(SecurityServiceManagerTestData.ApiResourceName, CancellationToken.None);

            apiResource.Name.ShouldBe(SecurityServiceManagerTestData.ApiResourceName);
            apiResource.Description.ShouldBe(SecurityServiceManagerTestData.ApiResourceDescription);
            apiResource.DisplayName.ShouldBe(SecurityServiceManagerTestData.ApiResourceDisplayName);
            apiResource.Scopes.ShouldNotBeEmpty();
            apiResource.Scopes.ShouldNotBeNull();
            apiResource.Scopes.Count.ShouldBe(1);
            apiResource.Scopes.First().Name.ShouldBe(SecurityServiceManagerTestData.ApiResourceScopes.First());
            apiResource.UserClaims.ShouldNotBeEmpty();
            apiResource.UserClaims.ShouldNotBeNull();
            apiResource.UserClaims.Count.ShouldBe(1);
            apiResource.UserClaims.First().ShouldBe(SecurityServiceManagerTestData.ApiResourceUserClaims.First());
        }

        [Fact]
        public async Task SecurityServiceManager_GetApiResource_ApiResourceNotFound_ErrorThrown()
        {
            String databaseName = Guid.NewGuid().ToString("N");
            IConfigurationDbContext context = this.GetConfigurationDbContext(databaseName);

            SecurityServiceManager securityServiceManager = this.SetupSecurityServiceManager(context);

            await Should.ThrowAsync<NotFoundException>(async () =>
            {
                await securityServiceManager.GetApiResource(SecurityServiceManagerTestData.ApiResourceName, CancellationToken.None);
            });
        }

        [Fact]
        public async Task SecurityServiceManager_GetApiResources_ApiResourcesAreReturned()
        {
            String databaseName = Guid.NewGuid().ToString("N");
            IConfigurationDbContext context = this.GetConfigurationDbContext(databaseName);
            await context.ApiResources.AddAsync(new ApiResource
                                                {
                                                    Id = 1,
                                                    Name = SecurityServiceManagerTestData.ApiResourceName,
                                                    DisplayName = SecurityServiceManagerTestData.ApiResourceDisplayName,
                                                    Description = SecurityServiceManagerTestData.ApiResourceDescription,
                                                    Scopes = new List<ApiScope>
                                                             {
                                                                 new ApiScope
                                                                 {
                                                                     Id = 1,
                                                                     Name = SecurityServiceManagerTestData.ApiResourceScopes.First()
                                                                 }
                                                             },
                                                    Secrets = new List<ApiSecret>
                                                              {
                                                                  new ApiSecret
                                                                  {
                                                                      Value = SecurityServiceManagerTestData.ApiResourceSecret.ToSha256()
                                                                  }
                                                              },
                                                    UserClaims = new List<ApiResourceClaim>
                                                                 {
                                                                     new ApiResourceClaim
                                                                     {
                                                                         Type = SecurityServiceManagerTestData.ApiResourceUserClaims.First()
                                                                     }
                                                                 }

                                                });
            await context.SaveChangesAsync();

            SecurityServiceManager securityServiceManager = this.SetupSecurityServiceManager(context);

            List<IdentityServer4.Models.ApiResource> apiResources = await securityServiceManager.GetApiResources(CancellationToken.None);

            apiResources.ShouldNotBeNull();
            apiResources.ShouldNotBeEmpty();
            apiResources.Count.ShouldBe(1);
            apiResources.First().Name.ShouldBe(SecurityServiceManagerTestData.ApiResourceName);
        }

        #endregion
    }
}