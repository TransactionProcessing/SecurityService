using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SecurityService.BusinessLogic.Requests;
using Shared.Results.Web;
using SimpleResults;

namespace SecurityService.Handlers
{
    using DataTransferObjects.Requests;
    using Factories;
    using Microsoft.AspNetCore.Http;

    public static class ClientHandler
    {
        public static async Task<IResult> CreateClient(IMediator mediator,
                                                       CreateClientRequest createClientRequest,
                                                       CancellationToken cancellationToken)
        {
            SecurityServiceCommands.CreateClientCommand command = new SecurityServiceCommands.CreateClientCommand(
                createClientRequest.ClientId,
                createClientRequest.Secret,
                createClientRequest.ClientName,
                createClientRequest.ClientDescription,
                createClientRequest.AllowedScopes,
                createClientRequest.AllowedGrantTypes,
                createClientRequest.ClientUri,
                createClientRequest.ClientRedirectUris,
                createClientRequest.ClientPostLogoutRedirectUris,
                createClientRequest.RequireConsent,
                createClientRequest.AllowOfflineAccess);

            Result result = await mediator.Send(command, cancellationToken);

            return ResponseFactory.FromResult(result);
        }

        public static async Task<IResult> GetClient(IMediator mediator,
                                                    string clientId,
                                                    CancellationToken cancellationToken)
        {
            SecurityServiceQueries.GetClientQuery query = new SecurityServiceQueries.GetClientQuery(clientId);

            Result<Duende.IdentityServer.Models.Client> result = await mediator.Send(query, cancellationToken);

            return ResponseFactory.FromResult(result, ModelFactory.ConvertFrom);
        }

        public static async Task<IResult> GetClients(IMediator mediator,
                                                     CancellationToken cancellationToken)
        {
            SecurityServiceQueries.GetClientsQuery query = new SecurityServiceQueries.GetClientsQuery();

            Result<List<Duende.IdentityServer.Models.Client>> result = await mediator.Send(query, cancellationToken);

            return ResponseFactory.FromResult(result, ModelFactory.ConvertFrom);
        }
    }
}