using System.Security.Cryptography;
using System.Text;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using SecurityService.BusinessLogic.Oidc;
using SecurityService.BusinessLogic.Requests;
using SecurityService.Database;
using SecurityService.Database.DbContexts;
using SecurityService.Database.Entities;
using SecurityService.Models;
using SecurityService.BusinessLogic.Validation;
using SimpleResults;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace SecurityService.BusinessLogic.RequestHandlers;

public sealed class ClientRequestHandler :
    IRequestHandler<SecurityServiceCommands.CreateClientCommand, Result>,
    IRequestHandler<SecurityServiceQueries.GetClientQuery, Result<ClientDetails>>,
    IRequestHandler<SecurityServiceQueries.GetClientsQuery, Result<List<ClientDetails>>>
{
    private readonly SecurityServiceDbContext DbContext;
    private readonly IOpenIddictApplicationManager ApplicationManager;
    private readonly OAuthOptions OAuthOptions;

    public ClientRequestHandler(
        SecurityServiceDbContext dbContext,
        IOpenIddictApplicationManager applicationManager,
        IOptions<ServiceOptions> options)
    {
        this.DbContext = dbContext;
        this.ApplicationManager = applicationManager;
        this.OAuthOptions = options.Value.OAuth;
    }

    public async Task<Result> Handle(SecurityServiceCommands.CreateClientCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.ClientId))
        {
            return Result.Invalid("Client id is required.");
        }

        if (command.AllowedGrantTypes.Count == 0)
        {
            return Result.Invalid("At least one grant type is required.");
        }

        String[] invalidGrantTypes = command.AllowedGrantTypes.Where(grantType => OAuthGrantPolicy.IsGrantAllowed(grantType, command.ClientId, this.OAuthOptions) == false).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        if (invalidGrantTypes.Length > 0)
        {
            return Result.Invalid($"Unsupported grant types: {string.Join(", ", invalidGrantTypes)}.");
        }

        List<string> redirectUris = command.ClientRedirectUris
            .Where(uri => string.IsNullOrWhiteSpace(uri) == false)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        Result<List<Uri>> redirectUriValidation = ClientRedirectUriValidator.Validate(redirectUris, nameof(command.ClientRedirectUris));
        if (redirectUriValidation.IsFailed)
        {
            return Result.Invalid(redirectUriValidation.Message);
        }

        List<string> postLogoutRedirectUris = command.ClientPostLogoutRedirectUris
            .Where(uri => string.IsNullOrWhiteSpace(uri) == false)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        Result<List<Uri>> postLogoutRedirectUriValidation = ClientRedirectUriValidator.Validate(postLogoutRedirectUris, nameof(command.ClientPostLogoutRedirectUris));
        if (postLogoutRedirectUriValidation.IsFailed)
        {
            return Result.Invalid(postLogoutRedirectUriValidation.Message);
        }

        if (await this.DbContext.ClientDefinitions.AnyAsync(client => client.ClientId == command.ClientId, cancellationToken))
        {
            return Result.Conflict($"A client with id '{command.ClientId}' already exists.");
        }

        OAuthGrantPolicyResult permissions = OAuthGrantPolicy.CreatePermissions(command.AllowedGrantTypes, command.ClientId, this.OAuthOptions);

        OpenIddictApplicationDescriptor descriptor = new OpenIddictApplicationDescriptor
        {
            ClientId = command.ClientId,
            DisplayName = command.ClientName,
            ConsentType = command.RequireConsent ? ConsentTypes.Explicit : ConsentTypes.Implicit,
            ClientType = string.IsNullOrWhiteSpace(command.Secret) ? ClientTypes.Public : ClientTypes.Confidential,
            ClientSecret = string.IsNullOrWhiteSpace(command.Secret) ? null : command.Secret
        };

        descriptor.Permissions.UnionWith(permissions.Permissions);
        descriptor.Requirements.UnionWith(permissions.Requirements);

        if (command.AllowOfflineAccess)
        {
            descriptor.Permissions.Add(Permissions.GrantTypes.RefreshToken);
        }

        foreach (Uri redirectUri in redirectUriValidation.Data!)
        {
            descriptor.RedirectUris.Add(redirectUri);
        }

        foreach (Uri postLogoutRedirectUri in postLogoutRedirectUriValidation.Data!)
        {
            descriptor.PostLogoutRedirectUris.Add(postLogoutRedirectUri);
        }

        ClientDefinition definition = new ClientDefinition
        {
            Id = Guid.NewGuid(),
            ClientId = command.ClientId,
            ClientName = command.ClientName,
            Description = command.ClientDescription,
            ClientUri = command.ClientUri,
            SecretHash = string.IsNullOrWhiteSpace(command.Secret) ? null : Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(command.Secret))),
            AllowedScopesJson = JsonListSerializer.Serialize(command.AllowedScopes),
            AllowedGrantTypesJson = JsonListSerializer.Serialize(command.AllowedGrantTypes),
            RedirectUrisJson = JsonListSerializer.Serialize(redirectUris),
            PostLogoutRedirectUrisJson = JsonListSerializer.Serialize(postLogoutRedirectUris),
            RequireConsent = command.RequireConsent,
            AllowOfflineAccess = command.AllowOfflineAccess,
            ClientType = descriptor.ClientType
        };

        try
        {
            await this.PersistClientAsync(descriptor, definition, command.ClientId, cancellationToken);
        }
        catch (DbUpdateException)
        {
            this.DbContext.ChangeTracker.Clear();
            if (await this.DbContext.ClientDefinitions.AnyAsync(client => client.ClientId == command.ClientId, cancellationToken) ||
                await this.ApplicationManager.FindByClientIdAsync(command.ClientId, cancellationToken) is not null)
            {
                return Result.Conflict($"A client with id '{command.ClientId}' already exists.");
            }

            throw;
        }

        return Result.Success();
    }

    private async Task PersistClientAsync(
        OpenIddictApplicationDescriptor descriptor,
        ClientDefinition definition,
        string clientId,
        CancellationToken cancellationToken)
    {
        IExecutionStrategy executionStrategy = this.DbContext.Database.CreateExecutionStrategy();
        await executionStrategy.ExecuteAsync(async () =>
        {
            if (this.DbContext.Database.IsRelational() == false)
            {
                await this.PersistClientWithoutTransactionAsync(descriptor, definition, clientId, cancellationToken);
                return;
            }

            await using IDbContextTransaction transaction = await this.DbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                object? application = await this.ApplicationManager.FindByClientIdAsync(clientId, cancellationToken);
                bool definitionExists = await this.DbContext.ClientDefinitions.AnyAsync(client => client.ClientId == clientId, cancellationToken);
                if (application is not null && definitionExists)
                {
                    return;
                }

                if (application is null)
                {
                    await this.ApplicationManager.CreateAsync(descriptor, cancellationToken);
                }

                if (definitionExists == false)
                {
                    await this.DbContext.ClientDefinitions.AddAsync(definition, cancellationToken);
                    await this.DbContext.SaveChangesAsync(cancellationToken);
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

    private async Task PersistClientWithoutTransactionAsync(
        OpenIddictApplicationDescriptor descriptor,
        ClientDefinition definition,
        string clientId,
        CancellationToken cancellationToken)
    {
        object? application = await this.ApplicationManager.FindByClientIdAsync(clientId, cancellationToken);
        bool definitionExists = await this.DbContext.ClientDefinitions.AnyAsync(client => client.ClientId == clientId, cancellationToken);
        if (application is not null && definitionExists)
        {
            return;
        }

        if (application is null)
        {
            await this.ApplicationManager.CreateAsync(descriptor, cancellationToken);
        }

        if (definitionExists == false)
        {
            await this.DbContext.ClientDefinitions.AddAsync(definition, cancellationToken);
            await this.DbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<Result<ClientDetails>> Handle(SecurityServiceQueries.GetClientQuery query, CancellationToken cancellationToken)
    {
        ClientDefinition? definition = await this.DbContext.ClientDefinitions.SingleOrDefaultAsync(client => client.ClientId == query.ClientId, cancellationToken);
        return definition is null
            ? Result.NotFound($"No client found with id '{query.ClientId}'.")
            : Result.Success(Factory.ConvertFrom(definition));
    }

    public async Task<Result<List<ClientDetails>>> Handle(SecurityServiceQueries.GetClientsQuery query, CancellationToken cancellationToken)
    {
        List<ClientDefinition> definitions = await this.DbContext.ClientDefinitions.OrderBy(client => client.ClientId).ToListAsync(cancellationToken);
        return Result.Success(definitions.Select(Factory.ConvertFrom).ToList());
    }

    
}
