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
        
        }
}
