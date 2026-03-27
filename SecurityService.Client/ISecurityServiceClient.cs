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
        #region Methods

        Task<Result> CreateApiResource(CreateApiResourceRequest createApiResourceRequest,
                                                                  CancellationToken cancellationToken);
        Task<Result> CreateApiScope(CreateApiScopeRequest createApiScopeRequest,
                                                           CancellationToken cancellationToken);

        Task<Result> CreateClient(CreateClientRequest createClientRequest,
                                                       CancellationToken cancellationToken);

        Task<Result> CreateIdentityResource(CreateIdentityResourceRequest createIdentityResourceRequest,
                                                                    CancellationToken cancellationToken);
        Task<Result> CreateRole(CreateRoleRequest createRoleRequest,
                                            CancellationToken cancellationToken);

        Task<Result> CreateUser(CreateUserRequest createUserRequest,
                                            CancellationToken cancellationToken);

        Task<Result<ApiResourceResponse>> GetApiResource(String apiResourceName,
                                                         CancellationToken cancellationToken);

        Task<Result<ApiScopeResponse>> GetApiScope(String apiScopeName,
                                                   CancellationToken cancellationToken);

        Task<Result<List<ApiResourceResponse>>> GetApiResources(CancellationToken cancellationToken);

        Task<Result<List<ApiScopeResponse>>> GetApiScopes(CancellationToken cancellationToken);

        Task<Result<ClientResponse>> GetClient(String clientId,
                                               CancellationToken cancellationToken);

        Task<Result<List<ClientResponse>>> GetClients(CancellationToken cancellationToken);

        Task<Result<IdentityResourceResponse>> GetIdentityResource(String identityResourceName,
                                                                   CancellationToken cancellationToken);

        Task<Result<List<IdentityResourceResponse>>> GetIdentityResources(CancellationToken cancellationToken);

        Task<Result<RoleResponse>> GetRole(String roleId,
                                           CancellationToken cancellationToken);

        Task<Result<List<RoleResponse>>> GetRoles(CancellationToken cancellationToken);

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

        Task<Result<UserResponse>> GetUser(String userId,
                                           CancellationToken cancellationToken);
        Task<Result<List<UserResponse>>> GetUsers(String userName,
                                                CancellationToken cancellationToken);

        #endregion
    }
}