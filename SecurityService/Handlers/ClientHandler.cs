using MediatR;
using SecurityService.BusinessLogic.Requests;
using SecurityService.DataTransferObjects;
using SecurityService.Factories;
using Shared.Results.Web;

namespace SecurityService.Handlers;

public static class ClientHandler
{
    public static async Task<IResult> CreateClient(IMediator mediator, CreateClientRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new SecurityServiceCommands.CreateClientCommand(
            request.ClientId,
            request.Secret,
            request.ClientName,
            request.ClientDescription,
            request.AllowedScopes,
            request.AllowedGrantTypes,
            request.ClientUri,
            request.ClientRedirectUris,
            request.ClientPostLogoutRedirectUris,
            request.RequireConsent,
            request.AllowOfflineAccess), cancellationToken);

        return ResponseFactory.FromResult(result);
    }

    public static async Task<IResult> GetClient(IMediator mediator, string clientId, CancellationToken cancellationToken)
        => ResponseFactory.FromResult(await mediator.Send(new SecurityServiceQueries.GetClientQuery(clientId), cancellationToken), ModelFactory.ConvertFrom);
    public static async Task<IResult> GetClients(IMediator mediator, CancellationToken cancellationToken)
        => ResponseFactory.FromResult(await mediator.Send(new SecurityServiceQueries.GetClientsQuery(), cancellationToken), ModelFactory.ConvertFrom);
}
