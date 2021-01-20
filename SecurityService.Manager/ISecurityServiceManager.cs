namespace SecurityService.Manager
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using IdentityServer4.Models;
    using Models;

    /// <summary>
    /// 
    /// </summary>
    public interface ISecurityServiceManager
    {
        #region Methods

        /// <summary>
        /// Creates the API scope.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="displayName">The display name.</param>
        /// <param name="description">The description.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<String> CreateApiScope(String name,
                                    String displayName,
                                    String description,
                                    CancellationToken cancellationToken);

        /// <summary>
        /// Gets the API scope.
        /// </summary>
        /// <param name="apiScopeName">Name of the API scope.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<ApiScope> GetApiScope(String apiScopeName, CancellationToken cancellationToken);

        /// <summary>
        /// Gets the API scopes.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<List<ApiScope>> GetApiScopes(CancellationToken cancellationToken);

        /// <summary>
        /// Creates the API resource.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="displayName">The display name.</param>
        /// <param name="description">The description.</param>
        /// <param name="secret">The secret.</param>
        /// <param name="scopes">The scopes.</param>
        /// <param name="userClaims">The user claims.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<String> CreateApiResource(String name,
                                       String displayName,
                                       String description,
                                       String secret,
                                       List<String> scopes,
                                       List<String> userClaims,
                                       CancellationToken cancellationToken);

        /// <summary>
        /// Creates the client.
        /// </summary>
        /// <param name="clientId">The client identifier.</param>
        /// <param name="secret">The secret.</param>
        /// <param name="clientName">Name of the client.</param>
        /// <param name="clientDescription">The client description.</param>
        /// <param name="allowedScopes">The allowed scopes.</param>
        /// <param name="allowedGrantTypes">The allowed grant types.</param>
        /// <param name="clientRedirectUris">The client redirect uris.</param>
        /// <param name="clientPostLogoutRedirectUris">The client post logout redirect uris.</param>
        /// <param name="requireConsent">if set to <c>true</c> [require consent].</param>
        /// <param name="allowOfflineAccess">if set to <c>true</c> [allow offline access].</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<String> CreateClient(String clientId,
                                  String secret,
                                  String clientName,
                                  String clientDescription,
                                  List<String> allowedScopes,
                                  List<String> allowedGrantTypes,
                                  List<String> clientRedirectUris,
                                  List<String> clientPostLogoutRedirectUris,
                                  Boolean requireConsent,
                                  Boolean allowOfflineAccess,
                                  CancellationToken cancellationToken);

        /// <summary>
        /// Creates the identity resource.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="displayName">The display name.</param>
        /// <param name="description">The description.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="emphasize">if set to <c>true</c> [emphasize].</param>
        /// <param name="showInDiscoveryDocument">if set to <c>true</c> [show in discovery document].</param>
        /// <param name="claims">The claims.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<String> CreateIdentityResource(String name, 
                                            String displayName,
                                            String description,
                                            Boolean required,
                                            Boolean emphasize,
                                            Boolean showInDiscoveryDocument,
                                            List<String> claims,
                                            CancellationToken cancellationToken);

        /// <summary>
        /// Creates the role.
        /// </summary>
        /// <param name="roleName">Name of the role.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<Guid> CreateRole(String roleName,
                              CancellationToken cancellationToken);

        /// <summary>
        /// Registers the user.
        /// </summary>
        /// <param name="givenName">Name of the given.</param>
        /// <param name="middleName">Name of the middle.</param>
        /// <param name="familyName">Name of the family.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="password">The password.</param>
        /// <param name="emailAddress">The email address.</param>
        /// <param name="phoneNumber">The phone number.</param>
        /// <param name="claims">The claims.</param>
        /// <param name="roles">The roles.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<Guid> CreateUser(String givenName,
                              String middleName,
                              String familyName,
                              String userName,
                              String password,
                              String emailAddress,
                              String phoneNumber,
                              Dictionary<String, String> claims,
                              List<String> roles,
                              CancellationToken cancellationToken);

        /// <summary>
        /// Gets the API resource.
        /// </summary>
        /// <param name="apiResourceName">Name of the API resource.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<ApiResource> GetApiResource(String apiResourceName,
                                         CancellationToken cancellationToken);

        /// <summary>
        /// Gets the API resources.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<List<ApiResource>> GetApiResources(CancellationToken cancellationToken);

        /// <summary>
        /// Gets the client.
        /// </summary>
        /// <param name="clientId">The client identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<Client> GetClient(String clientId,
                               CancellationToken cancellationToken);

        /// <summary>
        /// Gets the clients.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<List<Client>> GetClients(CancellationToken cancellationToken);

        /// <summary>
        /// Gets the identity resource.
        /// </summary>
        /// <param name="identityResourceName">Name of the identity resource.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<IdentityResource> GetIdentityResource(String identityResourceName,
                                                   CancellationToken cancellationToken);

        /// <summary>
        /// Gets the API resources.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<List<IdentityResource>> GetIdentityResources(CancellationToken cancellationToken);

        /// <summary>
        /// Gets the role.
        /// </summary>
        /// <param name="roleId">The role identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<RoleDetails> GetRole(Guid roleId,
                                  CancellationToken cancellationToken);

        /// <summary>
        /// Gets the roles.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<List<RoleDetails>> GetRoles(CancellationToken cancellationToken);

        /// <summary>
        /// Gets the user by user identifier.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<UserDetails> GetUser(Guid userId,
                                  CancellationToken cancellationToken);

        /// <summary>
        /// Gets the users.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<List<UserDetails>> GetUsers(String userName,
                                         CancellationToken cancellationToken);

        /// <summary>
        /// Signouts this instance.
        /// </summary>
        /// <returns></returns>
        Task Signout();

        /// <summary>
        /// Validates the credentials.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="password">The password.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<Boolean> ValidateCredentials(String userName,
                                          String password,
                                          CancellationToken cancellationToken);

        #endregion
    }
}