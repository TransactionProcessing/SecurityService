using MediatR;
using SecurityService.BusinessLogic.Requests;
using SecurityService.DataTransferObjects;
using SecurityService.Factories;
using SecurityService.Models;
using TransactionProcessing.UnionResults.AspNetCore;
using TransactionProcessing.UnionResults;

namespace SecurityService.Handlers;

public static class ClientHandler
{
    public static async Task<Result> CreateClient(IMediator mediator, CreateClientRequest request, CancellationToken cancellationToken)
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

        return result;
    }

    public static async Task<Result<ClientResponse>> GetClient(IMediator mediator,
                                                [System.ComponentModel.Description("OAuth client identifier assigned when the client registration was created.")] string clientId,
                                                CancellationToken cancellationToken) {
        SecurityServiceQueries.GetClientQuery query = new(clientId);

        Result<ClientDetails> result = await mediator.Send(query, cancellationToken);
        
        return result.Map(ModelFactory.ConvertFrom);
    }

    public static async Task<Result<List<ClientResponse>>> GetClients(IMediator mediator,
                                                 CancellationToken cancellationToken) {
        SecurityServiceQueries.GetClientsQuery query = new();

        Result<List<ClientDetails>> result = await mediator.Send(query, cancellationToken);

        return result.Map(ModelFactory.ConvertFrom);
    }
}
