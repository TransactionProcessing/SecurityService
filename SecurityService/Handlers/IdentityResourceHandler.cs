using MediatR;
using SecurityService.BusinessLogic.Requests;
using SecurityService.DataTransferObjects;
using SecurityService.Factories;
using Shared.Results.Web;

namespace SecurityService.Handlers;

public static class IdentityResourceHandler
{
    public static async Task<IResult> CreateIdentityResource(IMediator mediator, CreateIdentityResourceRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new SecurityServiceCommands.CreateIdentityResourceCommand(
            request.Name,
            request.DisplayName,
            request.Description,
            request.Required,
            request.Emphasize,
            request.ShowInDiscoveryDocument,
            request.Claims), cancellationToken);

        return ResponseFactory.FromResult(result);
    }

    public static async Task<IResult> GetIdentityResource(IMediator mediator, string name, CancellationToken cancellationToken)
        => ResponseFactory.FromResult(await mediator.Send(new SecurityServiceQueries.GetIdentityResourceQuery(name), cancellationToken), ModelFactory.ConvertFrom);

    public static async Task<IResult> GetIdentityResources(IMediator mediator, CancellationToken cancellationToken)
        => ResponseFactory.FromResult(await mediator.Send(new SecurityServiceQueries.GetIdentityResourcesQuery(), cancellationToken), ModelFactory.ConvertFrom);
}
