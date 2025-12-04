namespace SecurityService.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using DataTransferObjects.Responses;
    using Duende.IdentityServer.Models;
    using Factories;
    using Shouldly;
    using Xunit;
    using RoleDetails = Models.RoleDetails;
    using UserDetails = Models.UserDetails;

    public class ModelFactoryTests
    {
        #region Methods

        [Fact]
        public void ModelFactory_ConvertFrom_ApiResource_ModelConverted()
        {
            ApiResource apiResourceModel = new ApiResource
                                           {
                                               Scopes = new List<String>
                                                        {
                                                            TestData.AllowedScopes.First()
                                                        },
                                               Description = TestData.ApiResourceDescription,
                                               Name = TestData.ApiResourceName,
                                               DisplayName = TestData.ApiResourceDisplayName,
                                               UserClaims = TestData.ApiResourceUserClaims,
                                               Enabled = true
                                           };

            ApiResourceDetails apiResourceDto = ModelFactory.ConvertFrom(apiResourceModel);

            apiResourceDto.Scopes.First().ShouldBe(TestData.AllowedScopes.First());
            apiResourceDto.Description.ShouldBe(TestData.ApiResourceDescription);
            apiResourceDto.DisplayName.ShouldBe(TestData.ApiResourceDisplayName);
            apiResourceDto.Name.ShouldBe(TestData.ApiResourceName);
            apiResourceDto.UserClaims.ShouldBe(TestData.ApiResourceUserClaims);
            apiResourceDto.Enabled.ShouldBeTrue();
        }

        [Fact]
        public void ModelFactory_ConvertFrom_ApiResource_ModelIsNull_NullReturned()
        {
            ApiResource apiResourceModel = null;

            ApiResourceDetails apiResourceDto = ModelFactory.ConvertFrom(apiResourceModel);

            apiResourceDto.ShouldBeNull();
        }

        [Fact]
        public void ModelFactory_ConvertFrom_ApiScope_ModelConverted()
        {
            ApiScope apiScopeModel = new ApiScope
            {
                Description = TestData.ApiScopeDescription,
                Name = TestData.ApiScopeName,
                DisplayName = TestData.ApiScopeDisplayName,
                Enabled = true
            };

            ApiScopeDetails apiScopeDto = ModelFactory.ConvertFrom(apiScopeModel);

            apiScopeDto.Description.ShouldBe(TestData.ApiScopeDescription);
            apiScopeDto.DisplayName.ShouldBe(TestData.ApiScopeDisplayName);
            apiScopeDto.Name.ShouldBe(TestData.ApiScopeName);
            apiScopeDto.Enabled.ShouldBeTrue();
        }

        [Fact]
        public void ModelFactory_ConvertFrom_ApiScope_ModelIsNull_NullReturned()
        {
            ApiScope apiScopeModel = null;

            ApiScopeDetails apiScopeDto = ModelFactory.ConvertFrom(apiScopeModel);

            apiScopeDto.ShouldBeNull();
        }

        [Fact]
        public void ModelFactory_ConvertFrom_ApiResourceList_ListIsEmpty_NullReturned()
        {
            List<ApiResource> apiResourceList = new List<ApiResource>();

            List<ApiResourceDetails> apiResourceDtoList = ModelFactory.ConvertFrom(apiResourceList);

            apiResourceDtoList.ShouldBeEmpty();
        }

        [Fact]
        public void ModelFactory_ConvertFrom_ApiResourceList_ListIsNull_NullReturned()
        {
            List<ApiResource> apiResourceList = null;

            List<ApiResourceDetails> apiResourceDtoList = ModelFactory.ConvertFrom(apiResourceList);

            apiResourceDtoList.ShouldBeEmpty();
        }

        [Fact]
        public void ModelFactory_ConvertFrom_ApiResourceList_ModelsConverted()
        {
            ApiResource apiResourceModel = new ApiResource
                                           {
                                               Scopes = new List<String>
                                                        {
                                                            TestData.AllowedScopes.First()
                                                        },
                                               Description = TestData.ApiResourceDescription,
                                               Name = TestData.ApiResourceName,
                                               DisplayName = TestData.ApiResourceDisplayName,
                                               UserClaims = TestData.ApiResourceUserClaims,
                                               Enabled = true
                                           };

            List<ApiResource> apiResourceModelList = [
                apiResourceModel
            ];

            List<ApiResourceDetails> apiResourceDtoList = ModelFactory.ConvertFrom(apiResourceModelList);

            apiResourceDtoList.ShouldNotBeNull();
            apiResourceDtoList.ShouldNotBeEmpty();
            apiResourceDtoList.Count.ShouldBe(apiResourceModelList.Count);
            apiResourceDtoList.First().Scopes.First().ShouldBe(TestData.AllowedScopes.First());
            apiResourceDtoList.First().Description.ShouldBe(TestData.ApiResourceDescription);
            apiResourceDtoList.First().DisplayName.ShouldBe(TestData.ApiResourceDisplayName);
            apiResourceDtoList.First().Name.ShouldBe(TestData.ApiResourceName);
            apiResourceDtoList.First().UserClaims.ShouldBe(TestData.ApiResourceUserClaims);
            apiResourceDtoList.First().Enabled.ShouldBeTrue();
        }

        [Fact]
        public void ModelFactory_ConvertFrom_ApiScopeList_ListIsEmpty_NullReturned()
        {
            List<ApiScope> apiScopeList = new();

            List<ApiScopeDetails> apiScopeDtoList = ModelFactory.ConvertFrom(apiScopeList);

            apiScopeDtoList.ShouldBeEmpty();
        }

        [Fact]
        public void ModelFactory_ConvertFrom_ApiScopeList_ListIsNull_NullReturned()
        {
            List<ApiScope> apiScopeList = null;

            List<ApiScopeDetails> apiScopeDtoList = ModelFactory.ConvertFrom(apiScopeList);

            apiScopeDtoList.ShouldBeEmpty();
        }

        [Fact]
        public void ModelFactory_ConvertFrom_ApiScopeList_ModelsConverted()
        {
            ApiScope apiScopeModel = new ApiScope
            {
                Description = TestData.ApiScopeDescription,
                Name = TestData.ApiScopeName,
                DisplayName = TestData.ApiResourceDisplayName,
                Enabled = true
            };

            List<ApiScope> apiScopeModelList = [
                apiScopeModel
            ];

            List<ApiScopeDetails> apiScopeDtoList = ModelFactory.ConvertFrom(apiScopeModelList);

            apiScopeDtoList.ShouldNotBeNull();
            apiScopeDtoList.ShouldNotBeEmpty();
            apiScopeDtoList.Count.ShouldBe(apiScopeModelList.Count);
            apiScopeDtoList.First().Description.ShouldBe(TestData.ApiScopeDescription);
            apiScopeDtoList.First().DisplayName.ShouldBe(TestData.ApiResourceDisplayName);
            apiScopeDtoList.First().Name.ShouldBe(TestData.ApiScopeName);
            apiScopeDtoList.First().Enabled.ShouldBeTrue();
        }

        [Fact]
        public void ModelFactory_ConvertFrom_Client_ModelConverted()
        {
            Client clientModel = new Client
                                 {
                                     ClientId = TestData.ClientId,
                                     ClientName = TestData.ClientName,
                                     Description = TestData.ClientDescription,
                                     AllowedGrantTypes = TestData.AllowedGrantTypes,
                                     AllowedScopes = TestData.AllowedScopes,
                                     Enabled = true
                                 };

            ClientDetails clientDto = ModelFactory.ConvertFrom(clientModel);

            clientDto.AllowedGrantTypes.ShouldBe(TestData.AllowedGrantTypes);
            clientDto.AllowedScopes.ShouldBe(TestData.AllowedScopes);
            clientDto.ClientDescription.ShouldBe(TestData.ClientDescription);
            clientDto.ClientId.ShouldBe(TestData.ClientId);
            clientDto.ClientName.ShouldBe(TestData.ClientName);
            clientDto.Enabled.ShouldBeTrue();
        }

        [Fact]
        public void ModelFactory_ConvertFrom_Client_ModelIsNull_NullReturned()
        {
            Client clientModel = null;

            ClientDetails clientDto = ModelFactory.ConvertFrom(clientModel);

            clientDto.ShouldBeNull();
        }

        [Fact]
        public void ModelFactory_ConvertFrom_ClientList_ListIsEmpty_NullReturned()
        {
            List<Client> clientModelList = new();

            List<ClientDetails> clientDtoList = ModelFactory.ConvertFrom(clientModelList);

            clientDtoList.ShouldBeEmpty();
        }

        [Fact]
        public void ModelFactory_ConvertFrom_ClientList_ListIsNull_NullReturned()
        {
            List<Client> clientModelList = null;

            List<ClientDetails> clientDtoList = ModelFactory.ConvertFrom(clientModelList);

            clientDtoList.ShouldBeEmpty();
        }

        [Fact]
        public void ModelFactory_ConvertFrom_ClientList_ModelsConverted()
        {
            Client clientModel = new Client
                                 {
                                     ClientId = TestData.ClientId,
                                     ClientName = TestData.ClientName,
                                     Description = TestData.ClientDescription,
                                     AllowedGrantTypes = TestData.AllowedGrantTypes,
                                     AllowedScopes = TestData.AllowedScopes,
                                     Enabled = true
                                 };
            List<Client> clientModelList = [
                clientModel
            ];

            List<ClientDetails> clientDtoList = ModelFactory.ConvertFrom(clientModelList);

            clientDtoList.ShouldNotBeNull();
            clientDtoList.ShouldNotBeEmpty();
            clientDtoList.Count.ShouldBe(clientModelList.Count);
            clientDtoList.First().AllowedGrantTypes.ShouldBe(TestData.AllowedGrantTypes);
            clientDtoList.First().AllowedScopes.ShouldBe(TestData.AllowedScopes);
            clientDtoList.First().ClientDescription.ShouldBe(TestData.ClientDescription);
            clientDtoList.First().ClientId.ShouldBe(TestData.ClientId);
            clientDtoList.First().ClientName.ShouldBe(TestData.ClientName);
            clientDtoList.First().Enabled.ShouldBeTrue();
        }

        [Fact]
        public void ModelFactory_ConvertFrom_IdentityResource_ModelConverted()
        {
            IdentityResource identityResourceModel = new IdentityResource
                                                     {
                                                         Description = TestData.IdentityResourceDescription,
                                                         DisplayName = TestData.IdentityResourceDisplayName,
                                                         UserClaims = TestData.IdentityResourceUserClaims,
                                                         Emphasize = true,
                                                         ShowInDiscoveryDocument = true,
                                                         Required = true
                                                     };

            IdentityResourceDetails identityResourceDto = ModelFactory.ConvertFrom(identityResourceModel);

            identityResourceDto.Description.ShouldBe(TestData.IdentityResourceDescription);
            identityResourceDto.DisplayName.ShouldBe(TestData.IdentityResourceDisplayName);
            identityResourceDto.Claims.ShouldBe(TestData.IdentityResourceUserClaims);
            identityResourceDto.Emphasize.ShouldBeTrue();
            identityResourceDto.ShowInDiscoveryDocument.ShouldBeTrue();
            identityResourceDto.Required.ShouldBeTrue();
        }

        [Fact]
        public void ModelFactory_ConvertFrom_IdentityResource_ModelIsNull_NullReturned()
        {
            IdentityResource identityResourceModel = null;

            IdentityResourceDetails identityResourceDto = ModelFactory.ConvertFrom(identityResourceModel);

            identityResourceDto.ShouldBeNull();
        }

        [Fact]
        public void ModelFactory_ConvertFrom_IdentityResourceList_ListIsNull_NullReturned()
        {
            List<IdentityResource> identityResourceList = null;

            List<IdentityResourceDetails> identityResourceDtoList = ModelFactory.ConvertFrom(identityResourceList);

            identityResourceDtoList.ShouldBeEmpty();
        }

        [Fact]
        public void ModelFactory_ConvertFrom_IdentityResourceList_ModelConverted()
        {
            IdentityResource identityResourceModel = new IdentityResource
                                                     {
                                                         Description = TestData.IdentityResourceDescription,
                                                         DisplayName = TestData.IdentityResourceDisplayName,
                                                         UserClaims = TestData.IdentityResourceUserClaims,
                                                         Emphasize = true,
                                                         ShowInDiscoveryDocument = true,
                                                         Required = true
                                                     };

            List<IdentityResource> identityResourceModelList = [
                identityResourceModel
            ];

            List<IdentityResourceDetails> identityResourceDtoList = ModelFactory.ConvertFrom(identityResourceModelList);

            identityResourceDtoList.ShouldNotBeNull();
            identityResourceDtoList.ShouldNotBeEmpty();
            identityResourceDtoList.Count.ShouldBe(identityResourceModelList.Count);
            identityResourceDtoList.First().Description.ShouldBe(TestData.IdentityResourceDescription);
            identityResourceDtoList.First().DisplayName.ShouldBe(TestData.IdentityResourceDisplayName);
            identityResourceDtoList.First().Claims.ShouldBe(TestData.IdentityResourceUserClaims);
            identityResourceDtoList.First().Emphasize.ShouldBeTrue();
            identityResourceDtoList.First().ShowInDiscoveryDocument.ShouldBeTrue();
            identityResourceDtoList.First().Required.ShouldBeTrue();
        }

        [Fact]
        public void ModelFactory_ConvertFrom_ListRoleDetails_ListIsEmpty_NullReturned()
        {
            List<RoleDetails> roleDetailsModelList = new();

            List<DataTransferObjects.Responses.RoleDetails> roleDetailsDtoList = ModelFactory.ConvertFrom(roleDetailsModelList);

            roleDetailsDtoList.ShouldBeEmpty();
        }

        [Fact]
        public void ModelFactory_ConvertFrom_ListRoleDetails_ListIsNull_NullReturned()
        {
            List<RoleDetails> roleDetailsModelList = null;

            List<DataTransferObjects.Responses.RoleDetails> roleDetailsDtoList = ModelFactory.ConvertFrom(roleDetailsModelList);

            roleDetailsDtoList.ShouldBeEmpty();
        }

        [Fact]
        public void ModelFactory_ConvertFrom_ListRoleDetails_ModelsConverted()
        {
            RoleDetails roleDetailsModel = new RoleDetails
                                           {
                                               RoleId = Guid.Parse(TestData.Role1Id),
                                               RoleName = TestData.RoleName
                                           };
            List<RoleDetails> roleDetailsModelList = [
                roleDetailsModel
            ];

            List<DataTransferObjects.Responses.RoleDetails> rolesDetailsDtoList = ModelFactory.ConvertFrom(roleDetailsModelList);

            rolesDetailsDtoList.ShouldNotBeNull();
            rolesDetailsDtoList.ShouldNotBeEmpty();
            rolesDetailsDtoList.Count.ShouldBe(roleDetailsModelList.Count);
            rolesDetailsDtoList.First().RoleId.ShouldBe(Guid.Parse(TestData.Role1Id));
            rolesDetailsDtoList.First().RoleName.ShouldBe(TestData.RoleName);
        }

        [Fact]
        public void ModelFactory_ConvertFrom_ListUserDetails_ListIsEmpty_NullReturned()
        {
            List<UserDetails> userDetailsModelList = new();

            List<DataTransferObjects.Responses.UserDetails> userDetailsDtoList = ModelFactory.ConvertFrom(userDetailsModelList);

            userDetailsDtoList.ShouldNotBeNull();
            userDetailsDtoList.ShouldBeEmpty();
        }

        [Fact]
        public void ModelFactory_ConvertFrom_ListUserDetails_ListIsNull_NullReturned()
        {
            List<UserDetails> userDetailsModelList = null;

            List<DataTransferObjects.Responses.UserDetails> userDetailsDtoList = ModelFactory.ConvertFrom(userDetailsModelList);

            userDetailsDtoList.ShouldNotBeNull();
            userDetailsDtoList.ShouldBeEmpty();
        }

        [Fact]
        public void ModelFactory_ConvertFrom_ListUserDetails_ModelsConverted()
        {
            UserDetails userDetailsModel = new UserDetails
                                           {
                                               PhoneNumber = TestData.PhoneNumber,
                                               Username = TestData.UserName,
                                               Roles = TestData.Roles,
                                               UserId = Guid.Parse(TestData.User1Id),
                                               Email = TestData.EmailAddress,
                                               Claims = TestData.Claims,
                                               RegistrationDateTime = new DateTime(2025,2,1)
                                           };
            List<UserDetails> userDetailsModelList = [
                userDetailsModel
            ];

            List<DataTransferObjects.Responses.UserDetails> userDetailsDtoList = ModelFactory.ConvertFrom(userDetailsModelList);

            userDetailsDtoList.ShouldNotBeNull();
            userDetailsDtoList.ShouldNotBeEmpty();
            userDetailsDtoList.Count.ShouldBe(userDetailsModelList.Count);
            userDetailsDtoList.First().UserName.ShouldBe(TestData.UserName);
            userDetailsDtoList.First().EmailAddress.ShouldBe(TestData.EmailAddress);
            userDetailsDtoList.First().PhoneNumber.ShouldBe(TestData.PhoneNumber);
            userDetailsDtoList.First().UserId.ShouldBe(Guid.Parse(TestData.User1Id));
            userDetailsDtoList.First().Claims.ShouldBe(TestData.Claims);
            userDetailsDtoList.First().Roles.ShouldBe(TestData.Roles);
            userDetailsDtoList.First().RegistrationDateTime.ShouldBe(new DateTime(2025, 2, 1));
        }

        [Fact]
        public void ModelFactory_ConvertFrom_RoleDetails_ModelConverted()
        {
            RoleDetails roleDetailsModel = new RoleDetails
                                           {
                                               RoleId = Guid.Parse(TestData.Role1Id),
                                               RoleName = TestData.RoleName
                                           };

            DataTransferObjects.Responses.RoleDetails roleDetailsDto = ModelFactory.ConvertFrom(roleDetailsModel);

            roleDetailsDto.RoleId.ShouldBe(Guid.Parse(TestData.Role1Id));
            roleDetailsDto.RoleName.ShouldBe(TestData.RoleName);
        }

        [Fact]
        public void ModelFactory_ConvertFrom_RoleDetails_ModelIsNull_NullReturned()
        {
            RoleDetails roleDetailsModel = null;

            DataTransferObjects.Responses.RoleDetails roleDetailsDto = ModelFactory.ConvertFrom(roleDetailsModel);

            roleDetailsDto.ShouldBeNull();
        }

        [Fact]
        public void ModelFactory_ConvertFrom_UserDetails_ModelConverted()
        {
            UserDetails userDetailsModel = new UserDetails
                                           {
                                               PhoneNumber = TestData.PhoneNumber,
                                               Username = TestData.UserName,
                                               Roles = TestData.Roles,
                                               UserId = Guid.Parse(TestData.User1Id),
                                               Email = TestData.EmailAddress,
                                               Claims = TestData.Claims
                                           };

            DataTransferObjects.Responses.UserDetails userDetailsDto = ModelFactory.ConvertFrom(userDetailsModel);

            userDetailsDto.UserName.ShouldBe(TestData.UserName);
            userDetailsDto.EmailAddress.ShouldBe(TestData.EmailAddress);
            userDetailsDto.PhoneNumber.ShouldBe(TestData.PhoneNumber);
            userDetailsDto.UserId.ShouldBe(Guid.Parse(TestData.User1Id));
            userDetailsDto.Claims.ShouldBe(TestData.Claims);
            userDetailsDto.Roles.ShouldBe(TestData.Roles);
        }

        [Fact]
        public void ModelFactory_ConvertFrom_UserDetails_ModelIsNull_NullReturned()
        {
            UserDetails userDetailsModel = null;

            DataTransferObjects.Responses.UserDetails userDetailsDto = ModelFactory.ConvertFrom(userDetailsModel);

            userDetailsDto.ShouldBeNull();
        }

        #endregion
    }
}