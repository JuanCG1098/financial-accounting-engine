using FinancialAccountingEngine.Application.AccountingRules.Dtos;
using FinancialAccountingEngine.Application.Transactions.Dtos;
using FinancialAccountingEngine.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace FinancialAccountingEngine.UnitTests.Application;

public class DashboardServiceTests
{
    [Fact]
    public async Task GetSummary_ReflectsProcessedPendingAndFailedCounts()
    {
        using var harness = new TestHarness();

        await harness.Rules.CreateAsync(new CreateAccountingRuleRequest(
            TransactionType.CashDeposit, Currency: null,
            "100001", "Branch Cash", "200001", "Customer Liability",
            RequiresCashFlow: true, IsAccountingOnly: false, CostCenterBehavior.Propagate));

        // Two processed deposits in USD.
        var d1 = await Create(harness, TransactionType.CashDeposit, "USD", 1_000m);
        var d2 = await Create(harness, TransactionType.CashDeposit, "USD", 500m);
        await harness.Engine.ProcessAsync(d1.Id);
        await harness.Engine.ProcessAsync(d2.Id);

        // One pending deposit.
        await Create(harness, TransactionType.CashDeposit, "EUR", 250m);

        // One failed (no rule for withdrawals).
        var w = await Create(harness, TransactionType.CashWithdrawal, "USD", 75m);
        await harness.Engine.ProcessAsync(w.Id);

        var summary = await harness.Dashboard.GetSummaryAsync();

        summary.TotalTransactions.Should().Be(4);
        summary.ProcessedTransactions.Should().Be(2);
        summary.PendingTransactions.Should().Be(1);
        summary.FailedTransactions.Should().Be(1);
        summary.TotalAccountingEntries.Should().Be(2);

        summary.TotalAmountByCurrency.Should().ContainSingle()
            .Which.Should().BeEquivalentTo(new { Currency = "USD", TotalAmount = 1_500m });

        summary.RecentTransactions.Should().HaveCount(4);
        summary.RecentAuditEvents.Should().NotBeEmpty();
    }

    private static async Task<TransactionDto> Create(
        TestHarness harness, TransactionType type, string currency, decimal amount)
        => (await harness.Transactions.CreateAsync(
            new CreateTransactionRequest(type, currency, amount, "BR-001", "CC-001", "test"))).Value;
}
