using MediatR;
using SecurityService.BusinessLogic.Requests;
using SecurityService.DataTransferObjects;
using SecurityService.Factories;
using Shared.Results.Web;

namespace SecurityService.Handlers;

public static class RoleHandler
{
    public static async Task<IResult> CreateRole(IMediator mediator, CreateRoleRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new SecurityServiceCommands.CreateRoleCommand(request.Name), cancellationToken);
        return ResponseFactory.FromResult(result);
    }

    public static async Task<IResult> GetRole(IMediator mediator, string roleId, CancellationToken cancellationToken)
        => ResponseFactory.FromResult(await mediator.Send(new SecurityServiceQueries.GetRoleQuery(roleId), cancellationToken), ModelFactory.ConvertFrom);

    public static async Task<IResult> GetRoles(IMediator mediator, CancellationToken cancellationToken)
        => ResponseFactory.FromResult(await mediator.Send(new SecurityServiceQueries.GetRolesQuery(), cancellationToken), ModelFactory.ConvertFrom);
}
