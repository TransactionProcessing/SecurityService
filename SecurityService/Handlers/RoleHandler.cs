using MediatR;
using SecurityService.BusinessLogic.Requests;
using SecurityService.DataTransferObjects;
using SecurityService.Models;
using TransactionProcessing.UnionResults.AspNetCore;
using TransactionProcessing.UnionResults;

namespace SecurityService.Handlers;

public static class RoleHandler
{
    public static async Task<Result> RequireMfa(IMediator mediator,
                                                 [System.ComponentModel.Description("Stable identity ID of the role to update.")] string roleId,
                                                 CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new SecurityServiceCommands.RequireRoleMfaCommand(roleId), cancellationToken);
        return result;
    }

    public static async Task<Result> RemoveMfa(IMediator mediator,
                                                [System.ComponentModel.Description("Stable identity ID of the role to update.")] string roleId,
                                                CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new SecurityServiceCommands.RemoveRoleMfaCommand(roleId), cancellationToken);
        return result;
    }

    public static async Task<Result> CreateRole(IMediator mediator, CreateRoleRequest request, CancellationToken cancellationToken) {
        SecurityServiceCommands.CreateRoleCommand command = new(request.RoleName);

        Result result = await mediator.Send(command, cancellationToken);
        
        return result;
    }

    public static async Task<Result<RoleResponse>> GetRole(IMediator mediator,
                                              [System.ComponentModel.Description("Stable identity ID of the role to retrieve.")] string roleId,
                                              CancellationToken cancellationToken) {
        SecurityServiceQueries.GetRoleQuery query = new(roleId);

        Result<RoleDetails> result = await mediator.Send(query, cancellationToken);

        return result.Map(ModelFactory.ConvertFrom);
    }
    public static async Task<Result<List<RoleResponse>>> GetRoles(IMediator mediator, CancellationToken cancellationToken)
    {
        SecurityServiceQueries.GetRolesQuery query = new();

        Result<List<RoleDetails>> result = await mediator.Send(query, cancellationToken);

        return result.Map(ModelFactory.ConvertFrom);
    }
}
