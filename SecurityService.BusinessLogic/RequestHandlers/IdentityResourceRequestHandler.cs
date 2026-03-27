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

public sealed class IdentityResourceRequestHandler :
    IRequestHandler<SecurityServiceCommands.CreateIdentityResourceCommand, Result>,
    IRequestHandler<SecurityServiceQueries.GetIdentityResourceQuery, Result<IdentityResourceDetails>>,
    IRequestHandler<SecurityServiceQueries.GetIdentityResourcesQuery, Result<List<IdentityResourceDetails>>>
{
    private readonly SecurityServiceDbContext _dbContext;
    private readonly IOpenIddictScopeManager _scopeManager;

    public IdentityResourceRequestHandler(SecurityServiceDbContext dbContext, IOpenIddictScopeManager scopeManager)
    {
        this._dbContext = dbContext;
        this._scopeManager = scopeManager;
    }

    public async Task<Result> Handle(SecurityServiceCommands.CreateIdentityResourceCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Name))
        {
            return Result.Failure("Identity resource name is required.");
        }

        if (await this._dbContext.ResourceDefinitions.AnyAsync(resource => resource.Name == command.Name && resource.Type == ResourceType.IdentityResource, cancellationToken))
        {
            return Result.Conflict($"An identity resource named '{command.Name}' already exists.");
        }

        var descriptor = new OpenIddictScopeDescriptor
        {
            Name = command.Name,
            DisplayName = command.DisplayName,
            Description = command.Description
        };

        await this._scopeManager.CreateAsync(descriptor, cancellationToken);

        var resource = new ResourceDefinition
        {
            Id = Guid.NewGuid(),
            Name = command.Name,
            DisplayName = command.DisplayName,
            Description = command.Description,
            Type = ResourceType.IdentityResource,
            Required = command.Required,
            Emphasize = command.Emphasize,
            ShowInDiscoveryDocument = command.ShowInDiscoveryDocument,
            ClaimsJson = JsonListSerializer.Serialize(command.Claims)
        };

        this._dbContext.ResourceDefinitions.Add(resource);
        await this._dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result<IdentityResourceDetails>> Handle(SecurityServiceQueries.GetIdentityResourceQuery query, CancellationToken cancellationToken)
    {
        var resource = await this._dbContext.ResourceDefinitions.SingleOrDefaultAsync(definition => definition.Name == query.Name && definition.Type == ResourceType.IdentityResource, cancellationToken);
        return resource is null
            ? Result.NotFound($"No identity resource named '{query.Name}' was found.")
            : Result.Success(new IdentityResourceDetails(resource.Name, resource.DisplayName, resource.Description, resource.Required, resource.Emphasize, resource.ShowInDiscoveryDocument, JsonListSerializer.Deserialize(resource.ClaimsJson)));
    }

    public async Task<Result<List<IdentityResourceDetails>>> Handle(SecurityServiceQueries.GetIdentityResourcesQuery query, CancellationToken cancellationToken)
    {
        var resources = await this._dbContext.ResourceDefinitions.Where(definition => definition.Type == ResourceType.IdentityResource).OrderBy(definition => definition.Name).ToListAsync(cancellationToken);
        return Result.Success(resources.Select(resource => new IdentityResourceDetails(resource.Name, resource.DisplayName, resource.Description, resource.Required, resource.Emphasize, resource.ShowInDiscoveryDocument, JsonListSerializer.Deserialize(resource.ClaimsJson))).ToList());
    }
}
