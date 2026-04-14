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
using ResourceType = SecurityService.Database.Entities.ResourceType;

namespace SecurityService.BusinessLogic.RequestHandlers;

public sealed class ApiResourceRequestHandler :
    IRequestHandler<SecurityServiceCommands.CreateApiResourceCommand, Result>,
    IRequestHandler<SecurityServiceQueries.GetApiResourceQuery, Result<ApiResourceDetails>>,
    IRequestHandler<SecurityServiceQueries.GetApiResourcesQuery, Result<List<ApiResourceDetails>>>
{
    private readonly SecurityServiceDbContext DbContext;
    private readonly IOpenIddictScopeManager ScopeManager;

    public ApiResourceRequestHandler(SecurityServiceDbContext dbContext, IOpenIddictScopeManager scopeManager)
    {
        this.DbContext = dbContext;
        this.ScopeManager = scopeManager;
    }

    public async Task<Result> Handle(SecurityServiceCommands.CreateApiResourceCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Name)) {
            return Result.Invalid("API resource name is required.");
        }

        if (await this.DbContext.ResourceDefinitions.AnyAsync(resource => resource.Name == command.Name && resource.Type == ResourceType.ApiResource, cancellationToken)) {
            return Result.Conflict($"An API resource named '{command.Name}' already exists.");
        }

        foreach (String scopeName in command.Scopes.Distinct(StringComparer.OrdinalIgnoreCase)) {
            Object? scope = await this.ScopeManager.FindByNameAsync(scopeName, cancellationToken);
            if (scope is null) {
                scope = await this.ScopeManager.CreateAsync(new OpenIddictScopeDescriptor { Name = scopeName, DisplayName = scopeName, Description = $"Auto-created scope for API resource '{command.Name}'." }, cancellationToken);
            }

            OpenIddictScopeDescriptor descriptor = new();
            await this.ScopeManager.PopulateAsync(descriptor, scope, cancellationToken);
            if (descriptor.Resources.Contains(command.Name, StringComparer.OrdinalIgnoreCase) == false) {
                descriptor.Resources.Add(command.Name);
                await this.ScopeManager.UpdateAsync(scope, descriptor, cancellationToken);
            }
        }

        ResourceDefinition resource = new() {
            Id = Guid.NewGuid(),
            Name = command.Name,
            DisplayName = command.DisplayName,
            Description = command.Description,
            Type = ResourceType.ApiResource,
            SecretHash = string.IsNullOrWhiteSpace(command.Secret) ? null : Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(command.Secret))),
            ClaimsJson = JsonListSerializer.Serialize(command.UserClaims),
            ScopesJson = JsonListSerializer.Serialize(command.Scopes)
        };

        await this.DbContext.ResourceDefinitions.AddAsync(resource, cancellationToken);
        await this.DbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result<ApiResourceDetails>> Handle(SecurityServiceQueries.GetApiResourceQuery query, CancellationToken cancellationToken)
    {
        ResourceDefinition? resource = await this.DbContext.ResourceDefinitions.SingleOrDefaultAsync(definition => definition.Name == query.Name && definition.Type == ResourceType.ApiResource, cancellationToken);
        return resource switch {
            null => Result.NotFound($"No API resource named '{query.Name}' was found."),
            _ => Result.Success(Factory.ConvertFrom(resource))
        };
    }

    public async Task<Result<List<ApiResourceDetails>>> Handle(SecurityServiceQueries.GetApiResourcesQuery query, CancellationToken cancellationToken)
    {
        List<ResourceDefinition> resources = await this.DbContext.ResourceDefinitions.Where(definition => definition.Type == ResourceType.ApiResource).OrderBy(definition => definition.Name).ToListAsync(cancellationToken);
        return Result.Success(Factory.ConvertFrom(resources));
    }
}


public static class Factory
{
    public static ApiResourceDetails ConvertFrom(ResourceDefinition resource) {
        return new ApiResourceDetails(resource.Name, resource.DisplayName, resource.Description, JsonListSerializer.Deserialize(resource.ScopesJson), JsonListSerializer.Deserialize(resource.ClaimsJson));
    }

    public static List<ApiResourceDetails> ConvertFrom(List<ResourceDefinition> resources)
    {
        List<ApiResourceDetails> results = new();
        foreach (ResourceDefinition resource in resources) {
            results.Add(ConvertFrom(resource));
        }
        return results;
    }

    public static ApiScopeDetails ConvertFrom(String name, String displayName, String description)
    {
        return new ApiScopeDetails(name, displayName, description);
    }

    public static List<ApiScopeDetails> ConvertFrom(List<(String name, String displayName, String description)> resources) {
        List<ApiScopeDetails> results = new();
        foreach ((String name, String displayName, String description) resource in resources)
        {
            results.Add(ConvertFrom(resource.name, resource.displayName, resource.description));
        }
        return results;
    }

    public static ClientDetails ConvertFrom(ClientDefinition definition) => new(
        definition.ClientId,
        definition.ClientName,
        definition.Description,
        definition.ClientUri,
        JsonListSerializer.Deserialize(definition.AllowedScopesJson),
        JsonListSerializer.Deserialize(definition.AllowedGrantTypesJson),
        JsonListSerializer.Deserialize(definition.RedirectUrisJson),
        JsonListSerializer.Deserialize(definition.PostLogoutRedirectUrisJson),
        definition.RequireConsent,
        definition.AllowOfflineAccess,
        definition.ClientType);
}