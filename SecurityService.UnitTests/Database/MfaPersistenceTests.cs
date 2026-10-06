using Microsoft.EntityFrameworkCore;
using SecurityService.Database.DbContexts;
using SecurityService.Database.Entities;
using Shouldly;

namespace SecurityService.UnitTests.Database;

public sealed class MfaPersistenceTests
{
    [Fact]
    public void Model_contains_mfa_policy_and_recovery_code_sets_with_expected_indexes()
    {
        var options = new DbContextOptionsBuilder<SecurityServiceDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new SecurityServiceDbContext(options);

        context.MfaPolicies.ShouldNotBeNull();
        context.RecoveryCodes.ShouldNotBeNull();

        var policyIndexes = context.Model.FindEntityType(typeof(MfaPolicy))!.GetIndexes();
        policyIndexes.Any(index => index.Properties.Select(property => property.Name).SequenceEqual(new[] { nameof(MfaPolicy.TargetType), nameof(MfaPolicy.TargetId) }) && index.IsUnique).ShouldBeTrue();

        var recoveryIndexes = context.Model.FindEntityType(typeof(RecoveryCode))!.GetIndexes();
        recoveryIndexes.Any(index => index.Properties.Select(property => property.Name).SequenceEqual(new[] { nameof(RecoveryCode.UserId), nameof(RecoveryCode.ConsumedUtc) })).ShouldBeTrue();
    }

    [Fact]
    public async Task Recovery_code_records_persist_hash_and_consumed_timestamp()
    {
        var options = new DbContextOptionsBuilder<SecurityServiceDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var userId = Guid.NewGuid().ToString();
        var consumedUtc = DateTime.UtcNow;

        await using (var context = new SecurityServiceDbContext(options))
        {
            context.RecoveryCodes.Add(new RecoveryCode
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CodeHash = "hash",
                CreatedUtc = DateTime.UtcNow,
                ConsumedUtc = consumedUtc
            });
            await context.SaveChangesAsync();
        }

        await using (var context = new SecurityServiceDbContext(options))
        {
            var code = await context.RecoveryCodes.SingleAsync();
            code.UserId.ShouldBe(userId);
            code.CodeHash.ShouldBe("hash");
            code.ConsumedUtc.ShouldBe(consumedUtc);
        }
    }
}
