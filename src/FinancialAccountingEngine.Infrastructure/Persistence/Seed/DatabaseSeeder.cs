using FinancialAccountingEngine.Application.AccountingRules;
using FinancialAccountingEngine.Application.Processing;
using FinancialAccountingEngine.Application.Transactions;
using FinancialAccountingEngine.Application.Transactions.Dtos;
using FinancialAccountingEngine.Domain.Common;
using FinancialAccountingEngine.Domain.Entities;
using FinancialAccountingEngine.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static FinancialAccountingEngine.Infrastructure.Persistence.Seed.ChartOfAccounts;

namespace FinancialAccountingEngine.Infrastructure.Persistence.Seed;

/// <summary>
/// Populates the database with a fictional set of accounting rules and transactions so the
/// API and dashboard show meaningful data on first run. Seeded transactions are processed
/// through the real <see cref="IAccountingEngine"/> so the resulting entries and audit trail
/// are produced by the same code path as live traffic. The seeder is idempotent.
/// </summary>
public sealed class DatabaseSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly ITransactionService _transactions;
    private readonly IAccountingEngine _engine;
    private readonly IAccountingRuleService _rules;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(
        ApplicationDbContext context,
        ITransactionService transactions,
        IAccountingEngine engine,
        IAccountingRuleService rules,
        ILogger<DatabaseSeeder> logger)
    {
        _context = context;
        _transactions = transactions;
        _engine = engine;
        _rules = rules;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await SeedRulesAsync(cancellationToken);
        await SeedTransactionsAsync(cancellationToken);
    }

    private async Task SeedRulesAsync(CancellationToken cancellationToken)
    {
        if (await _context.AccountingRules.AnyAsync(cancellationToken))
            return;

        _logger.LogInformation("Seeding accounting rules.");

        var rules = new[]
        {
            new AccountingRule(TransactionType.CashDeposit, currency: null,
                BranchCashCode, BranchCashName, CustomerLiabilityCode, CustomerLiabilityName,
                requiresCashFlow: true, isAccountingOnly: false, CostCenterBehavior.Propagate),

            new AccountingRule(TransactionType.CashWithdrawal, currency: null,
                CustomerLiabilityCode, CustomerLiabilityName, BranchCashCode, BranchCashName,
                requiresCashFlow: true, isAccountingOnly: false, CostCenterBehavior.Propagate),

            new AccountingRule(TransactionType.InternalTransfer, currency: null,
                BranchCashCode, BranchCashName, InternalClearingCode, InternalClearingName,
                requiresCashFlow: true, isAccountingOnly: false, CostCenterBehavior.Propagate),

            new AccountingRule(TransactionType.AccountingAdjustment, currency: null,
                AdjustmentCode, AdjustmentName, OperationalExpenseCode, OperationalExpenseName,
                requiresCashFlow: true, isAccountingOnly: false, CostCenterBehavior.DebitLineOnly),

            new AccountingRule(TransactionType.AccountingOnlyOperation, currency: null,
                OperationalExpenseCode, OperationalExpenseName, AdjustmentCode, AdjustmentName,
                requiresCashFlow: false, isAccountingOnly: true, CostCenterBehavior.None)
        };

        await _context.AccountingRules.AddRangeAsync(rules, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedTransactionsAsync(CancellationToken cancellationToken)
    {
        if (await _context.Transactions.AnyAsync(cancellationToken))
            return;

        _logger.LogInformation("Seeding transactions.");

        // Transactions that will be created and immediately processed (PROCESSED outcome).
        var processed = new[]
        {
            new CreateTransactionRequest(TransactionType.CashDeposit, "ARS", 150_000m, "BR-001", "CC-001", "Branch cash deposit"),
            new CreateTransactionRequest(TransactionType.CashDeposit, "USD", 5_000m, "BR-002", "CC-002", "Foreign currency deposit"),
            new CreateTransactionRequest(TransactionType.CashWithdrawal, "ARS", 42_500m, "BR-001", "CC-001", "Counter withdrawal"),
            new CreateTransactionRequest(TransactionType.InternalTransfer, "EUR", 12_000m, "BR-003", "CC-003", "Inter-branch transfer"),
            new CreateTransactionRequest(TransactionType.AccountingAdjustment, "USD", 800m, "BR-002", "CC-002", "End-of-day adjustment"),
            new CreateTransactionRequest(TransactionType.AccountingOnlyOperation, "ARS", 3_200m, "BR-001", "CC-001", "Accrual (ledger only)")
        };

        foreach (var request in processed)
        {
            var created = await _transactions.CreateAsync(request, cancellationToken);
            if (created.IsSuccess)
                await _engine.ProcessAsync(created.Value.Id, cancellationToken);
        }

        // Transactions left in PENDING status to showcase the "process" action in the UI.
        var pending = new[]
        {
            new CreateTransactionRequest(TransactionType.CashDeposit, "EUR", 9_750m, "BR-003", "CC-003", "Awaiting processing"),
            new CreateTransactionRequest(TransactionType.CashWithdrawal, "USD", 1_250m, "BR-002", "CC-002", "Awaiting processing")
        };

        foreach (var request in pending)
            await _transactions.CreateAsync(request, cancellationToken);

        await SeedFailedTransactionAsync(cancellationToken);
    }

    /// <summary>
    /// Produces one genuinely FAILED transaction by processing a withdrawal while its rule is
    /// temporarily inactive, then restoring the rule. This exercises the engine's failure path.
    /// </summary>
    private async Task SeedFailedTransactionAsync(CancellationToken cancellationToken)
    {
        var withdrawalRule = await _context.AccountingRules
            .FirstOrDefaultAsync(r => r.TransactionType == TransactionType.CashWithdrawal, cancellationToken);
        if (withdrawalRule is null)
            return;

        await _rules.DeactivateAsync(withdrawalRule.Id, cancellationToken);

        var request = new CreateTransactionRequest(
            TransactionType.CashWithdrawal, "ARS", 7_000m, "BR-001", "CC-001", "Processed with no active rule");
        var created = await _transactions.CreateAsync(request, cancellationToken);
        if (created.IsSuccess)
            await _engine.ProcessAsync(created.Value.Id, cancellationToken);

        await _rules.ActivateAsync(withdrawalRule.Id, cancellationToken);
    }
}
