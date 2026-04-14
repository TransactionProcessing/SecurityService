using MediatR;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using SecurityService.BusinessLogic.Requests;
using SecurityService.Database.DbContexts;
using SecurityService.Database.Entities;
using SecurityService.Models;
using SimpleResults;
using ResourceType = SecurityService.Database.Entities.ResourceType;

namespace SecurityService.BusinessLogic.RequestHandlers;

public sealed class ApiScopeRequestHandler :
    IRequestHandler<SecurityServiceCommands.CreateApiScopeCommand, Result>,
    IRequestHandler<SecurityServiceQueries.GetApiScopeQuery, Result<ApiScopeDetails>>,
    IRequestHandler<SecurityServiceQueries.GetApiScopesQuery, Result<List<ApiScopeDetails>>>
{
    private readonly SecurityServiceDbContext DbContext;
    private readonly IOpenIddictScopeManager ScopeManager;

    public ApiScopeRequestHandler(SecurityServiceDbContext dbContext, IOpenIddictScopeManager scopeManager)
    {
        this.DbContext = dbContext;
        this.ScopeManager = scopeManager;
    }

    public async Task<Result> Handle(SecurityServiceCommands.CreateApiScopeCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Name))
        {
            return Result.Invalid("Scope name is required.");
        }

        if (await this.DbContext.ResourceDefinitions.AnyAsync(resource => resource.Name == command.Name && resource.Type == ResourceType.ApiScope, cancellationToken))
        {
            return Result.Conflict($"An API scope named '{command.Name}' already exists.");
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
            Type = ResourceType.ApiScope
        };

        await this.DbContext.ResourceDefinitions.AddAsync(resource, cancellationToken);
        await this.DbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result<ApiScopeDetails>> Handle(SecurityServiceQueries.GetApiScopeQuery query, CancellationToken cancellationToken)
    {
        var resource = await this.DbContext.ResourceDefinitions.SingleOrDefaultAsync(definition => definition.Name == query.Name && definition.Type == ResourceType.ApiScope, cancellationToken);
        return resource is null
            ? Result.NotFound($"No API scope named '{query.Name}' was found.")
            : Result.Success(Factory.ConvertFrom(resource.Name, resource.DisplayName, resource.Description));
    }

    public async Task<Result<List<ApiScopeDetails>>> Handle(SecurityServiceQueries.GetApiScopesQuery query, CancellationToken cancellationToken)
    {
        var scopes = await this.DbContext.ResourceDefinitions.Where(definition => definition.Type == ResourceType.ApiScope).OrderBy(definition => definition.Name).ToListAsync(cancellationToken);
        return Result.Success(Factory.ConvertFrom(scopes.Select(definition => (definition.Name, definition.DisplayName, definition.Description)).ToList()));
    }
}
