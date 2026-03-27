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
    private readonly SecurityServiceDbContext _dbContext;
    private readonly IOpenIddictScopeManager _scopeManager;

    public ApiResourceRequestHandler(SecurityServiceDbContext dbContext, IOpenIddictScopeManager scopeManager)
    {
        this._dbContext = dbContext;
        this._scopeManager = scopeManager;
    }

    public async Task<Result> Handle(SecurityServiceCommands.CreateApiResourceCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Name))
        {
            return Result.Invalid("API resource name is required.");
        }

        if (await this._dbContext.ResourceDefinitions.AnyAsync(resource => resource.Name == command.Name && resource.Type == ResourceType.ApiResource, cancellationToken))
        {
            return Result.Conflict($"An API resource named '{command.Name}' already exists.");
        }

        foreach (var scopeName in command.Scopes.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var scope = await this._scopeManager.FindByNameAsync(scopeName, cancellationToken);
            if (scope is null)
            {
                this._scopeManager.CreateAsync(new OpenIddictScopeDescriptor
                {
                    Name = scopeName,
                    DisplayName = scopeName,
                    Description = $"Auto-created scope for API resource '{command.Name}'."
                }, cancellationToken).GetAwaiter().GetResult();
                scope = await this._scopeManager.FindByNameAsync(scopeName, cancellationToken);
            }

            var descriptor = new OpenIddictScopeDescriptor();
            await this._scopeManager.PopulateAsync(descriptor, scope, cancellationToken);
            if (descriptor.Resources.Contains(command.Name, StringComparer.OrdinalIgnoreCase) == false)
            {
                descriptor.Resources.Add(command.Name);
                await this._scopeManager.UpdateAsync(scope, descriptor, cancellationToken);
            }
        }

        var resource = new ResourceDefinition
        {
            Id = Guid.NewGuid(),
            Name = command.Name,
            DisplayName = command.DisplayName,
            Description = command.Description,
            Type = ResourceType.ApiResource,
            SecretHash = string.IsNullOrWhiteSpace(command.Secret) ? null : Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(command.Secret))),
            ClaimsJson = JsonListSerializer.Serialize(command.UserClaims),
            ScopesJson = JsonListSerializer.Serialize(command.Scopes)
        };

        this._dbContext.ResourceDefinitions.Add(resource);
        await this._dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result<ApiResourceDetails>> Handle(SecurityServiceQueries.GetApiResourceQuery query, CancellationToken cancellationToken)
    {
        var resource = await this._dbContext.ResourceDefinitions.SingleOrDefaultAsync(definition => definition.Name == query.Name && definition.Type == ResourceType.ApiResource, cancellationToken);
        return resource is null
            ? Result.NotFound($"No API resource named '{query.Name}' was found.")
            : Result.Success(new ApiResourceDetails(resource.Name, resource.DisplayName, resource.Description, JsonListSerializer.Deserialize(resource.ScopesJson), JsonListSerializer.Deserialize(resource.ClaimsJson)));
    }

    public async Task<Result<List<ApiResourceDetails>>> Handle(SecurityServiceQueries.GetApiResourcesQuery query, CancellationToken cancellationToken)
    {
        var resources = await this._dbContext.ResourceDefinitions.Where(definition => definition.Type == ResourceType.ApiResource).OrderBy(definition => definition.Name).ToListAsync(cancellationToken);
        return Result.Success(resources.Select(resource => new ApiResourceDetails(resource.Name, resource.DisplayName, resource.Description, JsonListSerializer.Deserialize(resource.ScopesJson), JsonListSerializer.Deserialize(resource.ClaimsJson))).ToList());
    }
}
