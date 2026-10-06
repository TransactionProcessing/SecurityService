using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SecurityService.Database.DbContexts;
using SecurityService.Database.Entities;

namespace SecurityService.BusinessLogic.Mfa;

public sealed record MfaEnrollment(string AuthenticatorKey, string OtpauthUri);

public sealed class MfaAccountService
{
    private const string AuthenticatorProvider = "Authenticator";
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IPasswordHasher<ApplicationUser> _passwordHasher;
    private readonly SecurityServiceDbContext _dbContext;
    private readonly RecoveryCodeGenerator _recoveryCodeGenerator;

    public MfaAccountService(
        UserManager<ApplicationUser> userManager,
        IPasswordHasher<ApplicationUser> passwordHasher,
        SecurityServiceDbContext dbContext,
        RecoveryCodeGenerator recoveryCodeGenerator)
    {
        _userManager = userManager;
        _passwordHasher = passwordHasher;
        _dbContext = dbContext;
        _recoveryCodeGenerator = recoveryCodeGenerator;
    }

    public async Task<MfaEnrollment> BeginEnrollmentAsync(ApplicationUser user, string issuer, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var resetResult = await _userManager.ResetAuthenticatorKeyAsync(user);
        if (!resetResult.Succeeded)
        {
            throw new InvalidOperationException(string.Join("; ", resetResult.Errors.Select(error => error.Description)));
        }

        var key = await _userManager.GetAuthenticatorKeyAsync(user);
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new InvalidOperationException("The authenticator key could not be generated.");
        }

        var account = user.Email ?? user.UserName ?? user.Id;
        var uri = $"otpauth://totp/{Uri.EscapeDataString(issuer)}:{Uri.EscapeDataString(account)}?secret={key}&issuer={Uri.EscapeDataString(issuer)}";
        return new MfaEnrollment(key, uri);
    }

    public async Task<bool> ConfirmEnrollmentAsync(ApplicationUser user, string code, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (string.IsNullOrWhiteSpace(code))
        {
            return false;
        }

        var valid = await _userManager.VerifyTwoFactorTokenAsync(user, AuthenticatorProvider, code);
        if (valid == false)
        {
            return false;
        }

        return (await _userManager.SetTwoFactorEnabledAsync(user, true)).Succeeded;
    }

    public async Task<IReadOnlyList<string>> GenerateRecoveryCodesAsync(ApplicationUser user, int count, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var existingCodes = await _dbContext.RecoveryCodes.Where(code => code.UserId == user.Id).ToListAsync(cancellationToken);
        _dbContext.RecoveryCodes.RemoveRange(existingCodes);

        var codes = _recoveryCodeGenerator.Generate(count);
        foreach (var code in codes)
        {
            _dbContext.RecoveryCodes.Add(new RecoveryCode
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                CodeHash = _passwordHasher.HashPassword(user, code),
                CreatedUtc = DateTime.UtcNow
            });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return codes;
    }

    public async Task<bool> RedeemRecoveryCodeAsync(ApplicationUser user, string code, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return false;
        }

        var candidates = await _dbContext.RecoveryCodes
            .Where(recoveryCode => recoveryCode.UserId == user.Id && recoveryCode.ConsumedUtc == null)
            .ToListAsync(cancellationToken);
        foreach (var candidate in candidates)
        {
            var verification = _passwordHasher.VerifyHashedPassword(user, candidate.CodeHash, code);
            if (verification == PasswordVerificationResult.Failed)
            {
                continue;
            }

            candidate.ConsumedUtc = DateTime.UtcNow;
            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                return false;
            }
        }

        return false;
    }

    public async Task<bool> RemoveAuthenticatorAsync(ApplicationUser user, string currentPassword, string? code, CancellationToken cancellationToken)
    {
        var reauthenticated = await _userManager.CheckPasswordAsync(user, currentPassword);
        if (reauthenticated == false && string.IsNullOrWhiteSpace(code) == false)
        {
            reauthenticated = await _userManager.VerifyTwoFactorTokenAsync(user, AuthenticatorProvider, code);
        }

        if (reauthenticated == false)
        {
            return false;
        }

        var disableResult = await _userManager.SetTwoFactorEnabledAsync(user, false);
        if (disableResult.Succeeded == false)
        {
            return false;
        }

        await _userManager.ResetAuthenticatorKeyAsync(user);
        var recoveryCodes = await _dbContext.RecoveryCodes.Where(recoveryCode => recoveryCode.UserId == user.Id).ToListAsync(cancellationToken);
        _dbContext.RecoveryCodes.RemoveRange(recoveryCodes);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task RevokeRecoveryCodesAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        var recoveryCodes = await _dbContext.RecoveryCodes
            .Where(recoveryCode => recoveryCode.UserId == user.Id)
            .ToListAsync(cancellationToken);
        _dbContext.RecoveryCodes.RemoveRange(recoveryCodes);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
