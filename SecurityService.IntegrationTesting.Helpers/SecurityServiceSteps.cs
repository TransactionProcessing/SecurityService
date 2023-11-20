namespace SecurityService.IntegrationTesting.Helpers;

using Client;
using DataTransferObjects;
using DataTransferObjects.Requests;
using DataTransferObjects.Responses;
using Shared.IntegrationTesting;
using Shouldly;
using System.Collections.Generic;
using System.Threading;
using TechTalk.SpecFlow;

public class SecurityServiceSteps{
    private readonly ISecurityServiceClient SecurityServiceClient;

    public SecurityServiceSteps(ISecurityServiceClient securityServiceClient){
        this.SecurityServiceClient = securityServiceClient;
    }

    private async Task<CreateApiScopeResponse> CreateApiScope(CreateApiScopeRequest createApiScopeRequest,
                                                              CancellationToken cancellationToken){
        CreateApiScopeResponse createApiScopeResponse = await this.SecurityServiceClient
                                                                  .CreateApiScope(createApiScopeRequest, cancellationToken).ConfigureAwait(false);
        return createApiScopeResponse;
    }

    public async Task GivenICreateTheFollowingApiScopes(List<CreateApiScopeRequest> createApiScopeRequests){
        foreach (CreateApiScopeRequest createApiScopeRequest in createApiScopeRequests){
            CreateApiScopeResponse createApiScopeResponse = await this.CreateApiScope(createApiScopeRequest, CancellationToken.None).ConfigureAwait(false);

            createApiScopeResponse.ShouldNotBeNull();
            createApiScopeResponse.ApiScopeName.ShouldBe(createApiScopeRequest.Name);

            // TODO: can do a get in here to verify really created...
        }
    }

    public async Task GivenTheFollowingApiResourcesExist(Table table){
        foreach (TableRow tableRow in table.Rows){
            String resourceName = SpecflowTableHelper.GetStringRowValue(tableRow, "ResourceName");
            String displayName = SpecflowTableHelper.GetStringRowValue(tableRow, "DisplayName");
            String secret = SpecflowTableHelper.GetStringRowValue(tableRow, "Secret");
            String scopes = SpecflowTableHelper.GetStringRowValue(tableRow, "Scopes");
            String userClaims = SpecflowTableHelper.GetStringRowValue(tableRow, "UserClaims");

            List<String> splitScopes = scopes.Split(",").ToList();
            List<String> splitUserClaims = userClaims.Split(",").ToList();

            CreateApiResourceRequest createApiResourceRequest = new CreateApiResourceRequest{
                                                                                                Description = String.Empty,
                                                                                                DisplayName = displayName,
                                                                                                Name = resourceName,
                                                                                                Scopes = new List<String>(),
                                                                                                Secret = secret,
                                                                                                UserClaims = new List<String>()
                                                                                            };
            splitScopes.ForEach(a => { createApiResourceRequest.Scopes.Add(a.Trim()); });
            splitUserClaims.ForEach(a => { createApiResourceRequest.UserClaims.Add(a.Trim()); });

            CreateApiResourceResponse createApiResourceResponse = await this.SecurityServiceClient.CreateApiResource(createApiResourceRequest, CancellationToken.None).ConfigureAwait(false);

            createApiResourceResponse.ApiResourceName.ShouldBe(resourceName);
        }
    }

    public async Task<List<(String clientId, String secret, List<String> allowedGrantTypes)>> GivenTheFollowingClientsExist(List<CreateClientRequest> createClientRequests){
        List<(String clientId, String secret, List<String> allowedGrantTypes)> clients = new List<(String clientId, String secret, List<String> allowedGrantTypes)>();
        foreach (CreateClientRequest createClientRequest in createClientRequests){
            CreateClientResponse createClientResponse = await this.SecurityServiceClient.CreateClient(createClientRequest, CancellationToken.None).ConfigureAwait(false);

            createClientResponse.ClientId.ShouldBe(createClientRequest.ClientId);

            // TODO: What do i do here....
            clients.Add((createClientResponse.ClientId, createClientRequest.Secret, createClientRequest.AllowedGrantTypes));
        }

        return clients;
    }

    public async Task GivenTheFollowingApiResourcesExist(List<CreateApiResourceRequest> requests){
        foreach (CreateApiResourceRequest createApiResourceRequest in requests){
            CreateApiResourceResponse createApiResourceResponse = await this.SecurityServiceClient.CreateApiResource(createApiResourceRequest, CancellationToken.None).ConfigureAwait(false);

            createApiResourceResponse.ApiResourceName.ShouldBe(createApiResourceRequest.Name);
        }
    }

    public async Task<String> GetClientToken(String clientId, String secret, CancellationToken cancellationToken){
        TokenResponse tokenResponse = await this.SecurityServiceClient.GetToken(clientId, secret, cancellationToken).ConfigureAwait(false);
        return tokenResponse.AccessToken;
    }

    public async Task<String> GetPasswordToken(String clientId, String secret, String userName, String password, CancellationToken cancellationToken)
    {
        TokenResponse tokenResponse = await this.SecurityServiceClient.GetToken(userName,password, clientId, secret, cancellationToken).ConfigureAwait(false);
        return tokenResponse.AccessToken;
    }

    public async Task WhenIGetTheApiResourcesApiResourceDetailsAreReturnedAsFollows(List<ApiResourceDetails> expectedDetails, CancellationToken cancellationToken){
        List<ApiResourceDetails> apiResourceDetailsList = await this.SecurityServiceClient.GetApiResources(cancellationToken).ConfigureAwait(false);

        foreach (ApiResourceDetails apiResourceDetails in expectedDetails){
            ApiResourceDetails? foundRecord = apiResourceDetailsList.SingleOrDefault(a => a.Name == apiResourceDetails.Name);
            foundRecord.ShouldNotBeNull();
            foundRecord.Description.ShouldBe(apiResourceDetails.Description);
            foundRecord.DisplayName.ShouldBe(apiResourceDetails.DisplayName);
            foreach (String? scope in apiResourceDetails.Scopes){
                foundRecord.Scopes.ShouldContain(scope);
            }

            foreach (String? userClaim in apiResourceDetails.UserClaims){
                foundRecord.UserClaims.ShouldContain(userClaim);
            }
        }
    }

    public async Task WhenIGetTheApiResourceWithNameTheApiResourceDetailsAreReturnedAsFollows(List<ApiResourceDetails> expectedDetails, String apiResourceName, CancellationToken cancellationToken){
        ApiResourceDetails apiResourceDetails = await this.SecurityServiceClient.GetApiResource(apiResourceName, cancellationToken).ConfigureAwait(false);
        ApiResourceDetails expectedRecord = expectedDetails.Single();

        apiResourceDetails.ShouldNotBeNull();
        apiResourceDetails.Description.ShouldBe(expectedRecord.Description);
        apiResourceDetails.DisplayName.ShouldBe(expectedRecord.DisplayName);
        foreach (String? scope in expectedRecord.Scopes){
            apiResourceDetails.Scopes.ShouldContain(scope);
        }

        foreach (String? userClaim in expectedRecord.UserClaims){
            apiResourceDetails.UserClaims.ShouldContain(userClaim);
        }
    }

    public async Task WhenIGetTheApiScopesApiScopeDetailsAreReturnedAsFollows(List<ApiScopeDetails> expectedDetails, CancellationToken cancellationToken){
        List<ApiScopeDetails>? apiScopeDetailsList = await this.SecurityServiceClient.GetApiScopes(cancellationToken).ConfigureAwait(false);

        foreach (ApiScopeDetails apiScopeDetails in expectedDetails){
            ApiScopeDetails? foundRecord = apiScopeDetailsList.SingleOrDefault(a => a.Name == apiScopeDetails.Name);
            foundRecord.ShouldNotBeNull();
            foundRecord.Description.ShouldBe(apiScopeDetails.Description);
            foundRecord.DisplayName.ShouldBe(apiScopeDetails.DisplayName);
        }
    }

    public async Task WhenIGetTheApiScopeWithNameTheApiScopeDetailsAreReturnedAsFollows(List<ApiScopeDetails> expectedDetails, String apiScopeName, CancellationToken cancellationToken){
        ApiScopeDetails? apiScopeDetails = await this.SecurityServiceClient.GetApiScope(apiScopeName, cancellationToken).ConfigureAwait(false);
        ApiScopeDetails expectedRecord = expectedDetails.Single();

        apiScopeDetails.ShouldNotBeNull();
        apiScopeDetails.Description.ShouldBe(expectedRecord.Description);
        apiScopeDetails.DisplayName.ShouldBe(expectedRecord.DisplayName);
    }

    public async Task WhenIGetTheClientWithClientIdTheClientDetailsAreReturnedAsFollows(List<ClientDetails> expectedDetails, String clientId, CancellationToken cancellationToken){
        ClientDetails clientDetails = await this.SecurityServiceClient.GetClient(clientId, CancellationToken.None).ConfigureAwait(false);
        ClientDetails expectedRecord = expectedDetails.Single();

        clientDetails.ShouldNotBeNull();
        clientDetails.ClientId.ShouldBe(expectedRecord.ClientId);
        clientDetails.ClientName.ShouldBe(expectedRecord.ClientName);
        clientDetails.ClientDescription.ShouldBe(expectedRecord.ClientDescription);
        foreach (String expectedRecordAllowedGrantType in expectedRecord.AllowedGrantTypes){
            clientDetails.AllowedGrantTypes.ShouldContain(expectedRecordAllowedGrantType);
        }

        foreach (String expectedRecordAllowedScope in expectedRecord.AllowedScopes){
            clientDetails.AllowedScopes.ShouldContain(expectedRecordAllowedScope);
        }
    }

    public async Task WhenIGetTheClientsClientsDetailsAreReturnedAsFollows(List<ClientDetails> expectedDetails, CancellationToken cancellationToken){
        List<ClientDetails>? clientDetailsList = await this.SecurityServiceClient.GetClients(CancellationToken.None).ConfigureAwait(false);

        foreach (ClientDetails expectedRecord in expectedDetails){
            ClientDetails? foundRecord = clientDetailsList.SingleOrDefault(a => a.ClientId == expectedRecord.ClientId);
            foundRecord.ShouldNotBeNull();

            foundRecord.ClientId.ShouldBe(expectedRecord.ClientId);
            foundRecord.ClientName.ShouldBe(expectedRecord.ClientName);
            foundRecord.ClientDescription.ShouldBe(expectedRecord.ClientDescription);
            foreach (String expectedRecordAllowedGrantType in expectedRecord.AllowedGrantTypes){
                foundRecord.AllowedGrantTypes.ShouldContain(expectedRecordAllowedGrantType);
            }

            foreach (String expectedRecordAllowedScope in expectedRecord.AllowedScopes){
                foundRecord.AllowedScopes.ShouldContain(expectedRecordAllowedScope);
            }
        }
    }

    public async Task<List<CreateIdentityResourceResponse>> GivenICreateTheFollowingIdentityResources(List<CreateIdentityResourceRequest> requests, CancellationToken cancellationToken){
        List<CreateIdentityResourceResponse> names = new List<CreateIdentityResourceResponse>();
        foreach (CreateIdentityResourceRequest createIdentityResourceRequest in requests){
            CreateIdentityResourceResponse? createIdentityResourceResponse = await this.SecurityServiceClient.CreateIdentityResource(createIdentityResourceRequest, cancellationToken).ConfigureAwait(false);

            createIdentityResourceResponse.ShouldNotBeNull();
            createIdentityResourceResponse.IdentityResourceName.ShouldNotBeNullOrEmpty();

            names.Add(createIdentityResourceResponse);
        }

        return names;
    }

    public async Task WhenIGetTheIdentityResourceWithNameTheIdentityResourceDetailsAreReturnedAsFollows(List<IdentityResourceDetails> expectedDetails, String identityResourceName, CancellationToken cancellationToken)
    {
        IdentityResourceDetails identityResourceDetails = await this.SecurityServiceClient.GetIdentityResource(identityResourceName, cancellationToken).ConfigureAwait(false);
        IdentityResourceDetails expectedRecord = expectedDetails.Single();

        identityResourceDetails.Name.ShouldBe(expectedRecord.Name);
        identityResourceDetails.Description.ShouldBe(expectedRecord.Description);
        identityResourceDetails.DisplayName.ShouldBe(expectedRecord.DisplayName);

        foreach (String expectedRecordClaim in expectedRecord.Claims){
            identityResourceDetails.Claims.ShouldContain(expectedRecordClaim);
        }
    }

    public async Task WhenIGetTheIdentityResourcesIdentityResourceDetailsAreReturnedAsFollows(List<IdentityResourceDetails> expectedDetails, CancellationToken cancellationToken){
        List<IdentityResourceDetails>? identityResourceDetailsList = await this.SecurityServiceClient.GetIdentityResources(CancellationToken.None).ConfigureAwait(false);

        foreach (IdentityResourceDetails expectedRecord in expectedDetails){
            IdentityResourceDetails? foundRecord = identityResourceDetailsList.SingleOrDefault(a => a.Name == expectedRecord.Name);
            foundRecord.ShouldNotBeNull();
            foundRecord.Name.ShouldBe(expectedRecord.Name);
            foundRecord.Description.ShouldBe(expectedRecord.Description);
            foundRecord.DisplayName.ShouldBe(expectedRecord.DisplayName);

            foreach (String expectedRecordClaim in expectedRecord.Claims)
            {
                foundRecord.Claims.ShouldContain(expectedRecordClaim);
            }

        }
    }

    public async Task<List<(String, Guid)>> GivenICreateTheFollowingRoles(List<CreateRoleRequest> requests, CancellationToken cancellationToken){
        List<(String, Guid)> result = new List<(String, Guid)>();
        foreach (CreateRoleRequest request in requests){
            CreateRoleResponse? response = await this.SecurityServiceClient.CreateRole(request, cancellationToken).ConfigureAwait(false);

            response.ShouldNotBeNull();
            response.RoleId.ShouldNotBe(Guid.Empty);

            result.Add((request.RoleName, response.RoleId));
        }

        return result;
    }

    public async Task WhenIGetTheRoleWithNameTheRoleDetailsAreReturnedAsFollows(List<RoleDetails> expectedDetails, Guid roleId, CancellationToken cancellationToken)
    {
        RoleDetails roleDetails = await this.SecurityServiceClient.GetRole(roleId, cancellationToken).ConfigureAwait(false);
        RoleDetails expectedRecord = expectedDetails.Single();

        roleDetails.RoleName.ShouldBe(expectedRecord.RoleName);
    }

    public async Task WhenIGetTheRolesRolesDetailsAreReturnedAsFollows(List<RoleDetails> expectedDetails, CancellationToken cancellationToken)
    {
        List<RoleDetails>? rolesList = await this.SecurityServiceClient.GetRoles(CancellationToken.None).ConfigureAwait(false);
        foreach (RoleDetails expectedRecord in expectedDetails)
        {
            RoleDetails? foundRecord = rolesList.SingleOrDefault(a => a.RoleName == expectedRecord.RoleName);
            foundRecord.ShouldNotBeNull();
            foundRecord.RoleName.ShouldBe(expectedRecord.RoleName);
        }
    }

    public async Task<List<(String, Guid)>> GivenICreateTheFollowingUsers(List<CreateUserRequest> requests, CancellationToken cancellationToken){
        List<(String, Guid)> results = new List<(String, Guid)>();
        foreach (CreateUserRequest createUserRequest in requests){
            CreateUserResponse createUserResponse = await this.SecurityServiceClient.CreateUser(createUserRequest, cancellationToken).ConfigureAwait(false);

            createUserResponse.ShouldNotBeNull();
            createUserResponse.UserId.ShouldNotBe(Guid.Empty);

            results.Add((createUserRequest.EmailAddress, createUserResponse.UserId));
        }
        return results;
    }

    public async Task WhenIGetTheUsersUsersDetailsAreReturnedAsFollows(List<UserDetails> expectedDetails, CancellationToken cancellationToken)
    {
        List<UserDetails>? usersList = await this.SecurityServiceClient.GetUsers(String.Empty, CancellationToken.None).ConfigureAwait(false);
        foreach (UserDetails expectedRecord in expectedDetails)
        {
            UserDetails? foundRecord = usersList.SingleOrDefault(a => a.UserName == expectedRecord.UserName);
            foundRecord.ShouldNotBeNull();
            foundRecord.UserName.ShouldBe(expectedRecord.UserName);
            foundRecord.EmailAddress.ShouldBe(expectedRecord.EmailAddress);
            foundRecord.PhoneNumber.ShouldBe(expectedRecord.PhoneNumber);
            foreach (String expectedRecordRole in expectedRecord.Roles){
                foundRecord.Roles.ShouldContain(expectedRecordRole);
            }

            foreach (KeyValuePair<String, String> expectedRecordClaim in expectedRecord.Claims){
                foundRecord.Claims.ContainsKey(expectedRecordClaim.Key).ShouldBeTrue();
                String? claim = foundRecord.Claims[expectedRecordClaim.Key];
                claim.ShouldBe(expectedRecordClaim.Value);
            }
        }
    }

    public async Task WhenIGetTheUserWithUserNameTheUserDetailsAreReturnedAsFollows(List<UserDetails> expectedDetails, Guid userId, CancellationToken cancellationToken){
        var userDetails = await this.SecurityServiceClient.GetUser(userId, CancellationToken.None).ConfigureAwait(false);
        var expectedRecord = expectedDetails.Single();
        
        userDetails.ShouldNotBeNull();
        userDetails.UserName.ShouldBe(expectedRecord.UserName);
        userDetails.EmailAddress.ShouldBe(expectedRecord.EmailAddress);
        userDetails.PhoneNumber.ShouldBe(expectedRecord.PhoneNumber);
        foreach (String expectedRecordRole in expectedRecord.Roles){
            userDetails.Roles.ShouldContain(expectedRecordRole);
        }

        foreach (KeyValuePair<String, String> expectedRecordClaim in expectedRecord.Claims){
            userDetails.Claims.ContainsKey(expectedRecordClaim.Key).ShouldBeTrue();
            String? claim = userDetails.Claims[expectedRecordClaim.Key];
            claim.ShouldBe(expectedRecordClaim.Value);
        }
    }

}