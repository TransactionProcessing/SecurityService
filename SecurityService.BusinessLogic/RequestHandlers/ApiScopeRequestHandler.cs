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
    private readonly SecurityServiceDbContext _dbContext;
    private readonly IOpenIddictScopeManager _scopeManager;

    public ApiScopeRequestHandler(SecurityServiceDbContext dbContext, IOpenIddictScopeManager scopeManager)
    {
        this._dbContext = dbContext;
        this._scopeManager = scopeManager;
    }

    public async Task<Result> Handle(SecurityServiceCommands.CreateApiScopeCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Name))
        {
            return Result.Invalid("Scope name is required.");
        }

        if (await this._dbContext.ResourceDefinitions.AnyAsync(resource => resource.Name == command.Name && resource.Type == ResourceType.ApiScope, cancellationToken))
        {
            return Result.Conflict($"An API scope named '{command.Name}' already exists.");
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
            Type = ResourceType.ApiScope
        };

        this._dbContext.ResourceDefinitions.Add(resource);
        await this._dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result<ApiScopeDetails>> Handle(SecurityServiceQueries.GetApiScopeQuery query, CancellationToken cancellationToken)
    {
        var resource = await this._dbContext.ResourceDefinitions.SingleOrDefaultAsync(definition => definition.Name == query.Name && definition.Type == ResourceType.ApiScope, cancellationToken);
        return resource is null
            ? Result.NotFound($"No API scope named '{query.Name}' was found.")
            : Result.Success(new ApiScopeDetails(resource.Name, resource.DisplayName, resource.Description));
    }

    public async Task<Result<List<ApiScopeDetails>>> Handle(SecurityServiceQueries.GetApiScopesQuery query, CancellationToken cancellationToken)
    {
        var scopes = await this._dbContext.ResourceDefinitions.Where(definition => definition.Type == ResourceType.ApiScope).OrderBy(definition => definition.Name).Select(definition => new ApiScopeDetails(definition.Name, definition.DisplayName, definition.Description)).ToArrayAsync(cancellationToken);
        return Result.Success(scopes.ToList());
    }
}
