using MediatR;
using SecurityService.BusinessLogic.Requests;
using SecurityService.DataTransferObjects;
using SecurityService.Factories;
using SecurityService.Models;
using Shared.Results.Web;
using SimpleResults;

namespace SecurityService.Handlers;

public static class ClientHandler
{
    public static async Task<IResult> CreateClient(IMediator mediator, CreateClientRequest request, CancellationToken cancellationToken)
    {
        SecurityServiceCommands.CreateClientCommand command = new(request.ClientId,
            request.Secret,
            request.ClientName,
            request.ClientDescription,
            request.AllowedScopes,
            request.AllowedGrantTypes,
            request.ClientUri,
            request.ClientRedirectUris,
            request.ClientPostLogoutRedirectUris,
            request.RequireConsent,
            request.AllowOfflineAccess);

        Result result = await mediator.Send(command, cancellationToken);

        return ResponseFactory.FromResult(result);
    }

    public static async Task<IResult> GetClient(IMediator mediator,
                                                string clientId,
                                                CancellationToken cancellationToken) {
        SecurityServiceQueries.GetClientQuery query = new(clientId);

        Result<ClientDetails> result = await mediator.Send(query, cancellationToken);
        
        return ResponseFactory.FromResult(result, ModelFactory.ConvertFrom);
    }

    public static async Task<IResult> GetClients(IMediator mediator,
                                                 CancellationToken cancellationToken) {
        SecurityServiceQueries.GetClientsQuery query = new();

        Result<List<ClientDetails>> result = await mediator.Send(query, cancellationToken);

        return ResponseFactory.FromResult(result, ModelFactory.ConvertFrom);
    }
}
