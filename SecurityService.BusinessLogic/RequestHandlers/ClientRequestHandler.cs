using Duende.IdentityServer.Models;
using SimpleResults;

namespace SecurityService.BusinessLogic.RequestHandlers;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Requests;
using SecurityService.BusinessLogic.IdentityManagement;

public class ClientRequestHandler : IRequestHandler<SecurityServiceCommands.CreateClientCommand, Result>,
                                    IRequestHandler<SecurityServiceQueries.GetClientQuery, Result<Client>>,
                                    IRequestHandler<SecurityServiceQueries.GetClientsQuery, Result<List<Client>>>
{
    private readonly IIdentityManagementService IdentityManagementService;

    public ClientRequestHandler(IIdentityManagementService identityManagementService)
    {
        this.IdentityManagementService = identityManagementService;
    }

    public Task<Result> Handle(SecurityServiceCommands.CreateClientCommand command, CancellationToken cancellationToken) =>
        this.IdentityManagementService.CreateClient(command, cancellationToken);

    public Task<Result<Client>> Handle(SecurityServiceQueries.GetClientQuery query, CancellationToken cancellationToken) =>
        this.IdentityManagementService.GetClient(query, cancellationToken);

    public Task<Result<List<Client>>> Handle(SecurityServiceQueries.GetClientsQuery query, CancellationToken cancellationToken) =>
        this.IdentityManagementService.GetClients(query, cancellationToken);
}
