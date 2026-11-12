using MediatR;
using SecurityService.BusinessLogic.Requests;
using SecurityService.DataTransferObjects;
using SecurityService.Models;
using TransactionProcessing.UnionResults.AspNetCore;
using TransactionProcessing.UnionResults;

namespace SecurityService.Handlers;

public static class ApiResourceHandler
{
    public static async Task<Result> CreateApiResource(IMediator mediator, CreateApiResourceRequest request, CancellationToken cancellationToken) {
        SecurityServiceCommands.CreateApiResourceCommand command = new(request.Name, request.DisplayName, request.Description, request.Secret, request.Scopes, request.UserClaims);

        Result result = await mediator.Send(command, cancellationToken);

        return result;
    }

    public static async Task<Result<ApiResourceResponse>> GetApiResource(IMediator mediator,
                                                     [System.ComponentModel.Description("Exact configured API resource name, for example orders-api.")] string name,
                                                     CancellationToken cancellationToken) {

        SecurityServiceQueries.GetApiResourceQuery query = new(name);

        Result<ApiResourceDetails> result = await mediator.Send(query, cancellationToken);

        return result.Map(ModelFactory.ConvertFrom);
    }

    public static async Task<Result<List<ApiResourceResponse>>> GetApiResources(IMediator mediator,
                                                      CancellationToken cancellationToken)
    {

        SecurityServiceQueries.GetApiResourcesQuery query = new();

        Result<List<ApiResourceDetails>> result = await mediator.Send(query, cancellationToken);

        return result.Map(ModelFactory.ConvertFrom);
    }
}
