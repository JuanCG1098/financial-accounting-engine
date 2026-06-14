using FinancialAccountingEngine.Application.AccountingRules;
using FinancialAccountingEngine.Application.Dashboard;
using FinancialAccountingEngine.Application.Processing;
using FinancialAccountingEngine.Application.Transactions;
using FinancialAccountingEngine.Domain.Services;
using FinancialAccountingEngine.Infrastructure.Persistence;
using FinancialAccountingEngine.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace FinancialAccountingEngine.UnitTests;

/// <summary>
/// Wires the real services on top of an isolated EF Core in-memory database, so application-level
/// tests exercise the actual repositories, engine and persistence behaviour without external infra.
/// </summary>
internal sealed class TestHarness : IDisposable
{
    public ApplicationDbContext Context { get; }
    public ITransactionService Transactions { get; }
    public IAccountingEngine Engine { get; }
    public IAccountingRuleService Rules { get; }
    public IDashboardService Dashboard { get; }

    public TestHarness()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"fae-tests-{Guid.NewGuid()}")
            .Options;

        Context = new ApplicationDbContext(options);

        var transactionRepository = new TransactionRepository(Context);
        var ruleRepository = new AccountingRuleRepository(Context);
        var entryRepository = new AccountingEntryRepository(Context);
        var auditRepository = new AuditEventRepository(Context);
        var entryGenerator = new AccountingEntryGenerator();

        Transactions = new TransactionService(
            transactionRepository, Context, NullLogger<TransactionService>.Instance);

        Engine = new AccountingEngine(
            transactionRepository, ruleRepository, entryRepository, entryGenerator, Context,
            NullLogger<AccountingEngine>.Instance);

        Rules = new AccountingRuleService(ruleRepository, Context);

        Dashboard = new DashboardService(transactionRepository, entryRepository, auditRepository);
    }

    public void Dispose() => Context.Dispose();
}
