using MediatR;
using SecurityService.BusinessLogic.Requests;
using SecurityService.DataTransferObjects;
using SecurityService.Factories;
using SecurityService.Models;
using TransactionProcessing.UnionResults.AspNetCore;
using TransactionProcessing.UnionResults;

namespace SecurityService.Handlers;

public static class IdentityResourceHandler
{
    public static async Task<Result> CreateIdentityResource(IMediator mediator, CreateIdentityResourceRequest request, CancellationToken cancellationToken) {
        SecurityServiceCommands.CreateIdentityResourceCommand command = new(request.Name, request.DisplayName, request.Description, request.Required, request.Emphasize, request.ShowInDiscoveryDocument, request.Claims);

        Result result = await mediator.Send(command, cancellationToken);

        return result;
    }

    public static async Task<Result<IdentityResourceResponse>> GetIdentityResource(IMediator mediator, [System.ComponentModel.Description("Exact configured identity resource name, for example profile or email.")] string name, CancellationToken cancellationToken) {
        SecurityServiceQueries.GetIdentityResourceQuery query = new(name);

        Result<IdentityResourceDetails> result = await mediator.Send(query, cancellationToken);

        return result.Map(ModelFactory.ConvertFrom);
    }

    public static async Task<Result<List<IdentityResourceResponse>>> GetIdentityResources(IMediator mediator, CancellationToken cancellationToken) {
        SecurityServiceQueries.GetIdentityResourcesQuery query = new();

        Result<List<IdentityResourceDetails>> result = await mediator.Send(query, cancellationToken);

        return result.Map(ModelFactory.ConvertFrom);
    }
}
