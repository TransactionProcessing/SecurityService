namespace SecurityService.BusinessLogic
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Security.Claims;
    using System.Text;
    using System.Text.Encodings.Web;
    using System.Threading;
    using System.Threading.Tasks;
    using DataTransferObjects.Responses;
    using Duende.IdentityServer;
    using Duende.IdentityServer.EntityFramework.DbContexts;
    using Duende.IdentityServer.EntityFramework.Mappers;
    using Duende.IdentityServer.Models;
    using Exceptions;
    using IdentityModel;
    using MessagingService.Client;
    using MessagingService.DataTransferObjects;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using Shared.Exceptions;
    using Shared.General;
    using Shared.Logger;
    using RoleDetails = Models.RoleDetails;
    using UserDetails = Models.UserDetails;

    public class SecurityServiceManager : ISecurityServiceManager
    {
        #region Fields

        /// <summary>
        /// The configuration database context resolver
        /// </summary>
        private readonly ConfigurationDbContext ConfigurationDbContext;

        private readonly IdentityServerTools IdentityServerTools;

        private readonly ServiceOptions ServiceOptions;

        private readonly IMessagingServiceClient MessagingServiceClient;

        /// <summary>
        /// The password hasher
        /// </summary>
        private readonly IPasswordHasher<IdentityUser> PasswordHasher;

        /// <summary>
        /// The role manager
        /// </summary>
        private readonly RoleManager<IdentityRole> RoleManager;

        /// <summary>
        /// The sign in manager
        /// </summary>
        private readonly SignInManager<IdentityUser> SignInManager;

        private TokenResponse TokenResponse;

        /// <summary>
        /// The user manager
        /// </summary>
        private readonly UserManager<IdentityUser> UserManager;

        #endregion

        #region Constructors

        public SecurityServiceManager(IPasswordHasher<IdentityUser> passwordHasher,
                                      UserManager<IdentityUser> userManager,
                                      RoleManager<IdentityRole> roleManager,
                                      SignInManager<IdentityUser> signInManager,
                                      ConfigurationDbContext configurationDbContext,
                                      IMessagingServiceClient messagingServiceClient,
                                      IdentityServerTools identityServerTools,
                                      ServiceOptions serviceOptions) {
            this.PasswordHasher = passwordHasher;
            this.UserManager = userManager;
            this.RoleManager = roleManager;
            this.SignInManager = signInManager;
            this.ConfigurationDbContext = configurationDbContext;
            this.MessagingServiceClient = messagingServiceClient;
            this.IdentityServerTools = identityServerTools;
            this.ServiceOptions = serviceOptions;
        }

        #endregion

        #region Methods

        public async Task<(Boolean, String)> ChangePassword(String userName,
                                                            String currentPassword,
                                                            String newPassword,
                                                            String clientId,
                                                            CancellationToken cancellationToken) {
            // Find the user based on the user name passed in
            IdentityUser user = await this.UserManager.FindByNameAsync(userName);

            if (user == null) {
                // TODO: Redirect to a success page so the user doesnt know if the username is correct or not,
                // this prevents giving away info to a potential hacker...
                // TODO: maybe log something here...
                return (false, String.Empty);
            }

            IdentityResult result = await this.UserManager.ChangePasswordAsync(user, currentPassword, newPassword);

            if (result.Succeeded == false) {
                // Log any errors
                Logger.LogWarning($"Errors during password change for user [{userName} and Client [{clientId}]");
                foreach (IdentityError identityError in result.Errors) {
                    Logger.LogWarning($"Code {identityError.Code} Description {identityError.Description}");
                }
            }

            // build the redirect uri
            Duende.IdentityServer.EntityFramework.Entities.Client client = await this.ConfigurationDbContext.Clients.SingleOrDefaultAsync(c => c.ClientId == clientId);

            if (client == null) {
                Logger.LogWarning($"Client not found for clientId {clientId}");
                // TODO: need to redirect somewhere...
                return (false, String.Empty);
            }

            Logger.LogWarning($"Client uri {client.ClientUri}");
            return (true, client.ClientUri);
        }

        public async Task<Boolean> ConfirmEmailAddress(String userName,
                                                       String confirmEmailToken,
                                                       CancellationToken cancellationToken) {
            IdentityUser identityUser = await this.UserManager.FindByNameAsync(userName);

            if (identityUser == null) {
                Logger.LogWarning($"No user found with username {userName}");
                return false;
            }

            IdentityResult result = await this.UserManager.ConfirmEmailAsync(identityUser, confirmEmailToken);

            if (result.Succeeded == false) {
                Logger.LogWarning($"Errors during confirm email for user [{userName}");
                foreach (IdentityError identityError in result.Errors) {
                    Logger.LogWarning($"Code {identityError.Code} Description {identityError.Description}");
                }
            }

            return result.Succeeded;
        }
        
        
        public async Task<String> ProcessPasswordResetConfirmation(String username,
                                                                   String token,
                                                                   String password,
                                                                   String clientId,
                                                                   CancellationToken cancellationToken) {
            // Find the user based on the user name passed in
            IdentityUser user = await this.UserManager.FindByNameAsync(username);

            if (user == null) {
                // TODO: Redirect to a success page so the user doesnt know if the username is correct or not,
                // this prevents giving away info to a potential hacker...
                // TODO: maybe log something here...
                Logger.LogWarning($"user not found for username {username}");
                return String.Empty;
            }

            IdentityResult result = await this.UserManager.ResetPasswordAsync(user, token, password);

            // handle the result... 
            if (result.Succeeded == false) {
                // Log any errors
                Logger.LogWarning($"Errors during password reset for user [{username} and Client [{clientId}]");
                foreach (IdentityError identityError in result.Errors) {
                    Logger.LogWarning($"Code {identityError.Code} Description {identityError.Description}");
                }
            }

            // build the redirect uri
            Duende.IdentityServer.EntityFramework.Entities.Client client = await this.ConfigurationDbContext.Clients.SingleOrDefaultAsync(c => c.ClientId == clientId);

            if (client == null) {
                Logger.LogWarning($"Client not found for clientId {clientId}");
                // TODO: need to redirect somewhere...
                return String.Empty;
            }

            Logger.LogWarning($"Client uri {client.ClientUri}");
            return client.ClientUri;
        }

        public async Task ProcessPasswordResetRequest(String username,
                                                      String emailAddress,
                                                      String clientId,
                                                      CancellationToken cancellationToken) {
            // Find the user based on the user name passed in
            IdentityUser user = await this.UserManager.FindByNameAsync(username);

            if (user == null) {
                // TODO: Redirect to a success page so the user doesnt know if the username is correct or not,
                // this prevents giving away info to a potential hacker...
                // TODO: maybe log something here...
                return;
            }

            // User has been found so send an email with reset details
            String resetToken = await this.UserManager.GeneratePasswordResetTokenAsync(user);
            resetToken = UrlEncoder.Default.Encode(resetToken);
            String uri = $"{this.ServiceOptions.PublicOrigin}/Account/ForgotPassword/Confirm?userName={user.UserName}&resetToken={resetToken}&clientId={clientId}";

            TokenResponse token = await this.GetToken(cancellationToken);
            SendEmailRequest emailRequest = this.BuildPasswordResetEmailRequest(user, uri);
            try {
                await this.MessagingServiceClient.SendEmail(token.AccessToken, emailRequest, cancellationToken);
            }
            catch(Exception ex) {
                Logger.LogError(ex);
            }
        }

        public async Task SendWelcomeEmail(String userName,
                                           CancellationToken cancellationToken) {
            IdentityUser i = await this.UserManager.FindByNameAsync(userName);
            await this.UserManager.RemovePasswordAsync(i);
            String generatedPassword = SecurityServiceManager.GenerateRandomPassword(this.UserManager.Options.Password);
            await this.UserManager.AddPasswordAsync(i, generatedPassword);

            // Send Email
            TokenResponse token = await this.GetToken(cancellationToken);
            SendEmailRequest emailRequest = this.BuildWelcomeEmail(i.Email, generatedPassword);
            try {
                await this.MessagingServiceClient.SendEmail(token.AccessToken, emailRequest, cancellationToken);
            }
            catch(Exception ex) {
                Logger.LogError(ex);
            }
        }
        
        private SendEmailRequest BuildEmailConfirmationRequest(IdentityUser user,
                                                               String emailConfirmationToken) {
            StringBuilder mesasgeBuilder = new StringBuilder();

            mesasgeBuilder.Append("<html>");
            mesasgeBuilder.Append("<body>");
            mesasgeBuilder.Append("<p><strong>Thank you for registering</strong></p>");
            mesasgeBuilder.Append("<p></p>");
            mesasgeBuilder.Append($"<p>Please <a href=\"{emailConfirmationToken}\">click here</a> to confirm your email address.</p>");
            mesasgeBuilder.Append("<p>Thanks for your registration.</p>");
            mesasgeBuilder.Append("</body>");
            mesasgeBuilder.Append("</html>");

            SendEmailRequest request = new() {
                                                 Body = mesasgeBuilder.ToString(),
                                                 ConnectionIdentifier = Guid.NewGuid(),
                                                 FromAddress = "golfhandicapping@btinternet.com",
                                                 IsHtml = true,
                                                 Subject = "Email Address Confirmation",
                                                 ToAddresses = new List<String> {
                                                                                    user.Email,
                                                                                    "stuart_ferguson1@outlook.com"
                                                                                }
                                             };

            return request;
        }

        private SendEmailRequest BuildPasswordResetEmailRequest(IdentityUser user,
                                                                String resetToken) {
            StringBuilder mesasgeBuilder = new StringBuilder();

            mesasgeBuilder.Append("<html>");
            mesasgeBuilder.Append("<body>");
            mesasgeBuilder.Append("<p><strong>Thanks for your password reset request</strong></p>");
            mesasgeBuilder.Append("<p></p>");
            mesasgeBuilder.Append($"<p>Please <a href=\"{resetToken}\">click here</a> to confirm this password reset was from you.</p>");
            mesasgeBuilder.Append("<p>Thanks for your password reset request.</p>");
            mesasgeBuilder.Append("</body>");
            mesasgeBuilder.Append("</html>");

            SendEmailRequest request = new() {
                                                 Body = mesasgeBuilder.ToString(),
                                                 ConnectionIdentifier = Guid.NewGuid(),
                                                 FromAddress = "golfhandicapping@btinternet.com",
                                                 IsHtml = true,
                                                 Subject = "Password Reset Requested",
                                                 ToAddresses = new List<String> {
                                                                                    user.Email,
                                                                                    "stuart_ferguson1@outlook.com"
                                                                                }
                                             };

            return request;
        }

        private SendEmailRequest BuildWelcomeEmail(String emailAddress,
                                                   String password) {
            StringBuilder mesasgeBuilder = new StringBuilder();
            mesasgeBuilder.AppendLine("<html><body>");
            mesasgeBuilder.AppendLine("<p>Welcome to Transaction Processing System</p>");
            mesasgeBuilder.AppendLine("<p></p>");
            mesasgeBuilder.AppendLine("<p>Please find below your user details:</p>");
            mesasgeBuilder.AppendLine("<table>");
            mesasgeBuilder.AppendLine("<tr><td><strong>User Name</strong></td></tr>");
            mesasgeBuilder.AppendLine($"<tr><td id=\"username\">{emailAddress}</td></tr>");
            mesasgeBuilder.AppendLine("<tr><td><strong>Password</strong></td></tr>");
            mesasgeBuilder.AppendLine($"<tr><td id=\"password\">{password}</td></tr>");
            mesasgeBuilder.AppendLine("</table>");
            mesasgeBuilder.AppendLine("</body></html>");

            SendEmailRequest request = new() {
                                                 Body = mesasgeBuilder.ToString(),
                                                 ConnectionIdentifier = Guid.NewGuid(),
                                                 FromAddress = "golfhandicapping@btinternet.com",
                                                 IsHtml = true,
                                                 Subject = "Welcome to Transaction Processing",
                                                 ToAddresses = new List<String> {
                                                                                    emailAddress,
                                                                                    "stuart_ferguson1@outlook.com"
                                                                                }
                                             };

            return request;
        }

        /// <summary>
        /// Converts the users claims.
        /// </summary>
        /// <param name="identityUser">The identity user.</param>
        /// <returns></returns>
        private async Task<Dictionary<String, String>> ConvertUsersClaims(IdentityUser identityUser) {
            Dictionary<String, String> response = new Dictionary<String, String>();
            IList<Claim> claims = await this.UserManager.GetClaimsAsync(identityUser);
            foreach (Claim claim in claims) {
                response.Add(claim.Type, claim.Value);
            }

            return response;
        }

        /// <summary>
        /// Converts the users roles.
        /// </summary>
        /// <param name="identityUser">The identity user.</param>
        /// <returns></returns>
        private async Task<List<String>> ConvertUsersRoles(IdentityUser identityUser) {
            IList<String> roles = await this.UserManager.GetRolesAsync(identityUser);
            return roles.ToList();
        }

        private static String GenerateRandomPassword(Microsoft.AspNetCore.Identity.PasswordOptions opts = null) {
            if (opts == null)
                opts = new Microsoft.AspNetCore.Identity.PasswordOptions
                {
                                               RequiredLength = 8,
                                               RequiredUniqueChars = 4,
                                               RequireDigit = true,
                                               RequireLowercase = true,
                                               RequireNonAlphanumeric = true,
                                               RequireUppercase = true
                                           };

            String[] randomChars = {
                                       "ABCDEFGHJKLMNOPQRSTUVWXYZ", // uppercase 
                                       "abcdefghijkmnopqrstuvwxyz", // lowercase
                                       "0123456789", // digits
                                       "!@$?_-" // non-alphanumeric
                                   };

            Random rand = new Random(Environment.TickCount);
            List<Char> chars = new List<Char>();

            if (opts.RequireUppercase)
                chars.Insert(rand.Next(0, chars.Count), randomChars[0][rand.Next(0, randomChars[0].Length)]);

            if (opts.RequireLowercase)
                chars.Insert(rand.Next(0, chars.Count), randomChars[1][rand.Next(0, randomChars[1].Length)]);

            if (opts.RequireDigit)
                chars.Insert(rand.Next(0, chars.Count), randomChars[2][rand.Next(0, randomChars[2].Length)]);

            if (opts.RequireNonAlphanumeric)
                chars.Insert(rand.Next(0, chars.Count), randomChars[3][rand.Next(0, randomChars[3].Length)]);

            for (Int32 i = chars.Count; i < opts.RequiredLength || chars.Distinct().Count() < opts.RequiredUniqueChars; i++) {
                String rcs = randomChars[rand.Next(0, randomChars.Length)];
                chars.Insert(rand.Next(0, chars.Count), rcs[rand.Next(0, rcs.Length)]);
            }

            return new String(chars.ToArray());
        }

        private async Task<TokenResponse> GetToken(CancellationToken cancellationToken) {
            // Get a token to talk to the estate service
            String clientId = this.ServiceOptions.ClientId;
            String clientSecret = this.ServiceOptions.ClientSecret;

            Logger.LogInformation($"Client Id is {clientId}");
            Logger.LogInformation($"Client Secret is {clientSecret}");

            if (this.TokenResponse == null) {
                String clientToken = await this.IdentityServerTools.IssueClientJwtAsync(clientId, 3600);
                this.TokenResponse = TokenResponse.Create(clientToken, null, 3600, DateTimeOffset.Now, DateTimeOffset.Now.AddSeconds(3600));
                Logger.LogInformation($"Token is {this.TokenResponse.AccessToken}");
                return this.TokenResponse;
            }

            if (this.TokenResponse.Expires.UtcDateTime.Subtract(DateTime.UtcNow) < TimeSpan.FromMinutes(2)) {
                Logger.LogInformation($"Token is about to expire at {this.TokenResponse.Expires.DateTime:O}");
                Logger.LogInformation($"Token is {this.TokenResponse.AccessToken}");
                return this.TokenResponse;
            }

            return this.TokenResponse;
        }

        /// <summary>
        /// Validates the grant types.
        /// </summary>
        /// <param name="allowedGrantTypes">The allowed grant types.</param>
        /// <exception cref="ArgumentException">allowedGrantTypes - The grant types [{string.Join(", ", invalidGrantTypes)}] are not valid to create a new client</exception>
        private void ValidateGrantTypes(List<String> allowedGrantTypes) {
            // Get a list of valid grant types
            List<String> validTypesList = new List<String>();

            validTypesList.Add(GrantType.AuthorizationCode);
            validTypesList.Add(GrantType.ClientCredentials);
            validTypesList.Add(GrantType.DeviceFlow);
            validTypesList.Add(GrantType.Hybrid);
            validTypesList.Add(GrantType.Implicit);
            validTypesList.Add(GrantType.ResourceOwnerPassword);

            List<String> invalidGrantTypes = allowedGrantTypes.Where(a => validTypesList.All(v => v != a)).ToList();

            if (invalidGrantTypes.Any()) {
                throw new ArgumentException(nameof(allowedGrantTypes), $"The grant types [{String.Join(", ", invalidGrantTypes)}] are not valid to create a new client");
            }
        }

        #endregion
    }
}