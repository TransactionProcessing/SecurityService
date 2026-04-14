using MediatR;
using SecurityService.Models;
using SimpleResults;

namespace SecurityService.BusinessLogic.Requests;

public static class SecurityServiceQueries
{
    public record GetClientQuery(string ClientId) : IRequest<Result<ClientDetails>>;
    public record GetClientsQuery() : IRequest<Result<List<ClientDetails>>>;

    public record GetApiScopeQuery(string Name) : IRequest<Result<ApiScopeDetails>>;
    public record GetApiScopesQuery() : IRequest<Result<List<ApiScopeDetails>>>;

    public record GetApiResourceQuery(string Name) : IRequest<Result<ApiResourceDetails>>;
    public record GetApiResourcesQuery() : IRequest<Result<List<ApiResourceDetails>>>;

    public record GetIdentityResourceQuery(string Name) : IRequest<Result<IdentityResourceDetails>>;
    public record GetIdentityResourcesQuery() : IRequest<Result<List<IdentityResourceDetails>>>;

    public record GetRoleQuery(string RoleId) : IRequest<Result<RoleDetails>>;
    public record GetRolesQuery() : IRequest<Result<List<RoleDetails>>>;
    public record GetUserQuery(string UserId) : IRequest<Result<UserDetails>>;
    public record GetUsersQuery(string? UserName) : IRequest<Result<List<UserDetails>>>;
    public record GetExternalProvidersQuery() : IRequest<Result<List<ExternalProviderDetails>>>;
    public record GetUserGrantsQuery(string UserId) : IRequest<Result<List<GrantDetails>>>;
}
