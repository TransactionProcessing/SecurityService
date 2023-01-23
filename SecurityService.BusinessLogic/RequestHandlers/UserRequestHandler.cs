using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecurityService.BusinessLogic.RequestHandlers
{
    using System.Data;
    using System.Net.Mail;
    using System.Security.Claims;
    using System.Text.Encodings.Web;
    using System.Threading;
    using DataTransferObjects.Responses;
    using Duende.IdentityServer;
    using Duende.IdentityServer.Configuration;
    using Duende.IdentityServer.EntityFramework.DbContexts;
    using Duende.IdentityServer.EntityFramework.Entities;
    using Duende.IdentityServer.Extensions;
    using Duende.IdentityServer.Models;
    using Duende.IdentityServer.Services;
    using IdentityModel;
    using MediatR;
    using MessagingService.Client;
    using MessagingService.DataTransferObjects;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Requests;
    using SecurityService.BusinessLogic.Exceptions;
    using Shared.Exceptions;
    using Shared.General;
    using Shared.Logger;
    using Client = Duende.IdentityServer.EntityFramework.Entities.Client;
    using UserDetails = Models.UserDetails;

    public class UserRequestHandler : IRequestHandler<CreateUserRequest>,
                                      IRequestHandler<GetUserRequest, UserDetails>,
                                      IRequestHandler<GetUsersRequest, List<UserDetails>>,
    IRequestHandler<ChangeUserPasswordRequest, (Boolean,String)>,
                                      IRequestHandler<ConfirmUserEmailAddressRequest, Boolean>,
                                      IRequestHandler<ProcessPasswordResetConfirmationRequest,String>,
                                      IRequestHandler<ProcessPasswordResetRequest>,
    IRequestHandler<SendWelcomeEmailRequest>
    {
        private readonly IPasswordHasher<IdentityUser> PasswordHasher;

        private readonly UserManager<IdentityUser> UserManager;

        private readonly ServiceOptions ServiceOptions;

        private readonly IMessagingServiceClient MessagingServiceClient;

        private readonly IdentityServerTools IdentityServerTools;

        private readonly ConfigurationDbContext ConfigurationDbContext;

        public UserRequestHandler(IPasswordHasher<IdentityUser> passwordHasher,
                                  UserManager<IdentityUser> userManager,
                                  ServiceOptions serviceOptions,
                                  IMessagingServiceClient messagingServiceClient,
                                  IdentityServerTools identityServerTools,
                                  ConfigurationDbContext configurationDbContext)
        {
            this.PasswordHasher = passwordHasher;
            this.UserManager = userManager;
            this.ServiceOptions = serviceOptions;
            this.MessagingServiceClient = messagingServiceClient;
            this.IdentityServerTools = identityServerTools;
            this.ConfigurationDbContext = configurationDbContext;
        }
        public async Task<Unit> Handle(CreateUserRequest request, CancellationToken cancellationToken){

            // request is valid now add the user
            IdentityUser newIdentityUser = new IdentityUser
            {
                Id = request.UserId.ToString(),
                Email = request.EmailAddress,
                UserName = request.UserName,
                NormalizedEmail = request.EmailAddress.ToUpper(),
                NormalizedUserName = request.UserName.ToUpper(),
                SecurityStamp = Guid.NewGuid().ToString("D"),
                PhoneNumber = request.PhoneNumber,
            };

            String passwordValue = String.IsNullOrEmpty(request.Password) ? GenerateRandomPassword(this.UserManager.Options.Password) : request.Password;

            // Hash the default password
            newIdentityUser.PasswordHash =
                this.PasswordHasher.HashPassword(newIdentityUser, passwordValue);

            if (String.IsNullOrEmpty(newIdentityUser.PasswordHash))
            {
                throw new IdentityResultException("Error generating password hash value, hash was null or empty", IdentityResult.Failed());
            }

            // Default all IdentityResults to failed
            IdentityResult createResult = IdentityResult.Failed();
            IdentityResult addRolesResult = IdentityResult.Failed();
            IdentityResult addClaimsResult = IdentityResult.Failed();

            try
            {
                // Create the User
                createResult = await this.UserManager.CreateAsync(newIdentityUser);

                if (!createResult.Succeeded)
                {
                    throw new IdentityResultException($"Error creating user {newIdentityUser.UserName}", createResult);
                }

                // Add the requested roles to the user
                if (request.Roles != null && request.Roles.Any())
                {
                    addRolesResult = await this.UserManager.AddToRolesAsync(newIdentityUser, request.Roles);

                    if (!addRolesResult.Succeeded)
                    {
                        throw new IdentityResultException($"Error adding roles [{String.Join(",", request.Roles)}] to user {newIdentityUser.UserName}", addRolesResult);
                    }
                }
                else
                {
                    addRolesResult = IdentityResult.Success;
                }

                // Add the requested claims
                List<Claim> claimsToAdd = new List<Claim>();
                if (request.Claims != null)
                {
                    foreach (KeyValuePair<String, String> claim in request.Claims)
                    {
                        claimsToAdd.Add(new Claim(claim.Key, claim.Value));
                    }
                }

                // Add the email address and role as claims
                if (request.Roles != null)
                {
                    foreach (String requestRole in request.Roles)
                    {
                        claimsToAdd.Add(new Claim(JwtClaimTypes.Role, requestRole));
                    }
                }

                claimsToAdd.Add(new Claim(JwtClaimTypes.Email, request.EmailAddress));
                claimsToAdd.Add(new Claim(JwtClaimTypes.GivenName, request.GivenName));
                claimsToAdd.Add(new Claim(JwtClaimTypes.FamilyName, request.FamilyName));

                if (String.IsNullOrEmpty(request.MiddleName) == false)
                {
                    claimsToAdd.Add(new Claim(JwtClaimTypes.MiddleName, request.MiddleName));
                }

                addClaimsResult = await this.UserManager.AddClaimsAsync(newIdentityUser, claimsToAdd);

                if (!addClaimsResult.Succeeded)
                {
                    List<String> claimList = new List<String>();
                    claimsToAdd.ForEach(c => claimList.Add($"Name: {c.Type} Value: {c.Value}"));
                    throw new IdentityResultException($"Error adding claims [{String.Join(",", claimsToAdd)}] to user {newIdentityUser.UserName}", addClaimsResult);
                }

                // If we are here we have created the user
                String confirmationToken = await this.UserManager.GenerateEmailConfirmationTokenAsync(newIdentityUser);
                confirmationToken = UrlEncoder.Default.Encode(confirmationToken);
                String uri = $"{this.ServiceOptions.PublicOrigin}/Account/EmailConfirmation/Confirm?userName={newIdentityUser.UserName}&confirmationToken={confirmationToken}";

                TokenResponse token = await this.GetToken(cancellationToken);
                SendEmailRequest emailRequest = this.BuildEmailConfirmationRequest(newIdentityUser, uri);
                try
                {
                    await this.MessagingServiceClient.SendEmail(token.AccessToken, emailRequest, cancellationToken);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                }
            }
            finally
            {
                // Do some cleanup here (if the create was successful but one fo the other steps failed)
                if ((createResult == IdentityResult.Success) && (!addRolesResult.Succeeded || !addClaimsResult.Succeeded))
                {
                    // User has been created so need to remove this
                    IdentityResult deleteResult = await this.UserManager.DeleteAsync(newIdentityUser);

                    if (!deleteResult.Succeeded)
                    {
                        throw new IdentityResultException($"Error deleting user {newIdentityUser.UserName} as part of cleanup", deleteResult);
                    }
                }
            }

            return Unit.Value;
        }

        private TokenResponse TokenResponse;

        private async Task<TokenResponse> GetToken(CancellationToken cancellationToken)
        {
            // Get a token to talk to the estate service
            String clientId = this.ServiceOptions.ClientId;
            String clientSecret = this.ServiceOptions.ClientSecret;

            Logger.LogInformation($"Client Id is {clientId}");
            Logger.LogInformation($"Client Secret is {clientSecret}");

            if (this.TokenResponse == null)
            {
                String clientToken = await this.IdentityServerTools.IssueClientJwtAsync(clientId, 3600);
                this.TokenResponse = TokenResponse.Create(clientToken, null, 3600, DateTimeOffset.Now, DateTimeOffset.Now.AddSeconds(3600));
                Logger.LogInformation($"Token is {this.TokenResponse.AccessToken}");
                return this.TokenResponse;
            }

            if (this.TokenResponse.Expires.UtcDateTime.Subtract(DateTime.UtcNow) < TimeSpan.FromMinutes(2))
            {
                Logger.LogInformation($"Token is about to expire at {this.TokenResponse.Expires.DateTime:O}");
                Logger.LogInformation($"Token is {this.TokenResponse.AccessToken}");
                return this.TokenResponse;
            }

            return this.TokenResponse;
        }


        public async Task<UserDetails> Handle(GetUserRequest request, CancellationToken cancellationToken){
            Guard.ThrowIfInvalidGuid(request.UserId, nameof(request.UserId));

            IdentityUser user = await this.UserManager.FindByIdAsync(request.UserId.ToString());

            if (user == null)
            {
                throw new NotFoundException($"No user found with user Id {request.UserId}");
            }

            UserDetails response = new UserDetails();
            response.Email = user.Email;
            response.PhoneNumber = user.PhoneNumber;
            response.UserId = request.UserId;
            response.SubjectId = request.UserId.ToString();
            response.Username = user.UserName;

            // Get the users roles
            response.Roles = await this.ConvertUsersRoles(user);

            // Get the users claims
            response.Claims = await this.ConvertUsersClaims(user);

            return response;
        }

        // TODO: Another request ?
        private async Task<Dictionary<String, String>> ConvertUsersClaims(IdentityUser identityUser)
        {
            Dictionary<String, String> response = new Dictionary<String, String>();
            IList<Claim> claims = await this.UserManager.GetClaimsAsync(identityUser);
            foreach (Claim claim in claims)
            {
                response.Add(claim.Type, claim.Value);
            }

            return response;
        }

        // TODO: Another request ?
        private async Task<List<String>> ConvertUsersRoles(IdentityUser identityUser)
        {
            IList<String> roles = await this.UserManager.GetRolesAsync(identityUser);
            return roles.ToList();
        }

        public async Task<List<UserDetails>> Handle(GetUsersRequest request, CancellationToken cancellationToken){
            List<UserDetails> response = new List<UserDetails>();

            IQueryable<IdentityUser> query = this.UserManager.Users;

            if (String.IsNullOrEmpty(request.UserName) == false)
            {
                query = query.Where(u => u.UserName.Contains(request.UserName));
            }

            List<IdentityUser> users = await query.ToListAsyncSafe(cancellationToken);

            foreach (IdentityUser identityUser in users)
            {
                Dictionary<String, String> claims = await this.ConvertUsersClaims(identityUser);
                List<String> roles = await this.ConvertUsersRoles(identityUser);

                response.Add(new UserDetails
                             {
                                 UserId = Guid.Parse(identityUser.Id),
                                 SubjectId = identityUser.Id,
                                 Username = identityUser.UserName,
                                 Claims = claims,
                                 Email = identityUser.Email,
                                 PhoneNumber = identityUser.PhoneNumber,
                                 Roles = roles
                             });
            }

            return response;
        }

        private SendEmailRequest BuildEmailConfirmationRequest(IdentityUser user,
                                                               String emailConfirmationToken)
        {
            StringBuilder mesasgeBuilder = new StringBuilder();

            mesasgeBuilder.Append("<html>");
            mesasgeBuilder.Append("<body>");
            mesasgeBuilder.Append("<p><strong>Thank you for registering</strong></p>");
            mesasgeBuilder.Append("<p></p>");
            mesasgeBuilder.Append($"<p>Please <a href=\"{emailConfirmationToken}\">click here</a> to confirm your email address.</p>");
            mesasgeBuilder.Append("<p>Thanks for your registration.</p>");
            mesasgeBuilder.Append("</body>");
            mesasgeBuilder.Append("</html>");

            SendEmailRequest request = new()
            {
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
                                                                String resetToken)
        {
            StringBuilder mesasgeBuilder = new StringBuilder();

            mesasgeBuilder.Append("<html>");
            mesasgeBuilder.Append("<body>");
            mesasgeBuilder.Append("<p><strong>Thanks for your password reset request</strong></p>");
            mesasgeBuilder.Append("<p></p>");
            mesasgeBuilder.Append($"<p>Please <a href=\"{resetToken}\">click here</a> to confirm this password reset was from you.</p>");
            mesasgeBuilder.Append("<p>Thanks for your password reset request.</p>");
            mesasgeBuilder.Append("</body>");
            mesasgeBuilder.Append("</html>");

            SendEmailRequest request = new()
            {
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
                                                   String password)
        {
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

            SendEmailRequest request = new()
            {
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

        private static String GenerateRandomPassword(Microsoft.AspNetCore.Identity.PasswordOptions opts = null)
        {
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

            for (Int32 i = chars.Count; i < opts.RequiredLength || chars.Distinct().Count() < opts.RequiredUniqueChars; i++)
            {
                String rcs = randomChars[rand.Next(0, randomChars.Length)];
                chars.Insert(rand.Next(0, chars.Count), rcs[rand.Next(0, rcs.Length)]);
            }

            return new String(chars.ToArray());
        }

        public async Task<(Boolean, String)> Handle(ChangeUserPasswordRequest request, CancellationToken cancellationToken){
            // Find the user based on the user name passed in
            IdentityUser user = await this.UserManager.FindByNameAsync(request.UserName);

            if (user == null)
            {
                // TODO: Redirect to a success page so the user doesnt know if the username is correct or not,
                // this prevents giving away info to a potential hacker...
                // TODO: maybe log something here...
                return (false, String.Empty);
            }

            IdentityResult result = await this.UserManager.ChangePasswordAsync(user, request.CurrentPassword,
                                                                               request.NewPassword);

            if (result.Succeeded == false)
            {
                // Log any errors
                Logger.LogWarning($"Errors during password change for user [{request.UserName} and Client [{request.ClientId}]");
                foreach (IdentityError identityError in result.Errors)
                {
                    Logger.LogWarning($"Code {identityError.Code} Description {identityError.Description}");
                }
            }

            // build the redirect uri
            Client client = await this.ConfigurationDbContext.Clients.SingleOrDefaultAsync(c => c.ClientId == request.ClientId, cancellationToken:cancellationToken);

            if (client == null)
            {
                Logger.LogWarning($"Client not found for clientId {request.ClientId}");
                // TODO: need to redirect somewhere...
                return (false, String.Empty);
            }

            Logger.LogWarning($"Client uri {client.ClientUri}");
            return (true, client.ClientUri);
        }

        public async Task<Boolean> Handle(ConfirmUserEmailAddressRequest request, CancellationToken cancellationToken){
            IdentityUser identityUser = await this.UserManager.FindByNameAsync(request.UserName);

            if (identityUser == null)
            {
                Logger.LogWarning($"No user found with username {request.UserName}");
                return false;
            }

            IdentityResult result = await this.UserManager.ConfirmEmailAsync(identityUser, request.ConfirmEmailToken);

            if (result.Succeeded == false)
            {
                Logger.LogWarning($"Errors during confirm email for user [{request.UserName}");
                foreach (IdentityError identityError in result.Errors)
                {
                    Logger.LogWarning($"Code {identityError.Code} Description {identityError.Description}");
                }
            }

            return result.Succeeded;
        }

        public async Task<String> Handle(ProcessPasswordResetConfirmationRequest request, CancellationToken cancellationToken){
            // Find the user based on the user name passed in
            IdentityUser user = await this.UserManager.FindByNameAsync(request.Username);

            if (user == null)
            {
                // TODO: Redirect to a success page so the user doesnt know if the username is correct or not,
                // this prevents giving away info to a potential hacker...
                // TODO: maybe log something here...
                Logger.LogWarning($"user not found for username {request.Username}");
                return String.Empty;
            }

            IdentityResult result = await this.UserManager.ResetPasswordAsync(user, request.Token, request.Password);

            // handle the result... 
            if (result.Succeeded == false)
            {
                // Log any errors
                Logger.LogWarning($"Errors during password reset for user [{request.Username} and Client [{request.ClientId}]");
                foreach (IdentityError identityError in result.Errors)
                {
                    Logger.LogWarning($"Code {identityError.Code} Description {identityError.Description}");
                }
            }

            // build the redirect uri
            Client client = await this.ConfigurationDbContext.Clients.SingleOrDefaultAsync(c => c.ClientId == request.ClientId, cancellationToken:cancellationToken);

            if (client == null)
            {
                Logger.LogWarning($"Client not found for clientId {request.ClientId}");
                // TODO: need to redirect somewhere...
                return String.Empty;
            }

            Logger.LogWarning($"Client uri {client.ClientUri}");
            return client.ClientUri;
        }

        public async Task<Unit> Handle(ProcessPasswordResetRequest request, CancellationToken cancellationToken){
            // Find the user based on the user name passed in
            IdentityUser user = await this.UserManager.FindByNameAsync(request.Username);

            if (user == null)
            {
                // TODO: Redirect to a success page so the user doesnt know if the username is correct or not,
                // this prevents giving away info to a potential hacker...
                // TODO: maybe log something here...
                return Unit.Value;
            }

            // User has been found so send an email with reset details
            String resetToken = await this.UserManager.GeneratePasswordResetTokenAsync(user);
            resetToken = UrlEncoder.Default.Encode(resetToken);
            String uri = $"{this.ServiceOptions.PublicOrigin}/Account/ForgotPassword/Confirm?userName={user.UserName}&resetToken={resetToken}&clientId={request.ClientId}";

            TokenResponse token = await this.GetToken(cancellationToken);
            SendEmailRequest emailRequest = this.BuildPasswordResetEmailRequest(user, uri);
            try
            {
                await this.MessagingServiceClient.SendEmail(token.AccessToken, emailRequest, cancellationToken);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }

            return Unit.Value;
        }

        public async Task<Unit> Handle(SendWelcomeEmailRequest request, CancellationToken cancellationToken){
            IdentityUser i = await this.UserManager.FindByNameAsync(request.UserName);
            await this.UserManager.RemovePasswordAsync(i);
            String generatedPassword = UserRequestHandler.GenerateRandomPassword(this.UserManager.Options.Password);
            await this.UserManager.AddPasswordAsync(i, generatedPassword);

            // Send Email
            TokenResponse token = await this.GetToken(cancellationToken);
            SendEmailRequest emailRequest = this.BuildWelcomeEmail(i.Email, generatedPassword);
            try
            {
                await this.MessagingServiceClient.SendEmail(token.AccessToken, emailRequest, cancellationToken);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }

            return Unit.Value;
        }
    }
}
