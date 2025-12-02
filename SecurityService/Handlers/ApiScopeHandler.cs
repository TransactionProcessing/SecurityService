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

    public static class ApiScopeHandler
    {
        public static async Task<IResult> CreateApiScope(IMediator mediator,
                                                         CreateApiScopeRequest createApiScopeRequest,
                                                         CancellationToken cancellationToken)
        {
            var command = new SecurityServiceCommands.CreateApiScopeCommand(
                createApiScopeRequest.Name,
                createApiScopeRequest.DisplayName,
                createApiScopeRequest.Description);

            Result result = await mediator.Send(command, cancellationToken);

            return ResponseFactory.FromResult(result);
        }

        public static async Task<IResult> GetApiScope(IMediator mediator,
                                                      IModelFactory modelFactory,
                                                      string apiScopeName,
                                                      CancellationToken cancellationToken)
        {
            var query = new SecurityServiceQueries.GetApiScopeQuery(apiScopeName);

            Result<Duende.IdentityServer.Models.ApiScope> result = await mediator.Send(query, cancellationToken);

            return ResponseFactory.FromResult(result, modelFactory.ConvertFrom);
        }

        public static async Task<IResult> GetApiScopes(IMediator mediator,
                                                       IModelFactory modelFactory,
                                                       CancellationToken cancellationToken)
        {
            var query = new SecurityServiceQueries.GetApiScopesQuery();

            Result<List<Duende.IdentityServer.Models.ApiScope>> result = await mediator.Send(query, cancellationToken);

            return ResponseFactory.FromResult(result, modelFactory.ConvertFrom);
        }
    }
}