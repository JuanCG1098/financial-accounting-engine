using FinancialAccountingEngine.Domain.Entities;
using FinancialAccountingEngine.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace FinancialAccountingEngine.UnitTests.Domain;

public class FinancialTransactionTests
{
    [Fact]
    public void Create_WithValidData_ReturnsPendingTransaction()
    {
        var result = FinancialTransaction.Create(
            TransactionType.CashDeposit, "usd", 1_000m, "BR-001", "CC-001", "Deposit");

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(TransactionStatus.Pending);
        result.Value.Currency.Should().Be("USD"); // normalized to upper-case
        result.Value.Amount.Should().Be(1_000m);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-50)]
    public void Create_WithNonPositiveAmount_Fails(decimal amount)
    {
        var result = FinancialTransaction.Create(
            TransactionType.CashDeposit, "USD", amount, "BR-001", "CC-001", "Deposit");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("validation");
        result.Error.Message.Should().Contain("greater than zero");
    }

    [Fact]
    public void Create_WithUnsupportedCurrency_Fails()
    {
        var result = FinancialTransaction.Create(
            TransactionType.CashDeposit, "JPY", 100m, "BR-001", "CC-001", "Deposit");

        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("not supported");
    }

    [Fact]
    public void MarkProcessing_FromNonPendingStatus_Fails()
    {
        var transaction = CreateValid();
        transaction.MarkProcessing(); // -> Processing

        var second = transaction.MarkProcessing();

        second.IsFailure.Should().BeTrue();
        second.Error.Code.Should().Be("conflict");
    }

    [Fact]
    public void RecordAudit_AppendsEventToTrail()
    {
        var transaction = CreateValid();

        transaction.RecordAudit(
            AuditEventType.TransactionCreated, "created", null, TransactionStatus.Pending);

        transaction.AuditEvents.Should().ContainSingle()
            .Which.EventType.Should().Be(AuditEventType.TransactionCreated);
    }

    private static FinancialTransaction CreateValid() =>
        FinancialTransaction.Create(
            TransactionType.CashDeposit, "USD", 1_000m, "BR-001", "CC-001", "Deposit").Value;
}
