using Microsoft.EntityFrameworkCore;
using Publink.AuditTimeline.Domain.Audit;

namespace Publink.AuditTimeline.Infrastructure.Persistence;

public sealed class AuditDbContext : DbContext
{
    public AuditDbContext(DbContextOptions<AuditDbContext> options)
        : base(options)
    {
    }

    public DbSet<AuditedContract> Contracts => Set<AuditedContract>();

    public DbSet<AuditLogEntry> AuditLogEntries => Set<AuditLogEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuditedContract>(entity =>
        {
            entity.ToTable("Contracts");
            entity.HasKey(contract => contract.ContractId);
            entity.Property(contract => contract.ContractId).HasMaxLength(64);
            entity.Property(contract => contract.ContractNumber).HasMaxLength(64).IsRequired();
            entity.Property(contract => contract.CreatedAt).IsRequired();
            entity.Property(contract => contract.CreatedBy).HasMaxLength(128).IsRequired();
            entity.HasIndex(contract => contract.ContractNumber).IsUnique();
        });

        modelBuilder.Entity<AuditLogEntry>(entity =>
        {
            entity.ToTable("AuditLog");
            entity.HasKey(entry => entry.Id);
            entity.Property(entry => entry.Id).HasMaxLength(64);
            entity.Property(entry => entry.ContractId).HasMaxLength(64).IsRequired();
            entity.Property(entry => entry.EntityId).HasMaxLength(64).IsRequired();
            entity.Property(entry => entry.EntityType).HasConversion<int>().IsRequired();
            entity.Property(entry => entry.ChangeType).HasConversion<int>().IsRequired();
            entity.Property(entry => entry.ChangedAt).IsRequired();
            entity.Property(entry => entry.ChangedBy).HasMaxLength(128).IsRequired();
            entity.Property(entry => entry.FieldName).HasMaxLength(128).IsRequired();
            entity.Property(entry => entry.OldValue).HasMaxLength(2048);
            entity.Property(entry => entry.NewValue).HasMaxLength(2048);

            entity.HasIndex(entry => new { entry.ContractId, entry.ChangedAt });
            entity.HasIndex(entry => entry.ChangeType);
            entity.HasIndex(entry => entry.EntityType);
            entity.HasIndex(entry => entry.ChangedBy);
        });
    }
}
