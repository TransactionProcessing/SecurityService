using MediatR;
using SecurityService.BusinessLogic.Requests;
using SecurityService.DataTransferObjects;
using SecurityService.Factories;
using SecurityService.Models;
using Shared.Results.Web;
using SimpleResults;

namespace SecurityService.Handlers;

public static class IdentityResourceHandler
{
    public static async Task<IResult> CreateIdentityResource(IMediator mediator, CreateIdentityResourceRequest request, CancellationToken cancellationToken) {
        SecurityServiceCommands.CreateIdentityResourceCommand command = new(request.Name, request.DisplayName, request.Description, request.Required, request.Emphasize, request.ShowInDiscoveryDocument, request.Claims);

        Result result = await mediator.Send(command, cancellationToken);

        return ResponseFactory.FromResult(result);
    }

    public static async Task<IResult> GetIdentityResource(IMediator mediator, string name, CancellationToken cancellationToken) {
        SecurityServiceQueries.GetIdentityResourceQuery query = new(name);

        Result<IdentityResourceDetails> result = await mediator.Send(query, cancellationToken);

        return ResponseFactory.FromResult(result, ModelFactory.ConvertFrom);
    }

    public static async Task<IResult> GetIdentityResources(IMediator mediator, CancellationToken cancellationToken) {
        SecurityServiceQueries.GetIdentityResourcesQuery query = new();

        Result<List<IdentityResourceDetails>> result = await mediator.Send(query, cancellationToken);

        return ResponseFactory.FromResult(result, ModelFactory.ConvertFrom);
    }
}