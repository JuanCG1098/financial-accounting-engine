using FinancialAccountingEngine.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinancialAccountingEngine.Infrastructure.Persistence.Configurations;

public sealed class FinancialTransactionConfiguration : IEntityTypeConfiguration<FinancialTransaction>
{
    public void Configure(EntityTypeBuilder<FinancialTransaction> builder)
    {
        builder.ToTable("transactions");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).ValueGeneratedNever();

        builder.Property(t => t.Type).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(t => t.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(t => t.Currency).HasMaxLength(3).IsRequired();
        builder.Property(t => t.Amount).HasPrecision(18, 2);
        builder.Property(t => t.BranchCode).HasMaxLength(20).IsRequired();
        builder.Property(t => t.CostCenter).HasMaxLength(20);
        builder.Property(t => t.Description).HasMaxLength(500);
        builder.Property(t => t.CreatedAt).IsRequired();

        builder.HasIndex(t => t.Status);
        builder.HasIndex(t => t.Currency);
        builder.HasIndex(t => t.CreatedAt);

        // One-to-one with the generated accounting entry.
        builder.HasOne(t => t.AccountingEntry)
            .WithOne()
            .HasForeignKey<AccountingEntry>(e => e.TransactionId)
            .OnDelete(DeleteBehavior.Cascade);

        // One-to-many audit trail.
        builder.HasMany(t => t.AuditEvents)
            .WithOne()
            .HasForeignKey(a => a.TransactionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata.FindNavigation(nameof(FinancialTransaction.AuditEvents))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
