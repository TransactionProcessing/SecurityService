using SimpleResults;

namespace SecurityService.Client
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using DataTransferObjects;
    using DataTransferObjects.Requests;
    using DataTransferObjects.Responses;

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

        Task<Result<ApiResourceDetails>> GetApiResource(String apiResourceName,
                                                CancellationToken cancellationToken);

        Task<Result<ApiScopeDetails>> GetApiScope(String apiScopeName,
                                                CancellationToken cancellationToken);

        Task<Result<List<ApiResourceDetails>>> GetApiResources(CancellationToken cancellationToken);

        Task<Result<List<ApiScopeDetails>>> GetApiScopes(CancellationToken cancellationToken);

        Task<Result<ClientDetails>> GetClient(String clientId,
                                             CancellationToken cancellationToken);

        Task<Result<List<ClientDetails>>> GetClients(CancellationToken cancellationToken);

        Task<Result<IdentityResourceDetails>> GetIdentityResource(String identityResourceName,
                                                          CancellationToken cancellationToken);

        Task<Result<List<IdentityResourceDetails>>> GetIdentityResources(CancellationToken cancellationToken);

        Task<Result<RoleDetails>> GetRole(Guid roleId,
                                  CancellationToken cancellationToken);

        Task<Result<List<RoleDetails>>> GetRoles(CancellationToken cancellationToken);

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

        Task<Result<UserDetails>> GetUser(Guid userId,
                                         CancellationToken cancellationToken);
        Task<Result<List<UserDetails>>> GetUsers(String userName,
                                                CancellationToken cancellationToken);

        #endregion
    }
}