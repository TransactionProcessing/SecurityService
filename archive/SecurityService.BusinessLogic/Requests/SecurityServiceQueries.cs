using System;
using System.Collections.Generic;
using Duende.IdentityServer.Models;
using MediatR;
using SecurityService.Models;
using SimpleResults;

namespace SecurityService.BusinessLogic.Requests;

public record SecurityServiceQueries {
    public record GetApiResourceQuery(string Name) : IRequest<Result<ApiResource>>;
    public record GetApiResourcesQuery() : IRequest<Result<List<ApiResource>>>;

    public record GetApiScopeQuery(string Name) : IRequest<Result<ApiScope>>;

    public record GetApiScopesQuery() : IRequest<Result<List<ApiScope>>>;

    public record GetClientQuery(String ClientId) :IRequest<Result<Client>>;
    public record GetClientsQuery() : IRequest<Result<List<Client>>>;

    public record GetRoleQuery(Guid RoleId) : IRequest<Result<RoleDetails>>;
    public record GetRolesQuery() : IRequest<Result<List<RoleDetails>>>;

    public record GetIdentityResourceQuery(String IdentityResourceName) : IRequest<Result<IdentityResource>>;
    public record GetIdentityResourcesQuery() : IRequest<Result<List<IdentityResource>>>;

    public record GetUserQuery(Guid UserId) : IRequest<Result<UserDetails>>;
    public record GetUsersQuery(String UserName) : IRequest<Result<List<UserDetails>>>;
}