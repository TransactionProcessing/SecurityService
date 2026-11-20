using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SecurityService.BusinessLogic.Requests;
using SecurityService.Models;
using TransactionProcessing.UnionResults;

namespace SecurityService.BusinessLogic.RequestHandlers;

public sealed class RoleRequestHandler :
    IRequestHandler<SecurityServiceCommands.CreateRoleCommand, Result>,
    IRequestHandler<SecurityServiceQueries.GetRoleQuery, Result<RoleDetails>>,
    IRequestHandler<SecurityServiceQueries.GetRolesQuery, Result<List<RoleDetails>>>
{
    private readonly RoleManager<IdentityRole> RoleManager;

    public RoleRequestHandler(RoleManager<IdentityRole> roleManager)
    {
        this.RoleManager = roleManager;
    }

    public async Task<Result> Handle(SecurityServiceCommands.CreateRoleCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Name))
        {
            return Results.Validation("Role name is required.");
        }

        if (await this.RoleManager.RoleExistsAsync(command.Name))
        {
            return Results.Conflict($"A role named '{command.Name}' already exists.");
        }

        var role = new IdentityRole(command.Name);
        var result = await this.RoleManager.CreateAsync(role);
        if (result.Succeeded == false)
        {
            return Results.Validation(string.Join("; ", result.Errors.Select(error => error.Description)));
        }

        return Results.Ok();
    }

    public async Task<Result<RoleDetails>> Handle(SecurityServiceQueries.GetRoleQuery query, CancellationToken cancellationToken)
    {
        var role = await this.RoleManager.Roles.SingleOrDefaultAsync(item => item.Id == query.RoleId, cancellationToken);
        return role is null
            ? new Error(ErrorType.NotFound, $"No role found with id '{query.RoleId}'.")
            : Results.Ok(new RoleDetails(role.Id, role.Name!));
    }

    public async Task<Result<List<RoleDetails>>> Handle(SecurityServiceQueries.GetRolesQuery query, CancellationToken cancellationToken)
    {
        var roles = await this.RoleManager.Roles.OrderBy(role => role.Name).Select(role => new RoleDetails(role.Id, role.Name!)).ToArrayAsync(cancellationToken);
        return Results.Ok(roles.ToList());
    }
}
