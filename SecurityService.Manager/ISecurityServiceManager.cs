namespace SecurityService.Manager
{
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using IdentityServer4.Models;
    using Microsoft.AspNetCore.Identity;
    using Models;

    /// <summary>
    /// 
    /// </summary>
    public interface ISecurityServiceManager
    {
        #region Methods

        /// <summary>
        /// Creates the client.
        /// </summary>
        /// <param name="clientId">The client identifier.</param>
        /// <param name="secret">The secret.</param>
        /// <param name="clientName">Name of the client.</param>
        /// <param name="clientDescription">The client description.</param>
        /// <param name="allowedScopes">The allowed scopes.</param>
        /// <param name="allowedGrantTypes">The allowed grant types.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<String> CreateClient(String clientId,
                                  String secret,
                                  String clientName,
                                  String clientDescription,
                                  List<String> allowedScopes,
                                  List<String> allowedGrantTypes,
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
        /// Gets the user by user identifier.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<UserDetails> GetUser(Guid userId, CancellationToken cancellationToken);

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

        #endregion
    }
}