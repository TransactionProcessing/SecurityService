using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SecurityService.Database.DbContexts;
using SecurityService.Database.Entities;
using SimpleResults;

namespace SecurityService.BusinessLogic.Mfa;

public interface IMfaPolicyService
{
    Task<bool> IsMfaRequiredAsync(ApplicationUser user, CancellationToken cancellationToken);
}

public sealed class MfaPolicyService : IMfaPolicyService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly SecurityServiceDbContext _dbContext;

    public MfaPolicyService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        SecurityServiceDbContext dbContext)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _dbContext = dbContext;
    }

    public async Task<bool> IsMfaRequiredAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        var hasUserPolicy = await _dbContext.MfaPolicies
            .AnyAsync(policy => policy.TargetType == MfaPolicyTargetType.User && policy.TargetId == user.Id, cancellationToken);
        if (hasUserPolicy)
        {
            return true;
        }

        var roleNames = await _userManager.GetRolesAsync(user);
        if (roleNames.Count == 0)
        {
            return false;
        }

        var roleIds = await _roleManager.Roles
            .Where(role => role.Name != null && roleNames.Contains(role.Name))
            .Select(role => role.Id)
            .ToListAsync(cancellationToken);

        return await _dbContext.MfaPolicies
            .AnyAsync(policy => policy.TargetType == MfaPolicyTargetType.Role && roleIds.Contains(policy.TargetId), cancellationToken);
    }

    public async Task<Result> RequireUserAsync(string userId, CancellationToken cancellationToken)
    {
        if (await _userManager.FindByIdAsync(userId) is null)
        {
            return Result.NotFound($"No user found with id '{userId}'.");
        }

        return await RequireAsync(MfaPolicyTargetType.User, userId, cancellationToken);
    }

    public Task<Result> RemoveUserAsync(string userId, CancellationToken cancellationToken) =>
        RemoveAsync(MfaPolicyTargetType.User, userId, cancellationToken);

    public async Task<Result> RequireRoleAsync(string roleId, CancellationToken cancellationToken)
    {
        if (await _roleManager.FindByIdAsync(roleId) is null)
        {
            return Result.NotFound($"No role found with id '{roleId}'.");
        }

        return await RequireAsync(MfaPolicyTargetType.Role, roleId, cancellationToken);
    }

    public Task<Result> RemoveRoleAsync(string roleId, CancellationToken cancellationToken) =>
        RemoveAsync(MfaPolicyTargetType.Role, roleId, cancellationToken);

    private async Task<Result> RequireAsync(MfaPolicyTargetType targetType, string targetId, CancellationToken cancellationToken)
    {
        var exists = await _dbContext.MfaPolicies.AnyAsync(
            policy => policy.TargetType == targetType && policy.TargetId == targetId,
            cancellationToken);
        if (exists)
        {
            return Result.Success();
        }

        _dbContext.MfaPolicies.Add(new MfaPolicy
        {
            Id = Guid.NewGuid(),
            TargetType = targetType,
            TargetId = targetId,
            CreatedUtc = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    private async Task<Result> RemoveAsync(MfaPolicyTargetType targetType, string targetId, CancellationToken cancellationToken)
    {
        var policies = await _dbContext.MfaPolicies
            .Where(policy => policy.TargetType == targetType && policy.TargetId == targetId)
            .ToListAsync(cancellationToken);
        if (policies.Count == 0)
        {
            return Result.Success();
        }

        _dbContext.MfaPolicies.RemoveRange(policies);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
