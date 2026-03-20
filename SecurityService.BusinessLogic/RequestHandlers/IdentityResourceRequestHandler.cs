using Duende.IdentityServer.Models;
using SimpleResults;

namespace SecurityService.BusinessLogic.RequestHandlers;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Requests;
using SecurityService.BusinessLogic.IdentityManagement;

public class IdentityResourceRequestHandler : IRequestHandler<SecurityServiceCommands.CreateIdentityResourceCommand, Result>,
                                              IRequestHandler<SecurityServiceQueries.GetIdentityResourceQuery, Result<IdentityResource>>,
                                              IRequestHandler<SecurityServiceQueries.GetIdentityResourcesQuery, Result<List<IdentityResource>>>
{
    private readonly IIdentityManagementService IdentityManagementService;

    public IdentityResourceRequestHandler(IIdentityManagementService identityManagementService)
    {
        this.IdentityManagementService = identityManagementService;
    }

    public Task<Result> Handle(SecurityServiceCommands.CreateIdentityResourceCommand command, CancellationToken cancellationToken) =>
        this.IdentityManagementService.CreateIdentityResource(command, cancellationToken);

    public Task<Result<IdentityResource>> Handle(SecurityServiceQueries.GetIdentityResourceQuery query, CancellationToken cancellationToken) =>
        this.IdentityManagementService.GetIdentityResource(query, cancellationToken);

    public Task<Result<List<IdentityResource>>> Handle(SecurityServiceQueries.GetIdentityResourcesQuery query, CancellationToken cancellationToken) =>
        this.IdentityManagementService.GetIdentityResources(query, cancellationToken);
}
