using FinancialAccountingEngine.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace FinancialAccountingEngine.UnitTests.Domain;

public class AccountingEntryTests
{
    [Fact]
    public void Entry_WithEqualDebitAndCredit_IsBalanced()
    {
        var entry = AccountingEntry.Create(Guid.NewGuid(), "JE-1", "USD");
        entry.AddLine("100001", "Branch Cash", debit: 1_000m, credit: 0m, costCenter: null, description: "d");
        entry.AddLine("200001", "Customer Liability", debit: 0m, credit: 1_000m, costCenter: null, description: "c");

        entry.IsBalanced.Should().BeTrue();
        entry.TotalDebit.Should().Be(entry.TotalCredit);
    }

    [Fact]
    public void Entry_WithMismatchedDebitAndCredit_IsNotBalanced()
    {
        var entry = AccountingEntry.Create(Guid.NewGuid(), "JE-2", "USD");
        entry.AddLine("100001", "Branch Cash", debit: 1_000m, credit: 0m, costCenter: null, description: "d");
        entry.AddLine("200001", "Customer Liability", debit: 0m, credit: 900m, costCenter: null, description: "c");

        entry.IsBalanced.Should().BeFalse();
        entry.TotalDebit.Should().Be(1_000m);
        entry.TotalCredit.Should().Be(900m);
    }
}
