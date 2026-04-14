using MediatR;
using SecurityService.BusinessLogic.Requests;
using SecurityService.DataTransferObjects;
using SecurityService.Factories;
using SecurityService.Models;
using Shared.Results.Web;
using SimpleResults;

namespace SecurityService.Handlers;

public static class RoleHandler
{
    public static async Task<IResult> CreateRole(IMediator mediator, CreateRoleRequest request, CancellationToken cancellationToken) {
        SecurityServiceCommands.CreateRoleCommand command = new(request.Name);

        Result result = await mediator.Send(command, cancellationToken);
        
        return ResponseFactory.FromResult(result);
    }

    public static async Task<IResult> GetRole(IMediator mediator,
                                              string roleId,
                                              CancellationToken cancellationToken) {
        SecurityServiceQueries.GetRoleQuery query = new(roleId);

        Result<RoleDetails> result = await mediator.Send(query, cancellationToken);

        return ResponseFactory.FromResult(result, ModelFactory.ConvertFrom);
    }
    public static async Task<IResult> GetRoles(IMediator mediator, CancellationToken cancellationToken)
    {
        SecurityServiceQueries.GetRolesQuery query = new();

        Result<List<RoleDetails>> result = await mediator.Send(query, cancellationToken);

        return ResponseFactory.FromResult(result, ModelFactory.ConvertFrom);
    }
}
