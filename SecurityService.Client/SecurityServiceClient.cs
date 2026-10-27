using Shared.Results;
using SimpleResults;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SecurityService.DataTransferObjects;

namespace SecurityService.Client;

public class SecurityServiceClient : ClientProxyBase.ClientProxyBase, ISecurityServiceClient {
    private String BaseAddress;

    private readonly Func<String, String> BaseAddressResolver;

    public SecurityServiceClient(Func<String, String> baseAddressResolver,
                                 HttpClient httpClient,
                                 Func<object, string> serialise,
                                 Func<string, Type, object> deserialise) : base(httpClient, serialise, deserialise) {
        this.BaseAddressResolver = baseAddressResolver;
        this.BaseAddress = baseAddressResolver("SecurityService");
    }

    public async Task<Result> CreateApiResource(String accessToken,
                                                CreateApiResourceRequest createApiResourceRequest,
                                                CancellationToken cancellationToken) {
        String requestUri = this.BuildRequestUrl("/api/apiresources");

        try {
            Result result = await this.Post(requestUri, createApiResourceRequest, accessToken, cancellationToken);

            if (result.IsFailed) {
                return ResultHelpers.CreateFailure(result);
            }

            return Result.Success();
        }
        catch (Exception ex) {
            // An exception has occurred, add some additional information to the message
            Exception exception = new($"Error creating api resource {createApiResourceRequest.Name}.", ex);

            throw exception;
        }
    }

    public async Task<Result> CreateApiScope(String accessToken, CreateApiScopeRequest createApiScopeRequest,
                                             CancellationToken cancellationToken) {
        String requestUri = this.BuildRequestUrl("/api/apiscopes");

        try {
            Result result = await this.Post(requestUri, createApiScopeRequest, accessToken, cancellationToken);

            if (result.IsFailed) {
                return ResultHelpers.CreateFailure(result);
            }

            return Result.Success();
        }
        catch (Exception ex) {
            // An exception has occurred, add some additional information to the message
            Exception exception = new($"Error creating api scope {createApiScopeRequest.Name}.", ex);

            throw exception;
        }
    }

    public async Task<Result> CreateClient(String accessToken, CreateClientRequest createClientRequest,
                                           CancellationToken cancellationToken) {
        String requestUri = this.BuildRequestUrl("/api/clients");

        try {
            Result result = await this.Post(requestUri, createClientRequest, accessToken, cancellationToken);

            if (result.IsFailed) {
                return ResultHelpers.CreateFailure(result);
            }

            return Result.Success();
        }
        catch (Exception ex) {
            // An exception has occurred, add some additional information to the message
            Exception exception = new($"Error creating client {createClientRequest.ClientId}.", ex);

            throw exception;
        }
    }

    public async Task<Result> CreateIdentityResource(String accessToken, CreateIdentityResourceRequest createIdentityResourceRequest,
                                                     CancellationToken cancellationToken) {
        String requestUri = this.BuildRequestUrl("/api/identityresources");

        try {
            Result result = await this.Post(requestUri, createIdentityResourceRequest, accessToken, cancellationToken);

            if (result.IsFailed) {
                return ResultHelpers.CreateFailure(result);
            }

            return Result.Success();
        }
        catch (Exception ex) {
            // An exception has occurred, add some additional information to the message
            Exception exception = new($"Error creating identity resource {createIdentityResourceRequest.DisplayName}.", ex);

            throw exception;
        }
    }

    public async Task<Result> CreateRole(String accessToken, CreateRoleRequest createRoleRequest,
                                         CancellationToken cancellationToken) {
        String requestUri = this.BuildRequestUrl("/api/roles");

        try {
            Result result = await this.Post(requestUri, createRoleRequest, accessToken, cancellationToken);

            if (result.IsFailed) {
                return ResultHelpers.CreateFailure(result);
            }

            return Result.Success();
        }
        catch (Exception ex) {
            // An exception has occurred, add some additional information to the message
            Exception exception = new($"Error creating role {createRoleRequest.RoleName}.", ex);

            throw exception;
        }
    }

    public async Task<Result> CreateUser(String accessToken, CreateUserRequest createUserRequest,
                                         CancellationToken cancellationToken) {
        String requestUri = this.BuildRequestUrl("/api/users");

        try {
            Result result = await this.Post(requestUri, createUserRequest, accessToken, cancellationToken);

            if (result.IsFailed) {
                return ResultHelpers.CreateFailure(result);
            }

            return Result.Success();
        }
        catch (Exception ex) {
            // An exception has occurred, add some additional information to the message
            Exception exception = new($"Error creating user {createUserRequest.EmailAddress}.", ex);

            throw exception;
        }
    }

    public async Task<Result> RequireUserMfa(String accessToken, String userId, CancellationToken cancellationToken) => await this.Put(this.BuildRequestUrl($"/api/users/{userId}/mfa-policy"), new { }, accessToken, cancellationToken);

    public async Task<Result> RemoveUserMfa(String accessToken, String userId, CancellationToken cancellationToken) => await this.Delete(this.BuildRequestUrl($"/api/users/{userId}/mfa-policy"), accessToken, cancellationToken);

    public async Task<Result> RequireRoleMfa(String accessToken, String roleId, CancellationToken cancellationToken) => await this.Put(this.BuildRequestUrl($"/api/roles/{roleId}/mfa-policy"), new { }, accessToken, cancellationToken);

    public async Task<Result> RemoveRoleMfa(String accessToken, String roleId, CancellationToken cancellationToken) => await this.Delete(this.BuildRequestUrl($"/api/roles/{roleId}/mfa-policy"), accessToken, cancellationToken);

    public async Task<Result<ApiResourceResponse>> GetApiResource(String accessToken, String apiResourceName,
                                                                  CancellationToken cancellationToken) {
        String requestUri = this.BuildRequestUrl($"/api/apiresources/{apiResourceName}");

        try {
            Result<ApiResourceResponse> result = await this.Get<ApiResourceResponse>(requestUri, accessToken, cancellationToken);

            if (result.IsFailed) {
                return ResultHelpers.CreateFailure(result);
            }

            return Result.Success(result.Data);
        }
        catch (Exception ex) {
            // An exception has occurred, add some additional information to the message
            Exception exception = new($"Error getting api resource {apiResourceName}.", ex);

            throw exception;
        }
    }

    public async Task<Result<List<ApiResourceResponse>>> GetApiResources(String accessToken, CancellationToken cancellationToken) {
        String requestUri = this.BuildRequestUrl("/api/apiresources");

        try {
            Result<List<ApiResourceResponse>> result = await this.Get<List<ApiResourceResponse>>(requestUri, accessToken, cancellationToken);

            if (result.IsFailed) {
                return ResultHelpers.CreateFailure(result);
            }

            return Result.Success(result.Data);
        }
        catch (Exception ex) {
            // An exception has occurred, add some additional information to the message
            Exception exception = new("Error getting api resources.", ex);

            throw exception;
        }
    }

    public async Task<Result<ApiScopeResponse>> GetApiScope(String accessToken, String apiScopeName,
                                                            CancellationToken cancellationToken) {
        String requestUri = this.BuildRequestUrl($"/api/apiscopes/{apiScopeName}");

        try {
            Result<ApiScopeResponse> result = await this.Get<ApiScopeResponse>(requestUri, accessToken, cancellationToken);

            if (result.IsFailed) {
                return ResultHelpers.CreateFailure(result);
            }

            return Result.Success(result.Data);
        }
        catch (Exception ex) {
            // An exception has occurred, add some additional information to the message
            Exception exception = new($"Error getting api scope {apiScopeName}.", ex);

            throw exception;
        }
    }

    public async Task<Result<List<ApiScopeResponse>>> GetApiScopes(String accessToken, CancellationToken cancellationToken) {
        String requestUri = this.BuildRequestUrl("/api/apiscopes");

        try {
            Result<List<ApiScopeResponse>> result = await this.Get<List<ApiScopeResponse>>(requestUri, accessToken, cancellationToken);

            if (result.IsFailed) {
                return ResultHelpers.CreateFailure(result);
            }

            return Result.Success(result.Data);
        }
        catch (Exception ex) {
            // An exception has occurred, add some additional information to the message
            Exception exception = new("Error getting api scopes.", ex);

            throw exception;
        }
    }

    public async Task<Result<ClientResponse>> GetClient(String accessToken, String clientId,
                                                        CancellationToken cancellationToken) {
        String requestUri = this.BuildRequestUrl($"/api/clients/{clientId}");

        try {
            Result<ClientResponse> result = await this.Get<ClientResponse>(requestUri, accessToken, cancellationToken);

            if (result.IsFailed) {
                return ResultHelpers.CreateFailure(result);
            }

            return Result.Success(result.Data);
        }
        catch (Exception ex) {
            // An exception has occurred, add some additional information to the message
            Exception exception = new($"Error getting client {clientId}.", ex);

            throw exception;
        }
    }

    public async Task<Result<List<ClientResponse>>> GetClients(String accessToken, CancellationToken cancellationToken) {
        String requestUri = this.BuildRequestUrl("/api/clients");

        try {
            Result<List<ClientResponse>> result = await this.Get<List<ClientResponse>>(requestUri, accessToken, cancellationToken);

            if (result.IsFailed) {
                return ResultHelpers.CreateFailure(result);
            }

            return Result.Success(result.Data);
        }
        catch (Exception ex) {
            // An exception has occurred, add some additional information to the message
            Exception exception = new("Error getting clients.", ex);

            throw exception;
        }
    }

    public async Task<Result<IdentityResourceResponse>> GetIdentityResource(String accessToken, String identityResourceName,
                                                                            CancellationToken cancellationToken) {
        String requestUri = this.BuildRequestUrl($"/api/identityresources/{identityResourceName}");

        try {
            Result<IdentityResourceResponse> result = await this.Get<IdentityResourceResponse>(requestUri, accessToken, cancellationToken);

            if (result.IsFailed) {
                return ResultHelpers.CreateFailure(result);
            }

            return Result.Success(result.Data);
        }
        catch (Exception ex) {
            // An exception has occurred, add some additional information to the message
            Exception exception = new($"Error getting identity resource {identityResourceName}.", ex);

            throw exception;
        }
    }

    public async Task<Result<List<IdentityResourceResponse>>> GetIdentityResources(String accessToken, CancellationToken cancellationToken) {
        String requestUri = this.BuildRequestUrl("/api/identityresources");

        try {
            Result<List<IdentityResourceResponse>> result = await this.Get<List<IdentityResourceResponse>>(requestUri, accessToken, cancellationToken);

            if (result.IsFailed) {
                return ResultHelpers.CreateFailure(result);
            }

            return Result.Success(result.Data);
        }
        catch (Exception ex) {
            // An exception has occurred, add some additional information to the message
            Exception exception = new("Error getting identity resources.", ex);

            throw exception;
        }
    }

    public async Task<Result<RoleResponse>> GetRole(String accessToken, String roleId,
                                                    CancellationToken cancellationToken) {
        String requestUri = this.BuildRequestUrl($"/api/roles/{roleId}");

        try {
            Result<RoleResponse> result = await this.Get<RoleResponse>(requestUri, accessToken, cancellationToken);

            if (result.IsFailed) {
                return ResultHelpers.CreateFailure(result);
            }

            return Result.Success(result.Data);
        }
        catch (Exception ex) {
            // An exception has occurred, add some additional information to the message
            Exception exception = new($"Error get role with Id {roleId}.", ex);

            throw exception;
        }
    }

    public async Task<Result<List<RoleResponse>>> GetRoles(String accessToken, CancellationToken cancellationToken) {
        String requestUri = this.BuildRequestUrl("/api/roles");

        try {
            Result<List<RoleResponse>> result = await this.Get<List<RoleResponse>>(requestUri, accessToken, cancellationToken);

            if (result.IsFailed) {
                return ResultHelpers.CreateFailure(result);
            }

            return Result.Success(result.Data);
        }
        catch (Exception ex) {
            // An exception has occurred, add some additional information to the message
            Exception exception = new("Error get roles.", ex);

            throw exception;
        }
    }

    public async Task<Result<TokenResponse>> GetToken(String username,
                                                      String password,
                                                      String clientId,
                                                      String clientSecret,
                                                      CancellationToken cancellationToken) {
        StringBuilder queryString = new();

        queryString.Append("grant_type=password");
        queryString.Append($"&client_id={clientId}");
        queryString.Append($"&client_secret={clientSecret}");
        queryString.Append($"&username={username}");
        queryString.Append($"&password={password}");

        return await this.GetToken(queryString.ToString(), cancellationToken);
    }

    public async Task<Result<TokenResponse>> GetToken(String clientId,
                                                      String clientSecret,
                                                      CancellationToken cancellationToken) {
        StringBuilder queryString = new();

        queryString.Append("grant_type=client_credentials");
        queryString.Append($"&client_id={clientId}");
        queryString.Append($"&client_secret={clientSecret}");

        return await this.GetToken(queryString.ToString(), cancellationToken);
    }

    public async Task<Result<TokenResponse>> GetToken(String clientId,
                                                      String clientSecret,
                                                      String refreshToken,
                                                      CancellationToken cancellationToken) {
        StringBuilder queryString = new();

        queryString.Append("grant_type=client_credentials");
        queryString.Append($"&client_id={clientId}");
        queryString.Append($"&client_secret={clientSecret}");
        queryString.Append($"&refresh_token={refreshToken}");

        return await this.GetToken(queryString.ToString(), cancellationToken);
    }

    public async Task<Result<UserResponse>> GetUser(String accessToken, String userId,
                                                    CancellationToken cancellationToken) {
        String requestUri = this.BuildRequestUrl($"/api/users/{userId}");

        try {
            Result<UserResponse> result = await this.Get<UserResponse>(requestUri, accessToken, cancellationToken);

            if (result.IsFailed) {
                return ResultHelpers.CreateFailure(result);
            }

            return Result.Success(result.Data);
        }
        catch (Exception ex) {
            // An exception has occurred, add some additional information to the message
            Exception exception = new($"Error get user with Id {userId}.", ex);

            throw exception;
        }
    }

    public async Task<Result<List<UserResponse>>> GetUsers(String accessToken, String userName,
                                                           CancellationToken cancellationToken) {
        String requestUri = this.BuildRequestUrl("/api/users");

        try {
            if (string.IsNullOrEmpty(userName) == false) {
                requestUri = $"{requestUri}?username={userName}";
            }

            Result<List<UserResponse>> result = await this.Get<List<UserResponse>>(requestUri, accessToken, cancellationToken);

            if (result.IsFailed) {
                return ResultHelpers.CreateFailure(result);
            }

            return Result.Success(result.Data);
        }
        catch (Exception ex) {
            // An exception has occurred, add some additional information to the message
            Exception exception = new("Error get users.", ex);

            throw exception;
        }
    }

    private String BuildRequestUrl(String route) {
        if (string.IsNullOrEmpty(this.BaseAddress)) {
            this.BaseAddress = this.BaseAddressResolver("SecurityService");
        }

        String requestUri = $"{this.BaseAddress}{route}";
        return requestUri;
    }

    private async Task<Result<TokenResponse>> GetToken(String tokenRequest,
                                                       CancellationToken cancellationToken) {
        String requestUri = this.BuildRequestUrl("/connect/token");

        try {
            StringContent httpContent = new(tokenRequest, Encoding.UTF8, "application/x-www-form-urlencoded");

            // Make the Http Call here
            Result<String> result = await this.Post<String>(requestUri, httpContent, cancellationToken);


            if (result.IsFailed) {
                return ResultHelpers.CreateFailure(result);
            }

            // call was successful so now deserialise the body to the response object
            TokenResponse responseData = TokenResponse.Create(result.Data);

            return Result.Success(responseData);
        }
        catch (Exception ex) {
            // An exception has occurred, add some additional information to the message
            Exception exception = new("Error getting token.", ex);

            throw exception;
        }
    }
}