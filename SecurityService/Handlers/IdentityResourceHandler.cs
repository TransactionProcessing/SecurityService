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
            var command = new SecurityServiceCommands.CreateIdentityResourceCommand(
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
                                                         IModelFactory modelFactory,
                                                         string apiResourceName,
                                                         CancellationToken cancellationToken)
        {
            var query = new SecurityServiceQueries.GetIdentityResourceQuery(apiResourceName);

            Result<Duende.IdentityServer.Models.IdentityResource> result = await mediator.Send(query, cancellationToken);

            return ResponseFactory.FromResult(result, modelFactory.ConvertFrom);
        }

        public static async Task<IResult> GetIdentityResources(IMediator mediator,
                                                               IModelFactory modelFactory,
                                                               CancellationToken cancellationToken)
        {
            var query = new SecurityServiceQueries.GetIdentityResourcesQuery();

            Result<List<Duende.IdentityServer.Models.IdentityResource>> result = await mediator.Send(query, cancellationToken);

            return ResponseFactory.FromResult(result, modelFactory.ConvertFrom);
        }
    }
}