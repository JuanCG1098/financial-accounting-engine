using FinancialAccountingEngine.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinancialAccountingEngine.Infrastructure.Persistence.Configurations;

public sealed class AccountingRuleConfiguration : IEntityTypeConfiguration<AccountingRule>
{
    public void Configure(EntityTypeBuilder<AccountingRule> builder)
    {
        builder.ToTable("accounting_rules");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedNever();

        builder.Property(r => r.TransactionType).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(r => r.Currency).HasMaxLength(3);
        builder.Property(r => r.DebitAccountCode).HasMaxLength(20).IsRequired();
        builder.Property(r => r.DebitAccountName).HasMaxLength(120).IsRequired();
        builder.Property(r => r.CreditAccountCode).HasMaxLength(20).IsRequired();
        builder.Property(r => r.CreditAccountName).HasMaxLength(120).IsRequired();
        builder.Property(r => r.CostCenterBehavior).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(r => r.RequiresCashFlow).IsRequired();
        builder.Property(r => r.IsAccountingOnly).IsRequired();
        builder.Property(r => r.IsActive).IsRequired();

        builder.HasIndex(r => new { r.TransactionType, r.Currency, r.IsActive });
    }
}
