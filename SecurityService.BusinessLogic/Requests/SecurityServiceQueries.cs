using MediatR;
using SecurityService.Models;
using SimpleResults;

namespace SecurityService.BusinessLogic.Requests;

public static class SecurityServiceQueries
{
    public sealed record GetClientQuery(string ClientId) : IRequest<Result<ClientDetails>>;
    public sealed record GetClientsQuery() : IRequest<Result<List<ClientDetails>>>;

    public sealed record GetApiScopeQuery(string Name) : IRequest<Result<ApiScopeDetails>>;
    public sealed record GetApiScopesQuery() : IRequest<Result<List<ApiScopeDetails>>>;

    public sealed record GetApiResourceQuery(string Name) : IRequest<Result<ApiResourceDetails>>;
    public sealed record GetApiResourcesQuery() : IRequest<Result<List<ApiResourceDetails>>>;

    public sealed record GetIdentityResourceQuery(string Name) : IRequest<Result<IdentityResourceDetails>>;
    public sealed record GetIdentityResourcesQuery() : IRequest<Result<List<IdentityResourceDetails>>>;

    public sealed record GetRoleQuery(string RoleId) : IRequest<Result<RoleDetails>>;
    public sealed record GetRolesQuery() : IRequest<Result<List<RoleDetails>>>;
    public sealed record GetUserQuery(string UserId) : IRequest<Result<UserDetails>>;
    public sealed record GetUsersQuery(string? UserName) : IRequest<Result<List<UserDetails>>>;
    public sealed record GetExternalProvidersQuery() : IRequest<Result<List<ExternalProviderDetails>>>;
    public sealed record GetUserGrantsQuery(string UserId) : IRequest<Result<List<GrantDetails>>>;
}
