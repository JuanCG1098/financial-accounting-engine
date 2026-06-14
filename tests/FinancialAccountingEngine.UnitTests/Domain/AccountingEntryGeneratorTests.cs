using FinancialAccountingEngine.Domain.Entities;
using FinancialAccountingEngine.Domain.Enums;
using FinancialAccountingEngine.Domain.Services;
using FluentAssertions;
using Xunit;

namespace FinancialAccountingEngine.UnitTests.Domain;

public class AccountingEntryGeneratorTests
{
    private readonly AccountingEntryGenerator _generator = new();

    [Fact]
    public void Generate_ProducesBalancedDoubleEntry()
    {
        var transaction = FinancialTransaction.Create(
            TransactionType.CashDeposit, "USD", 1_000m, "BR-001", "CC-001", "Deposit").Value;
        var rule = CashDepositRule(CostCenterBehavior.Propagate);

        var entry = _generator.Generate(transaction, rule, "JE-TEST-1");

        entry.Lines.Should().HaveCount(2);
        entry.TotalDebit.Should().Be(1_000m);
        entry.TotalCredit.Should().Be(1_000m);
        entry.IsBalanced.Should().BeTrue();
        entry.Lines.Single(l => l.Debit > 0).AccountCode.Should().Be("100001");
        entry.Lines.Single(l => l.Credit > 0).AccountCode.Should().Be("200001");
    }

    [Fact]
    public void Generate_WithPropagateBehavior_CopiesCostCenterToBothLines()
    {
        var transaction = FinancialTransaction.Create(
            TransactionType.CashDeposit, "USD", 500m, "BR-001", "CC-007", "Deposit").Value;
        var rule = CashDepositRule(CostCenterBehavior.Propagate);

        var entry = _generator.Generate(transaction, rule, "JE-TEST-2");

        entry.Lines.Should().OnlyContain(l => l.CostCenter == "CC-007");
    }

    [Fact]
    public void Generate_WithDebitLineOnlyBehavior_CopiesCostCenterToDebitLineOnly()
    {
        var transaction = FinancialTransaction.Create(
            TransactionType.CashDeposit, "USD", 500m, "BR-001", "CC-007", "Deposit").Value;
        var rule = CashDepositRule(CostCenterBehavior.DebitLineOnly);

        var entry = _generator.Generate(transaction, rule, "JE-TEST-3");

        entry.Lines.Single(l => l.Debit > 0).CostCenter.Should().Be("CC-007");
        entry.Lines.Single(l => l.Credit > 0).CostCenter.Should().BeNull();
    }

    [Fact]
    public void Generate_ForAccountingOnlyRule_StillBalancesAndTagsDescription()
    {
        var transaction = FinancialTransaction.Create(
            TransactionType.AccountingOnlyOperation, "ARS", 3_200m, "BR-001", "CC-001", "Accrual").Value;
        var rule = new AccountingRule(
            TransactionType.AccountingOnlyOperation, currency: null,
            "500001", "Operational Expense Account", "400001", "Accounting Adjustment Account",
            requiresCashFlow: false, isAccountingOnly: true, CostCenterBehavior.None);

        var entry = _generator.Generate(transaction, rule, "JE-TEST-4");

        entry.IsBalanced.Should().BeTrue();
        entry.Lines.Should().OnlyContain(l => l.Description.Contains("accounting-only"));
    }

    private static AccountingRule CashDepositRule(CostCenterBehavior behavior) => new(
        TransactionType.CashDeposit, currency: null,
        "100001", "Branch Cash", "200001", "Customer Liability",
        requiresCashFlow: true, isAccountingOnly: false, behavior);
}
