using MediatR;
using SecurityService.BusinessLogic.Requests;
using SecurityService.DataTransferObjects;
using SecurityService.Factories;
using SecurityService.Models;
using Shared.Results.Web;
using SimpleResults;

namespace SecurityService.Handlers;

public static class ApiResourceHandler
{
    public static async Task<IResult> CreateApiResource(IMediator mediator, CreateApiResourceRequest request, CancellationToken cancellationToken) {
        SecurityServiceCommands.CreateApiResourceCommand command = new(request.Name, request.DisplayName, request.Description, request.Secret, request.Scopes, request.UserClaims);

        Result result = await mediator.Send(command, cancellationToken);

        return ResponseFactory.FromResult(result);
    }

    public static async Task<IResult> GetApiResource(IMediator mediator,
                                                     string name,
                                                     CancellationToken cancellationToken) {

        SecurityServiceQueries.GetApiResourceQuery query = new(name);

        Result<ApiResourceDetails> result = await mediator.Send(query, cancellationToken);

        return ResponseFactory.FromResult(result, ModelFactory.ConvertFrom);
    }

    public static async Task<IResult> GetApiResources(IMediator mediator,
                                                      CancellationToken cancellationToken)
    {

        SecurityServiceQueries.GetApiResourcesQuery query = new();

        Result<List<ApiResourceDetails>> result = await mediator.Send(query, cancellationToken);

        return ResponseFactory.FromResult(result, ModelFactory.ConvertFrom);
    }
}
