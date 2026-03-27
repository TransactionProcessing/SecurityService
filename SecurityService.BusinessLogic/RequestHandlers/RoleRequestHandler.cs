using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SecurityService.BusinessLogic.Requests;
using SecurityService.Models;
using SimpleResults;

namespace SecurityService.BusinessLogic.RequestHandlers;

public sealed class RoleRequestHandler :
    IRequestHandler<SecurityServiceCommands.CreateRoleCommand, Result>,
    IRequestHandler<SecurityServiceQueries.GetRoleQuery, Result<RoleDetails>>,
    IRequestHandler<SecurityServiceQueries.GetRolesQuery, Result<List<RoleDetails>>>
{
    private readonly RoleManager<IdentityRole> _roleManager;

    public RoleRequestHandler(RoleManager<IdentityRole> roleManager)
    {
        this._roleManager = roleManager;
    }

    public async Task<Result> Handle(SecurityServiceCommands.CreateRoleCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Name))
        {
            return Result.Invalid("Role name is required.");
        }

        if (await this._roleManager.RoleExistsAsync(command.Name))
        {
            return Result.Conflict($"A role named '{command.Name}' already exists.");
        }

        var role = new IdentityRole(command.Name);
        var result = await this._roleManager.CreateAsync(role);
        if (result.Succeeded == false)
        {
            return Result.Invalid(string.Join("; ", result.Errors.Select(error => error.Description)));
        }

        return Result.Success();
    }

    public async Task<Result<RoleDetails>> Handle(SecurityServiceQueries.GetRoleQuery query, CancellationToken cancellationToken)
    {
        var role = await this._roleManager.Roles.SingleOrDefaultAsync(item => item.Id == query.RoleId, cancellationToken);
        return role is null
            ? Result.NotFound($"No role found with id '{query.RoleId}'.")
            : Result.Success(new RoleDetails(role.Id, role.Name!));
    }

    public async Task<Result<List<RoleDetails>>> Handle(SecurityServiceQueries.GetRolesQuery query, CancellationToken cancellationToken)
    {
        var roles = await this._roleManager.Roles.OrderBy(role => role.Name).Select(role => new RoleDetails(role.Id, role.Name!)).ToArrayAsync(cancellationToken);
        return Result.Success(roles.ToList());
    }
}
