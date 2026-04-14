using System.Security.Cryptography;
using System.Text;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using SecurityService.BusinessLogic.Requests;
using SecurityService.Database;
using SecurityService.Database.DbContexts;
using SecurityService.Database.Entities;
using SecurityService.Models;
using SimpleResults;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace SecurityService.BusinessLogic.RequestHandlers;

public sealed class ClientRequestHandler :
    IRequestHandler<SecurityServiceCommands.CreateClientCommand, Result>,
    IRequestHandler<SecurityServiceQueries.GetClientQuery, Result<ClientDetails>>,
    IRequestHandler<SecurityServiceQueries.GetClientsQuery, Result<List<ClientDetails>>>
{
    private static readonly HashSet<string> SupportedGrantTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        GrantTypes.AuthorizationCode,
        GrantTypes.ClientCredentials,
        GrantTypes.DeviceCode,
        "hybrid",
        GrantTypes.Implicit,
        GrantTypes.Password,
        GrantTypes.RefreshToken
    };

    private readonly SecurityServiceDbContext DbContext;
    private readonly IOpenIddictApplicationManager ApplicationManager;

    public ClientRequestHandler(SecurityServiceDbContext dbContext, IOpenIddictApplicationManager applicationManager)
    {
        this.DbContext = dbContext;
        this.ApplicationManager = applicationManager;
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

        String[] invalidGrantTypes = command.AllowedGrantTypes.Where(grantType => SupportedGrantTypes.Contains(grantType) == false).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        if (invalidGrantTypes.Length > 0)
        {
            return Result.Invalid($"Unsupported grant types: {string.Join(", ", invalidGrantTypes)}.");
        }

        if (await this.DbContext.ClientDefinitions.AnyAsync(client => client.ClientId == command.ClientId, cancellationToken))
        {
            return Result.Conflict($"A client with id '{command.ClientId}' already exists.");
        }

        OpenIddictApplicationDescriptor descriptor = new OpenIddictApplicationDescriptor
        {
            ClientId = command.ClientId,
            DisplayName = command.ClientName,
            ConsentType = command.RequireConsent ? ConsentTypes.Explicit : ConsentTypes.Implicit,
            ClientType = string.IsNullOrWhiteSpace(command.Secret) ? ClientTypes.Public : ClientTypes.Confidential,
            ClientSecret = string.IsNullOrWhiteSpace(command.Secret) ? null : command.Secret
        };

        foreach (String redirectUri in command.ClientRedirectUris.Where(uri => string.IsNullOrWhiteSpace(uri) == false).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            descriptor.RedirectUris.Add(new Uri(redirectUri, UriKind.Absolute));
        }

        foreach (String postLogoutRedirectUri in command.ClientPostLogoutRedirectUris.Where(uri => string.IsNullOrWhiteSpace(uri) == false).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            descriptor.PostLogoutRedirectUris.Add(new Uri(postLogoutRedirectUri, UriKind.Absolute));
        }

        await this.ApplicationManager.CreateAsync(descriptor, cancellationToken);

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
            RedirectUrisJson = JsonListSerializer.Serialize(command.ClientRedirectUris),
            PostLogoutRedirectUrisJson = JsonListSerializer.Serialize(command.ClientPostLogoutRedirectUris),
            RequireConsent = command.RequireConsent,
            AllowOfflineAccess = command.AllowOfflineAccess,
            ClientType = descriptor.ClientType
        };

        await this.DbContext.ClientDefinitions.AddAsync(definition, cancellationToken);
        await this.DbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
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
