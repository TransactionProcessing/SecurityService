using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SecurityService.BusinessLogic.Requests;
using Shared.Results;
using Shared.Results.Web;
using SimpleResults;

namespace SecurityService.Handlers
{
    using DataTransferObjects.Requests;
    using Factories;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using SecurityService.BusinessLogic;

    public static class ClientHandler
    {
        public static async Task<IResult> CreateClient(IMediator mediator,
                                                       CreateClientRequest createClientRequest,
                                                       CancellationToken cancellationToken)
        {
            var command = new SecurityServiceCommands.CreateClientCommand(
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
                                                    IModelFactory modelFactory,
                                                    string clientId,
                                                    CancellationToken cancellationToken)
        {
            var query = new SecurityServiceQueries.GetClientQuery(clientId);

            Result<Duende.IdentityServer.Models.Client> result = await mediator.Send(query, cancellationToken);

            return ResponseFactory.FromResult(result, modelFactory.ConvertFrom);
        }

        public static async Task<IResult> GetClients(IMediator mediator,
                                                     IModelFactory modelFactory,
                                                     CancellationToken cancellationToken)
        {
            var query = new SecurityServiceQueries.GetClientsQuery();

            Result<List<Duende.IdentityServer.Models.Client>> result = await mediator.Send(query, cancellationToken);

            return ResponseFactory.FromResult(result, modelFactory.ConvertFrom);
        }
    }
}