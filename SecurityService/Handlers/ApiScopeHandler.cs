using MediatR;
using SecurityService.BusinessLogic.Requests;
using SecurityService.DataTransferObjects;
using SecurityService.Factories;
using Shared.Results.Web;

namespace SecurityService.Handlers;

public static class ApiScopeHandler
{
    public static async Task<IResult> CreateApiScope(IMediator mediator, CreateApiScopeRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new SecurityServiceCommands.CreateApiScopeCommand(request.Name, request.DisplayName, request.Description), cancellationToken);
        return ResponseFactory.FromResult(result);
    }

    public static async Task<IResult> GetApiScope(IMediator mediator,
                                                  string name,
                                                  CancellationToken cancellationToken) {
        var query = new SecurityServiceQueries.GetApiScopeQuery(name);
        var result = await mediator.Send(query, cancellationToken);
        return ResponseFactory.FromResult(result, ModelFactory.ConvertFrom);
    }

    public static async Task<IResult> GetApiScopes(IMediator mediator,
                                                   CancellationToken cancellationToken) {
        var query = new SecurityServiceQueries.GetApiScopesQuery();
        var result = await mediator.Send(query, cancellationToken);
        return ResponseFactory.FromResult(result, ModelFactory.ConvertFrom);
    }
}
