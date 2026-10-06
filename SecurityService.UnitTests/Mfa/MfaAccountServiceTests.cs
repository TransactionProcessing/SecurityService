using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Buffers.Binary;
using System.Security.Cryptography;
using SecurityService.BusinessLogic.Mfa;
using SecurityService.Database.DbContexts;
using SecurityService.Database.Entities;
using SecurityService.UnitTests.Infrastructure;
using Shouldly;

namespace SecurityService.UnitTests.Mfa;

public sealed class MfaAccountServiceTests
{
    [Fact]
    public async Task Recovery_codes_are_hashed_returned_once_and_consumed_once()
    {
        using var provider = TestServiceProviderFactory.Create(Guid.NewGuid().ToString());
        using var scope = provider.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = new ApplicationUser { UserName = "mfa-user" };
        (await userManager.CreateAsync(user, "Pa55word!")).Succeeded.ShouldBeTrue();
        var service = CreateService(scope);

        var codes = await service.GenerateRecoveryCodesAsync(user, 2, CancellationToken.None);

        codes.Count.ShouldBe(2);
        var stored = await scope.ServiceProvider.GetRequiredService<SecurityServiceDbContext>().RecoveryCodes.ToListAsync();
        stored.Count.ShouldBe(2);
        stored.ShouldAllBe(code => codes.Contains(code.CodeHash) == false);
        (await service.RedeemRecoveryCodeAsync(user, codes[0], CancellationToken.None)).ShouldBeTrue();
        (await service.RedeemRecoveryCodeAsync(user, codes[0], CancellationToken.None)).ShouldBeFalse();

        var consumed = stored.Single(code => code.ConsumedUtc is not null);
        consumed.ConsumedUtc.ShouldNotBeNull();
    }

    [Fact]
    public async Task Confirm_enrollment_enables_identity_two_factor_only_after_valid_code()
    {
        using var provider = TestServiceProviderFactory.Create(Guid.NewGuid().ToString());
        using var scope = provider.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = new ApplicationUser { UserName = "mfa-user", Email = "mfa@example.com" };
        (await userManager.CreateAsync(user, "Pa55word!")).Succeeded.ShouldBeTrue();
        var service = CreateService(scope);

        var enrollment = await service.BeginEnrollmentAsync(user, "SecurityService", CancellationToken.None);
        (await userManager.GetTwoFactorEnabledAsync(user)).ShouldBeFalse();
        var token = GenerateAuthenticatorCode(enrollment.AuthenticatorKey);

        (await service.ConfirmEnrollmentAsync(user, "000000", CancellationToken.None)).ShouldBeFalse();
        (await service.ConfirmEnrollmentAsync(user, token, CancellationToken.None)).ShouldBeTrue();
        (await userManager.GetTwoFactorEnabledAsync(user)).ShouldBeTrue();
        enrollment.AuthenticatorKey.ShouldNotBeNullOrWhiteSpace();
        enrollment.OtpauthUri.ShouldStartWith("otpauth://totp/");
    }

    [Fact]
    public async Task Removing_authenticator_requires_reauthentication_and_revokes_recovery_codes()
    {
        using var provider = TestServiceProviderFactory.Create(Guid.NewGuid().ToString());
        using var scope = provider.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = new ApplicationUser { UserName = "mfa-user", Email = "mfa@example.com" };
        (await userManager.CreateAsync(user, "Pa55word!")).Succeeded.ShouldBeTrue();
        var service = CreateService(scope);
        var enrollment = await service.BeginEnrollmentAsync(user, "SecurityService", CancellationToken.None);
        var token = GenerateAuthenticatorCode(enrollment.AuthenticatorKey);
        (await service.ConfirmEnrollmentAsync(user, token, CancellationToken.None)).ShouldBeTrue();
        await service.GenerateRecoveryCodesAsync(user, 2, CancellationToken.None);

        (await service.RemoveAuthenticatorAsync(user, "wrong", "000000", CancellationToken.None)).ShouldBeFalse();
        (await service.RemoveAuthenticatorAsync(user, "Pa55word!", null, CancellationToken.None)).ShouldBeTrue();

        (await userManager.GetTwoFactorEnabledAsync(user)).ShouldBeFalse();
        (await scope.ServiceProvider.GetRequiredService<SecurityServiceDbContext>().RecoveryCodes.CountAsync()).ShouldBe(0);
        enrollment.AuthenticatorKey.ShouldNotBeNullOrWhiteSpace();
    }

    private static MfaAccountService CreateService(IServiceScope scope)
    {
        return new MfaAccountService(
            scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>(),
            scope.ServiceProvider.GetRequiredService<IPasswordHasher<ApplicationUser>>(),
            scope.ServiceProvider.GetRequiredService<SecurityServiceDbContext>(),
            new RecoveryCodeGenerator());
    }

    private static string GenerateAuthenticatorCode(string key)
    {
        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        var bits = new List<byte>();
        var buffer = 0;
        var bitCount = 0;
        foreach (var character in key)
        {
            buffer = (buffer << 5) | alphabet.IndexOf(char.ToUpperInvariant(character));
            bitCount += 5;
            while (bitCount >= 8)
            {
                bitCount -= 8;
                bits.Add((byte)(buffer >> bitCount));
            }
        }

        var counter = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / 30;
        Span<byte> counterBytes = stackalloc byte[8];
        BinaryPrimitives.WriteInt64BigEndian(counterBytes, counter);
        using var hmac = new HMACSHA1(bits.ToArray());
        var hash = hmac.ComputeHash(counterBytes.ToArray());
        var offset = hash[^1] & 0x0f;
        var value = ((hash[offset] & 0x7f) << 24) |
                    (hash[offset + 1] << 16) |
                    (hash[offset + 2] << 8) |
                    hash[offset + 3];
        return (value % 1_000_000).ToString("D6");
    }
}
