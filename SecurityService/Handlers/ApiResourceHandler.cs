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

    public static class ApiResourceHandler
    {
        public static async Task<IResult> CreateApiResource(IMediator mediator,
                                                            CreateApiResourceRequest createApiResourceRequest,
                                                            CancellationToken cancellationToken)
        {
            var command = new SecurityServiceCommands.CreateApiResourceCommand(
                createApiResourceRequest.Name,
                createApiResourceRequest.DisplayName,
                createApiResourceRequest.Description,
                createApiResourceRequest.Secret,
                createApiResourceRequest.Scopes,
                createApiResourceRequest.UserClaims);

            Result result = await mediator.Send(command, cancellationToken);

            return ResponseFactory.FromResult(result);
        }

        public static async Task<IResult> GetApiResource(IMediator mediator,
                                                         IModelFactory modelFactory,
                                                         string apiResourceName,
                                                         CancellationToken cancellationToken)
        {
            var query = new SecurityServiceQueries.GetApiResourceQuery(apiResourceName);

            Result<Duende.IdentityServer.Models.ApiResource> result = await mediator.Send(query, cancellationToken);

            return ResponseFactory.FromResult(result, modelFactory.ConvertFrom);
        }

        public static async Task<IResult> GetApiResources(IMediator mediator,
                                                          IModelFactory modelFactory,
                                                          CancellationToken cancellationToken)
        {
            var query = new SecurityServiceQueries.GetApiResourcesQuery();

            Result<List<Duende.IdentityServer.Models.ApiResource>> result = await mediator.Send(query, cancellationToken);

            return ResponseFactory.FromResult(result, modelFactory.ConvertFrom);
        }
    }
}