namespace SecurityService.Manager
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using DataTransferObjects;
    using Exceptions;
    using Extensions;
    using IdentityModel;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
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
        public SecurityServiceManager(IPasswordHasher<IdentityUser> passwordHasher,
                                      UserManager<IdentityUser> userManager,
                                      RoleManager<IdentityRole> roleManager,
                                      SignInManager<IdentityUser> signInManager)
        {
            this.PasswordHasher = passwordHasher;
            this.UserManager = userManager;
            this.RoleManager = roleManager;
            this.SignInManager = signInManager;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Registers the user.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="System.NullReferenceException">Error generating password hash value, hash was null or empty</exception>
        /// <exception cref="IdentityResultException">
        /// Error creating user {newIdentityUser.UserName}
        /// or
        /// Error adding roles [{string.Join(",", request.Roles)}] to user {newIdentityUser.UserName}
        /// or
        /// Error adding claims [{string.Join(",", claims)}] to user {newIdentityUser.UserName}
        /// or
        /// Error deleting user {newIdentityUser.UserName} as part of cleanup
        /// </exception>
        public async Task<Guid> CreateUser(CreateUserRequest request,
                                           CancellationToken cancellationToken)
        {
            // Validate the input request
            this.ValidateRequest(request);

            Guid userId = Guid.NewGuid();

            // request is valid now add the user
            IdentityUser newIdentityUser = new IdentityUser
                                           {
                                               Id = userId.ToString(),
                                               Email = request.EmailAddress,
                                               EmailConfirmed = true,
                                               UserName = request.EmailAddress,
                                               NormalizedEmail = request.EmailAddress.ToUpper(),
                                               NormalizedUserName = request.EmailAddress.ToUpper(),
                                               SecurityStamp = Guid.NewGuid().ToString("D")
                                           };

            // Set the password
            //String password = String.IsNullOrEmpty(request.Password) ? GenerateRandomPassword() : request.Password;

            // Hash the new password
            newIdentityUser.PasswordHash = this.PasswordHasher.HashPassword(newIdentityUser, request.Password);

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
                if (request.Roles != null && request.Roles.Any())
                {
                    addRolesResult = await this.UserManager.AddToRolesAsync(newIdentityUser, request.Roles);

                    if (!addRolesResult.Succeeded)
                    {
                        throw new IdentityResultException($"Error adding roles [{string.Join(",", request.Roles)}] to user {newIdentityUser.UserName}", addRolesResult);
                    }
                }
                else
                {
                    addRolesResult = IdentityResult.Success;
                }

                // Add the requested claims
                if (request.Claims != null && request.Claims.Any())
                {
                    List<Claim> claims = request.Claims.Select(x => new Claim(x.Key, x.Value)).ToList();

                    // Add the email address and role as claims
                    foreach (String requestRole in request.Roles)
                    {
                        claims.Add(new Claim(JwtClaimTypes.Role, requestRole));
                    }

                    claims.Add(new Claim(JwtClaimTypes.Email, request.EmailAddress));
                    claims.Add(new Claim(JwtClaimTypes.GivenName, request.GivenName));
                    claims.Add(new Claim(JwtClaimTypes.FamilyName, request.FamilyName));

                    if (string.IsNullOrEmpty(request.MiddleName) == false)
                    {
                        claims.Add(new Claim(JwtClaimTypes.MiddleName, request.MiddleName));
                    }

                    addClaimsResult = await this.UserManager.AddClaimsAsync(newIdentityUser, claims);

                    if (!addClaimsResult.Succeeded)
                    {
                        List<String> claimList = new List<String>();
                        claims.ForEach(c => claimList.Add($"Name: {c.Type} Value: {c.Value}"));
                        throw new IdentityResultException($"Error adding claims [{string.Join(",", claims)}] to user {newIdentityUser.UserName}", addClaimsResult);
                    }
                }

                else
                {
                    addClaimsResult = IdentityResult.Success;
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

            if (String.IsNullOrEmpty(userName) == false)
            {
                query = query.Where(u => u.UserName.Contains(userName));
            }

            List<IdentityUser> users = await query.ToListAsyncSafe(cancellationToken);
            //users.ForEach(async u => response.Add(new UserDetails
            //                                           {
            //                                               UserId = Guid.Parse(u.Id),
            //                                               UserName = u.UserName,
            //                                               Claims = await this.ConvertUsersClaims(u),
            //                                               Email = u.Email,
            //                                               PhoneNumber = u.PhoneNumber,
            //                                               Roles = await this.ConvertUsersRoles(u)
            //                                           }));
            foreach (IdentityUser identityUser in users)
            {
                var claims = await this.ConvertUsersClaims(identityUser);
                var roles = await this.ConvertUsersRoles(identityUser);

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

        private async Task<List<String>> ConvertUsersRoles(IdentityUser identityUser)
        {
            IList<String> roles = await this.UserManager.GetRolesAsync(identityUser);
            return roles.ToList();
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
        /// Validates the request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        private void ValidateRequest(CreateUserRequest request)
        {
            Guard.ThrowIfNull(request, typeof(ArgumentNullException), "CreateUserRequest cannot be null");
            Guard.ThrowIfNullOrEmpty(request.EmailAddress, typeof(ArgumentNullException), "CreateUserRequest Email Address cannot be null or empty");
            Guard.ThrowIfNullOrEmpty(request.PhoneNumber, typeof(ArgumentNullException), "CreateUserRequest Phone Number cannot be null or empty");
            // TODO: Make a decision on these rules later :|
            //Guard.ThrowIfNull(request.Claims, typeof(ArgumentNullException), "CreateUserRequest Claims cannot be null or empty");
            //Guard.ThrowIfNull(request.Roles, typeof(ArgumentNullException), "CreateUserRequest Roles cannot be null or empty");
            Guard.ThrowIfNullOrEmpty(request.GivenName, typeof(ArgumentNullException), "CreateUserRequest Given Name cannot be null or empty");
            Guard.ThrowIfNullOrEmpty(request.FamilyName, typeof(ArgumentNullException), "CreateUserRequest Family Name cannot be null or empty");
        }

        #endregion
    }
}