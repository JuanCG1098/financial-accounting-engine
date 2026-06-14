using System.Reflection;
using FinancialAccountingEngine.Application.Abstractions.Persistence;
using FinancialAccountingEngine.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinancialAccountingEngine.Infrastructure.Persistence;

public sealed class ApplicationDbContext : DbContext, IUnitOfWork
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<FinancialTransaction> Transactions => Set<FinancialTransaction>();
    public DbSet<AccountingRule> AccountingRules => Set<AccountingRule>();
    public DbSet<AccountingEntry> AccountingEntries => Set<AccountingEntry>();
    public DbSet<AccountingEntryLine> AccountingEntryLines => Set<AccountingEntryLine>();
    public DbSet<AuditEvent> AuditEvents => Set<AuditEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}
