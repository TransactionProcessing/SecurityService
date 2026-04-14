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
    private readonly SecurityServiceDbContext DbContext;
    private readonly IOpenIddictScopeManager ScopeManager;

    public IdentityResourceRequestHandler(SecurityServiceDbContext dbContext, IOpenIddictScopeManager scopeManager)
    {
        this.DbContext = dbContext;
        this.ScopeManager = scopeManager;
    }

    public async Task<Result> Handle(SecurityServiceCommands.CreateIdentityResourceCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Name))
        {
            return Result.Failure("Identity resource name is required.");
        }

        if (await this.DbContext.ResourceDefinitions.AnyAsync(resource => resource.Name == command.Name && resource.Type == ResourceType.IdentityResource, cancellationToken))
        {
            return Result.Conflict($"An identity resource named '{command.Name}' already exists.");
        }

        var descriptor = new OpenIddictScopeDescriptor
        {
            Name = command.Name,
            DisplayName = command.DisplayName,
            Description = command.Description
        };

        await this.ScopeManager.CreateAsync(descriptor, cancellationToken);

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

        await this.DbContext.ResourceDefinitions.AddAsync(resource, cancellationToken);
        await this.DbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result<IdentityResourceDetails>> Handle(SecurityServiceQueries.GetIdentityResourceQuery query, CancellationToken cancellationToken)
    {
        var resource = await this.DbContext.ResourceDefinitions.SingleOrDefaultAsync(definition => definition.Name == query.Name && definition.Type == ResourceType.IdentityResource, cancellationToken);
        return resource is null
            ? Result.NotFound($"No identity resource named '{query.Name}' was found.")
            : Result.Success(new IdentityResourceDetails(resource.Name, resource.DisplayName, resource.Description, resource.Required, resource.Emphasize, resource.ShowInDiscoveryDocument, JsonListSerializer.Deserialize(resource.ClaimsJson)));
    }

    public async Task<Result<List<IdentityResourceDetails>>> Handle(SecurityServiceQueries.GetIdentityResourcesQuery query, CancellationToken cancellationToken)
    {
        var resources = await this.DbContext.ResourceDefinitions.Where(definition => definition.Type == ResourceType.IdentityResource).OrderBy(definition => definition.Name).ToListAsync(cancellationToken);
        return Result.Success(resources.Select(resource => new IdentityResourceDetails(resource.Name, resource.DisplayName, resource.Description, resource.Required, resource.Emphasize, resource.ShowInDiscoveryDocument, JsonListSerializer.Deserialize(resource.ClaimsJson))).ToList());
    }
}
