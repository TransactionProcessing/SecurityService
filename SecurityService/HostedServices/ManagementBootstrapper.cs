using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using SecurityService.BusinessLogic;
using SecurityService.BusinessLogic.Oidc;
using SecurityService.Database;
using SecurityService.Database.DbContexts;
using SecurityService.Database.Entities;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace SecurityService.HostedServices;

public sealed class ManagementBootstrapper
{
    private const string AdministratorRole = "Administrator";

    private readonly SecurityServiceDbContext _dbContext;
    private readonly IOpenIddictApplicationManager _applicationManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ManagementBootstrapOptions _options;
    private readonly OAuthOptions _oauthOptions;

    public ManagementBootstrapper(
        SecurityServiceDbContext dbContext,
        IOpenIddictApplicationManager applicationManager,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IOptions<ServiceOptions> options)
    {
        _dbContext = dbContext;
        _applicationManager = applicationManager;
        _userManager = userManager;
        _roleManager = roleManager;
        _options = options.Value.ManagementBootstrap;
        _oauthOptions = options.Value.OAuth;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        if (_options.Enabled == false)
        {
            return;
        }

        ValidateOptions();
        await EnsureBootstrapClientAsync(cancellationToken);
        await EnsureAdministratorAsync();
    }

    private void ValidateOptions()
    {
        if (string.IsNullOrWhiteSpace(_options.ClientId))
        {
            throw new InvalidOperationException("Management bootstrap is enabled but ServiceOptions:ManagementBootstrap:ClientId is empty.");
        }

        if (string.IsNullOrWhiteSpace(_options.ClientSecret))
        {
            throw new InvalidOperationException("Management bootstrap is enabled but ServiceOptions:ManagementBootstrap:ClientSecret is empty.");
        }

        bool anyAdminValue = string.IsNullOrWhiteSpace(_options.AdminUserName) == false ||
                             string.IsNullOrWhiteSpace(_options.AdminEmail) == false ||
                             string.IsNullOrWhiteSpace(_options.AdminPassword) == false;
        bool completeAdminValue = string.IsNullOrWhiteSpace(_options.AdminUserName) == false &&
                                  string.IsNullOrWhiteSpace(_options.AdminEmail) == false &&
                                  string.IsNullOrWhiteSpace(_options.AdminPassword) == false;
        if (anyAdminValue && completeAdminValue == false)
        {
            throw new InvalidOperationException("Management bootstrap administrator configuration requires AdminUserName, AdminEmail, and AdminPassword.");
        }
    }

    private async Task EnsureBootstrapClientAsync(CancellationToken cancellationToken)
    {
        if (_dbContext.Database.IsRelational() == false)
        {
            await EnsureBootstrapClientWithoutTransactionAsync(cancellationToken);
            return;
        }

        IExecutionStrategy executionStrategy = _dbContext.Database.CreateExecutionStrategy();
        await executionStrategy.ExecuteAsync(async () =>
        {
            await using IDbContextTransaction transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                object? application = await _applicationManager.FindByClientIdAsync(_options.ClientId, cancellationToken);
                if (application is null)
                {
                    var descriptor = new OpenIddictApplicationDescriptor
                    {
                        ClientId = _options.ClientId,
                        ClientSecret = _options.ClientSecret,
                        ClientType = ClientTypes.Confidential,
                        ConsentType = ConsentTypes.Implicit,
                        DisplayName = _options.ClientName
                    };

                    OAuthGrantPolicyResult policy = OAuthGrantPolicy.CreatePermissions([GrantTypes.ClientCredentials], _options.ClientId, _oauthOptions);
                    descriptor.Permissions.UnionWith(policy.Permissions);
                    descriptor.Requirements.UnionWith(policy.Requirements);

                    await _applicationManager.CreateAsync(descriptor, cancellationToken);
                }
                else if (await _applicationManager.ValidateClientSecretAsync(application, _options.ClientSecret, cancellationToken) == false)
                {
                    throw new InvalidOperationException("The existing management bootstrap application has a different client secret.");
                }

                await SynchronizeBootstrapClientPermissionsAsync(application, cancellationToken);

                ClientDefinition? definition = await _dbContext.ClientDefinitions
                    .SingleOrDefaultAsync(client => client.ClientId == _options.ClientId, cancellationToken);
                if (definition is null)
                {
                    await _dbContext.ClientDefinitions.AddAsync(new ClientDefinition
                    {
                        Id = Guid.NewGuid(),
                        ClientId = _options.ClientId,
                        ClientName = _options.ClientName,
                        SecretHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(_options.ClientSecret))),
                        AllowedGrantTypesJson = JsonListSerializer.Serialize([GrantTypes.ClientCredentials]),
                        AllowedScopesJson = "[]",
                        RedirectUrisJson = "[]",
                        PostLogoutRedirectUrisJson = "[]",
                        ClientType = ClientTypes.Confidential
                    }, cancellationToken);
                    await _dbContext.SaveChangesAsync(cancellationToken);
                }

                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(CancellationToken.None);
                throw;
            }
        });
    }

    private async Task EnsureBootstrapClientWithoutTransactionAsync(CancellationToken cancellationToken)
    {
        object? application = await _applicationManager.FindByClientIdAsync(_options.ClientId, cancellationToken);
        if (application is null)
        {
            var descriptor = new OpenIddictApplicationDescriptor
            {
                ClientId = _options.ClientId,
                ClientSecret = _options.ClientSecret,
                ClientType = ClientTypes.Confidential,
                ConsentType = ConsentTypes.Implicit,
                DisplayName = _options.ClientName
            };

            OAuthGrantPolicyResult policy = OAuthGrantPolicy.CreatePermissions([GrantTypes.ClientCredentials], _options.ClientId, _oauthOptions);
            descriptor.Permissions.UnionWith(policy.Permissions);
            descriptor.Requirements.UnionWith(policy.Requirements);

            await _applicationManager.CreateAsync(descriptor, cancellationToken);
        }
        else if (await _applicationManager.ValidateClientSecretAsync(application, _options.ClientSecret, cancellationToken) == false)
        {
            throw new InvalidOperationException("The existing management bootstrap application has a different client secret.");
        }

        await SynchronizeBootstrapClientPermissionsAsync(application, cancellationToken);

        ClientDefinition? definition = await _dbContext.ClientDefinitions
            .SingleOrDefaultAsync(client => client.ClientId == _options.ClientId, cancellationToken);
        if (definition is null)
        {
            await _dbContext.ClientDefinitions.AddAsync(new ClientDefinition
            {
                Id = Guid.NewGuid(),
                ClientId = _options.ClientId,
                ClientName = _options.ClientName,
                SecretHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(_options.ClientSecret))),
                AllowedGrantTypesJson = JsonListSerializer.Serialize([GrantTypes.ClientCredentials]),
                AllowedScopesJson = "[]",
                RedirectUrisJson = "[]",
                PostLogoutRedirectUrisJson = "[]",
                ClientType = ClientTypes.Confidential
            }, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task SynchronizeBootstrapClientPermissionsAsync(object? application, CancellationToken cancellationToken)
    {
        if (application is null)
        {
            return;
        }

        OpenIddictApplicationDescriptor descriptor = new();
        await _applicationManager.PopulateAsync(descriptor, application, cancellationToken);
        OAuthGrantPolicyResult policy = OAuthGrantPolicy.CreatePermissions([GrantTypes.ClientCredentials], _options.ClientId, _oauthOptions);
        descriptor.Permissions.RemoveWhere(permission => permission.StartsWith(Permissions.Prefixes.GrantType, StringComparison.Ordinal));
        descriptor.Permissions.UnionWith(policy.Permissions);
        descriptor.Requirements.UnionWith(policy.Requirements);
        await _applicationManager.UpdateAsync(application, descriptor, cancellationToken);
    }

    private async Task EnsureAdministratorAsync()
    {
        if (string.IsNullOrWhiteSpace(_options.AdminUserName))
        {
            return;
        }

        if (await _roleManager.RoleExistsAsync(AdministratorRole) == false)
        {
            IdentityResult roleResult = await _roleManager.CreateAsync(new IdentityRole(AdministratorRole));
            if (roleResult.Succeeded == false)
            {
                throw new InvalidOperationException($"Unable to create bootstrap administrator role: {roleResult}");
            }
        }

        ApplicationUser? user = await _userManager.FindByNameAsync(_options.AdminUserName);
        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = _options.AdminUserName,
                Email = _options.AdminEmail,
                EmailConfirmed = true,
                RegistrationDateTime = DateTime.UtcNow
            };

            IdentityResult userResult = await _userManager.CreateAsync(user, _options.AdminPassword!);
            if (userResult.Succeeded == false)
            {
                throw new InvalidOperationException($"Unable to create bootstrap administrator user: {userResult}");
            }
        }
        else if (string.Equals(user.Email, _options.AdminEmail, StringComparison.OrdinalIgnoreCase) == false)
        {
            throw new InvalidOperationException("The existing bootstrap administrator has a different email address.");
        }

        if (await _userManager.IsInRoleAsync(user, AdministratorRole) == false)
        {
            IdentityResult roleAssignmentResult = await _userManager.AddToRoleAsync(user, AdministratorRole);
            if (roleAssignmentResult.Succeeded == false)
            {
                throw new InvalidOperationException($"Unable to assign bootstrap administrator role: {roleAssignmentResult}");
            }
        }
    }
}
