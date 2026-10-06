using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SecurityService.Database.Entities;

namespace SecurityService.Database.DbContexts;

public sealed class SecurityServiceDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
{
    public SecurityServiceDbContext(DbContextOptions<SecurityServiceDbContext> options)
        : base(options)
    {
    }

    public DbSet<ClientDefinition> ClientDefinitions => this.Set<ClientDefinition>();

    public DbSet<ResourceDefinition> ResourceDefinitions => this.Set<ResourceDefinition>();

    public DbSet<MfaPolicy> MfaPolicies => this.Set<MfaPolicy>();

    public DbSet<RecoveryCode> RecoveryCodes => this.Set<RecoveryCode>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.UseOpenIddict();

        builder.Entity<ClientDefinition>(entity =>
        {
            entity.HasKey(client => client.Id);
            entity.HasIndex(client => client.ClientId).IsUnique();
            entity.Property(client => client.ClientId).HasMaxLength(200);
            entity.Property(client => client.ClientName).HasMaxLength(200);
            entity.Property(client => client.ClientType).HasMaxLength(50);
        });

        builder.Entity<ResourceDefinition>(entity =>
        {
            entity.HasKey(resource => resource.Id);
            entity.HasIndex(resource => new { resource.Name, resource.Type }).IsUnique();
            entity.Property(resource => resource.Name).HasMaxLength(200);
            entity.Property(resource => resource.Type).HasConversion(new EnumToStringConverter<ResourceType>());
        });

        builder.Entity<MfaPolicy>(entity =>
        {
            entity.HasKey(policy => policy.Id);
            entity.Property(policy => policy.TargetType).HasConversion<string>();
            entity.Property(policy => policy.TargetId).HasMaxLength(450).IsRequired();
            entity.HasIndex(policy => new { policy.TargetType, policy.TargetId }).IsUnique();
        });

        builder.Entity<RecoveryCode>(entity =>
        {
            entity.HasKey(code => code.Id);
            entity.Property(code => code.UserId).HasMaxLength(450).IsRequired();
            entity.Property(code => code.CodeHash).HasMaxLength(200).IsRequired();
            entity.HasIndex(code => new { code.UserId, code.CodeHash }).IsUnique();
            entity.HasIndex(code => new { code.UserId, code.ConsumedUtc });
            entity.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(code => code.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
