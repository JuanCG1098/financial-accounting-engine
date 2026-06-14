using FinancialAccountingEngine.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinancialAccountingEngine.Infrastructure.Persistence.Configurations;

public sealed class AccountingEntryConfiguration : IEntityTypeConfiguration<AccountingEntry>
{
    public void Configure(EntityTypeBuilder<AccountingEntry> builder)
    {
        builder.ToTable("accounting_entries");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();

        builder.Property(e => e.EntryNumber).HasMaxLength(40).IsRequired();
        builder.Property(e => e.Currency).HasMaxLength(3).IsRequired();
        builder.Property(e => e.TotalDebit).HasPrecision(18, 2);
        builder.Property(e => e.TotalCredit).HasPrecision(18, 2);
        builder.Property(e => e.IsBalanced).IsRequired();
        builder.Property(e => e.CreatedAt).IsRequired();

        builder.HasIndex(e => e.TransactionId).IsUnique();
        builder.HasIndex(e => e.EntryNumber).IsUnique();

        builder.HasMany(e => e.Lines)
            .WithOne()
            .HasForeignKey(l => l.AccountingEntryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata.FindNavigation(nameof(AccountingEntry.Lines))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}

public sealed class AccountingEntryLineConfiguration : IEntityTypeConfiguration<AccountingEntryLine>
{
    public void Configure(EntityTypeBuilder<AccountingEntryLine> builder)
    {
        builder.ToTable("accounting_entry_lines");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id).ValueGeneratedNever();

        builder.Property(l => l.AccountCode).HasMaxLength(20).IsRequired();
        builder.Property(l => l.AccountName).HasMaxLength(120).IsRequired();
        builder.Property(l => l.Debit).HasPrecision(18, 2);
        builder.Property(l => l.Credit).HasPrecision(18, 2);
        builder.Property(l => l.CostCenter).HasMaxLength(20);
        builder.Property(l => l.Description).HasMaxLength(500);
    }
}
