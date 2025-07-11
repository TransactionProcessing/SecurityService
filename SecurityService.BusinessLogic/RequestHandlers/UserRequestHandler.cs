using Duende.IdentityModel;
using Shared.Results;

namespace SecurityService.BusinessLogic.RequestHandlers{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Text;
    using System.Text.Encodings.Web;
    using System.Threading;
    using System.Threading.Tasks;
    using DataTransferObjects.Responses;
    using Duende.IdentityServer;
    using Duende.IdentityServer.EntityFramework.DbContexts;
    using Duende.IdentityServer.EntityFramework.Entities;
    using MediatR;
    using MessagingService.Client;
    using MessagingService.DataTransferObjects;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using Requests;
    using SecurityService.Models;
    using Shared.Logger;
    using SimpleResults;
    using UserDetails = Models.UserDetails;

    public class UserRequestHandler : IRequestHandler<SecurityServiceCommands.CreateUserCommand, Result>,
                                      IRequestHandler<SecurityServiceQueries.GetUserQuery, Result<UserDetails>>,
                                      IRequestHandler<SecurityServiceQueries.GetUsersQuery, Result<List<UserDetails>>>,
                                      IRequestHandler<SecurityServiceCommands.ChangeUserPasswordCommand, Result<ChangeUserPasswordResult>>,
                                      IRequestHandler<SecurityServiceCommands.ConfirmUserEmailAddressCommand, Result>,
                                      IRequestHandler<SecurityServiceCommands.ProcessPasswordResetConfirmationCommand, Result<String>>,
                                      IRequestHandler<SecurityServiceCommands.ProcessPasswordResetRequestCommand, Result>,
                                      IRequestHandler<SecurityServiceCommands.SendWelcomeEmailCommand, Result> {
        #region Fields

        private readonly ConfigurationDbContext ConfigurationDbContext;

        private readonly IdentityServerTools IdentityServerTools;

        private readonly IMessagingServiceClient MessagingServiceClient;

        private readonly IPasswordHasher<ApplicationUser> PasswordHasher;

        private readonly ServiceOptions ServiceOptions;

        private TokenResponse TokenResponse;

        private readonly UserManager<ApplicationUser> UserManager;

        #endregion

        #region Constructors

        public UserRequestHandler(IPasswordHasher<ApplicationUser> passwordHasher,
                                  UserManager<ApplicationUser> userManager,
                                  ServiceOptions serviceOptions,
                                  IMessagingServiceClient messagingServiceClient,
                                  IdentityServerTools identityServerTools,
                                  ConfigurationDbContext configurationDbContext){
            this.PasswordHasher = passwordHasher;
            this.UserManager = userManager;
            this.ServiceOptions = serviceOptions;
            this.MessagingServiceClient = messagingServiceClient;
            this.IdentityServerTools = identityServerTools;
            this.ConfigurationDbContext = configurationDbContext;
        }

        #endregion

        #region Methods

        public async Task<Result> Handle(SecurityServiceCommands.CreateUserCommand command,
                                         CancellationToken cancellationToken) {
            // request is valid now add the user
            ApplicationUser newIdentityUser = new() {
                Id = command.UserId.ToString(),
                Email = command.EmailAddress,
                UserName = command.UserName,
                NormalizedEmail = command.EmailAddress.ToUpper(),
                NormalizedUserName = command.UserName.ToUpper(),
                SecurityStamp = Guid.NewGuid().ToString("D"),
                PhoneNumber = command.PhoneNumber,
                RegistrationDateTime = DateTime.Now
            };

            String passwordValue = String.IsNullOrEmpty(command.Password) ? UserRequestHandler.GenerateRandomPassword(this.UserManager.Options.Password) : command.Password;
            
            // Hash the default password
            newIdentityUser.PasswordHash = this.PasswordHasher.HashPassword(newIdentityUser, passwordValue);

            if (String.IsNullOrEmpty(newIdentityUser.PasswordHash)) {
                return Result.Failure("Error generating password hash value, hash was null or empty");
            }

            // Create the User
            Result createResult = await this.CreateUser(newIdentityUser);
            Result addRolesToUserResult = await this.AddRolesToUser(newIdentityUser, command.Roles);
            Result addClaimsToUserResult = await this.AddClaimsToUser(newIdentityUser, command);

            Result sendEmailResult;
            if (createResult.IsSuccess && addRolesToUserResult.IsSuccess && addClaimsToUserResult.IsSuccess) {
                // If we are here we have created the user
                String confirmationToken = await this.UserManager.GenerateEmailConfirmationTokenAsync(newIdentityUser);
                confirmationToken = UrlEncoder.Default.Encode(confirmationToken);
                String uri = $"{this.ServiceOptions.PublicOrigin}/Account/EmailConfirmation/Confirm?userName={newIdentityUser.UserName}&confirmationToken={confirmationToken}";

                TokenResponse token = await this.GetToken(cancellationToken);
                SendEmailRequest emailRequest = this.BuildEmailConfirmationRequest(newIdentityUser, uri);
                sendEmailResult = await this.MessagingServiceClient.SendEmail(token.AccessToken, emailRequest, cancellationToken);
                // TODO: not so fussed if this fails, maybe just some logging that can be alerted on???
            }

            if (createResult.IsFailed || addRolesToUserResult.IsFailed || addClaimsToUserResult.IsFailed) {
                // User has been created so need to remove this
                IdentityResult deleteResult = await this.UserManager.DeleteAsync(newIdentityUser);

                if (deleteResult.Succeeded == false) {
                    return Result.Failure($"Error deleting user {newIdentityUser.UserName} as part of cleanup {deleteResult}");
                }

                return Result.Failure($"At least one part of the user creation failed - createResult: {createResult.IsSuccess} addRolesToUserResult: {addRolesToUserResult.IsSuccess} addClaimsToUserResult: {addClaimsToUserResult.IsSuccess}");
            }

            return Result.Success();
        }

        private async Task<Result> CreateUser(ApplicationUser newIdentityUser) {
            var createResult = await this.UserManager.CreateAsync(newIdentityUser);

            if (!createResult.Succeeded)
            {
                return Result.Failure($"Error creating user {newIdentityUser.UserName} {createResult}");
            }
            return Result.Success();
        }

        private async Task<Result> AddRolesToUser(ApplicationUser newIdentityUser, List<String> roles) {
            // Add the requested roles to the user
            if (roles != null && roles.Any())
            {
                IdentityResult addRolesResult = await this.UserManager.AddToRolesAsync(newIdentityUser, roles);

                if (!addRolesResult.Succeeded)
                {
                    return Result.Failure($"Error adding roles [{String.Join(",", roles)}] to user {newIdentityUser.UserName} {addRolesResult}");
                }
            }

            return Result.Success();
        }

        private async Task<Result> AddClaimsToUser(ApplicationUser newIdentityUser, SecurityServiceCommands.CreateUserCommand command) {
            // Add the requested claims
            List<Claim> claimsToAdd = new List<Claim>();
            if (command.Claims != null)
            {
                foreach (KeyValuePair<String, String> claim in command.Claims)
                {
                    claimsToAdd.Add(new Claim(claim.Key, claim.Value));
                }
            }

            // Add the email address and role as claims
            if (command.Roles != null)
            {
                foreach (String requestRole in command.Roles)
                {
                    claimsToAdd.Add(new Claim(JwtClaimTypes.Role, requestRole));
                }
            }

            claimsToAdd.Add(new Claim(JwtClaimTypes.Email, command.EmailAddress));
            claimsToAdd.Add(new Claim(JwtClaimTypes.GivenName, command.GivenName));
            claimsToAdd.Add(new Claim(JwtClaimTypes.FamilyName, command.FamilyName));

            if (String.IsNullOrEmpty(command.MiddleName) == false)
            {
                claimsToAdd.Add(new Claim(JwtClaimTypes.MiddleName, command.MiddleName));
            }

            var addClaimsResult = await this.UserManager.AddClaimsAsync(newIdentityUser, claimsToAdd);

            if (!addClaimsResult.Succeeded)
            {
                List<String> claimList = new List<String>();
                claimsToAdd.ForEach(c => claimList.Add($"Name: {c.Type} Value: {c.Value}"));
                return Result.Failure($"Error adding claims [{String.Join(",", claimsToAdd)}] to user {newIdentityUser.UserName} {addClaimsResult}");
            }

            return Result.Success();
        }

        public async Task<Result<UserDetails>> Handle(SecurityServiceQueries.GetUserQuery query, CancellationToken cancellationToken){

            ApplicationUser user = await this.UserManager.FindByIdAsync(query.UserId.ToString());

            if (user == null){
                return Result.NotFound($"No user found with user Id {query.UserId}");
            }

            UserDetails response = new UserDetails();
            response.Email = user.Email;
            response.PhoneNumber = user.PhoneNumber;
            response.UserId = query.UserId;
            response.SubjectId = query.UserId.ToString();
            response.Username = user.UserName;
            response.RegistrationDateTime = user.RegistrationDateTime;

            // Get the users roles
            response.Roles = await this.ConvertUsersRoles(user);

            // Get the users claims
            response.Claims = await this.ConvertUsersClaims(user);

            return Result.Success(response);
        }

        public async Task<Result<List<UserDetails>>> Handle(SecurityServiceQueries.GetUsersQuery query, CancellationToken cancellationToken){
            List<UserDetails> response = new List<UserDetails>();

            IQueryable<ApplicationUser> userQuery = this.UserManager.Users;

            if (String.IsNullOrEmpty(query.UserName) == false){
                userQuery = userQuery.Where(u => u.UserName.Contains(query.UserName));
            }

            List<ApplicationUser> users = await userQuery.ToListAsyncSafe(cancellationToken);

            foreach (ApplicationUser identityUser in users){
                Dictionary<String, String> claims = await this.ConvertUsersClaims(identityUser);
                List<String> roles = await this.ConvertUsersRoles(identityUser);

                response.Add(new UserDetails{
                                                UserId = Guid.Parse(identityUser.Id),
                                                SubjectId = identityUser.Id,
                                                Username = identityUser.UserName,
                                                Claims = claims,
                                                Email = identityUser.Email,
                                                PhoneNumber = identityUser.PhoneNumber,
                                                Roles = roles
                                            });
            }

            return  Result.Success(response);
        }

        public async Task<Result<ChangeUserPasswordResult>> Handle(SecurityServiceCommands.ChangeUserPasswordCommand command, CancellationToken cancellationToken){

            //Logger.LogWarning("In Handle ChangeUserPasswordCommand");
            // Find the user based on the user name passed in
            ApplicationUser user = await this.UserManager.FindByNameAsync(command.UserName);

            if (user == null){
                //Logger.LogWarning("In Handle ChangeUserPasswordCommand - user is null");
                // this prevents giving away info to a potential hacker...
                return Result.NotFound();
            }
            //Logger.LogWarning("In Handle ChangeUserPasswordCommand - user is not null");
            IdentityResult result = await this.UserManager.ChangePasswordAsync(user,
                command.CurrentPassword,
                command.NewPassword);

            if (result.Succeeded == false){
                // Log any errors
                Logger.LogInformation($"Errors during password change for user [{command.UserName} and Client [{command.ClientId}]");
                foreach (IdentityError identityError in result.Errors){
                    Logger.LogInformation($"Code {identityError.Code} Description {identityError.Description}");
                }

                return Result.Failure($"Errors during password change for user [{command.UserName} and Client [{command.ClientId}]");
            }

            //Logger.LogWarning("In Handle ChangeUserPasswordCommand - password changed");
            // build the redirect uri
            Client client = await this.ConfigurationDbContext.Clients.SingleOrDefaultAsync(c => c.ClientId == command.ClientId, cancellationToken:cancellationToken);

            if (client == null){
                //Logger.LogWarning("In Handle ChangeUserPasswordCommand - client not found");
                Logger.LogInformation($"Client not found for clientId {command.ClientId}");
                return Result.Invalid($"Client not found for clientId {command.ClientId}");
            }

            Logger.LogWarning($"Client uri: {client.ClientUri}");
            return Result.Success(new ChangeUserPasswordResult { IsSuccessful = true, RedirectUri = client.ClientUri});
        }

        public async Task<Result> Handle(SecurityServiceCommands.ConfirmUserEmailAddressCommand command, CancellationToken cancellationToken){
            ApplicationUser identityUser = await this.UserManager.FindByNameAsync(command.UserName);

            if (identityUser == null)
            {
                Logger.LogInformation($"No user found with username {command.UserName}");
                return Result.NotFound($"No user found with username {command.UserName}");
            }

            IdentityResult result = await this.UserManager.ConfirmEmailAsync(identityUser, command.ConfirmEmailToken);

            if (result.Succeeded == false)
            {
                Logger.LogInformation($"Errors during confirm email for user [{command.UserName}");
                foreach (IdentityError identityError in result.Errors)
                {
                    Logger.LogInformation($"Code {identityError.Code} Description {identityError.Description}");
                }
                return Result.Failure($"Errors during confirm email for user [{command.UserName}");
            }

            return Result.Success();
        }

        public async Task<Result<String>> Handle(SecurityServiceCommands.ProcessPasswordResetConfirmationCommand command, CancellationToken cancellationToken){
            // Find the user based on the user name passed in
            ApplicationUser user = await this.UserManager.FindByNameAsync(command.Username);

            if (user == null)
            {
                // this prevents giving away info to a potential hacker...
                Logger.LogInformation($"user not found for username {command.Username}");
                return Result.NotFound($"user not found for username {command.Username}");
            }

            IdentityResult result = await this.UserManager.ResetPasswordAsync(user, command.Token, command.Password);

            // handle the result... 
            if (result.Succeeded == false)
            {
                // Log any errors
                Logger.LogInformation($"Errors during password reset for user [{command.Username} and Client [{command.ClientId}]");
                foreach (IdentityError identityError in result.Errors)
                {
                    Logger.LogInformation($"Code {identityError.Code} Description {identityError.Description}");
                }

                return Result.Failure($"Errors during password reset for user [{command.Username} and Client [{command.ClientId}]");
            }

            // build the redirect uri
            Client client = await this.ConfigurationDbContext.Clients.SingleOrDefaultAsync(c => c.ClientId == command.ClientId, cancellationToken: cancellationToken);

            if (client == null)
            {
                Logger.LogInformation($"Client not found for clientId {command.ClientId}");
                return Result.Invalid($"Client not found for clientId {command.ClientId}");
            }

            return Result.Success<String>(client.ClientUri);
        }

        public async Task<Result> Handle(SecurityServiceCommands.ProcessPasswordResetRequestCommand command, CancellationToken cancellationToken){
            // Find the user based on the user name passed in
            ApplicationUser user = await this.UserManager.FindByNameAsync(command.Username);

            if (user == null)
            {
                // this prevents giving away info to a potential hacker...
                return Result.NotFound();
            }

            // User has been found so send an email with reset details
            String resetToken = await this.UserManager.GeneratePasswordResetTokenAsync(user);
            resetToken = UrlEncoder.Default.Encode(resetToken);
            String uri = $"{this.ServiceOptions.PublicOrigin}/Account/ForgotPassword/Confirm?userName={user.UserName}&resetToken={resetToken}&clientId={command.ClientId}";

            TokenResponse token = await this.GetToken(cancellationToken);
            SendEmailRequest emailRequest = this.BuildPasswordResetEmailRequest(user, uri);
            
            Result result = await this.MessagingServiceClient.SendEmail(token.AccessToken, emailRequest, cancellationToken);
            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return Result.Success();
        }

        public async Task<Result> Handle(SecurityServiceCommands.SendWelcomeEmailCommand command, CancellationToken cancellationToken){
            ApplicationUser i = await this.UserManager.FindByNameAsync(command.Username);
            await this.UserManager.RemovePasswordAsync(i);
            String generatedPassword = UserRequestHandler.GenerateRandomPassword(this.UserManager.Options.Password);
            await this.UserManager.AddPasswordAsync(i, generatedPassword);

            // Send Email
            TokenResponse token = await this.GetToken(cancellationToken);
            SendEmailRequest emailRequest = this.BuildWelcomeEmail(i.Email, generatedPassword);
            var result = await this.MessagingServiceClient.SendEmail(token.AccessToken, emailRequest, cancellationToken);
            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return Result.Success();
        }

        private SendEmailRequest BuildEmailConfirmationRequest(ApplicationUser user,
                                                               String emailConfirmationToken){
            StringBuilder mesasgeBuilder = new StringBuilder();

            mesasgeBuilder.Append("<html>");
            mesasgeBuilder.Append("<body>");
            mesasgeBuilder.Append("<p><strong>Thank you for registering</strong></p>");
            mesasgeBuilder.Append("<p></p>");
            mesasgeBuilder.Append($"<p>Please <a href=\"{emailConfirmationToken}\">click here</a> to confirm your email address.</p>");
            mesasgeBuilder.Append("<p>Thanks for your registration.</p>");
            mesasgeBuilder.Append("</body>");
            mesasgeBuilder.Append("</html>");

            SendEmailRequest request = new(){
                                                Body = mesasgeBuilder.ToString(),
                                                ConnectionIdentifier = Guid.NewGuid(),
                                                FromAddress = "golfhandicapping@btinternet.com",
                                                IsHtml = true,
                                                Subject = "Email Address Confirmation",
                                                ToAddresses = new List<String>{
                                                                                  user.Email,
                                                                                  "stuart_ferguson1@outlook.com"
                                                                              }
                                            };

            return request;
        }

        private SendEmailRequest BuildPasswordResetEmailRequest(ApplicationUser user,
                                                                String resetToken){
            StringBuilder mesasgeBuilder = new StringBuilder();

            mesasgeBuilder.Append("<html>");
            mesasgeBuilder.Append("<body>");
            mesasgeBuilder.Append("<p><strong>Thanks for your password reset request</strong></p>");
            mesasgeBuilder.Append("<p></p>");
            mesasgeBuilder.Append($"<p>Please <a href=\"{resetToken}\">click here</a> to confirm this password reset was from you.</p>");
            mesasgeBuilder.Append("<p>Thanks for your password reset request.</p>");
            mesasgeBuilder.Append("</body>");
            mesasgeBuilder.Append("</html>");

            SendEmailRequest request = new(){
                                                Body = mesasgeBuilder.ToString(),
                                                ConnectionIdentifier = Guid.NewGuid(),
                                                FromAddress = "golfhandicapping@btinternet.com",
                                                IsHtml = true,
                                                Subject = "Password Reset Requested",
                                                ToAddresses = new List<String>{
                                                                                  user.Email,
                                                                                  "stuart_ferguson1@outlook.com"
                                                                              }
                                            };

            return request;
        }

        private SendEmailRequest BuildWelcomeEmail(String emailAddress,
                                                   String password){
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

            SendEmailRequest request = new(){
                                                Body = mesasgeBuilder.ToString(),
                                                ConnectionIdentifier = Guid.NewGuid(),
                                                FromAddress = "golfhandicapping@btinternet.com",
                                                IsHtml = true,
                                                Subject = "Welcome to Transaction Processing",
                                                ToAddresses = new List<String>{
                                                                                  emailAddress,
                                                                                  "stuart_ferguson1@outlook.com"
                                                                              }
                                            };

            return request;
        }

        private async Task<Dictionary<String, String>> ConvertUsersClaims(ApplicationUser identityUser){
            Dictionary<String, String> response = new Dictionary<String, String>();
            IList<Claim> claims = await this.UserManager.GetClaimsAsync(identityUser);
            foreach (Claim claim in claims){
                response.Add(claim.Type, claim.Value);
            }

            return response;
        }

        private async Task<List<String>> ConvertUsersRoles(ApplicationUser identityUser){
            IList<String> roles = await this.UserManager.GetRolesAsync(identityUser);
            return roles.ToList();
        }

        private static String GenerateRandomPassword(PasswordOptions opts = null){
            if (opts == null)
                opts = new PasswordOptions{
                                              RequiredLength = 8,
                                              RequiredUniqueChars = 4,
                                              RequireDigit = true,
                                              RequireLowercase = true,
                                              RequireNonAlphanumeric = true,
                                              RequireUppercase = true
                                          };

            String[] randomChars ={
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

            for (Int32 i = chars.Count; i < opts.RequiredLength || chars.Distinct().Count() < opts.RequiredUniqueChars; i++){
                String rcs = randomChars[rand.Next(0, randomChars.Length)];
                chars.Insert(rand.Next(0, chars.Count), rcs[rand.Next(0, rcs.Length)]);
            }

            return new String(chars.ToArray());
        }

        private async Task<TokenResponse> GetToken(CancellationToken cancellationToken){
            // Get a token to talk to the estate service
            String clientId = this.ServiceOptions.ClientId;
            String clientSecret = this.ServiceOptions.ClientSecret;

            Logger.LogInformation($"Client Id is {clientId}");
            Logger.LogInformation($"Client Secret is {clientSecret}");

            if (this.TokenResponse == null){
                String clientToken = await this.IdentityServerTools.IssueClientJwtAsync(clientId, 3600);
                this.TokenResponse = TokenResponse.Create(clientToken, null, 3600, DateTimeOffset.Now, DateTimeOffset.Now.AddSeconds(3600));
                Logger.LogInformation($"Token is {this.TokenResponse.AccessToken}");
                return this.TokenResponse;
            }

            if (this.TokenResponse.Expires.UtcDateTime.Subtract(DateTime.UtcNow) < TimeSpan.FromMinutes(2)){
                Logger.LogInformation($"Token is about to expire at {this.TokenResponse.Expires.DateTime:O}");
                Logger.LogInformation($"Token is {this.TokenResponse.AccessToken}");
                return this.TokenResponse;
            }

            return this.TokenResponse;
        }

        #endregion
    }
}