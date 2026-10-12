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
    private readonly string AccessToken;

    private static void AssertExpectedResults<T, TKey>(
        IEnumerable<T> actual,
        IEnumerable<T> expected,
        Func<T, TKey> keySelector,
        bool requireExactCount = true)
    {
        List<T> actualList = actual.ToList();
        List<T> expectedList = expected.ToList();

        if (requireExactCount)
        {
            actualList.Count.ShouldBe(expectedList.Count);
        }

        actualList.Select(keySelector).Distinct().Count().ShouldBe(actualList.Count);
        expectedList.Select(keySelector).Distinct().Count().ShouldBe(expectedList.Count);
    }

    private static void AssertStringCollectionsMatch(
        IEnumerable<String> actual,
        IEnumerable<String> expected)
    {
        actual.OrderBy(value => value).ShouldBe(expected.OrderBy(value => value));
    }

    private static void AssertUserClaimsContain(
        IReadOnlyDictionary<String, String> actual,
        IReadOnlyDictionary<String, String>? expected)
    {
        if (expected is null)
        {
            return;
        }

        foreach (KeyValuePair<String, String> expectedClaim in expected)
        {
            actual.ContainsKey(expectedClaim.Key).ShouldBeTrue(expectedClaim.Key);
            actual[expectedClaim.Key].ShouldBe(expectedClaim.Value);
        }
    }

    private async Task<UserResponse> GetAndVerifyUser(CreateUserRequest request, CancellationToken cancellationToken)
    {
        Result<List<UserResponse>>? response = await this.SecurityServiceClient
            .GetUsers(this.AccessToken, request.EmailAddress, cancellationToken)
            .ConfigureAwait(false);

        response.IsSuccess.ShouldBeTrue(response.Message);
        UserResponse user = response.Data.Single();
        String expectedUserName = String.IsNullOrWhiteSpace(request.UserName)
            ? request.EmailAddress ?? String.Empty
            : request.UserName;

        user.UserName.ShouldBe(expectedUserName);
        user.EmailAddress.ShouldBe(request.EmailAddress);
        user.GivenName.ShouldBe(request.GivenName);
        user.MiddleName.ShouldBe(request.MiddleName);
        user.FamilyName.ShouldBe(request.FamilyName);
        user.PhoneNumber.ShouldBe(request.PhoneNumber);
        AssertStringCollectionsMatch(user.Roles, request.Roles);
        AssertUserClaimsContain(user.Claims, request.Claims);

        return user;
    }

    private async Task VerifyApiScope(CreateApiScopeRequest request)
    {
        Result<ApiScopeResponse>? response = await this.SecurityServiceClient
            .GetApiScope(this.AccessToken, request.Name, CancellationToken.None)
            .ConfigureAwait(false);

        response.IsSuccess.ShouldBeTrue(response.Message);
        response.Data.ShouldNotBeNull();
        response.Data.Name.ShouldBe(request.Name);
        response.Data.DisplayName.ShouldBe(request.DisplayName);
        response.Data.Description.ShouldBe(request.Description);
    }

    private async Task VerifyApiResource(CreateApiResourceRequest request)
    {
        Result<ApiResourceResponse>? response = await this.SecurityServiceClient
            .GetApiResource(this.AccessToken, request.Name, CancellationToken.None)
            .ConfigureAwait(false);

        response.IsSuccess.ShouldBeTrue(response.Message);
        response.Data.ShouldNotBeNull();
        response.Data.Name.ShouldBe(request.Name);
        response.Data.DisplayName.ShouldBe(request.DisplayName);
        response.Data.Description.ShouldBe(request.Description);
        AssertStringCollectionsMatch(response.Data.Scopes, request.Scopes);
        AssertStringCollectionsMatch(response.Data.UserClaims, request.UserClaims);
    }

    private async Task VerifyClient(CreateClientRequest request)
    {
        Result<ClientResponse>? response = await this.SecurityServiceClient
            .GetClient(this.AccessToken, request.ClientId, CancellationToken.None)
            .ConfigureAwait(false);

        response.IsSuccess.ShouldBeTrue(response.Message);
        response.Data.ShouldNotBeNull();
        response.Data.ClientId.ShouldBe(request.ClientId);
        response.Data.ClientName.ShouldBe(request.ClientName);
        response.Data.Description.ShouldBe(request.ClientDescription);
        response.Data.ClientUri.ShouldBe(request.ClientUri);
        response.Data.RequireConsent.ShouldBe(request.RequireConsent);
        response.Data.AllowOfflineAccess.ShouldBe(request.AllowOfflineAccess);
        AssertStringCollectionsMatch(response.Data.AllowedScopes, request.AllowedScopes);
        AssertStringCollectionsMatch(response.Data.AllowedGrantTypes, request.AllowedGrantTypes);
        AssertStringCollectionsMatch(response.Data.ClientRedirectUris, request.ClientRedirectUris);
        AssertStringCollectionsMatch(response.Data.ClientPostLogoutRedirectUris, request.ClientPostLogoutRedirectUris);
    }

    private async Task VerifyIdentityResource(CreateIdentityResourceRequest request)
    {
        Result<IdentityResourceResponse>? response = await this.SecurityServiceClient
            .GetIdentityResource(this.AccessToken, request.Name, CancellationToken.None)
            .ConfigureAwait(false);

        response.IsSuccess.ShouldBeTrue(response.Message);
        response.Data.ShouldNotBeNull();
        response.Data.Name.ShouldBe(request.Name);
        response.Data.DisplayName.ShouldBe(request.DisplayName);
        response.Data.Description.ShouldBe(request.Description);
        AssertStringCollectionsMatch(response.Data.Claims, request.Claims ?? Enumerable.Empty<String>());
    }

    public SecurityServiceSteps(ISecurityServiceClient securityServiceClient, String accessToken){
        this.SecurityServiceClient = securityServiceClient;
        this.AccessToken = accessToken;
    }

    private async Task CreateApiScope(String accessToken, CreateApiScopeRequest createApiScopeRequest,
                                                              CancellationToken cancellationToken){
        Result? result = await this.SecurityServiceClient.CreateApiScope(this.AccessToken, createApiScopeRequest, cancellationToken).ConfigureAwait(false);
        result.IsSuccess.ShouldBeTrue();
    }

    public async Task GivenICreateTheFollowingApiScopes(List<CreateApiScopeRequest> createApiScopeRequests){
        foreach (CreateApiScopeRequest createApiScopeRequest in createApiScopeRequests){
            await this.CreateApiScope(this.AccessToken, createApiScopeRequest, CancellationToken.None).ConfigureAwait(false);
            await this.VerifyApiScope(createApiScopeRequest).ConfigureAwait(false);
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

            Result? result = await this.SecurityServiceClient.CreateApiResource(this.AccessToken, createApiResourceRequest, CancellationToken.None).ConfigureAwait(false);
            result.IsSuccess.ShouldBeTrue(result.Message);
            await this.VerifyApiResource(createApiResourceRequest).ConfigureAwait(false);
        }
    }

    public async Task<List<(String clientId, String secret, List<String> allowedGrantTypes)>> GivenTheFollowingClientsExist(List<CreateClientRequest> createClientRequests){
        List<(String clientId, String secret, List<String> allowedGrantTypes)> clients = new List<(String clientId, String secret, List<String> allowedGrantTypes)>();
        foreach (CreateClientRequest createClientRequest in createClientRequests){
            Result? result = await this.SecurityServiceClient.CreateClient(this.AccessToken, createClientRequest, CancellationToken.None).ConfigureAwait(false);
            result.IsSuccess.ShouldBeTrue(result.Message);
            await this.VerifyClient(createClientRequest).ConfigureAwait(false);

            clients.Add((createClientRequest.ClientId, createClientRequest.Secret, createClientRequest.AllowedGrantTypes));
        }

        return clients;
    }

    public async Task GivenTheFollowingApiResourcesExist(List<CreateApiResourceRequest> requests){
        foreach (CreateApiResourceRequest createApiResourceRequest in requests){
            Result? result = await this.SecurityServiceClient.CreateApiResource(this.AccessToken, createApiResourceRequest, CancellationToken.None).ConfigureAwait(false);
            result.IsSuccess.ShouldBeTrue(result.Message);
            await this.VerifyApiResource(createApiResourceRequest).ConfigureAwait(false);
        }
    }

    public async Task<String> GetClientToken(String clientId, String secret, CancellationToken cancellationToken){
        Result<TokenResponse>? tokenResponseResult = await this.SecurityServiceClient.GetToken(clientId, secret, cancellationToken).ConfigureAwait(false);
        tokenResponseResult.IsSuccess.ShouldBeTrue();

        return tokenResponseResult.Data.AccessToken;
    }

    public async Task<String> GetPasswordToken(String clientId, String secret, String userName, String password, CancellationToken cancellationToken)
    {
        Result<TokenResponse>? tokenResponseResult = await this.RequestPasswordToken(clientId, secret, userName, password, cancellationToken).ConfigureAwait(false);
        tokenResponseResult.IsSuccess.ShouldBeTrue();
        return tokenResponseResult.Data.AccessToken;
    }

    public Task<Result<TokenResponse>> RequestPasswordToken(String clientId, String secret, String userName, String password, CancellationToken cancellationToken) =>
        this.SecurityServiceClient.GetToken(userName, password, clientId, secret, cancellationToken);

    public async Task WhenIGetTheApiResourcesApiResourceDetailsAreReturnedAsFollows(List<ApiResourceResponse> expectedDetails, CancellationToken cancellationToken){
        Result<List<ApiResourceResponse>>? apiResourceDetailsListResult = await this.SecurityServiceClient.GetApiResources(this.AccessToken, cancellationToken).ConfigureAwait(false);
        apiResourceDetailsListResult.IsSuccess.ShouldBeTrue();
        List<ApiResourceResponse> apiResourceDetailsList = apiResourceDetailsListResult.Data;
        AssertExpectedResults(apiResourceDetailsList, expectedDetails, resource => resource.Name);
        foreach (ApiResourceResponse apiResourceDetails in expectedDetails){
            ApiResourceResponse? foundRecord = apiResourceDetailsList.SingleOrDefault(a => a.Name == apiResourceDetails.Name);
            foundRecord.ShouldNotBeNull();
            foundRecord.Description.ShouldBe(apiResourceDetails.Description);
            foundRecord.DisplayName.ShouldBe(apiResourceDetails.DisplayName);
            AssertStringCollectionsMatch(foundRecord.Scopes, apiResourceDetails.Scopes);
            AssertStringCollectionsMatch(foundRecord.UserClaims, apiResourceDetails.UserClaims);
        }
    }

    public async Task WhenIGetTheApiResourceWithNameTheApiResourceDetailsAreReturnedAsFollows(List<ApiResourceResponse> expectedDetails, String apiResourceName, CancellationToken cancellationToken){
        Result<ApiResourceResponse>? apiResourceDetailsResult = await this.SecurityServiceClient.GetApiResource(this.AccessToken, apiResourceName, cancellationToken).ConfigureAwait(false);
        apiResourceDetailsResult.IsSuccess.ShouldBeTrue();

        ApiResourceResponse? apiResourceDetails = apiResourceDetailsResult.Data;
        ApiResourceResponse expectedRecord = expectedDetails.Single();

        apiResourceDetails.ShouldNotBeNull();
        apiResourceDetails.Description.ShouldBe(expectedRecord.Description);
        apiResourceDetails.DisplayName.ShouldBe(expectedRecord.DisplayName);
        AssertStringCollectionsMatch(apiResourceDetails.Scopes, expectedRecord.Scopes);
        AssertStringCollectionsMatch(apiResourceDetails.UserClaims, expectedRecord.UserClaims);
    }

    public async Task WhenIGetTheApiScopesApiScopeDetailsAreReturnedAsFollows(List<ApiScopeResponse> expectedDetails, CancellationToken cancellationToken){
        Result<List<ApiScopeResponse>>? apiScopeDetailsListResult = await this.SecurityServiceClient.GetApiScopes(this.AccessToken, cancellationToken).ConfigureAwait(false);
        apiScopeDetailsListResult.IsSuccess.ShouldBeTrue();

        List<ApiScopeResponse>? apiScopeDetailsList = apiScopeDetailsListResult.Data;
        AssertExpectedResults(apiScopeDetailsList!, expectedDetails, scope => scope.Name);
        foreach (ApiScopeResponse apiScopeDetails in expectedDetails){
            ApiScopeResponse? foundRecord = apiScopeDetailsList.SingleOrDefault(a => a.Name == apiScopeDetails.Name);
            foundRecord.ShouldNotBeNull();
            foundRecord.Description.ShouldBe(apiScopeDetails.Description);
            foundRecord.DisplayName.ShouldBe(apiScopeDetails.DisplayName);
        }
    }

    public async Task WhenIGetTheApiScopeWithNameTheApiScopeDetailsAreReturnedAsFollows(List<ApiScopeResponse> expectedDetails, String apiScopeName, CancellationToken cancellationToken){
        Result<ApiScopeResponse>? apiScopeDetailsResult = await this.SecurityServiceClient.GetApiScope(this.AccessToken, apiScopeName, cancellationToken).ConfigureAwait(false);
        apiScopeDetailsResult.IsSuccess.ShouldBeTrue();
        ApiScopeResponse? apiScopeDetails = apiScopeDetailsResult.Data;

        ApiScopeResponse expectedRecord = expectedDetails.Single();

        apiScopeDetails.ShouldNotBeNull();
        apiScopeDetails.Description.ShouldBe(expectedRecord.Description);
        apiScopeDetails.DisplayName.ShouldBe(expectedRecord.DisplayName);
    }

    public async Task WhenIGetTheClientWithClientIdTheClientDetailsAreReturnedAsFollows(List<ClientResponse> expectedDetails, String clientId, CancellationToken cancellationToken){
        Result<ClientResponse>? clientDetailsResult = await this.SecurityServiceClient.GetClient(this.AccessToken, clientId, cancellationToken).ConfigureAwait(false);
        clientDetailsResult.IsSuccess.ShouldBeTrue();
        ClientResponse clientDetails = clientDetailsResult.Data;
        ClientResponse expectedRecord = expectedDetails.Single();

        clientDetails.ShouldNotBeNull();
        clientDetails.ClientId.ShouldBe(expectedRecord.ClientId);
        clientDetails.ClientName.ShouldBe(expectedRecord.ClientName);
        clientDetails.Description.ShouldBe(expectedRecord.Description);
        AssertStringCollectionsMatch(clientDetails.AllowedGrantTypes, expectedRecord.AllowedGrantTypes);
        AssertStringCollectionsMatch(clientDetails.AllowedScopes, expectedRecord.AllowedScopes);
    }

    public async Task WhenIGetTheClientsClientsDetailsAreReturnedAsFollows(List<ClientResponse> expectedDetails, CancellationToken cancellationToken){
        Result<List<ClientResponse>>? clientDetailsListResult = await this.SecurityServiceClient.GetClients(this.AccessToken, cancellationToken).ConfigureAwait(false);
        clientDetailsListResult.IsSuccess.ShouldBeTrue();
        List<ClientResponse>? clientDetailsList = clientDetailsListResult.Data;
        AssertExpectedResults(clientDetailsList!, expectedDetails, client => client.ClientId);
        foreach (ClientResponse expectedRecord in expectedDetails){
            ClientResponse? foundRecord = clientDetailsList.SingleOrDefault(a => a.ClientId == expectedRecord.ClientId);
            foundRecord.ShouldNotBeNull();

            foundRecord.ClientId.ShouldBe(expectedRecord.ClientId);
            foundRecord.ClientName.ShouldBe(expectedRecord.ClientName);
            foundRecord.Description.ShouldBe(expectedRecord.Description);
            AssertStringCollectionsMatch(foundRecord.AllowedGrantTypes, expectedRecord.AllowedGrantTypes);
            AssertStringCollectionsMatch(foundRecord.AllowedScopes, expectedRecord.AllowedScopes);
        }
    }

    public async Task GivenICreateTheFollowingIdentityResources(List<CreateIdentityResourceRequest> requests, CancellationToken cancellationToken){
        foreach (CreateIdentityResourceRequest createIdentityResourceRequest in requests){
            Result? result = await this.SecurityServiceClient.CreateIdentityResource(this.AccessToken, createIdentityResourceRequest, cancellationToken).ConfigureAwait(false);
            result.IsSuccess.ShouldBeTrue(result.Message);
            await this.VerifyIdentityResource(createIdentityResourceRequest).ConfigureAwait(false);
        }
    }

    public async Task WhenIGetTheIdentityResourceWithNameTheIdentityResourceDetailsAreReturnedAsFollows(List<IdentityResourceResponse> expectedDetails, String identityResourceName, CancellationToken cancellationToken)
    {
        Result<IdentityResourceResponse>? identityResourceDetailsResult = await this.SecurityServiceClient.GetIdentityResource(this.AccessToken, identityResourceName, cancellationToken).ConfigureAwait(false);
        identityResourceDetailsResult.IsSuccess.ShouldBeTrue();
        IdentityResourceResponse identityResourceDetails = identityResourceDetailsResult.Data;
        IdentityResourceResponse expectedRecord = expectedDetails.Single();

        identityResourceDetails.Name.ShouldBe(expectedRecord.Name);
        identityResourceDetails.Description.ShouldBe(expectedRecord.Description);
        identityResourceDetails.DisplayName.ShouldBe(expectedRecord.DisplayName);

        AssertStringCollectionsMatch(identityResourceDetails.Claims, expectedRecord.Claims);
    }

    public async Task WhenIGetTheIdentityResourcesIdentityResourceDetailsAreReturnedAsFollows(List<IdentityResourceResponse> expectedDetails, CancellationToken cancellationToken){
        Result<List<IdentityResourceResponse>>? getIdentityResourcesResult = await this.SecurityServiceClient.GetIdentityResources(this.AccessToken, cancellationToken).ConfigureAwait(false);
        getIdentityResourcesResult.IsSuccess.ShouldBeTrue();
        List<IdentityResourceResponse>? identityResourceDetailsList = getIdentityResourcesResult.Data;
        AssertExpectedResults(identityResourceDetailsList!, expectedDetails, resource => resource.Name);
        foreach (IdentityResourceResponse expectedRecord in expectedDetails){
            IdentityResourceResponse? foundRecord = identityResourceDetailsList.SingleOrDefault(a => a.Name == expectedRecord.Name);
            foundRecord.ShouldNotBeNull();
            foundRecord.Name.ShouldBe(expectedRecord.Name);
            foundRecord.Description.ShouldBe(expectedRecord.Description);
            foundRecord.DisplayName.ShouldBe(expectedRecord.DisplayName);

            AssertStringCollectionsMatch(foundRecord.Claims, expectedRecord.Claims);

        }
    }

    public async Task<List<(String, String)>> GivenICreateTheFollowingRoles(List<CreateRoleRequest> requests, CancellationToken cancellationToken) {
        List<(String, String)> roleList = new List<(String, String)>();
        foreach (CreateRoleRequest request in requests){
            Result? result = await this.SecurityServiceClient.CreateRole(this.AccessToken, request, cancellationToken).ConfigureAwait(false);
            result.IsSuccess.ShouldBeTrue(result.Message);
        }

        Result<List<RoleResponse>>? roles = await this.SecurityServiceClient.GetRoles(this.AccessToken, cancellationToken);
        roles.IsSuccess.ShouldBeTrue(roles.Message);

        roles.Data.Count.ShouldBe(requests.Count);
        roles.Data.Select(role => role.RoleName).Distinct().Count().ShouldBe(roles.Data.Count);
        requests.Select(request => request.RoleName).Distinct().Count().ShouldBe(requests.Count);

        foreach (CreateRoleRequest request in requests) {
            RoleResponse r = roles.Data.Single(r => r.RoleName == request.RoleName);
            r.RoleId.ShouldNotBeNullOrWhiteSpace();
            roleList.Add((r.RoleName, r.RoleId));
        }

        return roleList;
    }

    public async Task WhenIGetTheRoleWithNameTheRoleDetailsAreReturnedAsFollows(List<RoleResponse> expectedDetails, String roleId, CancellationToken cancellationToken)
    {
        Result<RoleResponse>? getRoleResult = await this.SecurityServiceClient.GetRole(this.AccessToken, roleId, cancellationToken).ConfigureAwait(false);
        getRoleResult.IsSuccess.ShouldBeTrue();
        RoleResponse roleDetails =getRoleResult.Data;
        RoleResponse expectedRecord = expectedDetails.Single();

        roleDetails.RoleName.ShouldBe(expectedRecord.RoleName);
    }

    public async Task WhenIGetTheRolesRolesDetailsAreReturnedAsFollows(List<RoleResponse> expectedDetails, CancellationToken cancellationToken)
    {
        Result<List<RoleResponse>>? getRolesResult = await this.SecurityServiceClient.GetRoles(this.AccessToken, cancellationToken).ConfigureAwait(false);
        getRolesResult.IsSuccess.ShouldBeTrue();
        List<RoleResponse>? rolesList = getRolesResult.Data;
        AssertExpectedResults(rolesList!, expectedDetails, role => role.RoleName);
        foreach (RoleResponse expectedRecord in expectedDetails)
        {
            RoleResponse? foundRecord = rolesList.SingleOrDefault(a => a.RoleName == expectedRecord.RoleName);
            foundRecord.ShouldNotBeNull();
            foundRecord.RoleName.ShouldBe(expectedRecord.RoleName);
        }
    }

    public async Task<List<(String, String)>> GivenICreateTheFollowingUsers(List<CreateUserRequest> requests, CancellationToken cancellationToken){
        List<(String, String)> results = new List<(String, String)>();
        foreach (CreateUserRequest createUserRequest in requests){
            Result? result = await this.SecurityServiceClient.CreateUser(this.AccessToken, createUserRequest, cancellationToken).ConfigureAwait(false);
            result.IsSuccess.ShouldBeTrue(result.Message);

            UserResponse user = await this.GetAndVerifyUser(createUserRequest, cancellationToken).ConfigureAwait(false);

            String userKey = String.IsNullOrWhiteSpace(createUserRequest.UserName)
                                 ? createUserRequest.EmailAddress ?? String.Empty
                                 : createUserRequest.UserName;
            results.Add((userKey, user.UserId));
        }
        return results;
    }

    public Task<Result> RequireUserMfa(String userId, CancellationToken cancellationToken) =>
        this.SecurityServiceClient.RequireUserMfa(this.AccessToken, userId, cancellationToken);

    public Task<Result> RemoveUserMfa(String userId, CancellationToken cancellationToken) =>
        this.SecurityServiceClient.RemoveUserMfa(this.AccessToken, userId, cancellationToken);

    public Task<Result> RequireRoleMfa(String roleId, CancellationToken cancellationToken) =>
        this.SecurityServiceClient.RequireRoleMfa(this.AccessToken, roleId, cancellationToken);

    public Task<Result> RemoveRoleMfa(String roleId, CancellationToken cancellationToken) =>
        this.SecurityServiceClient.RemoveRoleMfa(this.AccessToken, roleId, cancellationToken);

    public async Task WhenIGetTheUsersUsersDetailsAreReturnedAsFollows(List<UserResponse> expectedDetails, CancellationToken cancellationToken)
    {
        Result<List<UserResponse>>? getUsersResult = await this.SecurityServiceClient.GetUsers(this.AccessToken, String.Empty, cancellationToken).ConfigureAwait(false);
        getUsersResult.IsSuccess.ShouldBeTrue();
        List<UserResponse>? usersList = getUsersResult.Data;
        AssertExpectedResults(usersList!, expectedDetails, user => user.UserName);
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
                foundRecord.Claims.ContainsKey(expectedRecordClaim.Key).ShouldBeTrue(expectedRecordClaim.Key);
                String? claim = foundRecord.Claims[expectedRecordClaim.Key];
                claim.ShouldBe(expectedRecordClaim.Value);
            }
        }
    }

    public async Task WhenIGetTheUserWithUserNameTheUserDetailsAreReturnedAsFollows(List<UserResponse> expectedDetails, String userId, CancellationToken cancellationToken){
        var userDetails = await this.SecurityServiceClient.GetUser(this.AccessToken, userId, cancellationToken).ConfigureAwait(false);
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
            userDetails.Data.Claims.ContainsKey(expectedRecordClaim.Key).ShouldBeTrue(expectedRecordClaim.Key);
            String? claim = userDetails.Data.Claims[expectedRecordClaim.Key];
            claim.ShouldBe(expectedRecordClaim.Value);
        }
        // TODO: Add back in
        //userDetails.Data.RegistrationDateTime.Date.ShouldBe(expectedRecord.RegistrationDateTime.Date);
    }

}
