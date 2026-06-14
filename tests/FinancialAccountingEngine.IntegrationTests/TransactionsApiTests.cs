using System.Net;
using System.Net.Http.Json;
using FinancialAccountingEngine.Application.AccountingEntries.Dtos;
using FinancialAccountingEngine.Application.Audit.Dtos;
using FinancialAccountingEngine.Application.Transactions.Dtos;
using FinancialAccountingEngine.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace FinancialAccountingEngine.IntegrationTests;

public class TransactionsApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public TransactionsApiTests(CustomWebApplicationFactory factory) => _client = factory.CreateClient();

    [Fact]
    public async Task GetTransactions_ReturnsSeededData()
    {
        var response = await _client.GetAsync("/api/transactions");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var transactions = await response.ReadAsAsync<List<TransactionDto>>();
        transactions.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateTransaction_WithValidPayload_Returns201AndPendingStatus()
    {
        var request = new CreateTransactionRequest(
            TransactionType.CashDeposit, "USD", 1_234.56m, "BR-009", "CC-001", "Integration deposit");

        var response = await _client.PostJsonAsync("/api/transactions", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.ReadAsAsync<TransactionDto>();
        created.Status.Should().Be(TransactionStatus.Pending);
        created.Currency.Should().Be("USD");
    }

    [Fact]
    public async Task CreateTransaction_WithInvalidAmount_Returns400()
    {
        var request = new CreateTransactionRequest(
            TransactionType.CashDeposit, "USD", 0m, "BR-009", "CC-001", "Invalid");

        var response = await _client.PostJsonAsync("/api/transactions", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ProcessTransaction_GeneratesBalancedEntryAndAuditTrail()
    {
        // Create.
        var createResponse = await _client.PostJsonAsync("/api/transactions",
            new CreateTransactionRequest(TransactionType.CashDeposit, "ARS", 10_000m, "BR-001", "CC-001", "To process"));
        var created = await createResponse.ReadAsAsync<TransactionDto>();

        // Process.
        var processResponse = await _client.PostAsync($"/api/transactions/{created.Id}/process", null);
        processResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var processed = await processResponse.ReadAsAsync<TransactionDto>();
        processed.Status.Should().Be(TransactionStatus.Processed);

        // Entry exists and is balanced.
        var entryResponse = await _client.GetAsync($"/api/accounting-entries/by-transaction/{created.Id}");
        entryResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var entry = await entryResponse.ReadAsAsync<AccountingEntryDto>();
        entry.IsBalanced.Should().BeTrue();
        entry.TotalDebit.Should().Be(entry.TotalCredit);
        entry.Lines.Should().HaveCount(2);

        // Audit trail records the completed lifecycle.
        var auditResponse = await _client.GetAsync($"/api/audit/by-transaction/{created.Id}");
        var audit = await auditResponse.ReadAsAsync<List<AuditEventDto>>();
        audit.Select(a => a.EventType).Should().Contain(AuditEventType.ProcessingCompleted);
    }

    [Fact]
    public async Task GetTransactionById_WhenMissing_Returns404()
    {
        var response = await _client.GetAsync($"/api/transactions/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
