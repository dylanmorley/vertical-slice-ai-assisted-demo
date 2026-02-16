using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using VerticalSlice.Web.Api.Contracts;
using VerticalSlice.Web.Api.Model;

namespace VerticalSlice.Web.Api.Data;

[ExcludeFromCodeCoverage]
public class VerticalSliceDataContext(DbContextOptions<VerticalSliceDataContext> dataContextOptions)
    : DbContext(dataContextOptions)
{
    public DbSet<Audit> Audit { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Audit table
        modelBuilder.Entity<Audit>(entity =>
        {
            entity.HasKey(e => e.AuditId);
            entity.Property(e => e.AuditId).ValueGeneratedOnAdd();

            entity.Property(e => e.Operation).IsRequired().HasMaxLength(50);
            entity.Property(e => e.EntityType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.EntityId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.UserId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.UserName).IsRequired().HasMaxLength(255);

            entity.Property(e => e.IpAddress).HasMaxLength(45); // IPv6 max length
            entity.Property(e => e.UserAgent).HasMaxLength(500);
            entity.Property(e => e.OldValues); // Database will use appropriate text type
            entity.Property(e => e.NewValues); // Database will use appropriate text type
            entity.Property(e => e.Context).HasMaxLength(1000);
            entity.Property(e => e.HttpMethod).HasMaxLength(10);
            entity.Property(e => e.Endpoint).HasMaxLength(500);
            entity.Property(e => e.ErrorMessage).HasMaxLength(2000);
            entity.Property(e => e.OrganizationId).HasMaxLength(100);
            entity.Property(e => e.Tags).HasMaxLength(500);
            entity.Property(e => e.CorrelationId).HasMaxLength(100);

            entity.Property(e => e.Timestamp).IsRequired();
            entity.Property(e => e.IsSuccess).IsRequired();

            // Create indexes for common queries
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => e.Operation);
            entity.HasIndex(e => e.EntityType);
            entity.HasIndex(e => e.EntityId);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.OrganizationId);
            entity.HasIndex(e => e.CorrelationId);
        });
        modelBuilder.Model
            .GetEntityTypes()
            .SelectMany(e => e.GetForeignKeys())
            .ToList()
            .ForEach(fk => fk.DeleteBehavior = DeleteBehavior.Restrict);
    }
}
