namespace SecurityService.Manager
{
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using DataTransferObjects;
    using Microsoft.AspNetCore.Identity;

    /// <summary>
    /// 
    /// </summary>
    public interface ISecurityServiceManager
    {
        #region Methods

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
        /// Registers the user.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<Guid> CreateUser(CreateUserRequest request,
                                                CancellationToken cancellationToken);
        
        #endregion
    }
}