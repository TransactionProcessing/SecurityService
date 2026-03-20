using Duende.IdentityServer.Models;
using SimpleResults;

namespace SecurityService.BusinessLogic.RequestHandlers;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Requests;
using SecurityService.BusinessLogic.IdentityManagement;

public class ApiResourceRequestHandler : IRequestHandler<SecurityServiceCommands.CreateApiResourceCommand, Result>,
                                         IRequestHandler<SecurityServiceQueries.GetApiResourceQuery, Result<ApiResource>>,
                                         IRequestHandler<SecurityServiceQueries.GetApiResourcesQuery, Result<List<ApiResource>>>
{
    private readonly IIdentityManagementService IdentityManagementService;

    public ApiResourceRequestHandler(IIdentityManagementService identityManagementService)
    {
        this.IdentityManagementService = identityManagementService;
    }

    public Task<Result> Handle(SecurityServiceCommands.CreateApiResourceCommand command, CancellationToken cancellationToken) =>
        this.IdentityManagementService.CreateApiResource(command, cancellationToken);

    public Task<Result<ApiResource>> Handle(SecurityServiceQueries.GetApiResourceQuery query, CancellationToken cancellationToken) =>
        this.IdentityManagementService.GetApiResource(query, cancellationToken);

    public Task<Result<List<ApiResource>>> Handle(SecurityServiceQueries.GetApiResourcesQuery query, CancellationToken cancellationToken) =>
        this.IdentityManagementService.GetApiResources(query, cancellationToken);
}
