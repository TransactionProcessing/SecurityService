namespace SecurityService.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessLogic.Exceptions;
    using Duende.IdentityServer.EntityFramework.DbContexts;
    using Duende.IdentityServer.EntityFramework.Entities;
    using IdentityModel;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Models;
    using Moq;
    using SecurityService.BusinessLogic;
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

            // Run the test against one instance of the context
            ConfigurationDbContext configurationDbContext = this.GetConfigurationDbContext(Guid.NewGuid().ToString());

            SecurityServiceManager securityServiceManager =
                new SecurityServiceManager(passwordHasher.Object, userManager, roleManager, signInManager, configurationDbContext);

            securityServiceManager.ShouldNotBeNull();
        }

        [Theory]
        [InlineData(true, false)]
        [InlineData(false, false)]
        public async Task SecurityServiceManager_CreateApiResource_ApiResourceIsCreated(Boolean nullScopes,
                                                                                        Boolean emptyScopes)
        {
            String databaseName = Guid.NewGuid().ToString("N");
            ConfigurationDbContext context = this.GetConfigurationDbContext(databaseName);

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
        public async Task SecurityServiceManager_CreateApiScope_ApiScopeIsCreated()
        {
            String databaseName = Guid.NewGuid().ToString("N");
            ConfigurationDbContext context = this.GetConfigurationDbContext(databaseName);

            SecurityServiceManager securityServiceManager = this.SetupSecurityServiceManager(context);

            String apiScopeId = await securityServiceManager.CreateApiScope(SecurityServiceManagerTestData.ApiScopeName,
                                                                          SecurityServiceManagerTestData.ApiScopeDisplayName,
                                                                          SecurityServiceManagerTestData.ApiScopeDescription,
                                                                          CancellationToken.None);

            apiScopeId.ShouldBe(SecurityServiceManagerTestData.ApiScopeName);
        }

        [Fact]
        public async Task SecurityServiceManager_CreateClient_ClientIsCreated()
        {
            String databaseName = Guid.NewGuid().ToString("N");
            ConfigurationDbContext context = this.GetConfigurationDbContext(databaseName);

            SecurityServiceManager securityServiceManager = this.SetupSecurityServiceManager(context);

            String clientId = await securityServiceManager.CreateClient(SecurityServiceManagerTestData.ClientId,
                                                                        SecurityServiceManagerTestData.ClientSecret,
                                                                        SecurityServiceManagerTestData.ClientName,
                                                                        SecurityServiceManagerTestData.ClientDescription,
                                                                        SecurityServiceManagerTestData.AllowedScopes,
                                                                        SecurityServiceManagerTestData.AllowedGrantTypes,
                                                                        SecurityServiceManagerTestData.ClientRedirectUris,
                                                                        SecurityServiceManagerTestData.ClientPostLogoutRedirectUris,
                                                                        SecurityServiceManagerTestData.RequireConsentTrue,
                                                                        SecurityServiceManagerTestData.AllowOfflineAccessTrue,
                                                                        CancellationToken.None);

            clientId.ShouldBe(SecurityServiceManagerTestData.ClientId);
        }

        [Fact]
        public async Task SecurityServiceManager_CreateClient_EmptyPostLogoutRedirectUris_ClientIsCreated()
        {
            String databaseName = Guid.NewGuid().ToString("N");
            ConfigurationDbContext context = this.GetConfigurationDbContext(databaseName);

            SecurityServiceManager securityServiceManager = this.SetupSecurityServiceManager(context);

            String clientId = await securityServiceManager.CreateClient(SecurityServiceManagerTestData.ClientId,
                                                                        SecurityServiceManagerTestData.ClientSecret,
                                                                        SecurityServiceManagerTestData.ClientName,
                                                                        SecurityServiceManagerTestData.ClientDescription,
                                                                        SecurityServiceManagerTestData.AllowedScopes,
                                                                        SecurityServiceManagerTestData.AllowedGrantTypes,
                                                                        SecurityServiceManagerTestData.ClientRedirectUris,
                                                                        SecurityServiceManagerTestData.EmptyClientPostLogoutRedirectUris,
                                                                        SecurityServiceManagerTestData.RequireConsentTrue,
                                                                        SecurityServiceManagerTestData.AllowOfflineAccessTrue,
                                                                        CancellationToken.None);

            clientId.ShouldBe(SecurityServiceManagerTestData.ClientId);
        }

        [Fact]
        public async Task SecurityServiceManager_CreateClient_EmptyRedirectUris_ClientIsCreated()
        {
            String databaseName = Guid.NewGuid().ToString("N");
            ConfigurationDbContext context = this.GetConfigurationDbContext(databaseName);

            SecurityServiceManager securityServiceManager = this.SetupSecurityServiceManager(context);

            String clientId = await securityServiceManager.CreateClient(SecurityServiceManagerTestData.ClientId,
                                                                        SecurityServiceManagerTestData.ClientSecret,
                                                                        SecurityServiceManagerTestData.ClientName,
                                                                        SecurityServiceManagerTestData.ClientDescription,
                                                                        SecurityServiceManagerTestData.AllowedScopes,
                                                                        SecurityServiceManagerTestData.AllowedGrantTypes,
                                                                        SecurityServiceManagerTestData.EmptyClientRedirectUris,
                                                                        SecurityServiceManagerTestData.ClientPostLogoutRedirectUris,
                                                                        SecurityServiceManagerTestData.RequireConsentTrue,
                                                                        SecurityServiceManagerTestData.AllowOfflineAccessTrue,
                                                                        CancellationToken.None);

            clientId.ShouldBe(SecurityServiceManagerTestData.ClientId);
        }

        [Fact]
        public async Task SecurityServiceManager_CreateClient_InvalidAllowedGrantType_ClientIsCreated()
        {
            String databaseName = Guid.NewGuid().ToString("N");
            ConfigurationDbContext context = this.GetConfigurationDbContext(databaseName);

            SecurityServiceManager securityServiceManager = this.SetupSecurityServiceManager(context);

            List<String> allowedGrantTypes = new List<String>
                                             {
                                                 "InvalidGrant"
                                             };

            ArgumentException exception = await Should.ThrowAsync<ArgumentException>(async () =>
                                                                                     {
                                                                                         await
                                                                                             securityServiceManager.CreateClient(SecurityServiceManagerTestData.ClientId,
                                                                                                                                 SecurityServiceManagerTestData
                                                                                                                                     .ClientSecret,
                                                                                                                                 SecurityServiceManagerTestData
                                                                                                                                     .ClientName,
                                                                                                                                 SecurityServiceManagerTestData
                                                                                                                                     .ClientDescription,
                                                                                                                                 SecurityServiceManagerTestData
                                                                                                                                     .AllowedScopes,
                                                                                                                                 allowedGrantTypes,
                                                                                                                                 SecurityServiceManagerTestData
                                                                                                                                     .ClientRedirectUris,
                                                                                                                                 SecurityServiceManagerTestData
                                                                                                                                     .ClientPostLogoutRedirectUris,
                                                                                                                                 SecurityServiceManagerTestData
                                                                                                                                     .RequireConsentTrue,
                                                                                                                                 SecurityServiceManagerTestData
                                                                                                                                     .AllowOfflineAccessTrue,
                                                                                                                                 CancellationToken.None);
                                                                                     });

            exception.Message.ShouldContain($"The grant types [{allowedGrantTypes.First()}] are not valid to create a new client");
        }

        [Fact]
        public async Task SecurityServiceManager_CreateClient_NullPostLogoutRedirectUris_ClientIsCreated()
        {
            String databaseName = Guid.NewGuid().ToString("N");
            ConfigurationDbContext context = this.GetConfigurationDbContext(databaseName);

            SecurityServiceManager securityServiceManager = this.SetupSecurityServiceManager(context);

            String clientId = await securityServiceManager.CreateClient(SecurityServiceManagerTestData.ClientId,
                                                                        SecurityServiceManagerTestData.ClientSecret,
                                                                        SecurityServiceManagerTestData.ClientName,
                                                                        SecurityServiceManagerTestData.ClientDescription,
                                                                        SecurityServiceManagerTestData.AllowedScopes,
                                                                        SecurityServiceManagerTestData.AllowedGrantTypes,
                                                                        SecurityServiceManagerTestData.ClientRedirectUris,
                                                                        null,
                                                                        SecurityServiceManagerTestData.RequireConsentTrue,
                                                                        SecurityServiceManagerTestData.AllowOfflineAccessTrue,
                                                                        CancellationToken.None);

            clientId.ShouldBe(SecurityServiceManagerTestData.ClientId);
        }

        [Fact]
        public async Task SecurityServiceManager_CreateClient_NullRedirectUris_ClientIsCreated()
        {
            String databaseName = Guid.NewGuid().ToString("N");
            ConfigurationDbContext context = this.GetConfigurationDbContext(databaseName);

            SecurityServiceManager securityServiceManager = this.SetupSecurityServiceManager(context);

            String clientId = await securityServiceManager.CreateClient(SecurityServiceManagerTestData.ClientId,
                                                                        SecurityServiceManagerTestData.ClientSecret,
                                                                        SecurityServiceManagerTestData.ClientName,
                                                                        SecurityServiceManagerTestData.ClientDescription,
                                                                        SecurityServiceManagerTestData.AllowedScopes,
                                                                        SecurityServiceManagerTestData.AllowedGrantTypes,
                                                                        null,
                                                                        SecurityServiceManagerTestData.ClientPostLogoutRedirectUris,
                                                                        SecurityServiceManagerTestData.RequireConsentTrue,
                                                                        SecurityServiceManagerTestData.AllowOfflineAccessTrue,
                                                                        CancellationToken.None);

            clientId.ShouldBe(SecurityServiceManagerTestData.ClientId);
        }

        [Fact]
        public async Task SecurityServiceManager_CreateIdentityResource_IdentityResourceIsCreated()
        {
            String databaseName = Guid.NewGuid().ToString("N");
            ConfigurationDbContext context = this.GetConfigurationDbContext(databaseName);

            SecurityServiceManager securityServiceManager = this.SetupSecurityServiceManager(context);

            String identityResourceName = await securityServiceManager.CreateIdentityResource(SecurityServiceManagerTestData.IdentityResourceName,
                                                                                              SecurityServiceManagerTestData.IdentityResourceDisplayName,
                                                                                              SecurityServiceManagerTestData.IdentityResourceDescription,
                                                                                              true,
                                                                                              true,
                                                                                              true,
                                                                                              SecurityServiceManagerTestData.IdentityResourceUserClaims,
                                                                                              CancellationToken.None);

            identityResourceName.ShouldBe(SecurityServiceManagerTestData.IdentityResourceName);
        }

        [Fact]
        public async Task SecurityServiceManager_CreateRole_CreateFailed_ErrorThrown()
        {
            this.RoleStore.Setup(r => r.CreateAsync(It.IsAny<IdentityRole>(), It.IsAny<CancellationToken>())).ReturnsAsync(IdentityResult.Failed());

            SecurityServiceManager securityServiceManager = this.SetupSecurityServiceManager();

            Should.Throw<IdentityResultException>(async () =>
                                                  {
                                                      await securityServiceManager.CreateRole(SecurityServiceManagerTestData.RoleName, CancellationToken.None);
                                                  });
        }

        [Fact]
        public async Task SecurityServiceManager_CreateRole_RoleAlreadyExists_ErrorThrown()
        {
            this.RoleStore.Setup(r => r.FindByNameAsync(It.IsAny<String>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new IdentityRole(SecurityServiceManagerTestData.RoleName));

            SecurityServiceManager securityServiceManager = this.SetupSecurityServiceManager();

            Should.Throw<IdentityResultException>(async () =>
                                                  {
                                                      await securityServiceManager.CreateRole(SecurityServiceManagerTestData.RoleName, CancellationToken.None);
                                                  });
        }

        [Fact]
        public async Task SecurityServiceManager_CreateRole_RoleIsCreated()
        {
            this.RoleStore.Setup(r => r.CreateAsync(It.IsAny<IdentityRole>(), It.IsAny<CancellationToken>())).ReturnsAsync(IdentityResult.Success);

            SecurityServiceManager securityServiceManager = this.SetupSecurityServiceManager();

            Guid roleId = await securityServiceManager.CreateRole(SecurityServiceManagerTestData.RoleName, CancellationToken.None);

            roleId.ShouldNotBe(Guid.Empty);
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
        public async Task SecurityServiceManager_GetApiResource_ApiResourceIsReturned()
        {
            String databaseName = Guid.NewGuid().ToString("N");
            ConfigurationDbContext context = this.GetConfigurationDbContext(databaseName);
            await context.ApiResources.AddAsync(new ApiResource
                                                {
                                                    Id = 1,
                                                    Name = SecurityServiceManagerTestData.ApiResourceName,
                                                    DisplayName = SecurityServiceManagerTestData.ApiResourceDisplayName,
                                                    Description = SecurityServiceManagerTestData.ApiResourceDescription,
                                                    Scopes = new List<ApiResourceScope>
                                                             {
                                                                 new ApiResourceScope
                                                                 {
                                                                     Id = 1,
                                                                     Scope= SecurityServiceManagerTestData.ApiResourceScopes.First()
                                                                 }
                                                             },
                                                    Secrets = new List<ApiResourceSecret>
                                                              {
                                                                  new ApiResourceSecret
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

            Duende.IdentityServer.Models.ApiResource apiResource =
                await securityServiceManager.GetApiResource(SecurityServiceManagerTestData.ApiResourceName, CancellationToken.None);

            apiResource.Name.ShouldBe(SecurityServiceManagerTestData.ApiResourceName);
            apiResource.Description.ShouldBe(SecurityServiceManagerTestData.ApiResourceDescription);
            apiResource.DisplayName.ShouldBe(SecurityServiceManagerTestData.ApiResourceDisplayName);
            apiResource.Scopes.ShouldNotBeEmpty();
            apiResource.Scopes.ShouldNotBeNull();
            apiResource.Scopes.Count.ShouldBe(1);
            apiResource.Scopes.First().ShouldBe(SecurityServiceManagerTestData.ApiResourceScopes.First());
            apiResource.UserClaims.ShouldNotBeEmpty();
            apiResource.UserClaims.ShouldNotBeNull();
            apiResource.UserClaims.Count.ShouldBe(1);
            apiResource.UserClaims.First().ShouldBe(SecurityServiceManagerTestData.ApiResourceUserClaims.First());
        }

        [Fact]
        public async Task SecurityServiceManager_GetApiResource_ApiResourceNotFound_ErrorThrown()
        {
            String databaseName = Guid.NewGuid().ToString("N");
            ConfigurationDbContext context = this.GetConfigurationDbContext(databaseName);

            SecurityServiceManager securityServiceManager = this.SetupSecurityServiceManager(context);

            await Should.ThrowAsync<NotFoundException>(async () =>
                                                       {
                                                           await securityServiceManager.GetApiResource(SecurityServiceManagerTestData.ApiResourceName,
                                                                                                       CancellationToken.None);
                                                       });
        }

        [Fact]
        public async Task SecurityServiceManager_GetApiResources_ApiResourcesAreReturned()
        {
            String databaseName = Guid.NewGuid().ToString("N");
            ConfigurationDbContext context = this.GetConfigurationDbContext(databaseName);
            await context.ApiResources.AddAsync(new ApiResource
                                                {
                                                    Id = 1,
                                                    Name = SecurityServiceManagerTestData.ApiResourceName,
                                                    DisplayName = SecurityServiceManagerTestData.ApiResourceDisplayName,
                                                    Description = SecurityServiceManagerTestData.ApiResourceDescription,
                                                    Scopes = new List<ApiResourceScope>
                                                             {
                                                                 new ApiResourceScope
                                                                 {
                                                                     Id = 1,
                                                                     Scope = SecurityServiceManagerTestData.ApiResourceScopes.First()
                                                                 }
                                                             },
                                                    Secrets = new List<ApiResourceSecret>
                                                              {
                                                                  new ApiResourceSecret
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

            List<Duende.IdentityServer.Models.ApiResource> apiResources = await securityServiceManager.GetApiResources(CancellationToken.None);

            apiResources.ShouldNotBeNull();
            apiResources.ShouldNotBeEmpty();
            apiResources.Count.ShouldBe(1);
            apiResources.First().Name.ShouldBe(SecurityServiceManagerTestData.ApiResourceName);
        }

        [Fact]
        public async Task SecurityServiceManager_GetClient_ClientIsReturned()
        {
            String databaseName = Guid.NewGuid().ToString("N");
            ConfigurationDbContext context = this.GetConfigurationDbContext(databaseName);
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
                                                               },
                                               RedirectUris = new List<ClientRedirectUri>
                                                              {
                                                                  new ClientRedirectUri
                                                                  {
                                                                      Id = 1,
                                                                      RedirectUri = SecurityServiceManagerTestData.ClientRedirectUris.First()
                                                                  },
                                                              },
                                               PostLogoutRedirectUris = new List<ClientPostLogoutRedirectUri>
                                                                        {
                                                                            new ClientPostLogoutRedirectUri
                                                                            {
                                                                                Id = 1,
                                                                                PostLogoutRedirectUri =
                                                                                    SecurityServiceManagerTestData.ClientPostLogoutRedirectUris.First()
                                                                            }
                                                                        },
                                               RequireConsent = SecurityServiceManagerTestData.RequireConsentTrue
                                           });
            await context.SaveChangesAsync();

            SecurityServiceManager securityServiceManager = this.SetupSecurityServiceManager(context);

            Duende.IdentityServer.Models.Client client = await securityServiceManager.GetClient(SecurityServiceManagerTestData.ClientId, CancellationToken.None);

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
            client.RedirectUris.ShouldNotBeEmpty();
            client.RedirectUris.ShouldNotBeNull();
            client.RedirectUris.Count.ShouldBe(1);
            client.RedirectUris.First().ShouldBe(SecurityServiceManagerTestData.ClientRedirectUris.First());
            client.PostLogoutRedirectUris.ShouldNotBeEmpty();
            client.PostLogoutRedirectUris.ShouldNotBeNull();
            client.PostLogoutRedirectUris.Count.ShouldBe(1);
            client.PostLogoutRedirectUris.First().ShouldBe(SecurityServiceManagerTestData.ClientPostLogoutRedirectUris.First());
            client.RequireConsent.ShouldBe(SecurityServiceManagerTestData.RequireConsentTrue);
        }

        [Fact]
        public async Task SecurityServiceManager_GetClient_ClientNotFound_ErrorThrown()
        {
            String databaseName = Guid.NewGuid().ToString("N");
            ConfigurationDbContext context = this.GetConfigurationDbContext(databaseName);

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
            ConfigurationDbContext context = this.GetConfigurationDbContext(databaseName);
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

            List<Duende.IdentityServer.Models.Client> clients = await securityServiceManager.GetClients(CancellationToken.None);

            clients.ShouldNotBeNull();
            clients.ShouldNotBeEmpty();
            clients.Count.ShouldBe(1);
            clients.First().ClientId.ShouldBe(SecurityServiceManagerTestData.ClientId);
        }

        [Fact]
        public async Task SecurityServiceManager_GetIdentityResource_IdentityResourceIsReturned()
        {
            String databaseName = Guid.NewGuid().ToString("N");
            ConfigurationDbContext context = this.GetConfigurationDbContext(databaseName);
            await context.IdentityResources.AddAsync(new IdentityResource
                                                     {
                                                         Id = 1,
                                                         Name = SecurityServiceManagerTestData.IdentityResourceName,
                                                         DisplayName = SecurityServiceManagerTestData.IdentityResourceDisplayName,
                                                         Description = SecurityServiceManagerTestData.IdentityResourceDescription,
                                                         UserClaims = new List<IdentityResourceClaim>
                                                                      {
                                                                          new IdentityResourceClaim
                                                                          {
                                                                              Type = SecurityServiceManagerTestData.IdentityResourceUserClaims.First()
                                                                          }
                                                                      }
                                                     });
            await context.SaveChangesAsync();

            SecurityServiceManager securityServiceManager = this.SetupSecurityServiceManager(context);

            Duende.IdentityServer.Models.IdentityResource identityResource =
                await securityServiceManager.GetIdentityResource(SecurityServiceManagerTestData.IdentityResourceName, CancellationToken.None);

            identityResource.Name.ShouldBe(SecurityServiceManagerTestData.IdentityResourceName);
            identityResource.Description.ShouldBe(SecurityServiceManagerTestData.IdentityResourceDescription);
            identityResource.DisplayName.ShouldBe(SecurityServiceManagerTestData.IdentityResourceDisplayName);
            identityResource.UserClaims.ShouldNotBeEmpty();
            identityResource.UserClaims.ShouldNotBeNull();
            identityResource.UserClaims.Count.ShouldBe(1);
            identityResource.UserClaims.First().ShouldBe(SecurityServiceManagerTestData.IdentityResourceUserClaims.First());
        }

        [Fact]
        public async Task SecurityServiceManager_GetIdentityResource_IdentityResourceNotFound_ErrorThrown()
        {
            String databaseName = Guid.NewGuid().ToString("N");
            ConfigurationDbContext context = this.GetConfigurationDbContext(databaseName);

            SecurityServiceManager securityServiceManager = this.SetupSecurityServiceManager(context);

            await Should.ThrowAsync<NotFoundException>(async () =>
                                                       {
                                                           await securityServiceManager.GetIdentityResource(SecurityServiceManagerTestData.IdentityResourceDisplayName,
                                                                                                            CancellationToken.None);
                                                       });
        }

        [Fact]
        public async Task SecurityServiceManager_GetIdentityResources_IdentityResourcesAreReturned()
        {
            String databaseName = Guid.NewGuid().ToString("N");
            ConfigurationDbContext context = this.GetConfigurationDbContext(databaseName);
            await context.IdentityResources.AddAsync(new IdentityResource
                                                     {
                                                         Id = 1,
                                                         Name = SecurityServiceManagerTestData.IdentityResourceName,
                                                         DisplayName = SecurityServiceManagerTestData.IdentityResourceDisplayName,
                                                         Description = SecurityServiceManagerTestData.IdentityResourceDescription,
                                                         UserClaims = new List<IdentityResourceClaim>
                                                                      {
                                                                          new IdentityResourceClaim
                                                                          {
                                                                              Type = SecurityServiceManagerTestData.IdentityResourceUserClaims.First()
                                                                          }
                                                                      }
                                                     });
            await context.SaveChangesAsync();

            SecurityServiceManager securityServiceManager = this.SetupSecurityServiceManager(context);

            List<Duende.IdentityServer.Models.IdentityResource> identityResources = await securityServiceManager.GetIdentityResources(CancellationToken.None);

            identityResources.ShouldNotBeNull();
            identityResources.ShouldNotBeEmpty();
            identityResources.Count.ShouldBe(1);
            identityResources.First().Name.ShouldBe(SecurityServiceManagerTestData.IdentityResourceName);
        }

        [Fact]
        public async Task SecurityServiceManager_GetRole_RoleDataReturned()
        {
            this.RoleStore.Setup(r => r.FindByIdAsync(It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(SecurityServiceManagerTestData.IdentityRole);

            SecurityServiceManager securityServiceManager = this.SetupSecurityServiceManager();

            RoleDetails roleDetails = await securityServiceManager.GetRole(Guid.Parse(SecurityServiceManagerTestData.Role1Id), CancellationToken.None);

            roleDetails.RoleId.ShouldBe(Guid.Parse(SecurityServiceManagerTestData.Role1Id));
            roleDetails.RoleName.ShouldBe(SecurityServiceManagerTestData.RoleName);
        }

        [Fact]
        public async Task SecurityServiceManager_GetRole_RoleNotFound_ErrorThrown()
        {
            SecurityServiceManager securityServiceManager = this.SetupSecurityServiceManager();

            Should.Throw<NotFoundException>(async () =>
                                            {
                                                await securityServiceManager.GetRole(Guid.Parse(SecurityServiceManagerTestData.Role1Id), CancellationToken.None);
                                            });
        }

        [Fact]
        public async Task SecurityServiceManager_GetRoles_RoleDataReturned()
        {
            List<IdentityRole> identityRoles = new List<IdentityRole>
                                               {
                                                   SecurityServiceManagerTestData.IdentityRole
                                               };

            this.RoleStore.Setup(r => r.Roles).Returns(identityRoles.AsQueryable());

            SecurityServiceManager securityServiceManager = this.SetupSecurityServiceManager();

            List<RoleDetails> roleDetailsList = await securityServiceManager.GetRoles(CancellationToken.None);

            roleDetailsList.ShouldHaveSingleItem();
            roleDetailsList.Single().RoleId.ShouldBe(Guid.Parse(SecurityServiceManagerTestData.Role1Id));
            roleDetailsList.Single().RoleName.ShouldBe(SecurityServiceManagerTestData.RoleName);
        }

        [Fact]
        public async Task SecurityServiceManager_GetUser_InvalidData_ErrorThrown()
        {
            SecurityServiceManager securityServiceManager = this.SetupSecurityServiceManager();

            Should.Throw<ArgumentNullException>(async () => { await securityServiceManager.GetUser(Guid.Empty, CancellationToken.None); });
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
            userDetails.Username.ShouldBe(SecurityServiceManagerTestData.UserName);
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
            userDetails.First().Username.ShouldBe(SecurityServiceManagerTestData.UserName1);
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
        public async Task SecurityServiceManager_ValidateCredentials_InvalidCredentials_ResultIsFalse()
        {
            this.UserStore.Setup(u => u.FindByNameAsync(It.IsAny<String>(), CancellationToken.None)).ReturnsAsync(SecurityServiceManagerTestData.IdentityUser);

            this.PasswordHasher.Setup(p => p.VerifyHashedPassword(It.IsAny<IdentityUser>(), It.IsAny<String>(), It.IsAny<String>()))
                .Returns(PasswordVerificationResult.Failed);

            SecurityServiceManager securityServiceManager = this.SetupSecurityServiceManager();

            Boolean validateResult =
                await securityServiceManager.ValidateCredentials(SecurityServiceManagerTestData.UserName,
                                                                 SecurityServiceManagerTestData.Password,
                                                                 CancellationToken.None);

            validateResult.ShouldBeFalse();
        }

        [Fact]
        public async Task SecurityServiceManager_ValidateCredentials_ResultIsTrue()
        {
            this.UserStore.Setup(u => u.FindByNameAsync(It.IsAny<String>(), CancellationToken.None)).ReturnsAsync(SecurityServiceManagerTestData.IdentityUser);

            this.PasswordHasher.Setup(p => p.VerifyHashedPassword(It.IsAny<IdentityUser>(), It.IsAny<String>(), It.IsAny<String>()))
                .Returns(PasswordVerificationResult.Success);

            SecurityServiceManager securityServiceManager = this.SetupSecurityServiceManager();

            Boolean validateResult =
                await securityServiceManager.ValidateCredentials(SecurityServiceManagerTestData.UserName,
                                                                 SecurityServiceManagerTestData.Password,
                                                                 CancellationToken.None);

            validateResult.ShouldBeTrue();
        }

        #endregion

        [Fact]
        public async Task SecurityServiceManager_GetApiScope_ApiScopeIsReturned()
        {
            String databaseName = Guid.NewGuid().ToString("N");
            ConfigurationDbContext context = this.GetConfigurationDbContext(databaseName);
            await context.ApiScopes.AddAsync(new ApiScope
                                             {
                                                 Id = 1,
                                                 Name = SecurityServiceManagerTestData.ApiScopeName,
                                                 DisplayName = SecurityServiceManagerTestData.ApiResourceDisplayName,
                                                 Description = SecurityServiceManagerTestData.ApiScopeDescription,
                                                 Emphasize = false,
                                                 Enabled = true,
                                                 Required = false,
                                                 ShowInDiscoveryDocument = true
                                             });
            await context.SaveChangesAsync();

            SecurityServiceManager securityServiceManager = this.SetupSecurityServiceManager(context);

            Duende.IdentityServer.Models.ApiScope apiScope = await securityServiceManager.GetApiScope(SecurityServiceManagerTestData.ApiScopeName, CancellationToken.None);

            apiScope.Name.ShouldBe(SecurityServiceManagerTestData.ApiScopeName);
            apiScope.Description.ShouldBe(SecurityServiceManagerTestData.ApiScopeDescription);
            apiScope.DisplayName.ShouldBe(SecurityServiceManagerTestData.ApiResourceDisplayName);
            apiScope.Emphasize.ShouldBeFalse();
            apiScope.Required.ShouldBeFalse();
            apiScope.ShowInDiscoveryDocument.ShouldBeTrue();
            apiScope.Enabled.ShouldBeTrue();
        }

        [Fact]
        public async Task SecurityServiceManager_GetApiScope_ApiScopeNotFound_ErrorThrown()
        {
            String databaseName = Guid.NewGuid().ToString("N");
            ConfigurationDbContext context = this.GetConfigurationDbContext(databaseName);

            SecurityServiceManager securityServiceManager = this.SetupSecurityServiceManager(context);

            await Should.ThrowAsync<NotFoundException>(async () =>
            {
                await securityServiceManager.GetApiScope(SecurityServiceManagerTestData.ApiScopeName,
                                                            CancellationToken.None);
            });
        }

        [Fact]
        public async Task SecurityServiceManager_GetApiScopes_ApiScopesAreReturned()
        {
            String databaseName = Guid.NewGuid().ToString("N");
            ConfigurationDbContext context = this.GetConfigurationDbContext(databaseName);
            await context.ApiScopes.AddAsync(new ApiScope
                                             {
                                                 Id = 1,
                                                 Name = SecurityServiceManagerTestData.ApiScopeName,
                                                 DisplayName = SecurityServiceManagerTestData.ApiResourceDisplayName,
                                                 Description = SecurityServiceManagerTestData.ApiScopeDescription,
                                                 Emphasize = false,
                                                 Enabled = true,
                                                 Required = false,
                                                 ShowInDiscoveryDocument = true
                                             });
            await context.SaveChangesAsync();

            SecurityServiceManager securityServiceManager = this.SetupSecurityServiceManager(context);

            List<Duende.IdentityServer.Models.ApiScope> apiScopes = await securityServiceManager.GetApiScopes(CancellationToken.None);

            apiScopes.ShouldNotBeNull();
            apiScopes.ShouldNotBeEmpty();
            apiScopes.Count.ShouldBe(1);
            apiScopes.First().Name.ShouldBe(SecurityServiceManagerTestData.ApiScopeName);
        }
    }
}