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
            IModelFactory modelFactory = new ModelFactory();

            ApiResource apiResourceModel = new ApiResource
                                           {
                                               Scopes = new List<String>
                                                        {
                                                            SecurityServiceManagerTestData.AllowedScopes.First()
                                                        },
                                               Description = SecurityServiceManagerTestData.ApiResourceDescription,
                                               Name = SecurityServiceManagerTestData.ApiResourceName,
                                               DisplayName = SecurityServiceManagerTestData.ApiResourceDisplayName,
                                               UserClaims = SecurityServiceManagerTestData.ApiResourceUserClaims,
                                               Enabled = true
                                           };

            ApiResourceDetails apiResourceDto = modelFactory.ConvertFrom(apiResourceModel);

            apiResourceDto.Scopes.First().ShouldBe(SecurityServiceManagerTestData.AllowedScopes.First());
            apiResourceDto.Description.ShouldBe(SecurityServiceManagerTestData.ApiResourceDescription);
            apiResourceDto.DisplayName.ShouldBe(SecurityServiceManagerTestData.ApiResourceDisplayName);
            apiResourceDto.Name.ShouldBe(SecurityServiceManagerTestData.ApiResourceName);
            apiResourceDto.UserClaims.ShouldBe(SecurityServiceManagerTestData.ApiResourceUserClaims);
            apiResourceDto.Enabled.ShouldBeTrue();
        }

        [Fact]
        public void ModelFactory_ConvertFrom_ApiResource_ModelIsNull_NullReturned()
        {
            IModelFactory modelFactory = new ModelFactory();

            ApiResource apiResourceModel = null;

            ApiResourceDetails apiResourceDto = modelFactory.ConvertFrom(apiResourceModel);

            apiResourceDto.ShouldBeNull();
        }

        [Fact]
        public void ModelFactory_ConvertFrom_ApiScope_ModelConverted()
        {
            IModelFactory modelFactory = new ModelFactory();

            ApiScope apiScopeModel = new ApiScope
            {
                Description = SecurityServiceManagerTestData.ApiScopeDescription,
                Name = SecurityServiceManagerTestData.ApiScopeName,
                DisplayName = SecurityServiceManagerTestData.ApiScopeDisplayName,
                Enabled = true
            };

            ApiScopeDetails apiScopeDto = modelFactory.ConvertFrom(apiScopeModel);

            apiScopeDto.Description.ShouldBe(SecurityServiceManagerTestData.ApiScopeDescription);
            apiScopeDto.DisplayName.ShouldBe(SecurityServiceManagerTestData.ApiScopeDisplayName);
            apiScopeDto.Name.ShouldBe(SecurityServiceManagerTestData.ApiScopeName);
            apiScopeDto.Enabled.ShouldBeTrue();
        }

        [Fact]
        public void ModelFactory_ConvertFrom_ApiScope_ModelIsNull_NullReturned()
        {
            IModelFactory modelFactory = new ModelFactory();

            ApiScope apiScopeModel = null;

            ApiScopeDetails apiScopeDto = modelFactory.ConvertFrom(apiScopeModel);

            apiScopeDto.ShouldBeNull();
        }

        [Fact]
        public void ModelFactory_ConvertFrom_ApiResourceList_ListIsEmpty_NullReturned()
        {
            IModelFactory modelFactory = new ModelFactory();

            List<ApiResource> apiResourceList = new List<ApiResource>();

            List<ApiResourceDetails> apiResourceDtoList = modelFactory.ConvertFrom(apiResourceList);

            apiResourceDtoList.ShouldBeNull();
        }

        [Fact]
        public void ModelFactory_ConvertFrom_ApiResourceList_ListIsNull_NullReturned()
        {
            IModelFactory modelFactory = new ModelFactory();

            List<ApiResource> apiResourceList = null;

            List<ApiResourceDetails> apiResourceDtoList = modelFactory.ConvertFrom(apiResourceList);

            apiResourceDtoList.ShouldBeNull();
        }

        [Fact]
        public void ModelFactory_ConvertFrom_ApiResourceList_ModelsConverted()
        {
            IModelFactory modelFactory = new ModelFactory();

            ApiResource apiResourceModel = new ApiResource
                                           {
                                               Scopes = new List<String>
                                                        {
                                                            SecurityServiceManagerTestData.AllowedScopes.First()
                                                        },
                                               Description = SecurityServiceManagerTestData.ApiResourceDescription,
                                               Name = SecurityServiceManagerTestData.ApiResourceName,
                                               DisplayName = SecurityServiceManagerTestData.ApiResourceDisplayName,
                                               UserClaims = SecurityServiceManagerTestData.ApiResourceUserClaims,
                                               Enabled = true
                                           };

            List<ApiResource> apiResourceModelList = new List<ApiResource>();
            apiResourceModelList.Add(apiResourceModel);

            List<ApiResourceDetails> apiResourceDtoList = modelFactory.ConvertFrom(apiResourceModelList);

            apiResourceDtoList.ShouldNotBeNull();
            apiResourceDtoList.ShouldNotBeEmpty();
            apiResourceDtoList.Count.ShouldBe(apiResourceModelList.Count);
            apiResourceDtoList.First().Scopes.First().ShouldBe(SecurityServiceManagerTestData.AllowedScopes.First());
            apiResourceDtoList.First().Description.ShouldBe(SecurityServiceManagerTestData.ApiResourceDescription);
            apiResourceDtoList.First().DisplayName.ShouldBe(SecurityServiceManagerTestData.ApiResourceDisplayName);
            apiResourceDtoList.First().Name.ShouldBe(SecurityServiceManagerTestData.ApiResourceName);
            apiResourceDtoList.First().UserClaims.ShouldBe(SecurityServiceManagerTestData.ApiResourceUserClaims);
            apiResourceDtoList.First().Enabled.ShouldBeTrue();
        }

        [Fact]
        public void ModelFactory_ConvertFrom_ApiScopeList_ListIsEmpty_NullReturned()
        {
            IModelFactory modelFactory = new ModelFactory();

            List<ApiScope> apiScopeList = new List<ApiScope>();

            List<ApiScopeDetails> apiScopeDtoList = modelFactory.ConvertFrom(apiScopeList);

            apiScopeDtoList.ShouldBeNull();
        }

        [Fact]
        public void ModelFactory_ConvertFrom_ApiScopeList_ListIsNull_NullReturned()
        {
            IModelFactory modelFactory = new ModelFactory();

            List<ApiScope> apiScopeList = null;

            List<ApiScopeDetails> apiScopeDtoList = modelFactory.ConvertFrom(apiScopeList);

            apiScopeDtoList.ShouldBeNull();
        }

        [Fact]
        public void ModelFactory_ConvertFrom_ApiScopeList_ModelsConverted()
        {
            IModelFactory modelFactory = new ModelFactory();

            ApiScope apiScopeModel = new ApiScope
            {
                Description = SecurityServiceManagerTestData.ApiScopeDescription,
                Name = SecurityServiceManagerTestData.ApiScopeName,
                DisplayName = SecurityServiceManagerTestData.ApiResourceDisplayName,
                Enabled = true
            };

            List<ApiScope> apiScopeModelList = new List<ApiScope>();
            apiScopeModelList.Add(apiScopeModel);

            List<ApiScopeDetails> apiScopeDtoList = modelFactory.ConvertFrom(apiScopeModelList);

            apiScopeDtoList.ShouldNotBeNull();
            apiScopeDtoList.ShouldNotBeEmpty();
            apiScopeDtoList.Count.ShouldBe(apiScopeModelList.Count);
            apiScopeDtoList.First().Description.ShouldBe(SecurityServiceManagerTestData.ApiScopeDescription);
            apiScopeDtoList.First().DisplayName.ShouldBe(SecurityServiceManagerTestData.ApiResourceDisplayName);
            apiScopeDtoList.First().Name.ShouldBe(SecurityServiceManagerTestData.ApiScopeName);
            apiScopeDtoList.First().Enabled.ShouldBeTrue();
        }

        [Fact]
        public void ModelFactory_ConvertFrom_Client_ModelConverted()
        {
            IModelFactory modelFactory = new ModelFactory();

            Client clientModel = new Client
                                 {
                                     ClientId = SecurityServiceManagerTestData.ClientId,
                                     ClientName = SecurityServiceManagerTestData.ClientName,
                                     Description = SecurityServiceManagerTestData.ClientDescription,
                                     AllowedGrantTypes = SecurityServiceManagerTestData.AllowedGrantTypes,
                                     AllowedScopes = SecurityServiceManagerTestData.AllowedScopes,
                                     Enabled = true
                                 };

            ClientDetails clientDto = modelFactory.ConvertFrom(clientModel);

            clientDto.AllowedGrantTypes.ShouldBe(SecurityServiceManagerTestData.AllowedGrantTypes);
            clientDto.AllowedScopes.ShouldBe(SecurityServiceManagerTestData.AllowedScopes);
            clientDto.ClientDescription.ShouldBe(SecurityServiceManagerTestData.ClientDescription);
            clientDto.ClientId.ShouldBe(SecurityServiceManagerTestData.ClientId);
            clientDto.ClientName.ShouldBe(SecurityServiceManagerTestData.ClientName);
            clientDto.Enabled.ShouldBeTrue();
        }

        [Fact]
        public void ModelFactory_ConvertFrom_Client_ModelIsNull_NullReturned()
        {
            IModelFactory modelFactory = new ModelFactory();

            Client clientModel = null;

            ClientDetails clientDto = modelFactory.ConvertFrom(clientModel);

            clientDto.ShouldBeNull();
        }

        [Fact]
        public void ModelFactory_ConvertFrom_ClientList_ListIsEmpty_NullReturned()
        {
            IModelFactory modelFactory = new ModelFactory();

            List<Client> clientModelList = new List<Client>();

            List<ClientDetails> clientDtoList = modelFactory.ConvertFrom(clientModelList);

            clientDtoList.ShouldBeNull();
        }

        [Fact]
        public void ModelFactory_ConvertFrom_ClientList_ListIsNull_NullReturned()
        {
            IModelFactory modelFactory = new ModelFactory();

            List<Client> clientModelList = null;

            List<ClientDetails> clientDtoList = modelFactory.ConvertFrom(clientModelList);

            clientDtoList.ShouldBeNull();
        }

        [Fact]
        public void ModelFactory_ConvertFrom_ClientList_ModelsConverted()
        {
            IModelFactory modelFactory = new ModelFactory();

            Client clientModel = new Client
                                 {
                                     ClientId = SecurityServiceManagerTestData.ClientId,
                                     ClientName = SecurityServiceManagerTestData.ClientName,
                                     Description = SecurityServiceManagerTestData.ClientDescription,
                                     AllowedGrantTypes = SecurityServiceManagerTestData.AllowedGrantTypes,
                                     AllowedScopes = SecurityServiceManagerTestData.AllowedScopes,
                                     Enabled = true
                                 };
            List<Client> clientModelList = new List<Client>();
            clientModelList.Add(clientModel);

            List<ClientDetails> clientDtoList = modelFactory.ConvertFrom(clientModelList);

            clientDtoList.ShouldNotBeNull();
            clientDtoList.ShouldNotBeEmpty();
            clientDtoList.Count.ShouldBe(clientModelList.Count);
            clientDtoList.First().AllowedGrantTypes.ShouldBe(SecurityServiceManagerTestData.AllowedGrantTypes);
            clientDtoList.First().AllowedScopes.ShouldBe(SecurityServiceManagerTestData.AllowedScopes);
            clientDtoList.First().ClientDescription.ShouldBe(SecurityServiceManagerTestData.ClientDescription);
            clientDtoList.First().ClientId.ShouldBe(SecurityServiceManagerTestData.ClientId);
            clientDtoList.First().ClientName.ShouldBe(SecurityServiceManagerTestData.ClientName);
            clientDtoList.First().Enabled.ShouldBeTrue();
        }

        [Fact]
        public void ModelFactory_ConvertFrom_IdentityResource_ModelConverted()
        {
            IModelFactory modelFactory = new ModelFactory();

            IdentityResource identityResourceModel = new IdentityResource
                                                     {
                                                         Description = SecurityServiceManagerTestData.IdentityResourceDescription,
                                                         DisplayName = SecurityServiceManagerTestData.IdentityResourceDisplayName,
                                                         UserClaims = SecurityServiceManagerTestData.IdentityResourceUserClaims,
                                                         Emphasize = true,
                                                         ShowInDiscoveryDocument = true,
                                                         Required = true
                                                     };

            IdentityResourceDetails identityResourceDto = modelFactory.ConvertFrom(identityResourceModel);

            identityResourceDto.Description.ShouldBe(SecurityServiceManagerTestData.IdentityResourceDescription);
            identityResourceDto.DisplayName.ShouldBe(SecurityServiceManagerTestData.IdentityResourceDisplayName);
            identityResourceDto.Claims.ShouldBe(SecurityServiceManagerTestData.IdentityResourceUserClaims);
            identityResourceDto.Emphasize.ShouldBeTrue();
            identityResourceDto.ShowInDiscoveryDocument.ShouldBeTrue();
            identityResourceDto.Required.ShouldBeTrue();
        }

        [Fact]
        public void ModelFactory_ConvertFrom_IdentityResource_ModelIsNull_NullReturned()
        {
            IModelFactory modelFactory = new ModelFactory();

            IdentityResource identityResourceModel = null;

            IdentityResourceDetails identityResourceDto = modelFactory.ConvertFrom(identityResourceModel);

            identityResourceDto.ShouldBeNull();
        }

        [Fact]
        public void ModelFactory_ConvertFrom_IdentityResourceList_ListIsNull_NullReturned()
        {
            IModelFactory modelFactory = new ModelFactory();

            List<IdentityResource> identityResourceList = null;

            List<IdentityResourceDetails> identityResourceDtoList = modelFactory.ConvertFrom(identityResourceList);

            identityResourceDtoList.ShouldBeNull();
        }

        [Fact]
        public void ModelFactory_ConvertFrom_IdentityResourceList_ModelConverted()
        {
            IModelFactory modelFactory = new ModelFactory();

            IdentityResource identityResourceModel = new IdentityResource
                                                     {
                                                         Description = SecurityServiceManagerTestData.IdentityResourceDescription,
                                                         DisplayName = SecurityServiceManagerTestData.IdentityResourceDisplayName,
                                                         UserClaims = SecurityServiceManagerTestData.IdentityResourceUserClaims,
                                                         Emphasize = true,
                                                         ShowInDiscoveryDocument = true,
                                                         Required = true
                                                     };

            List<IdentityResource> identityResourceModelList = new List<IdentityResource>();
            identityResourceModelList.Add(identityResourceModel);

            List<IdentityResourceDetails> identityResourceDtoList = modelFactory.ConvertFrom(identityResourceModelList);

            identityResourceDtoList.ShouldNotBeNull();
            identityResourceDtoList.ShouldNotBeEmpty();
            identityResourceDtoList.Count.ShouldBe(identityResourceModelList.Count);
            identityResourceDtoList.First().Description.ShouldBe(SecurityServiceManagerTestData.IdentityResourceDescription);
            identityResourceDtoList.First().DisplayName.ShouldBe(SecurityServiceManagerTestData.IdentityResourceDisplayName);
            identityResourceDtoList.First().Claims.ShouldBe(SecurityServiceManagerTestData.IdentityResourceUserClaims);
            identityResourceDtoList.First().Emphasize.ShouldBeTrue();
            identityResourceDtoList.First().ShowInDiscoveryDocument.ShouldBeTrue();
            identityResourceDtoList.First().Required.ShouldBeTrue();
        }

        [Fact]
        public void ModelFactory_ConvertFrom_ListRoleDetails_ListIsEmpty_NullReturned()
        {
            IModelFactory modelFactory = new ModelFactory();

            List<RoleDetails> roleDetailsModelList = new List<RoleDetails>();

            List<DataTransferObjects.Responses.RoleDetails> roleDetailsDtoList = modelFactory.ConvertFrom(roleDetailsModelList);

            roleDetailsDtoList.ShouldBeNull();
        }

        [Fact]
        public void ModelFactory_ConvertFrom_ListRoleDetails_ListIsNull_NullReturned()
        {
            IModelFactory modelFactory = new ModelFactory();

            List<RoleDetails> roleDetailsModelList = null;

            List<DataTransferObjects.Responses.RoleDetails> roleDetailsDtoList = modelFactory.ConvertFrom(roleDetailsModelList);

            roleDetailsDtoList.ShouldBeNull();
        }

        [Fact]
        public void ModelFactory_ConvertFrom_ListRoleDetails_ModelsConverted()
        {
            IModelFactory modelFactory = new ModelFactory();

            RoleDetails roleDetailsModel = new RoleDetails
                                           {
                                               RoleId = Guid.Parse(SecurityServiceManagerTestData.Role1Id),
                                               RoleName = SecurityServiceManagerTestData.RoleName
                                           };
            List<RoleDetails> roleDetailsModelList = new List<RoleDetails>();
            roleDetailsModelList.Add(roleDetailsModel);

            List<DataTransferObjects.Responses.RoleDetails> rolesDetailsDtoList = modelFactory.ConvertFrom(roleDetailsModelList);

            rolesDetailsDtoList.ShouldNotBeNull();
            rolesDetailsDtoList.ShouldNotBeEmpty();
            rolesDetailsDtoList.Count.ShouldBe(roleDetailsModelList.Count);
            rolesDetailsDtoList.First().RoleId.ShouldBe(Guid.Parse(SecurityServiceManagerTestData.Role1Id));
            rolesDetailsDtoList.First().RoleName.ShouldBe(SecurityServiceManagerTestData.RoleName);
        }

        [Fact]
        public void ModelFactory_ConvertFrom_ListUserDetails_ListIsEmpty_NullReturned()
        {
            IModelFactory modelFactory = new ModelFactory();

            List<UserDetails> userDetailsModelList = new List<UserDetails>();

            List<DataTransferObjects.Responses.UserDetails> userDetailsDtoList = modelFactory.ConvertFrom(userDetailsModelList);

            userDetailsDtoList.ShouldBeNull();
        }

        [Fact]
        public void ModelFactory_ConvertFrom_ListUserDetails_ListIsNull_NullReturned()
        {
            IModelFactory modelFactory = new ModelFactory();

            List<UserDetails> userDetailsModelList = null;

            List<DataTransferObjects.Responses.UserDetails> userDetailsDtoList = modelFactory.ConvertFrom(userDetailsModelList);

            userDetailsDtoList.ShouldBeNull();
        }

        [Fact]
        public void ModelFactory_ConvertFrom_ListUserDetails_ModelsConverted()
        {
            IModelFactory modelFactory = new ModelFactory();

            UserDetails userDetailsModel = new UserDetails
                                           {
                                               PhoneNumber = SecurityServiceManagerTestData.PhoneNumber,
                                               Username = SecurityServiceManagerTestData.UserName,
                                               Roles = SecurityServiceManagerTestData.Roles,
                                               UserId = Guid.Parse(SecurityServiceManagerTestData.User1Id),
                                               Email = SecurityServiceManagerTestData.EmailAddress,
                                               Claims = SecurityServiceManagerTestData.Claims
                                           };
            List<UserDetails> userDetailsModelList = new List<UserDetails>();
            userDetailsModelList.Add(userDetailsModel);

            List<DataTransferObjects.Responses.UserDetails> userDetailsDtoList = modelFactory.ConvertFrom(userDetailsModelList);

            userDetailsDtoList.ShouldNotBeNull();
            userDetailsDtoList.ShouldNotBeEmpty();
            userDetailsDtoList.Count.ShouldBe(userDetailsModelList.Count);
            userDetailsDtoList.First().UserName.ShouldBe(SecurityServiceManagerTestData.UserName);
            userDetailsDtoList.First().EmailAddress.ShouldBe(SecurityServiceManagerTestData.EmailAddress);
            userDetailsDtoList.First().PhoneNumber.ShouldBe(SecurityServiceManagerTestData.PhoneNumber);
            userDetailsDtoList.First().UserId.ShouldBe(Guid.Parse(SecurityServiceManagerTestData.User1Id));
            userDetailsDtoList.First().Claims.ShouldBe(SecurityServiceManagerTestData.Claims);
            userDetailsDtoList.First().Roles.ShouldBe(SecurityServiceManagerTestData.Roles);
        }

        [Fact]
        public void ModelFactory_ConvertFrom_RoleDetails_ModelConverted()
        {
            IModelFactory modelFactory = new ModelFactory();

            RoleDetails roleDetailsModel = new RoleDetails
                                           {
                                               RoleId = Guid.Parse(SecurityServiceManagerTestData.Role1Id),
                                               RoleName = SecurityServiceManagerTestData.RoleName
                                           };

            DataTransferObjects.Responses.RoleDetails roleDetailsDto = modelFactory.ConvertFrom(roleDetailsModel);

            roleDetailsDto.RoleId.ShouldBe(Guid.Parse(SecurityServiceManagerTestData.Role1Id));
            roleDetailsDto.RoleName.ShouldBe(SecurityServiceManagerTestData.RoleName);
        }

        [Fact]
        public void ModelFactory_ConvertFrom_RoleDetails_ModelIsNull_NullReturned()
        {
            IModelFactory modelFactory = new ModelFactory();

            RoleDetails roleDetailsModel = null;

            DataTransferObjects.Responses.RoleDetails roleDetailsDto = modelFactory.ConvertFrom(roleDetailsModel);

            roleDetailsDto.ShouldBeNull();
        }

        [Fact]
        public void ModelFactory_ConvertFrom_UserDetails_ModelConverted()
        {
            IModelFactory modelFactory = new ModelFactory();

            UserDetails userDetailsModel = new UserDetails
                                           {
                                               PhoneNumber = SecurityServiceManagerTestData.PhoneNumber,
                                               Username = SecurityServiceManagerTestData.UserName,
                                               Roles = SecurityServiceManagerTestData.Roles,
                                               UserId = Guid.Parse(SecurityServiceManagerTestData.User1Id),
                                               Email = SecurityServiceManagerTestData.EmailAddress,
                                               Claims = SecurityServiceManagerTestData.Claims
                                           };

            DataTransferObjects.Responses.UserDetails userDetailsDto = modelFactory.ConvertFrom(userDetailsModel);

            userDetailsDto.UserName.ShouldBe(SecurityServiceManagerTestData.UserName);
            userDetailsDto.EmailAddress.ShouldBe(SecurityServiceManagerTestData.EmailAddress);
            userDetailsDto.PhoneNumber.ShouldBe(SecurityServiceManagerTestData.PhoneNumber);
            userDetailsDto.UserId.ShouldBe(Guid.Parse(SecurityServiceManagerTestData.User1Id));
            userDetailsDto.Claims.ShouldBe(SecurityServiceManagerTestData.Claims);
            userDetailsDto.Roles.ShouldBe(SecurityServiceManagerTestData.Roles);
        }

        [Fact]
        public void ModelFactory_ConvertFrom_UserDetails_ModelIsNull_NullReturned()
        {
            IModelFactory modelFactory = new ModelFactory();

            UserDetails userDetailsModel = null;

            DataTransferObjects.Responses.UserDetails userDetailsDto = modelFactory.ConvertFrom(userDetailsModel);

            userDetailsDto.ShouldBeNull();
        }

        #endregion
    }
}