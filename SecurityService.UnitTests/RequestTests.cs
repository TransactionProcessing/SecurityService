using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecurityService.UnitTests
{
    using NLog.Web.LayoutRenderers;
    using SecurityService.BusinessLogic.Requests;
    using Shouldly;
    using Xunit;

    public class RequestTests{
        [Fact]
        public void ChangeUserPasswordRequest_CanBeCreated_IsCreated(){
            ChangeUserPasswordRequest request = ChangeUserPasswordRequest.Create(TestData.UserName,
                                                                                 TestData.Password,
                                                                                 TestData.NewPassword,
                                                                                 TestData.ClientId);

            request.ShouldNotBeNull();
            request.UserName.ShouldBe(TestData.UserName);
            request.CurrentPassword.ShouldBe(TestData.Password);
            request.NewPassword.ShouldBe(TestData.NewPassword);
            request.ClientId.ShouldBe(TestData.ClientId);
        }

        [Fact]
        public void ConfirmUserEmailAddressRequest_CanBeCreated_IsCreated(){
            ConfirmUserEmailAddressRequest request = ConfirmUserEmailAddressRequest.Create(TestData.UserName,
                                                                                           TestData.ConfirmEmailToken);
            request.ShouldNotBeNull();
            request.UserName.ShouldBe(TestData.UserName);
            request.ConfirmEmailToken.ShouldBe(TestData.ConfirmEmailToken);
        }

        [Fact]
        public void CreateApiResourceRequest_CanBeCreated_IsCreated(){
            CreateApiResourceRequest request = CreateApiResourceRequest.Create(TestData.ApiResourceName,
                                                                               TestData.ApiResourceDisplayName,
                                                                               TestData.ApiResourceDescription,
                                                                               TestData.ApiResourceSecret,
                                                                               TestData.ApiResourceScopes,
                                                                               TestData.ApiResourceUserClaims);

            request.ShouldNotBeNull();
            request.Name.ShouldBe(TestData.ApiResourceName);
            request.DisplayName.ShouldBe(TestData.ApiResourceDisplayName);
            request.Description.ShouldBe(TestData.ApiResourceDescription);
            request.Secret.ShouldBe(TestData.ApiResourceSecret);
            request.Scopes.Count.ShouldBe(TestData.ApiResourceScopes.Count);
            request.UserClaims.Count.ShouldBe(TestData.ApiResourceUserClaims.Count);
        }

        [Fact]
        public void CreateApiScopeRequest_CanBeCreated_IsCreated(){
            CreateApiScopeRequest request = CreateApiScopeRequest.Create(TestData.ApiScopeName,
                                                                         TestData.ApiScopeDisplayName,
                                                                         TestData.ApiScopeDescription);

            request.ShouldNotBeNull();
            request.Name.ShouldBe(TestData.ApiScopeName);
            request.DisplayName.ShouldBe(TestData.ApiScopeDisplayName);
            request.Description.ShouldBe(TestData.ApiScopeDescription);
        }

        [Fact]
        public void CreateClientRequest_CanBeCreated_IsCreated(){
            CreateClientRequest request = CreateClientRequest.Create(TestData.ClientId,
                                                                     TestData.ClientSecret,
                                                                     TestData.ClientName,
                                                                     TestData.ClientDescription,
                                                                     TestData.AllowedScopes,
                                                                     TestData.AllowedGrantTypes,
                                                                     TestData.ClientUri,
                                                                     TestData.ClientRedirectUris,
                                                                     TestData.ClientPostLogoutRedirectUris,
                                                                     TestData.RequireConsentFalse,
                                                                     TestData.AllowOfflineAccessFalse);

            request.ShouldNotBeNull();
            request.ClientId.ShouldBe(TestData.ClientId);
            request.Secret.ShouldBe(TestData.ClientSecret);
            request.ClientName.ShouldBe(TestData.ClientName);
            request.ClientDescription.ShouldBe(TestData.ClientDescription);
            request.AllowedScopes.Count().ShouldBe(TestData.AllowedScopes.Count);
            request.AllowedGrantTypes.Count().ShouldBe(TestData.AllowedGrantTypes.Count);
            request.ClientUri.ShouldBe(TestData.ClientUri);
            request.ClientRedirectUris.Count().ShouldBe(TestData.ClientRedirectUris.Count);
            request.ClientPostLogoutRedirectUris.Count().ShouldBe(TestData.ClientPostLogoutRedirectUris.Count);
            request.RequireConsent.ShouldBe(TestData.RequireConsentFalse);
            request.AllowOfflineAccess.ShouldBe(TestData.AllowOfflineAccessFalse);
        }

        [Fact]
        public void CreateIdentityResourceRequest_CanBeCreated_IsCreated(){
            CreateIdentityResourceRequest request = CreateIdentityResourceRequest.Create(TestData.IdentityResourceName,
                                                                                         TestData.IdentityResourceDisplayName,
                                                                                         TestData.IdentityResourceDescription,
                                                                                         TestData.IdentityResourceRequired,
                                                                                         TestData.IdentityResourceEmphasize,
                                                                                         TestData.IdentityResourceShowInDiscoveryDocument,
                                                                                         TestData.IdentityResourceUserClaims);

            request.ShouldNotBeNull();
            request.Name.ShouldBe(TestData.IdentityResourceName);
            request.DisplayName.ShouldBe(TestData.IdentityResourceDisplayName);
            request.Description.ShouldBe(TestData.IdentityResourceDescription);
            request.Required.ShouldBe(TestData.IdentityResourceRequired);
            request.Emphasize.ShouldBe(TestData.IdentityResourceEmphasize);
            request.ShowInDiscoveryDocument.ShouldBe(TestData.IdentityResourceShowInDiscoveryDocument);
            request.Claims.Count().ShouldBe(TestData.IdentityResourceUserClaims.Count);
        }

        [Fact]
        public void CreateRoleRequest_CanBeCreated_IsCreated(){
            CreateRoleRequest request = CreateRoleRequest.Create(Guid.Parse(TestData.Role1Id), TestData.RoleName);

            request.ShouldNotBeNull();
            request.RoleId.ToString().ShouldBe(TestData.Role1Id, StringCompareShould.IgnoreCase);
            request.Name.ShouldBe(TestData.RoleName);
        }

        [Fact]
        public void CreateUserRequest_CanBeCreated_IsCreated(){
            CreateUserRequest request = CreateUserRequest.Create(Guid.Parse(TestData.UserId),
                                                                 TestData.GivenName,
                                                                 TestData.MiddleName,
                                                                 TestData.FamilyName,
                                                                 TestData.UserName,
                                                                 TestData.Password,
                                                                 TestData.EmailAddress,
                                                                 TestData.PhoneNumber,
                                                                 TestData.Claims,
                                                                 TestData.Roles);

            request.ShouldNotBeNull();
            request.UserId.ToString().ShouldBe(TestData.UserId, StringCompareShould.IgnoreCase);
            request.GivenName.ShouldBe(TestData.GivenName);
            request.MiddleName.ShouldBe(TestData.MiddleName);
            request.FamilyName.ShouldBe(TestData.FamilyName);
            request.UserName.ShouldBe(TestData.UserName);
            request.Password.ShouldBe(TestData.Password);
            request.EmailAddress.ShouldBe(TestData.EmailAddress);
            request.PhoneNumber.ShouldBe(TestData.PhoneNumber);
            request.Claims.Count().ShouldBe(TestData.Claims.Count);
            request.Roles.Count().ShouldBe(TestData.Roles.Count);
        }

        [Fact]
        public void GetApiResourceRequest_CanBeCreated_IsCreated(){
            GetApiResourceRequest request = GetApiResourceRequest.Create(TestData.ApiResourceName);

            request.ShouldNotBeNull();
            request.Name.ShouldBe(TestData.ApiResourceName);
        }

        [Fact]
        public void GetApiResourcesRequest_CanBeCreated_IsCreated(){
            GetApiResourcesRequest request = GetApiResourcesRequest.Create();

            request.ShouldNotBeNull();
        }

        [Fact]
        public void GetApiScopeRequest_CanBeCreated_IsCreated(){
            GetApiScopeRequest request = GetApiScopeRequest.Create(TestData.ApiScopeName);

            request.ShouldNotBeNull();
            request.Name.ShouldBe(TestData.ApiScopeName);
        }

        [Fact]
        public void GetApiScopesRequest_CanBeCreated_IsCreated(){
            GetApiScopesRequest request = GetApiScopesRequest.Create();

            request.ShouldNotBeNull();
        }

        [Fact]
        public void GetClientRequest_CanBeCreated_IsCreated(){
            GetClientRequest request = GetClientRequest.Create(TestData.ClientId);

            request.ShouldNotBeNull();
            request.ClientId.ShouldBe(TestData.ClientId);
        }

        [Fact]
        public void GetClientsRequest_CanBeCreated_IsCreated(){
            GetClientsRequest request = GetClientsRequest.Create();

            request.ShouldNotBeNull();
        }

        [Fact]
        public void GetIdentityResourceRequest_CanBeCreated_IsCreated(){
            GetIdentityResourceRequest request = GetIdentityResourceRequest.Create(TestData.IdentityResourceName);

            request.ShouldNotBeNull();
            request.IdentityResourceName.ShouldBe(TestData.IdentityResourceName);
        }

        [Fact]
        public void GetIdentityResourcesRequest_CanBeCreated_IsCreated(){
            GetIdentityResourcesRequest request = GetIdentityResourcesRequest.Create();

            request.ShouldNotBeNull();
        }

        [Fact]
        public void GetRoleRequest_CanBeCreated_IsCreated(){
            GetRoleRequest request = GetRoleRequest.Create(Guid.Parse(TestData.Role1Id));

            request.ShouldNotBeNull();
            request.RoleId.ToString().ShouldBe(TestData.Role1Id, StringCompareShould.IgnoreCase);
        }

        [Fact]
        public void GetRolesRequest_CanBeCreated_IsCreated(){
            GetRolesRequest request = GetRolesRequest.Create();

            request.ShouldNotBeNull();
        }

        [Fact]
        public void ProcessPasswordResetConfirmationRequest_CanBeCreated_IsCreated(){
            ProcessPasswordResetConfirmationRequest request = ProcessPasswordResetConfirmationRequest.Create(TestData.UserName,
                                                                                                             TestData.PasswordResetToken,
                                                                                                             TestData.Password,
                                                                                                             TestData.ClientId);

            request.ShouldNotBeNull();
            request.ClientId.ShouldBe(TestData.ClientId);
            request.Username.ShouldBe(TestData.UserName);
            request.Password.ShouldBe(TestData.Password);
            request.Token.ShouldBe(TestData.PasswordResetToken);
        }

        [Fact]
        public void ProcessPasswordResetRequest_CanBeCreated_IsCreated(){
            ProcessPasswordResetRequest request = ProcessPasswordResetRequest.Create(TestData.UserName,
                                                                                     TestData.EmailAddress,
                                                                                     TestData.ClientId);

            request.ShouldNotBeNull();
            request.ClientId.ShouldBe(TestData.ClientId);
            request.EmailAddress.ShouldBe(TestData.EmailAddress);
            request.Username.ShouldBe(TestData.UserName);
        }

        [Fact]
        public void SendWelcomeEmailRequest_CanBeCreated_IsCreated(){
            SendWelcomeEmailRequest request = SendWelcomeEmailRequest.Create(TestData.UserName);

            request.ShouldNotBeNull();
            request.UserName.ShouldBe(TestData.UserName);
        }
    }
}
