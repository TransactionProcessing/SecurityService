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
    }
}
