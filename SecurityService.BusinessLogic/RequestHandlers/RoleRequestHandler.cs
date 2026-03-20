using SimpleResults;

namespace SecurityService.BusinessLogic.RequestHandlers;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Models;
using Requests;
using SecurityService.BusinessLogic.IdentityManagement;

public class RoleRequestHandler : IRequestHandler<SecurityServiceCommands.CreateRoleCommand, Result>,
                                  IRequestHandler<SecurityServiceQueries.GetRoleQuery, Result<RoleDetails>>,
                                  IRequestHandler<SecurityServiceQueries.GetRolesQuery, Result<List<RoleDetails>>>
{
    private readonly IIdentityManagementService IdentityManagementService;

    public RoleRequestHandler(IIdentityManagementService identityManagementService)
    {
        this.IdentityManagementService = identityManagementService;
    }

    public Task<Result> Handle(SecurityServiceCommands.CreateRoleCommand command, CancellationToken cancellationToken) =>
        this.IdentityManagementService.CreateRole(command, cancellationToken);

    public Task<Result<RoleDetails>> Handle(SecurityServiceQueries.GetRoleQuery query, CancellationToken cancellationToken) =>
        this.IdentityManagementService.GetRole(query, cancellationToken);

    public Task<Result<List<RoleDetails>>> Handle(SecurityServiceQueries.GetRolesQuery query, CancellationToken cancellationToken) =>
        this.IdentityManagementService.GetRoles(query, cancellationToken);
}
