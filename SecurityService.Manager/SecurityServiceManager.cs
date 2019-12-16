namespace SecurityService.Manager
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using Exceptions;
    using Extensions;
    using IdentityModel;
    using IdentityServer4.EntityFramework.DbContexts;
    using IdentityServer4.EntityFramework.Interfaces;
    using IdentityServer4.EntityFramework.Mappers;
    using IdentityServer4.Models;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using Models;
    using Shared.Exceptions;
    using Shared.General;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="SecurityService.Manager.ISecurityServiceManager" />
    /// <seealso cref="ISecurityServiceManager" />
    public class SecurityServiceManager : ISecurityServiceManager
    {
        #region Fields

        /// <summary>
        /// The configuration database context resolver
        /// </summary>
        private readonly ConfigurationDbContext ConfigurationDbContext;

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

        /// <summary>
        /// The user manager
        /// </summary>
        private readonly UserManager<IdentityUser> UserManager;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SecurityServiceManager" /> class.
        /// </summary>
        /// <param name="passwordHasher">The password hasher.</param>
        /// <param name="userManager">The user manager.</param>
        /// <param name="roleManager">The role manager.</param>
        /// <param name="signInManager">The sign in manager.</param>
        /// <param name="configurationDbContext">The configuration database context.</param>
        public SecurityServiceManager(IPasswordHasher<IdentityUser> passwordHasher,
                                      UserManager<IdentityUser> userManager,
                                      RoleManager<IdentityRole> roleManager,
                                      SignInManager<IdentityUser> signInManager,
                                      ConfigurationDbContext configurationDbContext)
        {
            this.PasswordHasher = passwordHasher;
            this.UserManager = userManager;
            this.RoleManager = roleManager;
            this.SignInManager = signInManager;
            this.ConfigurationDbContext = configurationDbContext;
        }

        #endregion

        #region Methods

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
        public async Task<String> CreateApiResource(String name,
                                                    String displayName,
                                                    String description,
                                                    String secret,
                                                    List<String> scopes,
                                                    List<String> userClaims,
                                                    CancellationToken cancellationToken)
        {
            ApiResource apiResource = new ApiResource
                                      {
                                          ApiSecrets = new List<Secret>
                                                       {
                                                           new Secret(secret.ToSha256())
                                                       },
                                          Description = description,
                                          DisplayName = displayName,
                                          Name = name,
                                          UserClaims = userClaims
                                      };

            if (scopes != null && scopes.Any())
            {
                foreach (String scope in scopes)
                {
                    apiResource.Scopes.Add(new Scope(scope));
                }
            }

            // Now translate the model to the entity
            await this.ConfigurationDbContext.ApiResources.AddAsync(apiResource.ToEntity(), cancellationToken);

            // Save the changes
            await this.ConfigurationDbContext.SaveChangesAsync();

            return name;
        }

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
        public async Task<String> CreateClient(String clientId,
                                               String secret,
                                               String clientName,
                                               String clientDescription,
                                               List<String> allowedScopes,
                                               List<String> allowedGrantTypes,
                                               CancellationToken cancellationToken)
        {
            // Validate the grant types list
            this.ValidateGrantTypes(allowedGrantTypes);

            // Create the model from the request
            Client client = new Client
                            {
                                ClientId = clientId,
                                ClientName = clientName,
                                Description = clientDescription,
                                ClientSecrets =
                                {
                                    new Secret(secret.ToSha256())
                                },
                                AllowedGrantTypes = allowedGrantTypes,
                                AllowedScopes = allowedScopes
                            };

            // Now translate the model to the entity
            await this.ConfigurationDbContext.Clients.AddAsync(client.ToEntity(), cancellationToken);

            // Save the changes
            await this.ConfigurationDbContext.SaveChangesAsync();

            return clientId;
        }

        /// <summary>
        /// Creates the role.
        /// </summary>
        /// <param name="roleName">Name of the role.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="IdentityResultException">
        /// Role {newIdentityRole.Name} already exists
        /// or
        /// Error creating role {newIdentityRole.Name}
        /// </exception>
        public async Task<Guid> CreateRole(String roleName,
                                           CancellationToken cancellationToken)
        {
            Guid roleId = Guid.NewGuid();

            IdentityRole newIdentityRole = new IdentityRole
                                           {
                                               Id = roleId.ToString(),
                                               Name = roleName,
                                               NormalizedName = roleName.ToUpper()
                                           };

            // Default all IdentityResults to failed
            IdentityResult createResult = IdentityResult.Failed();

            // Ensure role name is not a duplicate
            if (await this.RoleManager.RoleExistsAsync(newIdentityRole.Name))
            {
                throw new IdentityResultException($"Role {newIdentityRole.Name} already exists", IdentityResult.Failed());
            }

            createResult = await this.RoleManager.CreateAsync(newIdentityRole);

            if (!createResult.Succeeded)
            {
                throw new IdentityResultException($"Error creating role {newIdentityRole.Name}", createResult);
            }

            return roleId;
        }

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
        /// <exception cref="NullReferenceException">Error generating password hash value, hash was null or empty</exception>
        /// <exception cref="IdentityResultException">Error creating user {newIdentityUser.UserName}
        /// or
        /// Error adding roles [{string.Join(",", request.Roles)}] to user {newIdentityUser.UserName}
        /// or
        /// Error adding claims [{string.Join(",", claims)}] to user {newIdentityUser.UserName}
        /// or
        /// Error deleting user {newIdentityUser.UserName} as part of cleanup</exception>
        /// <exception cref="System.NullReferenceException">Error generating password hash value, hash was null or empty</exception>
        public async Task<Guid> CreateUser(String givenName,
                                           String middleName,
                                           String familyName,
                                           String userName,
                                           String password,
                                           String emailAddress,
                                           String phoneNumber,
                                           Dictionary<String, String> claims,
                                           List<String> roles,
                                           CancellationToken cancellationToken)
        {
            Guid userId = Guid.NewGuid();

            // request is valid now add the user
            IdentityUser newIdentityUser = new IdentityUser
                                           {
                                               Id = userId.ToString(),
                                               Email = emailAddress,
                                               EmailConfirmed = true,
                                               UserName = userName,
                                               NormalizedEmail = emailAddress.ToUpper(),
                                               NormalizedUserName = userName.ToUpper(),
                                               SecurityStamp = Guid.NewGuid().ToString("D"),
                                               PhoneNumber = phoneNumber
                                           };

            // Set the password
            //String password = String.IsNullOrEmpty(request.Password) ? GenerateRandomPassword() : request.Password;

            // Hash the new password
            newIdentityUser.PasswordHash = this.PasswordHasher.HashPassword(newIdentityUser, password);

            if (string.IsNullOrEmpty(newIdentityUser.PasswordHash))
            {
                throw new NullReferenceException("Error generating password hash value, hash was null or empty");
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
                if (roles != null && roles.Any())
                {
                    addRolesResult = await this.UserManager.AddToRolesAsync(newIdentityUser, roles);

                    if (!addRolesResult.Succeeded)
                    {
                        throw new IdentityResultException($"Error adding roles [{string.Join(",", roles)}] to user {newIdentityUser.UserName}", addRolesResult);
                    }
                }
                else
                {
                    addRolesResult = IdentityResult.Success;
                }

                // Add the requested claims
                List<Claim> claimsToAdd = new List<Claim>();
                if (claims != null && claims.Any())
                {
                    claimsToAdd.AddRange(claims.Select(x => new Claim(x.Key, x.Value)).ToList());
                }

                // Add the email address and role as claims
                if (roles != null)
                {
                    foreach (String requestRole in roles)
                    {
                        claimsToAdd.Add(new Claim(JwtClaimTypes.Role, requestRole));
                    }
                }

                claimsToAdd.Add(new Claim(JwtClaimTypes.Email, emailAddress));
                claimsToAdd.Add(new Claim(JwtClaimTypes.GivenName, givenName));
                claimsToAdd.Add(new Claim(JwtClaimTypes.FamilyName, familyName));

                if (string.IsNullOrEmpty(middleName) == false)
                {
                    claimsToAdd.Add(new Claim(JwtClaimTypes.MiddleName, middleName));
                }

                addClaimsResult = await this.UserManager.AddClaimsAsync(newIdentityUser, claimsToAdd);

                if (!addClaimsResult.Succeeded)
                {
                    List<String> claimList = new List<String>();
                    claimsToAdd.ForEach(c => claimList.Add($"Name: {c.Type} Value: {c.Value}"));
                    throw new IdentityResultException($"Error adding claims [{string.Join(",", claimsToAdd)}] to user {newIdentityUser.UserName}", addClaimsResult);
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

            return userId;
        }

        /// <summary>
        /// Gets the API resource.
        /// </summary>
        /// <param name="apiResourceName">Name of the API resource.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="NotFoundException">No Api Resource found with Name [{apiResourceName}]</exception>
        public async Task<ApiResource> GetApiResource(String apiResourceName,
                                                      CancellationToken cancellationToken)
        {
            ApiResource apiResourceModel = null;
            
            IdentityServer4.EntityFramework.Entities.ApiResource apiResourceEntity = await this.ConfigurationDbContext.ApiResources.Where(a => a.Name == apiResourceName)
                                                                                               .Include(a => a.Scopes).Include(a => a.UserClaims)
                                                                                               .SingleOrDefaultAsync(cancellationToken:cancellationToken);

            if (apiResourceEntity == null)
            {
                throw new NotFoundException($"No Api Resource found with Name [{apiResourceName}]");
            }

            apiResourceModel = apiResourceEntity.ToModel();

            return apiResourceModel;
        }

        /// <summary>
        /// Gets the API resources.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public async Task<List<ApiResource>> GetApiResources(CancellationToken cancellationToken)
        {
            List<ApiResource> apiResourceModels = new List<ApiResource>();

            List<IdentityServer4.EntityFramework.Entities.ApiResource> apiResourceEntities =
                await this.ConfigurationDbContext.ApiResources.Include(a => a.Scopes).Include(a => a.UserClaims).ToListAsync(cancellationToken:cancellationToken);

            if (apiResourceEntities.Any())
            {
                foreach (IdentityServer4.EntityFramework.Entities.ApiResource apiResourceEntity in apiResourceEntities)
                {
                    apiResourceModels.Add(apiResourceEntity.ToModel());
                }
            }

            return apiResourceModels;
        }

        /// <summary>
        /// Gets the client.
        /// </summary>
        /// <param name="clientId">The client identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="NotFoundException">No client found with Client Id [{clientId}]</exception>
        public async Task<Client> GetClient(String clientId,
                                            CancellationToken cancellationToken)
        {
            Client clientModel = null;

            IdentityServer4.EntityFramework.Entities.Client clientEntity = await this.ConfigurationDbContext
                                                                                     .Clients.Include(c => c.AllowedGrantTypes).Include(c => c.AllowedScopes)
                                                                                     .Where(c => c.ClientId == clientId)
                                                                                     .SingleOrDefaultAsync(cancellationToken:cancellationToken);

            if (clientEntity == null)
            {
                throw new NotFoundException($"No client found with Client Id [{clientId}]");
            }

            clientModel = clientEntity.ToModel();

            return clientModel;
        }

        /// <summary>
        /// Gets the clients.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public async Task<List<Client>> GetClients(CancellationToken cancellationToken)
        {
            List<Client> clientModels = new List<Client>();

            List<IdentityServer4.EntityFramework.Entities.Client> clientEntities =
                await this.ConfigurationDbContext.Clients.Include(c => c.AllowedGrantTypes).Include(c => c.AllowedScopes).ToListAsync(cancellationToken:cancellationToken);

            if (clientEntities.Any())
            {
                foreach (IdentityServer4.EntityFramework.Entities.Client clientEntity in clientEntities)
                {
                    clientModels.Add(clientEntity.ToModel());
                }
            }

            return clientModels;
        }

        /// <summary>
        /// Gets the role.
        /// </summary>
        /// <param name="roleId">The role identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="NotFoundException">No role found with Id {roleId}</exception>
        public async Task<RoleDetails> GetRole(Guid roleId,
                                               CancellationToken cancellationToken)
        {
            IdentityRole identityRole = await this.RoleManager.FindByIdAsync(roleId.ToString());

            if (identityRole == null)
            {
                throw new NotFoundException($"No role found with Id {roleId}");
            }

            // Role has been found
            RoleDetails response = new RoleDetails
                                   {
                                       RoleId = Guid.Parse(identityRole.Id),
                                       RoleName = identityRole.Name
                                   };

            return response;
        }

        /// <summary>
        /// Gets the roles.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public async Task<List<RoleDetails>> GetRoles(CancellationToken cancellationToken)
        {
            List<RoleDetails> response = new List<RoleDetails>();

            IQueryable<IdentityRole> query = this.RoleManager.Roles;

            List<IdentityRole> roles = await query.ToListAsyncSafe(cancellationToken);

            foreach (IdentityRole identityRole in roles)
            {
                response.Add(new RoleDetails
                             {
                                 RoleId = Guid.Parse(identityRole.Id),
                                 RoleName = identityRole.Name
                             });
            }

            return response;
        }

        /// <summary>
        /// Gets the user by user identifier.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="NotFoundException">No user found with user Id {userId}</exception>
        public async Task<UserDetails> GetUser(Guid userId,
                                               CancellationToken cancellationToken)
        {
            Guard.ThrowIfInvalidGuid(userId, nameof(userId));

            IdentityUser user = await this.UserManager.FindByIdAsync(userId.ToString());

            if (user == null)
            {
                throw new NotFoundException($"No user found with user Id {userId}");
            }

            UserDetails response = new UserDetails();
            response.Email = user.Email;
            response.PhoneNumber = user.PhoneNumber;
            response.UserId = userId;
            response.UserName = user.UserName;

            // Get the users roles
            response.Roles = await this.ConvertUsersRoles(user);

            // Get the users claims
            response.Claims = await this.ConvertUsersClaims(user);

            return response;
        }

        /// <summary>
        /// Gets the users.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public async Task<List<UserDetails>> GetUsers(String userName,
                                                      CancellationToken cancellationToken)
        {
            List<UserDetails> response = new List<UserDetails>();

            IQueryable<IdentityUser> query = this.UserManager.Users;

            if (string.IsNullOrEmpty(userName) == false)
            {
                query = query.Where(u => u.UserName.Contains(userName));
            }

            List<IdentityUser> users = await query.ToListAsyncSafe(cancellationToken);

            foreach (IdentityUser identityUser in users)
            {
                Dictionary<String, String> claims = await this.ConvertUsersClaims(identityUser);
                List<String> roles = await this.ConvertUsersRoles(identityUser);

                response.Add(new UserDetails
                             {
                                 UserId = Guid.Parse(identityUser.Id),
                                 UserName = identityUser.UserName,
                                 Claims = claims,
                                 Email = identityUser.Email,
                                 PhoneNumber = identityUser.PhoneNumber,
                                 Roles = roles
                             });
            }

            return response;
        }

        /// <summary>
        /// Converts the users claims.
        /// </summary>
        /// <param name="identityUser">The identity user.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Converts the users roles.
        /// </summary>
        /// <param name="identityUser">The identity user.</param>
        /// <returns></returns>
        private async Task<List<String>> ConvertUsersRoles(IdentityUser identityUser)
        {
            IList<String> roles = await this.UserManager.GetRolesAsync(identityUser);
            return roles.ToList();
        }

        /// <summary>
        /// Validates the grant types.
        /// </summary>
        /// <param name="allowedGrantTypes">The allowed grant types.</param>
        /// <exception cref="ArgumentException">allowedGrantTypes - The grant types [{string.Join(", ", invalidGrantTypes)}] are not valid to create a new client</exception>
        private void ValidateGrantTypes(List<String> allowedGrantTypes)
        {
            // Get a list of valid grant types
            List<String> validTypesList = new List<String>();

            validTypesList.AddRange(GrantTypes.ClientCredentials);
            validTypesList.AddRange(GrantTypes.Code);
            validTypesList.AddRange(GrantTypes.DeviceFlow);
            validTypesList.AddRange(GrantTypes.Hybrid);
            validTypesList.AddRange(GrantTypes.Implicit);
            validTypesList.AddRange(GrantTypes.ResourceOwnerPassword);

            List<String> invalidGrantTypes = allowedGrantTypes.Where(a => validTypesList.All(v => v != a)).ToList();

            if (invalidGrantTypes.Any())
            {
                throw new ArgumentException(nameof(allowedGrantTypes), $"The grant types [{string.Join(", ", invalidGrantTypes)}] are not valid to create a new client");
            }
        }

        #endregion
    }
}