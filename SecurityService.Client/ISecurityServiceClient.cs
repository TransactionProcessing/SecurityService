using SimpleResults;

namespace SecurityService.Client
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using DataTransferObjects;

    /// <summary>
    /// 
    /// </summary>
    public interface ISecurityServiceClient
    {        
        Task<Result> CreateApiResource(String accessToken, CreateApiResourceRequest createApiResourceRequest, CancellationToken cancellationToken);
        Task<Result> CreateApiScope(String accessToken, CreateApiScopeRequest createApiScopeRequest,CancellationToken cancellationToken);

        Task<Result> CreateClient(String accessToken, CreateClientRequest createClientRequest,CancellationToken cancellationToken);

        Task<Result> CreateIdentityResource(String accessToken, CreateIdentityResourceRequest createIdentityResourceRequest,CancellationToken cancellationToken);
        Task<Result> CreateRole(String accessToken, CreateRoleRequest createRoleRequest,CancellationToken cancellationToken);

        Task<Result> CreateUser(String accessToken, CreateUserRequest createUserRequest, CancellationToken cancellationToken);

        Task<Result> RequireUserMfa(String accessToken, String userId, CancellationToken cancellationToken);
        Task<Result> RemoveUserMfa(String accessToken, String userId, CancellationToken cancellationToken);
        Task<Result> RequireRoleMfa(String accessToken, String roleId, CancellationToken cancellationToken);
        Task<Result> RemoveRoleMfa(String accessToken, String roleId, CancellationToken cancellationToken);

        Task<Result<ApiResourceResponse>> GetApiResource(String accessToken, String apiResourceName,CancellationToken cancellationToken);

        Task<Result<ApiScopeResponse>> GetApiScope(String accessToken, String apiScopeName,CancellationToken cancellationToken);

        Task<Result<List<ApiResourceResponse>>> GetApiResources(String accessToken, CancellationToken cancellationToken);

        Task<Result<List<ApiScopeResponse>>> GetApiScopes(String accessToken, CancellationToken cancellationToken);

        Task<Result<ClientResponse>> GetClient(String accessToken, String clientId,CancellationToken cancellationToken);

        Task<Result<List<ClientResponse>>> GetClients(String accessToken, CancellationToken cancellationToken);

        Task<Result<IdentityResourceResponse>> GetIdentityResource(String accessToken, String identityResourceName,CancellationToken cancellationToken);

        Task<Result<List<IdentityResourceResponse>>> GetIdentityResources(String accessToken, CancellationToken cancellationToken);

        Task<Result<RoleResponse>> GetRole(String accessToken, String roleId,CancellationToken cancellationToken);

        Task<Result<List<RoleResponse>>> GetRoles(String accessToken, CancellationToken cancellationToken);

        Task<Result<TokenResponse>> GetToken(String username,
                                            String password,
                                            String clientId,
                                            String clientSecret,
                                            CancellationToken cancellationToken);

        Task<Result<TokenResponse>> GetToken(String clientId,
                                            String clientSecret,
                                            CancellationToken cancellationToken);

        Task<Result<TokenResponse>> GetToken(String clientId,
                                            String clientSecret,
                                            String refreshToken,
                                            CancellationToken cancellationToken);

        Task<Result<UserResponse>> GetUser(String accessToken, String userId,CancellationToken cancellationToken);
        Task<Result<List<UserResponse>>> GetUsers(String accessToken, String userName,CancellationToken cancellationToken);
    }
}
