using Duende.IdentityServer.Models;
using SimpleResults;

namespace SecurityService.BusinessLogic.RequestHandlers;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Requests;
using SecurityService.BusinessLogic.IdentityManagement;

public class ApiScopeRequestHandler : IRequestHandler<SecurityServiceCommands.CreateApiScopeCommand, Result>,
                                      IRequestHandler<SecurityServiceQueries.GetApiScopeQuery, Result<ApiScope>>,
                                      IRequestHandler<SecurityServiceQueries.GetApiScopesQuery, Result<List<ApiScope>>>
{
    private readonly IIdentityManagementService IdentityManagementService;

    public ApiScopeRequestHandler(IIdentityManagementService identityManagementService)
    {
        this.IdentityManagementService = identityManagementService;
    }

    public Task<Result> Handle(SecurityServiceCommands.CreateApiScopeCommand command, CancellationToken cancellationToken) =>
        this.IdentityManagementService.CreateApiScope(command, cancellationToken);

    public Task<Result<ApiScope>> Handle(SecurityServiceQueries.GetApiScopeQuery query, CancellationToken cancellationToken) =>
        this.IdentityManagementService.GetApiScope(query, cancellationToken);

    public Task<Result<List<ApiScope>>> Handle(SecurityServiceQueries.GetApiScopesQuery query, CancellationToken cancellationToken) =>
        this.IdentityManagementService.GetApiScopes(query, cancellationToken);
}
