using System.Collections.Generic;
using MediatR;
using SecurityService.BusinessLogic.Requests;
using Shared.Results;
using Shared.Results.Web;
using SimpleResults;
using System.Threading;
using System.Threading.Tasks;

namespace SecurityService.Handlers
{
    using DataTransferObjects.Requests;
    using Factories;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using SecurityService.BusinessLogic;

    public static class IdentityResourceHandler
    {
        public static async Task<IResult> CreateIdentityResource(IMediator mediator,
                                                                 CreateIdentityResourceRequest createIdentityResourceRequest,
                                                                 CancellationToken cancellationToken)
        {
            SecurityServiceCommands.CreateIdentityResourceCommand command = new(
                createIdentityResourceRequest.Name,
                createIdentityResourceRequest.DisplayName,
                createIdentityResourceRequest.Description,
                createIdentityResourceRequest.Required,
                createIdentityResourceRequest.Emphasize,
                createIdentityResourceRequest.ShowInDiscoveryDocument,
                createIdentityResourceRequest.Claims);

            Result result = await mediator.Send(command, cancellationToken);

            return ResponseFactory.FromResult(result);
        }

        public static async Task<IResult> GetIdentityResource(IMediator mediator,
                                                         string apiResourceName,
                                                         CancellationToken cancellationToken)
        {
            SecurityServiceQueries.GetIdentityResourceQuery query = new(apiResourceName);

            Result<Duende.IdentityServer.Models.IdentityResource> result = await mediator.Send(query, cancellationToken);

            return ResponseFactory.FromResult(result, ModelFactory.ConvertFrom);
        }

        public static async Task<IResult> GetIdentityResources(IMediator mediator,
                                                               CancellationToken cancellationToken)
        {
            SecurityServiceQueries.GetIdentityResourcesQuery query = new();

            Result<List<Duende.IdentityServer.Models.IdentityResource>> result = await mediator.Send(query, cancellationToken);

            return ResponseFactory.FromResult(result, ModelFactory.ConvertFrom);
        }
    }
}