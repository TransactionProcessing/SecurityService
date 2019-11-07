using System;
using System.Collections.Generic;
using System.Text;

namespace SecurityService.UnitTests
{
    using System.Linq;
    using DataTransferObjects.Responses;
    using Factories;
    using IdentityServer4.Models;
    using Shouldly;
    using Xunit;
    using UserDetails = Models.UserDetails;

    public class ModelFactoryTests
    {
        [Fact]
        public void ModelFactory_ConvertFrom_UserDetails_ModelConverted()
        {
            IModelFactory modelFactory = new ModelFactory();

            UserDetails userDetailsModel = new UserDetails
                                           {
                                               PhoneNumber = SecurityServiceManagerTestData.PhoneNumber,
                                               UserName = SecurityServiceManagerTestData.UserName,
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

        [Fact]
        public void ModelFactory_ConvertFrom_ListUserDetails_ModelsConverted()
        {
            IModelFactory modelFactory = new ModelFactory();

            UserDetails userDetailsModel = new UserDetails
                                           {
                                               PhoneNumber = SecurityServiceManagerTestData.PhoneNumber,
                                               UserName = SecurityServiceManagerTestData.UserName,
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
        public void ModelFactory_ConvertFrom_ListUserDetails_ListIsNull_NullReturned()
        {
            IModelFactory modelFactory = new ModelFactory();

            List<UserDetails> userDetailsModelList = null;

            List<DataTransferObjects.Responses.UserDetails> userDetailsDtoList = modelFactory.ConvertFrom(userDetailsModelList);

            userDetailsDtoList.ShouldBeNull();
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
        public void ModelFactory_ConvertFrom_ClientList_ListIsNull_NullReturned()
        {
            IModelFactory modelFactory = new ModelFactory();

            List<Client> clientModelList = null;

            List<ClientDetails> clientDtoList = modelFactory.ConvertFrom(clientModelList);

            clientDtoList.ShouldBeNull();
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
        public void ModelFactory_ConvertFrom_ApiResource_ModelConverted()
        {
            IModelFactory modelFactory = new ModelFactory();

            ApiResource apiResourceModel = new ApiResource
            {
                                     Scopes = new List<Scope>
                                              {
                                                  new Scope(SecurityServiceManagerTestData.AllowedScopes.First())
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
        public void ModelFactory_ConvertFrom_ApiResourceList_ModelsConverted()
        {
            IModelFactory modelFactory = new ModelFactory();

            ApiResource apiResourceModel = new ApiResource
                                           {
                                               Scopes = new List<Scope>
                                                        {
                                                            new Scope(SecurityServiceManagerTestData.AllowedScopes.First())
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
        public void ModelFactory_ConvertFrom_ApiResourceList_ListIsNull_NullReturned()
        {
            IModelFactory modelFactory = new ModelFactory();

            List<ApiResource> apiResourceList = null;

            List<ApiResourceDetails> apiResourceDtoList = modelFactory.ConvertFrom(apiResourceList);

            apiResourceDtoList.ShouldBeNull();
        }

        [Fact]
        public void ModelFactory_ConvertFrom_ApiResourceList_ListIsEmpty_NullReturned()
        {
            IModelFactory modelFactory = new ModelFactory();

            List<ApiResource> apiResourceList = new List<ApiResource>();

            List<ApiResourceDetails> apiResourceDtoList = modelFactory.ConvertFrom(apiResourceList);

            apiResourceDtoList.ShouldBeNull();
        }
    }
}
