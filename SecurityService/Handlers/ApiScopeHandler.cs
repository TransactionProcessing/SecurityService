using MediatR;
using SecurityService.BusinessLogic.Requests;
using SecurityService.DataTransferObjects;
using SecurityService.Factories;
using SecurityService.Models;
using Shared.Results.Web;
using SimpleResults;

namespace SecurityService.Handlers;

public static class ApiScopeHandler
{
    public static async Task<IResult> CreateApiScope(IMediator mediator, CreateApiScopeRequest request, CancellationToken cancellationToken) {
        SecurityServiceCommands.CreateApiScopeCommand command = new(request.Name, request.DisplayName, request.Description);

        Result result = await mediator.Send(command, cancellationToken);

        return ResponseFactory.FromResult(result);
    }

    public static async Task<IResult> GetApiScope(IMediator mediator,
                                                  [System.ComponentModel.Description("Exact configured API scope name, for example orders.read.")] string name,
                                                  CancellationToken cancellationToken) {
        SecurityServiceQueries.GetApiScopeQuery query = new(name);

        Result<ApiScopeDetails> result = await mediator.Send(query, cancellationToken);

        return ResponseFactory.FromResult(result, ModelFactory.ConvertFrom);
    }

    public static async Task<IResult> GetApiScopes(IMediator mediator,
                                                   CancellationToken cancellationToken) {
        SecurityServiceQueries.GetApiScopesQuery query = new();
        Result<List<ApiScopeDetails>> result = await mediator.Send(query, cancellationToken);

        return ResponseFactory.FromResult(result, ModelFactory.ConvertFrom);
    }
}
