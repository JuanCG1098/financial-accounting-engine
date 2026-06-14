using FinancialAccountingEngine.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinancialAccountingEngine.Infrastructure.Persistence.Configurations;

public sealed class AuditEventConfiguration : IEntityTypeConfiguration<AuditEvent>
{
    public void Configure(EntityTypeBuilder<AuditEvent> builder)
    {
        builder.ToTable("audit_events");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedNever();

        builder.Property(a => a.EventType).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(a => a.Message).HasMaxLength(1000).IsRequired();
        builder.Property(a => a.PreviousStatus).HasConversion<string>().HasMaxLength(20);
        builder.Property(a => a.NewStatus).HasConversion<string>().HasMaxLength(20);
        builder.Property(a => a.CreatedAt).IsRequired();

        builder.HasIndex(a => a.TransactionId);
        builder.HasIndex(a => a.CreatedAt);
    }
}
