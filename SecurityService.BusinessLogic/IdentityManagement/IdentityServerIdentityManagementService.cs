namespace SecurityService.BusinessLogic.IdentityManagement;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using SecurityService.BusinessLogic.RequestHandlers;
using Duende.IdentityModel;
using Duende.IdentityServer;
using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Mappers;
using Duende.IdentityServer.Models;
using MediatR;
using MessagingService.Client;
using MessagingService.DataTransferObjects;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Requests;
using SecurityService.Models;
using Shared.Exceptions;
using Shared.Logger;
using Shared.Results;
using SimpleResults;
using ClientEntity = Duende.IdentityServer.EntityFramework.Entities.Client;
using Secret = Duende.IdentityServer.Models.Secret;
using UserDetails = SecurityService.Models.UserDetails;

public class IdentityServerIdentityManagementService : IIdentityManagementService
{
    private readonly ConfigurationDbContext ConfigurationDbContext;
    private readonly IdentityServerTools IdentityServerTools;
    private readonly IMessagingServiceClient MessagingServiceClient;
    private readonly IPasswordHasher<ApplicationUser> PasswordHasher;
    private readonly RoleManager<IdentityRole> RoleManager;
    private readonly ServiceOptions ServiceOptions;
    private TokenResponse TokenResponse;
    private readonly UserManager<ApplicationUser> UserManager;

    public IdentityServerIdentityManagementService(IPasswordHasher<ApplicationUser> passwordHasher,
                                                   UserManager<ApplicationUser> userManager,
                                                   RoleManager<IdentityRole> roleManager,
                                                   ServiceOptions serviceOptions,
                                                   IMessagingServiceClient messagingServiceClient,
                                                   IdentityServerTools identityServerTools,
                                                   ConfigurationDbContext configurationDbContext)
    {
        this.PasswordHasher = passwordHasher;
        this.UserManager = userManager;
        this.RoleManager = roleManager;
        this.ServiceOptions = serviceOptions;
        this.MessagingServiceClient = messagingServiceClient;
        this.IdentityServerTools = identityServerTools;
        this.ConfigurationDbContext = configurationDbContext;
    }

    public async Task<Result> CreateApiResource(SecurityServiceCommands.CreateApiResourceCommand command, CancellationToken cancellationToken)
    {
        ApiResource apiResource = new ApiResource
        {
            ApiSecrets = new List<Secret> { new Secret(command.Secret.ToSha256()) },
            Description = command.Description,
            DisplayName = command.DisplayName,
            Name = command.Name,
            UserClaims = command.UserClaims,
        };

        if (command.Scopes != null && command.Scopes.Any())
        {
            foreach (String scope in command.Scopes)
            {
                apiResource.Scopes.Add(scope);
            }
        }

        await this.ConfigurationDbContext.ApiResources.AddAsync(apiResource.ToEntity(), cancellationToken);
        await this.ConfigurationDbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result<ApiResource>> GetApiResource(SecurityServiceQueries.GetApiResourceQuery query, CancellationToken cancellationToken)
    {
        Duende.IdentityServer.EntityFramework.Entities.ApiResource apiResourceEntity = await this.ConfigurationDbContext.ApiResources
            .Where(a => a.Name == query.Name)
            .Include(a => a.Scopes)
            .Include(a => a.UserClaims)
            .SingleOrDefaultAsync(cancellationToken: cancellationToken);

        if (apiResourceEntity == null)
        {
            return Result.NotFound($"No Api Resource found with Name [{query.Name}]");
        }

        return Result.Success(apiResourceEntity.ToModel());
    }

    public async Task<Result<List<ApiResource>>> GetApiResources(SecurityServiceQueries.GetApiResourcesQuery query, CancellationToken cancellationToken)
    {
        List<ApiResource> apiResourceModels = new List<ApiResource>();

        List<Duende.IdentityServer.EntityFramework.Entities.ApiResource> apiResourceEntities = await this.ConfigurationDbContext.ApiResources
            .Include(a => a.Scopes)
            .Include(a => a.UserClaims)
            .ToListAsync(cancellationToken: cancellationToken);

        if (apiResourceEntities.Any())
        {
            foreach (Duende.IdentityServer.EntityFramework.Entities.ApiResource apiResourceEntity in apiResourceEntities)
            {
                apiResourceModels.Add(apiResourceEntity.ToModel());
            }
        }

        return Result.Success(apiResourceModels);
    }

    public async Task<Result> CreateApiScope(SecurityServiceCommands.CreateApiScopeCommand command, CancellationToken cancellationToken)
    {
        ApiScope apiScope = new ApiScope
        {
            Description = command.Description,
            DisplayName = command.DisplayName,
            Name = command.Name,
            Emphasize = false,
            Enabled = true,
            Required = false,
            ShowInDiscoveryDocument = true
        };

        await this.ConfigurationDbContext.ApiScopes.AddAsync(apiScope.ToEntity(), cancellationToken);
        await this.ConfigurationDbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result<ApiScope>> GetApiScope(SecurityServiceQueries.GetApiScopeQuery query, CancellationToken cancellationToken)
    {
        Duende.IdentityServer.EntityFramework.Entities.ApiScope apiScopeEntity = await this.ConfigurationDbContext.ApiScopes
            .Where(a => a.Name == query.Name)
            .Include(a => a.Properties)
            .Include(a => a.UserClaims)
            .SingleOrDefaultAsync(cancellationToken: cancellationToken);

        if (apiScopeEntity == null)
        {
            return Result.NotFound($"No Api Scope found with Name [{query.Name}]");
        }

        return Result.Success(apiScopeEntity.ToModel());
    }

    public async Task<Result<List<ApiScope>>> GetApiScopes(SecurityServiceQueries.GetApiScopesQuery query, CancellationToken cancellationToken)
    {
        List<ApiScope> apiScopeModels = new List<ApiScope>();

        List<Duende.IdentityServer.EntityFramework.Entities.ApiScope> apiScopeEntities = await this.ConfigurationDbContext.ApiScopes
            .Include(a => a.Properties)
            .Include(a => a.UserClaims)
            .ToListAsync(cancellationToken: cancellationToken);

        if (apiScopeEntities.Any())
        {
            foreach (Duende.IdentityServer.EntityFramework.Entities.ApiScope apiScopeEntity in apiScopeEntities)
            {
                apiScopeModels.Add(apiScopeEntity.ToModel());
            }
        }

        return Result.Success(apiScopeModels);
    }

    public async Task<Result> CreateClient(SecurityServiceCommands.CreateClientCommand command, CancellationToken cancellationToken)
    {
        Result validationResult = this.ValidateGrantTypes(command.AllowedGrantTypes);
        if (validationResult.IsFailed)
        {
            return validationResult;
        }

        Client client = new Client
        {
            ClientId = command.ClientId,
            ClientName = command.ClientName,
            Description = command.ClientDescription,
            ClientSecrets = { new Secret(command.Secret.ToSha256()) },
            AllowedGrantTypes = command.AllowedGrantTypes,
            AllowedScopes = command.AllowedScopes,
            RequireConsent = command.RequireConsent,
            AllowOfflineAccess = command.AllowOfflineAccess,
            ClientUri = command.ClientUri
        };

        if (command.AllowedGrantTypes.Contains("hybrid"))
        {
            client.RequirePkce = false;
        }

        if (command.ClientRedirectUris != null && command.ClientRedirectUris.Any())
        {
            client.RedirectUris = new List<String>();
            foreach (String clientRedirectUri in command.ClientRedirectUris)
            {
                client.RedirectUris.Add(clientRedirectUri);
            }
        }

        if (command.ClientPostLogoutRedirectUris != null && command.ClientPostLogoutRedirectUris.Any())
        {
            client.PostLogoutRedirectUris = new List<String>();
            foreach (String clientPostLogoutRedirectUri in command.ClientPostLogoutRedirectUris)
            {
                client.PostLogoutRedirectUris.Add(clientPostLogoutRedirectUri);
            }
        }

        await this.ConfigurationDbContext.Clients.AddAsync(client.ToEntity(), cancellationToken);
        await this.ConfigurationDbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result<Client>> GetClient(SecurityServiceQueries.GetClientQuery query, CancellationToken cancellationToken)
    {
        ClientEntity clientEntity = await this.ConfigurationDbContext.Clients.Include(c => c.AllowedGrantTypes)
            .Include(c => c.AllowedScopes)
            .Where(c => c.ClientId == query.ClientId)
            .SingleOrDefaultAsync(cancellationToken: cancellationToken);

        if (clientEntity == null)
        {
            return Result.NotFound($"No client found with Client Id [{query.ClientId}]");
        }

        return Result.Success(clientEntity.ToModel());
    }

    public async Task<Result<List<Client>>> GetClients(SecurityServiceQueries.GetClientsQuery query, CancellationToken cancellationToken)
    {
        List<Client> clientModels = new List<Client>();

        List<ClientEntity> clientEntities = await this.ConfigurationDbContext.Clients.Include(c => c.AllowedGrantTypes)
            .Include(c => c.AllowedScopes)
            .ToListAsync(cancellationToken: cancellationToken);

        if (clientEntities.Any())
        {
            foreach (ClientEntity clientEntity in clientEntities)
            {
                clientModels.Add(clientEntity.ToModel());
            }
        }

        return Result.Success(clientModels);
    }

    public async Task<Result> CreateIdentityResource(SecurityServiceCommands.CreateIdentityResourceCommand command, CancellationToken cancellationToken)
    {
        IdentityResource identityResource = new IdentityResource(command.Name, command.DisplayName, command.Claims)
        {
            Emphasize = command.Emphasize,
            Required = command.Required,
            ShowInDiscoveryDocument = command.ShowInDiscoveryDocument,
            Description = command.Description
        };

        await this.ConfigurationDbContext.IdentityResources.AddAsync(identityResource.ToEntity(), cancellationToken);
        await this.ConfigurationDbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result<IdentityResource>> GetIdentityResource(SecurityServiceQueries.GetIdentityResourceQuery query, CancellationToken cancellationToken)
    {
        Duende.IdentityServer.EntityFramework.Entities.IdentityResource identityResourceEntity = await this.ConfigurationDbContext.IdentityResources
            .Where(a => a.Name == query.IdentityResourceName)
            .Include(a => a.UserClaims)
            .SingleOrDefaultAsync(cancellationToken: cancellationToken);

        if (identityResourceEntity == null)
        {
            return Result.NotFound($"No Identity Resource found with Name [{query.IdentityResourceName}]");
        }

        return Result.Success(identityResourceEntity.ToModel());
    }

    public async Task<Result<List<IdentityResource>>> GetIdentityResources(SecurityServiceQueries.GetIdentityResourcesQuery query, CancellationToken cancellationToken)
    {
        List<IdentityResource> identityResourceModels = new List<IdentityResource>();

        List<Duende.IdentityServer.EntityFramework.Entities.IdentityResource> identityResourceEntities = await this.ConfigurationDbContext.IdentityResources
            .Include(a => a.UserClaims)
            .ToListAsync(cancellationToken: cancellationToken);

        if (identityResourceEntities.Any())
        {
            foreach (Duende.IdentityServer.EntityFramework.Entities.IdentityResource identityResourceEntity in identityResourceEntities)
            {
                identityResourceModels.Add(identityResourceEntity.ToModel());
            }
        }

        return Result.Success(identityResourceModels);
    }

    public async Task<Result> CreateRole(SecurityServiceCommands.CreateRoleCommand command, CancellationToken cancellationToken)
    {
        IdentityRole newIdentityRole = new IdentityRole
        {
            Id = command.RoleId.ToString(),
            Name = command.Name,
            NormalizedName = command.Name.ToUpper()
        };

        if (await this.RoleManager.RoleExistsAsync(newIdentityRole.Name))
        {
            return Result.Conflict($"Role {newIdentityRole.Name} already exists");
        }

        IdentityResult createResult = await this.RoleManager.CreateAsync(newIdentityRole);

        if (!createResult.Succeeded)
        {
            return Result.Failure($"Error creating role {newIdentityRole.Name} {createResult}");
        }

        return Result.Success();
    }

    public async Task<Result<RoleDetails>> GetRole(SecurityServiceQueries.GetRoleQuery query, CancellationToken cancellationToken)
    {
        IdentityRole identityRole = await this.RoleManager.FindByIdAsync(query.RoleId.ToString());

        if (identityRole == null)
        {
            return Result.NotFound($"No role found with Id {query.RoleId}");
        }

        RoleDetails response = new RoleDetails
        {
            RoleId = Guid.Parse(identityRole.Id),
            RoleName = identityRole.Name
        };

        return Result.Success(response);
    }

    public async Task<Result<List<RoleDetails>>> GetRoles(SecurityServiceQueries.GetRolesQuery query, CancellationToken cancellationToken)
    {
        List<RoleDetails> response = new List<RoleDetails>();

        IQueryable<IdentityRole> roleQuery = this.RoleManager.Roles;
        List<IdentityRole> roles = await roleQuery.ToListAsyncSafe(cancellationToken);

        foreach (IdentityRole identityRole in roles)
        {
            response.Add(new RoleDetails
            {
                RoleId = Guid.Parse(identityRole.Id),
                RoleName = identityRole.Name
            });
        }

        return Result.Success(response);
    }

    public async Task<Result> CreateUser(SecurityServiceCommands.CreateUserCommand command, CancellationToken cancellationToken)
    {
        ApplicationUser newIdentityUser = new()
        {
            Id = command.UserId.ToString(),
            Email = command.EmailAddress,
            UserName = command.UserName,
            NormalizedEmail = command.EmailAddress.ToUpper(),
            NormalizedUserName = command.UserName.ToUpper(),
            SecurityStamp = Guid.NewGuid().ToString("D"),
            PhoneNumber = command.PhoneNumber,
            RegistrationDateTime = DateTime.Now
        };

        Result<String> passwordValueResult = String.IsNullOrEmpty(command.Password)
            ? PasswordGenerator.GenerateRandomPassword(this.UserManager.Options.Password)
            : command.Password;

        if (passwordValueResult.IsFailed)
        {
            return ResultHelpers.CreateFailure(passwordValueResult);
        }

        newIdentityUser.PasswordHash = this.PasswordHasher.HashPassword(newIdentityUser, passwordValueResult.Data);

        if (String.IsNullOrEmpty(newIdentityUser.PasswordHash))
        {
            return Result.Failure("Error generating password hash value, hash was null or empty");
        }

        Result createResult = await this.CreateUserInternal(newIdentityUser);
        Result addRolesToUserResult = await this.AddRolesToUser(newIdentityUser, command.Roles);
        Result addClaimsToUserResult = await this.AddClaimsToUser(newIdentityUser, command);

        Result result = (createResult.IsSuccess, addRolesToUserResult.IsSuccess, addClaimsToUserResult.IsSuccess) switch
        {
            (true, true, true) => await this.SendConfirmationEmail(newIdentityUser, cancellationToken),
            _ => await this.DeleteUser(createResult, addRolesToUserResult, addClaimsToUserResult, newIdentityUser)
        };

        return result;
    }

    public async Task<Result<UserDetails>> GetUser(SecurityServiceQueries.GetUserQuery query, CancellationToken cancellationToken)
    {
        ApplicationUser user = await this.UserManager.FindByIdAsync(query.UserId.ToString());

        if (user == null)
        {
            return Result.NotFound($"No user found with user Id {query.UserId}");
        }

        UserDetails response = new UserDetails
        {
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            UserId = query.UserId,
            SubjectId = query.UserId.ToString(),
            Username = user.UserName,
            RegistrationDateTime = user.RegistrationDateTime,
            Roles = await this.ConvertUsersRoles(user),
            Claims = await this.ConvertUsersClaims(user)
        };

        return Result.Success(response);
    }

    public async Task<Result<List<UserDetails>>> GetUsers(SecurityServiceQueries.GetUsersQuery query, CancellationToken cancellationToken)
    {
        List<UserDetails> response = new List<UserDetails>();

        IQueryable<ApplicationUser> userQuery = this.UserManager.Users;

        if (String.IsNullOrEmpty(query.UserName) == false)
        {
            userQuery = userQuery.Where(u => u.UserName.Contains(query.UserName));
        }

        List<ApplicationUser> users = await userQuery.ToListAsyncSafe(cancellationToken);

        foreach (ApplicationUser identityUser in users)
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

        return Result.Success(response);
    }

    public async Task<Result<ChangeUserPasswordResult>> ChangeUserPassword(SecurityServiceCommands.ChangeUserPasswordCommand command, CancellationToken cancellationToken)
    {
        ApplicationUser user = await this.UserManager.FindByNameAsync(command.UserName);

        if (user == null)
        {
            return Result.NotFound();
        }

        IdentityResult result = await this.UserManager.ChangePasswordAsync(user, command.CurrentPassword, command.NewPassword);

        if (result.Succeeded == false)
        {
            Logger.LogInformation($"Errors during password change for user [{command.UserName} and Client [{command.ClientId}]");
            foreach (IdentityError identityError in result.Errors)
            {
                Logger.LogInformation($"Code {identityError.Code} Description {identityError.Description}");
            }

            return Result.Failure($"Errors during password change for user [{command.UserName} and Client [{command.ClientId}]");
        }

        ClientEntity client = await this.ConfigurationDbContext.Clients.SingleOrDefaultAsync(c => c.ClientId == command.ClientId, cancellationToken: cancellationToken);

        if (client == null)
        {
            Logger.LogInformation($"Client not found for clientId {command.ClientId}");
            return Result.Invalid($"Client not found for clientId {command.ClientId}");
        }

        Logger.LogWarning($"Client uri: {client.ClientUri}");
        return Result.Success(new ChangeUserPasswordResult { IsSuccessful = true, RedirectUri = client.ClientUri });
    }

    public async Task<Result> ConfirmUserEmailAddress(SecurityServiceCommands.ConfirmUserEmailAddressCommand command, CancellationToken cancellationToken)
    {
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

    public async Task<Result<string>> ProcessPasswordResetConfirmation(SecurityServiceCommands.ProcessPasswordResetConfirmationCommand command, CancellationToken cancellationToken)
    {
        ApplicationUser user = await this.UserManager.FindByNameAsync(command.Username);

        if (user == null)
        {
            Logger.LogInformation($"user not found for username {command.Username}");
            return Result.NotFound($"user not found for username {command.Username}");
        }

        IdentityResult result = await this.UserManager.ResetPasswordAsync(user, command.Token, command.Password);

        if (result.Succeeded == false)
        {
            Logger.LogInformation($"Errors during password reset for user [{command.Username} and Client [{command.ClientId}]");
            foreach (IdentityError identityError in result.Errors)
            {
                Logger.LogInformation($"Code {identityError.Code} Description {identityError.Description}");
            }

            return Result.Failure($"Errors during password reset for user [{command.Username} and Client [{command.ClientId}]");
        }

        ClientEntity client = await this.ConfigurationDbContext.Clients.SingleOrDefaultAsync(c => c.ClientId == command.ClientId, cancellationToken: cancellationToken);

        if (client == null)
        {
            Logger.LogInformation($"Client not found for clientId {command.ClientId}");
            return Result.Invalid($"Client not found for clientId {command.ClientId}");
        }

        return Result.Success(client.ClientUri);
    }

    public async Task<Result> ProcessPasswordResetRequest(SecurityServiceCommands.ProcessPasswordResetRequestCommand command, CancellationToken cancellationToken)
    {
        ApplicationUser user = await this.UserManager.FindByNameAsync(command.Username);

        if (user == null)
        {
            return Result.NotFound();
        }

        String resetToken = await this.UserManager.GeneratePasswordResetTokenAsync(user);
        resetToken = UrlEncoder.Default.Encode(resetToken);
        String uri = $"{this.ServiceOptions.PublicOrigin}/Account/ForgotPassword/Confirm?userName={user.UserName}&resetToken={resetToken}&clientId={command.ClientId}";

        TokenResponse token = await this.GetToken();
        SendEmailRequest emailRequest = this.BuildPasswordResetEmailRequest(user, uri);

        Result result = await this.MessagingServiceClient.SendEmail(token.AccessToken, emailRequest, cancellationToken);
        if (result.IsFailed)
        {
            return ResultHelpers.CreateFailure(result);
        }

        return Result.Success();
    }

    public async Task<Result> SendWelcomeEmail(SecurityServiceCommands.SendWelcomeEmailCommand command, CancellationToken cancellationToken)
    {
        ApplicationUser user = await this.UserManager.FindByNameAsync(command.Username);
        await this.UserManager.RemovePasswordAsync(user);
        Result<String> generatedPasswordResult = PasswordGenerator.GenerateRandomPassword(this.UserManager.Options.Password);
        if (generatedPasswordResult.IsFailed)
        {
            return ResultHelpers.CreateFailure(generatedPasswordResult);
        }

        await this.UserManager.AddPasswordAsync(user, generatedPasswordResult.Data);

        TokenResponse token = await this.GetToken();
        SendEmailRequest emailRequest = this.BuildWelcomeEmail(user.Email, generatedPasswordResult.Data);
        Result result = await this.MessagingServiceClient.SendEmail(token.AccessToken, emailRequest, cancellationToken);
        if (result.IsFailed)
        {
            return ResultHelpers.CreateFailure(result);
        }

        return Result.Success();
    }

    private async Task<Result> SendConfirmationEmail(ApplicationUser newIdentityUser, CancellationToken cancellationToken)
    {
        String confirmationToken = await this.UserManager.GenerateEmailConfirmationTokenAsync(newIdentityUser);
        confirmationToken = UrlEncoder.Default.Encode(confirmationToken);
        String uri = $"{this.ServiceOptions.PublicOrigin}/Account/EmailConfirmation/Confirm?userName={newIdentityUser.UserName}&confirmationToken={confirmationToken}";

        TokenResponse token = await this.GetToken();
        SendEmailRequest emailRequest = this.BuildEmailConfirmationRequest(newIdentityUser, uri);
        Result sendEmailResult = await this.MessagingServiceClient.SendEmail(token.AccessToken, emailRequest, cancellationToken);
        if (sendEmailResult.IsFailed)
        {
            Logger.LogWarning($"Error sending email to {newIdentityUser.Email} as part of user creation {sendEmailResult}");
        }

        return Result.Success();
    }

    private async Task<Result> DeleteUser(Result createResult, Result addRolesToUserResult, Result addClaimsToUserResult, ApplicationUser identityUserToDelete)
    {
        IdentityResult deleteResult = await this.UserManager.DeleteAsync(identityUserToDelete);

        if (deleteResult.Succeeded == false)
        {
            return Result.Failure($"Error deleting user {identityUserToDelete.UserName} as part of cleanup {deleteResult}");
        }

        return Result.Failure($"At least one part of the user creation failed - createResult: {createResult.IsSuccess} addRolesToUserResult: {addRolesToUserResult.IsSuccess} addClaimsToUserResult: {addClaimsToUserResult.IsSuccess}");
    }

    private async Task<Result> CreateUserInternal(ApplicationUser newIdentityUser)
    {
        IdentityResult createResult = await this.UserManager.CreateAsync(newIdentityUser);

        if (!createResult.Succeeded)
        {
            return Result.Failure($"Error creating user {newIdentityUser.UserName} {createResult}");
        }

        return Result.Success();
    }

    private async Task<Result> AddRolesToUser(ApplicationUser newIdentityUser, List<String> roles)
    {
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

    private async Task<Result> AddClaimsToUser(ApplicationUser newIdentityUser, SecurityServiceCommands.CreateUserCommand command)
    {
        List<Claim> claimsToAdd = new List<Claim>();
        if (command.Claims != null)
        {
            foreach (KeyValuePair<String, String> claim in command.Claims)
            {
                claimsToAdd.Add(new Claim(claim.Key, claim.Value));
            }
        }

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

        IdentityResult addClaimsResult = await this.UserManager.AddClaimsAsync(newIdentityUser, claimsToAdd);

        if (!addClaimsResult.Succeeded)
        {
            return Result.Failure($"Error adding claims [{String.Join(",", claimsToAdd)}] to user {newIdentityUser.UserName} {addClaimsResult}");
        }

        return Result.Success();
    }

    private SendEmailRequest BuildEmailConfirmationRequest(ApplicationUser user, String emailConfirmationToken)
    {
        StringBuilder messageBuilder = new StringBuilder();
        messageBuilder.Append("<html>");
        messageBuilder.Append("<body>");
        messageBuilder.Append("<p><strong>Thank you for registering</strong></p>");
        messageBuilder.Append("<p></p>");
        messageBuilder.Append($"<p>Please <a href=\"{emailConfirmationToken}\">click here</a> to confirm your email address.</p>");
        messageBuilder.Append("<p>Thanks for your registration.</p>");
        messageBuilder.Append("</body>");
        messageBuilder.Append("</html>");

        return new SendEmailRequest
        {
            Body = messageBuilder.ToString(),
            ConnectionIdentifier = Guid.NewGuid(),
            FromAddress = "golfhandicapping@btinternet.com",
            IsHtml = true,
            Subject = "Email Address Confirmation",
            ToAddresses = new List<String> { user.Email, "stuart_ferguson1@outlook.com" }
        };
    }

    private SendEmailRequest BuildPasswordResetEmailRequest(ApplicationUser user, String resetToken)
    {
        StringBuilder messageBuilder = new StringBuilder();
        messageBuilder.Append("<html>");
        messageBuilder.Append("<body>");
        messageBuilder.Append("<p><strong>Thanks for your password reset request</strong></p>");
        messageBuilder.Append("<p></p>");
        messageBuilder.Append($"<p>Please <a href=\"{resetToken}\">click here</a> to confirm this password reset was from you.</p>");
        messageBuilder.Append("<p>Thanks for your password reset request.</p>");
        messageBuilder.Append("</body>");
        messageBuilder.Append("</html>");

        return new SendEmailRequest
        {
            Body = messageBuilder.ToString(),
            ConnectionIdentifier = Guid.NewGuid(),
            FromAddress = "golfhandicapping@btinternet.com",
            IsHtml = true,
            Subject = "Password Reset Requested",
            ToAddresses = new List<String> { user.Email, "stuart_ferguson1@outlook.com" }
        };
    }

    private SendEmailRequest BuildWelcomeEmail(String emailAddress, String password)
    {
        StringBuilder messageBuilder = new StringBuilder();
        messageBuilder.AppendLine("<html><body>");
        messageBuilder.AppendLine("<p>Welcome to Transaction Processing System</p>");
        messageBuilder.AppendLine("<p></p>");
        messageBuilder.AppendLine("<p>Please find below your user details:</p>");
        messageBuilder.AppendLine("<table>");
        messageBuilder.AppendLine("<tr><td><strong>User Name</strong></td></tr>");
        messageBuilder.AppendLine($"<tr><td id=\"username\">{emailAddress}</td></tr>");
        messageBuilder.AppendLine("<tr><td><strong>Password</strong></td></tr>");
        messageBuilder.AppendLine($"<tr><td id=\"password\">{password}</td></tr>");
        messageBuilder.AppendLine("</table>");
        messageBuilder.AppendLine("</body></html>");

        return new SendEmailRequest
        {
            Body = messageBuilder.ToString(),
            ConnectionIdentifier = Guid.NewGuid(),
            FromAddress = "golfhandicapping@btinternet.com",
            IsHtml = true,
            Subject = "Welcome to Transaction Processing",
            ToAddresses = new List<String> { emailAddress, "stuart_ferguson1@outlook.com" }
        };
    }

    private async Task<Dictionary<String, String>> ConvertUsersClaims(ApplicationUser identityUser)
    {
        Dictionary<String, String> response = new Dictionary<String, String>();
        IList<Claim> claims = await this.UserManager.GetClaimsAsync(identityUser);
        foreach (Claim claim in claims)
        {
            response.Add(claim.Type, claim.Value);
        }

        return response;
    }

    private async Task<List<String>> ConvertUsersRoles(ApplicationUser identityUser)
    {
        IList<String> roles = await this.UserManager.GetRolesAsync(identityUser);
        return roles.ToList();
    }

    private async Task<TokenResponse> GetToken()
    {
        String clientId = this.ServiceOptions.ClientId;

        Logger.LogInformation($"Client Id is {clientId}");
        Logger.LogInformation($"Client Secret is {this.ServiceOptions.ClientSecret}");

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

    private Result ValidateGrantTypes(List<String> allowedGrantTypes)
    {
        List<String> validTypesList = new List<String>
        {
            GrantType.AuthorizationCode,
            GrantType.ClientCredentials,
            GrantType.DeviceFlow,
            GrantType.Hybrid,
            GrantType.Implicit,
            GrantType.ResourceOwnerPassword
        };

        List<String> invalidGrantTypes = allowedGrantTypes.Where(a => validTypesList.All(v => v != a)).ToList();

        if (invalidGrantTypes.Any())
        {
            return Result.Invalid($"The grant types [{String.Join(", ", invalidGrantTypes)}] are not valid to create a new client");
        }

        return Result.Success();
    }
}
