using FinancialAccountingEngine.Application.AccountingRules.Dtos;
using FinancialAccountingEngine.Application.Transactions.Dtos;
using FinancialAccountingEngine.Domain.Entities;
using FinancialAccountingEngine.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace FinancialAccountingEngine.UnitTests.Application;

public class AccountingEngineTests
{
    [Fact]
    public async Task Process_WithActiveRule_ProducesBalancedEntryAndProcessedStatus()
    {
        using var harness = new TestHarness();
        await SeedCashDepositRuleAsync(harness);
        var transaction = await CreateAsync(harness, TransactionType.CashDeposit, "USD", 1_000m);

        var result = await harness.Engine.ProcessAsync(transaction.Id);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(TransactionStatus.Processed);

        var persisted = await harness.Context.AccountingEntries
            .FindAsync(harness.Context.AccountingEntries.Single().Id);
        persisted!.IsBalanced.Should().BeTrue();
        persisted.TotalDebit.Should().Be(1_000m);
    }

    [Fact]
    public async Task Process_WithNoActiveRule_FailsAndMarksTransactionFailed()
    {
        using var harness = new TestHarness();
        // No rule seeded.
        var transaction = await CreateAsync(harness, TransactionType.CashWithdrawal, "USD", 500m);

        var result = await harness.Engine.ProcessAsync(transaction.Id);

        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("No active accounting rule");

        var reloaded = await harness.Transactions.GetByIdAsync(transaction.Id);
        reloaded.Value.Status.Should().Be(TransactionStatus.Failed);
    }

    [Fact]
    public async Task Process_AccountingOnlyOperation_SucceedsWithoutCashFlow()
    {
        using var harness = new TestHarness();
        harness.Context.AccountingRules.Add(new AccountingRule(
            TransactionType.AccountingOnlyOperation, currency: null,
            "500001", "Operational Expense Account", "400001", "Accounting Adjustment Account",
            requiresCashFlow: false, isAccountingOnly: true, CostCenterBehavior.None));
        await harness.Context.SaveChangesAsync();

        var transaction = await CreateAsync(harness, TransactionType.AccountingOnlyOperation, "ARS", 3_200m);

        var result = await harness.Engine.ProcessAsync(transaction.Id);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(TransactionStatus.Processed);
        var rule = harness.Context.AccountingRules.Single();
        rule.RequiresCashFlow.Should().BeFalse();
        rule.IsAccountingOnly.Should().BeTrue();
    }

    [Fact]
    public async Task Process_RecordsFullAuditTrail()
    {
        using var harness = new TestHarness();
        await SeedCashDepositRuleAsync(harness);
        var transaction = await CreateAsync(harness, TransactionType.CashDeposit, "EUR", 250m);

        await harness.Engine.ProcessAsync(transaction.Id);

        var events = harness.Context.AuditEvents
            .Where(a => a.TransactionId == transaction.Id)
            .Select(a => a.EventType)
            .ToList();

        events.Should().Contain(new[]
        {
            AuditEventType.TransactionCreated,
            AuditEventType.ProcessingStarted,
            AuditEventType.AccountingRuleApplied,
            AuditEventType.EntryGenerated,
            AuditEventType.BalanceValidated,
            AuditEventType.ProcessingCompleted
        });
    }

    [Fact]
    public async Task Process_FailedTransactionRecordsProcessingFailedAudit()
    {
        using var harness = new TestHarness();
        var transaction = await CreateAsync(harness, TransactionType.InternalTransfer, "USD", 100m);

        await harness.Engine.ProcessAsync(transaction.Id);

        harness.Context.AuditEvents
            .Where(a => a.TransactionId == transaction.Id)
            .Select(a => a.EventType)
            .Should().Contain(AuditEventType.ProcessingFailed);
    }

    private static async Task SeedCashDepositRuleAsync(TestHarness harness)
    {
        await harness.Rules.CreateAsync(new CreateAccountingRuleRequest(
            TransactionType.CashDeposit, Currency: null,
            "100001", "Branch Cash", "200001", "Customer Liability",
            RequiresCashFlow: true, IsAccountingOnly: false, CostCenterBehavior.Propagate));
    }

    private static async Task<TransactionDto> CreateAsync(
        TestHarness harness, TransactionType type, string currency, decimal amount)
    {
        var created = await harness.Transactions.CreateAsync(
            new CreateTransactionRequest(type, currency, amount, "BR-001", "CC-001", "test"));
        created.IsSuccess.Should().BeTrue();
        return created.Value;
    }
}
