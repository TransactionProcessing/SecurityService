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

        public async Task<String> CreateApiResource(String name,
                                                    String displayName,
                                                    String description,
                                                    String secret,
                                                    List<String> scopes,
                                                    List<String> userClaims,
                                                    CancellationToken cancellationToken) {
            ApiResource apiResource = new ApiResource {
                                                          ApiSecrets = new List<Secret> {
                                                                                            new Secret(secret.ToSha256())
                                                                                        },
                                                          Description = description,
                                                          DisplayName = displayName,
                                                          Name = name,
                                                          UserClaims = userClaims
                                                      };

            if (scopes != null && scopes.Any()) {
                foreach (String scope in scopes) {
                    apiResource.Scopes.Add(scope);
                }
            }

            // Now translate the model to the entity
            await this.ConfigurationDbContext.ApiResources.AddAsync(apiResource.ToEntity(), cancellationToken);

            // Save the changes
            await this.ConfigurationDbContext.SaveChangesAsync();

            return name;
        }
        
        public async Task<String> CreateClient(String clientId,
                                               String secret,
                                               String clientName,
                                               String clientDescription,
                                               List<String> allowedScopes,
                                               List<String> allowedGrantTypes,
                                               String clientUri,
                                               List<String> clientRedirectUris,
                                               List<String> clientPostLogoutRedirectUris,
                                               Boolean requireConsent,
                                               Boolean allowOfflineAccess,
                                               CancellationToken cancellationToken) {
            // Validate the grant types list
            this.ValidateGrantTypes(allowedGrantTypes);

            // Create the model from the request
            Client client = new Client {
                                           ClientId = clientId,
                                           ClientName = clientName,
                                           Description = clientDescription,
                                           ClientSecrets = {
                                                               new Secret(secret.ToSha256())
                                                           },
                                           AllowedGrantTypes = allowedGrantTypes,
                                           AllowedScopes = allowedScopes,
                                           RequireConsent = requireConsent,
                                           AllowOfflineAccess = allowOfflineAccess,
                                           ClientUri = clientUri
                                       };

            if (allowedGrantTypes.Contains("hybrid")) {
                client.RequirePkce = false;
            }

            if (clientRedirectUris != null && clientRedirectUris.Any()) {
                client.RedirectUris = new List<String>();
                foreach (String clientRedirectUri in clientRedirectUris) {
                    client.RedirectUris.Add(clientRedirectUri);
                }
            }

            if (clientPostLogoutRedirectUris != null && clientPostLogoutRedirectUris.Any()) {
                client.PostLogoutRedirectUris = new List<String>();
                foreach (String clientPostLogoutRedirectUri in clientPostLogoutRedirectUris) {
                    client.PostLogoutRedirectUris.Add(clientPostLogoutRedirectUri);
                }
            }

            // Now translate the model to the entity
            await this.ConfigurationDbContext.Clients.AddAsync(client.ToEntity(), cancellationToken);

            // Save the changes
            await this.ConfigurationDbContext.SaveChangesAsync();

            return clientId;
        }

        public async Task<String> CreateIdentityResource(String name,
                                                         String displayName,
                                                         String description,
                                                         Boolean required,
                                                         Boolean emphasize,
                                                         Boolean showInDiscoveryDocument,
                                                         List<String> claims,
                                                         CancellationToken cancellationToken) {
            IdentityResource identityResource = new IdentityResource(name, displayName, claims);
            identityResource.Emphasize = emphasize;
            identityResource.Required = required;
            identityResource.ShowInDiscoveryDocument = showInDiscoveryDocument;
            identityResource.Description = description;

            // Now translate the model to the entity
            await this.ConfigurationDbContext.IdentityResources.AddAsync(identityResource.ToEntity(), cancellationToken);

            // Save the changes
            await this.ConfigurationDbContext.SaveChangesAsync();

            return name;
        }

        public async Task<Guid> CreateRole(String roleName,
                                           CancellationToken cancellationToken) {
            Guid roleId = Guid.NewGuid();

            IdentityRole newIdentityRole = new IdentityRole {
                                                                Id = roleId.ToString(),
                                                                Name = roleName,
                                                                NormalizedName = roleName.ToUpper()
                                                            };

            // Ensure role name is not a duplicate
            if (await this.RoleManager.RoleExistsAsync(newIdentityRole.Name)) {
                throw new IdentityResultException($"Role {newIdentityRole.Name} already exists", IdentityResult.Failed());
            }

            IdentityResult createResult = await this.RoleManager.CreateAsync(newIdentityRole);

            if (!createResult.Succeeded) {
                throw new IdentityResultException($"Error creating role {newIdentityRole.Name}", createResult);
            }

            return roleId;
        }

        public async Task<Guid> CreateUser(String givenName,
                                           String middleName,
                                           String familyName,
                                           String userName,
                                           String password,
                                           String emailAddress,
                                           String phoneNumber,
                                           Dictionary<String, String> claims,
                                           List<String> roles,
                                           CancellationToken cancellationToken) {
            Guid userId = Guid.NewGuid();

            // request is valid now add the user
            IdentityUser newIdentityUser = new IdentityUser {
                                                                Id = userId.ToString(),
                                                                Email = emailAddress,
                                                                UserName = userName,
                                                                NormalizedEmail = emailAddress.ToUpper(),
                                                                NormalizedUserName = userName.ToUpper(),
                                                                SecurityStamp = Guid.NewGuid().ToString("D"),
                                                                PhoneNumber = phoneNumber
                                                            };

            String passwordValue = String.IsNullOrEmpty(password) ? SecurityServiceManager.GenerateRandomPassword(this.UserManager.Options.Password) : password;

            // Hash the default password
            newIdentityUser.PasswordHash =
                this.PasswordHasher.HashPassword(newIdentityUser, passwordValue);

            if (String.IsNullOrEmpty(newIdentityUser.PasswordHash)) {
                throw new IdentityResultException("Error generating password hash value, hash was null or empty", IdentityResult.Failed());
            }

            // Default all IdentityResults to failed
            IdentityResult createResult = IdentityResult.Failed();
            IdentityResult addRolesResult = IdentityResult.Failed();
            IdentityResult addClaimsResult = IdentityResult.Failed();

            try {
                // Create the User
                createResult = await this.UserManager.CreateAsync(newIdentityUser);

                if (!createResult.Succeeded) {
                    throw new IdentityResultException($"Error creating user {newIdentityUser.UserName}", createResult);
                }

                // Add the requested roles to the user
                if (roles != null && roles.Any()) {
                    addRolesResult = await this.UserManager.AddToRolesAsync(newIdentityUser, roles);

                    if (!addRolesResult.Succeeded) {
                        throw new IdentityResultException($"Error adding roles [{String.Join(",", roles)}] to user {newIdentityUser.UserName}", addRolesResult);
                    }
                }
                else {
                    addRolesResult = IdentityResult.Success;
                }

                // Add the requested claims
                List<Claim> claimsToAdd = new List<Claim>();
                if (claims != null) {
                    foreach (KeyValuePair<String, String> claim in claims) {
                        claimsToAdd.Add(new Claim(claim.Key, claim.Value));
                    }
                }

                // Add the email address and role as claims
                if (roles != null) {
                    foreach (String requestRole in roles) {
                        claimsToAdd.Add(new Claim(JwtClaimTypes.Role, requestRole));
                    }
                }

                claimsToAdd.Add(new Claim(JwtClaimTypes.Email, emailAddress));
                claimsToAdd.Add(new Claim(JwtClaimTypes.GivenName, givenName));
                claimsToAdd.Add(new Claim(JwtClaimTypes.FamilyName, familyName));

                if (String.IsNullOrEmpty(middleName) == false) {
                    claimsToAdd.Add(new Claim(JwtClaimTypes.MiddleName, middleName));
                }

                addClaimsResult = await this.UserManager.AddClaimsAsync(newIdentityUser, claimsToAdd);

                if (!addClaimsResult.Succeeded) {
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
                try {
                    await this.MessagingServiceClient.SendEmail(token.AccessToken, emailRequest, cancellationToken);
                }
                catch(Exception ex) {
                    Logger.LogError(ex);
                }
            }
            finally {
                // Do some cleanup here (if the create was successful but one fo the other steps failed)
                if ((createResult == IdentityResult.Success) && (!addRolesResult.Succeeded || !addClaimsResult.Succeeded)) {
                    // User has been created so need to remove this
                    IdentityResult deleteResult = await this.UserManager.DeleteAsync(newIdentityUser);

                    if (!deleteResult.Succeeded) {
                        throw new IdentityResultException($"Error deleting user {newIdentityUser.UserName} as part of cleanup", deleteResult);
                    }
                }
            }

            return userId;
        }

        public async Task<ApiResource> GetApiResource(String apiResourceName,
                                                      CancellationToken cancellationToken) {
            ApiResource apiResourceModel = null;

            Duende.IdentityServer.EntityFramework.Entities.ApiResource apiResourceEntity = await this.ConfigurationDbContext.ApiResources
                                                                                                     .Where(a => a.Name == apiResourceName).Include(a => a.Scopes)
                                                                                                     .Include(a => a.UserClaims)
                                                                                                     .SingleOrDefaultAsync(cancellationToken:cancellationToken);

            if (apiResourceEntity == null) {
                throw new NotFoundException($"No Api Resource found with Name [{apiResourceName}]");
            }

            apiResourceModel = apiResourceEntity.ToModel();

            return apiResourceModel;
        }

        public async Task<List<ApiResource>> GetApiResources(CancellationToken cancellationToken) {
            List<ApiResource> apiResourceModels = new List<ApiResource>();

            List<Duende.IdentityServer.EntityFramework.Entities.ApiResource> apiResourceEntities = await this.ConfigurationDbContext.ApiResources.Include(a => a.Scopes)
                                                                                                             .Include(a => a.UserClaims)
                                                                                                             .ToListAsync(cancellationToken:cancellationToken);

            if (apiResourceEntities.Any()) {
                foreach (Duende.IdentityServer.EntityFramework.Entities.ApiResource apiResourceEntity in apiResourceEntities) {
                    apiResourceModels.Add(apiResourceEntity.ToModel());
                }
            }

            return apiResourceModels;
        }
        
        public async Task<Client> GetClient(String clientId,
                                            CancellationToken cancellationToken) {
            Client clientModel = null;

            Duende.IdentityServer.EntityFramework.Entities.Client clientEntity = await this.ConfigurationDbContext.Clients.Include(c => c.AllowedGrantTypes)
                                                                                           .Include(c => c.AllowedScopes).Where(c => c.ClientId == clientId)
                                                                                           .SingleOrDefaultAsync(cancellationToken:cancellationToken);

            if (clientEntity == null) {
                throw new NotFoundException($"No client found with Client Id [{clientId}]");
            }

            clientModel = clientEntity.ToModel();

            return clientModel;
        }

        public async Task<List<Client>> GetClients(CancellationToken cancellationToken) {
            List<Client> clientModels = new List<Client>();

            List<Duende.IdentityServer.EntityFramework.Entities.Client> clientEntities = await this.ConfigurationDbContext.Clients.Include(c => c.AllowedGrantTypes)
                                                                                                   .Include(c => c.AllowedScopes)
                                                                                                   .ToListAsync(cancellationToken:cancellationToken);

            if (clientEntities.Any()) {
                foreach (Duende.IdentityServer.EntityFramework.Entities.Client clientEntity in clientEntities) {
                    clientModels.Add(clientEntity.ToModel());
                }
            }

            return clientModels;
        }

        public async Task<IdentityResource> GetIdentityResource(String identityResourceName,
                                                                CancellationToken cancellationToken) {
            IdentityResource identityResourceModel = null;

            Duende.IdentityServer.EntityFramework.Entities.IdentityResource identityResourceEntity = await this.ConfigurationDbContext.IdentityResources
                                                                                                               .Where(a => a.Name == identityResourceName)
                                                                                                               .Include(a => a.UserClaims)
                                                                                                               .SingleOrDefaultAsync(cancellationToken:cancellationToken);

            if (identityResourceEntity == null) {
                throw new NotFoundException($"No Identity Resource found with Name [{identityResourceName}]");
            }

            identityResourceModel = identityResourceEntity.ToModel();

            return identityResourceModel;
        }

        public async Task<List<IdentityResource>> GetIdentityResources(CancellationToken cancellationToken) {
            List<IdentityResource> identityResourceModels = new List<IdentityResource>();

            List<Duende.IdentityServer.EntityFramework.Entities.IdentityResource> identityResourceEntities =
                await this.ConfigurationDbContext.IdentityResources.Include(a => a.UserClaims).ToListAsync(cancellationToken:cancellationToken);

            if (identityResourceEntities.Any()) {
                foreach (Duende.IdentityServer.EntityFramework.Entities.IdentityResource identityResourceEntity in identityResourceEntities) {
                    identityResourceModels.Add(identityResourceEntity.ToModel());
                }
            }

            return identityResourceModels;
        }

        public async Task<RoleDetails> GetRole(Guid roleId,
                                               CancellationToken cancellationToken) {
            IdentityRole identityRole = await this.RoleManager.FindByIdAsync(roleId.ToString());

            if (identityRole == null) {
                throw new NotFoundException($"No role found with Id {roleId}");
            }

            // Role has been found
            RoleDetails response = new RoleDetails {
                                                       RoleId = Guid.Parse(identityRole.Id),
                                                       RoleName = identityRole.Name
                                                   };

            return response;
        }

        public async Task<List<RoleDetails>> GetRoles(CancellationToken cancellationToken) {
            List<RoleDetails> response = new List<RoleDetails>();

            IQueryable<IdentityRole> query = this.RoleManager.Roles;

            List<IdentityRole> roles = await query.ToListAsyncSafe(cancellationToken);

            foreach (IdentityRole identityRole in roles) {
                response.Add(new RoleDetails {
                                                 RoleId = Guid.Parse(identityRole.Id),
                                                 RoleName = identityRole.Name
                                             });
            }

            return response;
        }

        public async Task<UserDetails> GetUser(Guid userId,
                                               CancellationToken cancellationToken) {
            Guard.ThrowIfInvalidGuid(userId, nameof(userId));

            IdentityUser user = await this.UserManager.FindByIdAsync(userId.ToString());

            if (user == null) {
                throw new NotFoundException($"No user found with user Id {userId}");
            }

            UserDetails response = new UserDetails();
            response.Email = user.Email;
            response.PhoneNumber = user.PhoneNumber;
            response.UserId = userId;
            response.SubjectId = userId.ToString();
            response.Username = user.UserName;

            // Get the users roles
            response.Roles = await this.ConvertUsersRoles(user);

            // Get the users claims
            response.Claims = await this.ConvertUsersClaims(user);

            return response;
        }

        public async Task<List<UserDetails>> GetUsers(String userName,
                                                      CancellationToken cancellationToken) {
            List<UserDetails> response = new List<UserDetails>();

            IQueryable<IdentityUser> query = this.UserManager.Users;

            if (String.IsNullOrEmpty(userName) == false) {
                query = query.Where(u => u.UserName.Contains(userName));
            }

            List<IdentityUser> users = await query.ToListAsyncSafe(cancellationToken);

            foreach (IdentityUser identityUser in users) {
                Dictionary<String, String> claims = await this.ConvertUsersClaims(identityUser);
                List<String> roles = await this.ConvertUsersRoles(identityUser);

                response.Add(new UserDetails {
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

        [ExcludeFromCodeCoverage]
        public async Task Signout() {
            await this.SignInManager.SignOutAsync();
        }

        public async Task<Boolean> ValidateCredentials(String userName,
                                                       String password,
                                                       CancellationToken cancellationToken) {
            // Get the user record by name
            IdentityUser user = await this.UserManager.FindByNameAsync(userName);

            // Now validate the entered password
            PasswordVerificationResult verificationResult = this.PasswordHasher.VerifyHashedPassword(user, user.PasswordHash, password);

            // Return the result
            return verificationResult == PasswordVerificationResult.Success;
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