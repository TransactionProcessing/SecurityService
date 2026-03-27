using SecurityService.Models;
using SimpleResults;

namespace SecurityService.IntegrationTesting.Helpers;

using Client;
using DataTransferObjects;
using Shared.IntegrationTesting;
using Shouldly;
using System.Collections.Generic;
using System.Threading;
using Reqnroll;

public class SecurityServiceSteps{
    private readonly ISecurityServiceClient SecurityServiceClient;

    public SecurityServiceSteps(ISecurityServiceClient securityServiceClient){
        this.SecurityServiceClient = securityServiceClient;
    }

    private async Task CreateApiScope(CreateApiScopeRequest createApiScopeRequest,
                                                              CancellationToken cancellationToken){
        Result? result = await this.SecurityServiceClient
                                                                  .CreateApiScope(createApiScopeRequest, cancellationToken).ConfigureAwait(false);
        result.IsSuccess.ShouldBeTrue();
    }

    public async Task GivenICreateTheFollowingApiScopes(List<CreateApiScopeRequest> createApiScopeRequests){
        foreach (CreateApiScopeRequest createApiScopeRequest in createApiScopeRequests){
            await this.CreateApiScope(createApiScopeRequest, CancellationToken.None).ConfigureAwait(false);
            
            // TODO: can do a get in here to verify really created...
        }
    }

    public async Task GivenTheFollowingApiResourcesExist(DataTable table){
        foreach (DataTableRow tableRow in table.Rows){
            String resourceName = ReqnrollTableHelper.GetStringRowValue(tableRow, "ResourceName");
            String displayName = ReqnrollTableHelper.GetStringRowValue(tableRow, "DisplayName");
            String secret = ReqnrollTableHelper.GetStringRowValue(tableRow, "Secret");
            String scopes = ReqnrollTableHelper.GetStringRowValue(tableRow, "Scopes");
            String userClaims = ReqnrollTableHelper.GetStringRowValue(tableRow, "UserClaims");

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

            Result? result = await this.SecurityServiceClient.CreateApiResource(createApiResourceRequest, CancellationToken.None).ConfigureAwait(false);
            result.IsSuccess.ShouldBeTrue();
        }
    }

    public async Task<List<(String clientId, String secret, List<String> allowedGrantTypes)>> GivenTheFollowingClientsExist(List<CreateClientRequest> createClientRequests){
        List<(String clientId, String secret, List<String> allowedGrantTypes)> clients = new List<(String clientId, String secret, List<String> allowedGrantTypes)>();
        foreach (CreateClientRequest createClientRequest in createClientRequests){
            Result? result = await this.SecurityServiceClient.CreateClient(createClientRequest, CancellationToken.None).ConfigureAwait(false);
            result.IsSuccess.ShouldBeTrue();
            
            // TODO: What do i do here....
            clients.Add((createClientRequest.ClientId, createClientRequest.Secret, createClientRequest.AllowedGrantTypes));
        }

        return clients;
    }

    public async Task GivenTheFollowingApiResourcesExist(List<CreateApiResourceRequest> requests){
        foreach (CreateApiResourceRequest createApiResourceRequest in requests){
            Result? result = await this.SecurityServiceClient.CreateApiResource(createApiResourceRequest, CancellationToken.None).ConfigureAwait(false);
            result.IsSuccess.ShouldBeTrue();
        }
    }

    public async Task<String> GetClientToken(String clientId, String secret, CancellationToken cancellationToken){
        Result<TokenResponse>? tokenResponseResult = await this.SecurityServiceClient.GetToken(clientId, secret, cancellationToken).ConfigureAwait(false);
        tokenResponseResult.IsSuccess.ShouldBeTrue();

        return tokenResponseResult.Data.AccessToken;
    }

    public async Task<String> GetPasswordToken(String clientId, String secret, String userName, String password, CancellationToken cancellationToken)
    {
        Result<TokenResponse>? tokenResponseResult = await this.SecurityServiceClient.GetToken(userName,password, clientId, secret, cancellationToken).ConfigureAwait(false);
        tokenResponseResult.IsSuccess.ShouldBeTrue();
        return tokenResponseResult.Data.AccessToken;
    }

    public async Task WhenIGetTheApiResourcesApiResourceDetailsAreReturnedAsFollows(List<ApiResourceResponse> expectedDetails, CancellationToken cancellationToken){
        Result<List<ApiResourceResponse>>? apiResourceDetailsListResult = await this.SecurityServiceClient.GetApiResources(cancellationToken).ConfigureAwait(false);
        apiResourceDetailsListResult.IsSuccess.ShouldBeTrue();
        List<ApiResourceResponse> apiResourceDetailsList = apiResourceDetailsListResult.Data;
        foreach (ApiResourceResponse apiResourceDetails in expectedDetails){
            ApiResourceResponse? foundRecord = apiResourceDetailsList.SingleOrDefault(a => a.Name == apiResourceDetails.Name);
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

    public async Task WhenIGetTheApiResourceWithNameTheApiResourceDetailsAreReturnedAsFollows(List<ApiResourceResponse> expectedDetails, String apiResourceName, CancellationToken cancellationToken){
        Result<ApiResourceResponse>? apiResourceDetailsResult = await this.SecurityServiceClient.GetApiResource(apiResourceName, cancellationToken).ConfigureAwait(false);
        apiResourceDetailsResult.IsSuccess.ShouldBeTrue();

        ApiResourceResponse? apiResourceDetails = apiResourceDetailsResult.Data;
        ApiResourceResponse expectedRecord = expectedDetails.Single();

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

    public async Task WhenIGetTheApiScopesApiScopeDetailsAreReturnedAsFollows(List<ApiScopeResponse> expectedDetails, CancellationToken cancellationToken){
        Result<List<ApiScopeResponse>>? apiScopeDetailsListResult = await this.SecurityServiceClient.GetApiScopes(cancellationToken).ConfigureAwait(false);
        apiScopeDetailsListResult.IsSuccess.ShouldBeTrue();

        List<ApiScopeResponse>? apiScopeDetailsList = apiScopeDetailsListResult.Data;
        foreach (ApiScopeResponse apiScopeDetails in expectedDetails){
            ApiScopeResponse? foundRecord = apiScopeDetailsList.SingleOrDefault(a => a.Name == apiScopeDetails.Name);
            foundRecord.ShouldNotBeNull();
            foundRecord.Description.ShouldBe(apiScopeDetails.Description);
            foundRecord.DisplayName.ShouldBe(apiScopeDetails.DisplayName);
        }
    }

    public async Task WhenIGetTheApiScopeWithNameTheApiScopeDetailsAreReturnedAsFollows(List<ApiScopeResponse> expectedDetails, String apiScopeName, CancellationToken cancellationToken){
        Result<ApiScopeResponse>? apiScopeDetailsResult = await this.SecurityServiceClient.GetApiScope(apiScopeName, cancellationToken).ConfigureAwait(false);
        apiScopeDetailsResult.IsSuccess.ShouldBeTrue();
        ApiScopeResponse? apiScopeDetails = apiScopeDetailsResult.Data;

        ApiScopeResponse expectedRecord = expectedDetails.Single();

        apiScopeDetails.ShouldNotBeNull();
        apiScopeDetails.Description.ShouldBe(expectedRecord.Description);
        apiScopeDetails.DisplayName.ShouldBe(expectedRecord.DisplayName);
    }

    public async Task WhenIGetTheClientWithClientIdTheClientDetailsAreReturnedAsFollows(List<ClientResponse> expectedDetails, String clientId, CancellationToken cancellationToken){
        Result<ClientResponse>? clientDetailsResult = await this.SecurityServiceClient.GetClient(clientId, CancellationToken.None).ConfigureAwait(false);
        clientDetailsResult.IsSuccess.ShouldBeTrue();
        ClientResponse clientDetails = clientDetailsResult.Data;
        ClientResponse expectedRecord = expectedDetails.Single();

        clientDetails.ShouldNotBeNull();
        clientDetails.ClientId.ShouldBe(expectedRecord.ClientId);
        clientDetails.ClientName.ShouldBe(expectedRecord.ClientName);
        clientDetails.Description.ShouldBe(expectedRecord.Description);
        foreach (String expectedRecordAllowedGrantType in expectedRecord.AllowedGrantTypes){
            clientDetails.AllowedGrantTypes.ShouldContain(expectedRecordAllowedGrantType);
        }

        foreach (String expectedRecordAllowedScope in expectedRecord.AllowedScopes){
            clientDetails.AllowedScopes.ShouldContain(expectedRecordAllowedScope);
        }
    }

    public async Task WhenIGetTheClientsClientsDetailsAreReturnedAsFollows(List<ClientResponse> expectedDetails, CancellationToken cancellationToken){
        Result<List<ClientResponse>>? clientDetailsListResult = await this.SecurityServiceClient.GetClients(CancellationToken.None).ConfigureAwait(false);
        clientDetailsListResult.IsSuccess.ShouldBeTrue();
        List<ClientResponse>? clientDetailsList = clientDetailsListResult.Data;
        foreach (ClientResponse expectedRecord in expectedDetails){
            ClientResponse? foundRecord = clientDetailsList.SingleOrDefault(a => a.ClientId == expectedRecord.ClientId);
            foundRecord.ShouldNotBeNull();

            foundRecord.ClientId.ShouldBe(expectedRecord.ClientId);
            foundRecord.ClientName.ShouldBe(expectedRecord.ClientName);
            foundRecord.Description.ShouldBe(expectedRecord.Description);
            foreach (String expectedRecordAllowedGrantType in expectedRecord.AllowedGrantTypes){
                foundRecord.AllowedGrantTypes.ShouldContain(expectedRecordAllowedGrantType);
            }

            foreach (String expectedRecordAllowedScope in expectedRecord.AllowedScopes){
                foundRecord.AllowedScopes.ShouldContain(expectedRecordAllowedScope);
            }
        }
    }

    public async Task GivenICreateTheFollowingIdentityResources(List<CreateIdentityResourceRequest> requests, CancellationToken cancellationToken){
        foreach (CreateIdentityResourceRequest createIdentityResourceRequest in requests){
            Result? result = await this.SecurityServiceClient.CreateIdentityResource(createIdentityResourceRequest, cancellationToken).ConfigureAwait(false);
            result.IsSuccess.ShouldBeTrue();
        }
    }

    public async Task WhenIGetTheIdentityResourceWithNameTheIdentityResourceDetailsAreReturnedAsFollows(List<IdentityResourceResponse> expectedDetails, String identityResourceName, CancellationToken cancellationToken)
    {
        Result<IdentityResourceResponse>? identityResourceDetailsResult = await this.SecurityServiceClient.GetIdentityResource(identityResourceName, cancellationToken).ConfigureAwait(false);
        identityResourceDetailsResult.IsSuccess.ShouldBeTrue();
        IdentityResourceResponse identityResourceDetails = identityResourceDetailsResult.Data;
        IdentityResourceResponse expectedRecord = expectedDetails.Single();

        identityResourceDetails.Name.ShouldBe(expectedRecord.Name);
        identityResourceDetails.Description.ShouldBe(expectedRecord.Description);
        identityResourceDetails.DisplayName.ShouldBe(expectedRecord.DisplayName);

        foreach (String expectedRecordClaim in expectedRecord.Claims){
            identityResourceDetails.Claims.ShouldContain(expectedRecordClaim);
        }
    }

    public async Task WhenIGetTheIdentityResourcesIdentityResourceDetailsAreReturnedAsFollows(List<IdentityResourceResponse> expectedDetails, CancellationToken cancellationToken){
        Result<List<IdentityResourceResponse>>? getIdentityResourcesResult = await this.SecurityServiceClient.GetIdentityResources(CancellationToken.None).ConfigureAwait(false);
        getIdentityResourcesResult.IsSuccess.ShouldBeTrue();
        List<IdentityResourceResponse>? identityResourceDetailsList = getIdentityResourcesResult.Data;
        foreach (IdentityResourceResponse expectedRecord in expectedDetails){
            IdentityResourceResponse? foundRecord = identityResourceDetailsList.SingleOrDefault(a => a.Name == expectedRecord.Name);
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

    public async Task<List<(String, String)>> GivenICreateTheFollowingRoles(List<CreateRoleRequest> requests, CancellationToken cancellationToken) {
        List<(String, String)> roleList = new List<(String, String)>();
        foreach (CreateRoleRequest request in requests){
            Result? result = await this.SecurityServiceClient.CreateRole(request, cancellationToken).ConfigureAwait(false);
            result.IsSuccess.ShouldBeTrue();
        }

        Result<List<RoleResponse>>? roles = await this.SecurityServiceClient.GetRoles(cancellationToken);
        roles.IsSuccess.ShouldBeTrue();

        foreach (CreateRoleRequest request in requests) {
            RoleResponse r = roles.Data.Single(r => r.Name == request.Name);
            roleList.Add((r.Name, r.RoleId));
        }

        return roleList;
    }

    public async Task WhenIGetTheRoleWithNameTheRoleDetailsAreReturnedAsFollows(List<RoleResponse> expectedDetails, String roleId, CancellationToken cancellationToken)
    {
        Result<RoleResponse>? getRoleResult = await this.SecurityServiceClient.GetRole(roleId, cancellationToken).ConfigureAwait(false);
        getRoleResult.IsSuccess.ShouldBeTrue();
        RoleResponse roleDetails =getRoleResult.Data;
        RoleResponse expectedRecord = expectedDetails.Single();

        roleDetails.Name.ShouldBe(expectedRecord.Name);
    }

    public async Task WhenIGetTheRolesRolesDetailsAreReturnedAsFollows(List<RoleResponse> expectedDetails, CancellationToken cancellationToken)
    {
        Result<List<RoleResponse>>? getRolesResult = await this.SecurityServiceClient.GetRoles(CancellationToken.None).ConfigureAwait(false);
        getRolesResult.IsSuccess.ShouldBeTrue();
        List<RoleResponse>? rolesList = getRolesResult.Data;
        foreach (RoleResponse expectedRecord in expectedDetails)
        {
            RoleResponse? foundRecord = rolesList.SingleOrDefault(a => a.Name == expectedRecord.Name);
            foundRecord.ShouldNotBeNull();
            foundRecord.Name.ShouldBe(expectedRecord.Name);
        }
    }

    public async Task<List<(String, String)>> GivenICreateTheFollowingUsers(List<CreateUserRequest> requests, CancellationToken cancellationToken){
        List<(String, String)> results = new List<(String, String)>();
        foreach (CreateUserRequest createUserRequest in requests){
            Result? result = await this.SecurityServiceClient.CreateUser(createUserRequest, cancellationToken).ConfigureAwait(false);
            result.IsSuccess.ShouldBeTrue();

            Result<List<UserResponse>>? user = await this.SecurityServiceClient.GetUsers(createUserRequest.EmailAddress, cancellationToken);
            user.IsSuccess.ShouldBeTrue();

            results.Add((createUserRequest.EmailAddress, user.Data.Single().UserId));
        }
        return results;
    }

    public async Task WhenIGetTheUsersUsersDetailsAreReturnedAsFollows(List<UserResponse> expectedDetails, CancellationToken cancellationToken)
    {
        Result<List<UserResponse>>? getUsersResult = await this.SecurityServiceClient.GetUsers(String.Empty, CancellationToken.None).ConfigureAwait(false);
        getUsersResult.IsSuccess.ShouldBeTrue();
        List<UserResponse>? usersList = getUsersResult.Data;
        foreach (UserResponse expectedRecord in expectedDetails)
        {
            UserResponse? foundRecord = usersList.SingleOrDefault(a => a.UserName == expectedRecord.UserName);
            foundRecord.ShouldNotBeNull();
            foundRecord.UserName.ShouldBe(expectedRecord.UserName);
            foundRecord.EmailAddress.ShouldBe(expectedRecord.EmailAddress);
            foundRecord.PhoneNumber.ShouldBe(expectedRecord.PhoneNumber);
            foreach (String expectedRecordRole in expectedRecord.Roles){
                foundRecord.Roles.ShouldContain(expectedRecordRole);
            }

            foreach (KeyValuePair<String, String> expectedRecordClaim in expectedRecord.Claims){
                var claimName = $"http://schemas.xmlsoap.org/ws/2005/05/identity/claims/{expectedRecordClaim.Key}";
                foundRecord.Claims.ContainsKey(claimName).ShouldBeTrue(claimName);
                String? claim = foundRecord.Claims[claimName];
                claim.ShouldBe(expectedRecordClaim.Value);
            }
        }
    }

    public async Task WhenIGetTheUserWithUserNameTheUserDetailsAreReturnedAsFollows(List<UserResponse> expectedDetails, String userId, CancellationToken cancellationToken){
        var userDetails = await this.SecurityServiceClient.GetUser(userId, CancellationToken.None).ConfigureAwait(false);
        var expectedRecord = expectedDetails.Single();
        
        userDetails.IsSuccess.ShouldBeTrue();
        userDetails.Data.ShouldNotBeNull();
        userDetails.Data.UserName.ShouldBe(expectedRecord.UserName);
        userDetails.Data.EmailAddress.ShouldBe(expectedRecord.EmailAddress);
        userDetails.Data.PhoneNumber.ShouldBe(expectedRecord.PhoneNumber);
        foreach (String expectedRecordRole in expectedRecord.Roles){
            userDetails.Data.Roles.ShouldContain(expectedRecordRole);
        }

        foreach (KeyValuePair<String, String> expectedRecordClaim in expectedRecord.Claims){
            var claimName = $"http://schemas.xmlsoap.org/ws/2005/05/identity/claims/{expectedRecordClaim.Key}";
            userDetails.Data.Claims.ContainsKey(claimName).ShouldBeTrue(claimName);
            String? claim = userDetails.Data.Claims[claimName];
            claim.ShouldBe(expectedRecordClaim.Value);
        }
        // TODO: Add back in
        //userDetails.Data.RegistrationDateTime.Date.ShouldBe(expectedRecord.RegistrationDateTime.Date);
    }

}