using System;

namespace SecurityService.BusinessLogic
{
    using System.Collections.Generic;
    using System.Net.Mime;
    using System.Threading;
    using System.Threading.Tasks;
    using Duende.IdentityServer.Models;
    using SecurityService.Models;

    public interface ISecurityServiceManager
    {
        Task SendWelcomeEmail(String userName, CancellationToken cancellationToken);

        Task<Boolean> ConfirmEmailAddress(String userName,
                                 String confirmEmailToken,
                                 CancellationToken cancellationToken);

        Task<(Boolean, String)> ChangePassword(String userName,
                                               String currentPassword,
                                               String newPassword,
                                               String clientId,
                                               CancellationToken cancellationToken);

        Task ProcessPasswordResetRequest(String username,
                                         String emailAddress,
                                         String clientId,
                                         CancellationToken cancellationToken);

        Task<String> ProcessPasswordResetConfirmation(String username,
                                              String token,
                                              String password,
                                              String clientId,
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
    }
}
