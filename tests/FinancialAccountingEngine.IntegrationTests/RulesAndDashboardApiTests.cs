using System.Net;
using FinancialAccountingEngine.Application.AccountingRules.Dtos;
using FinancialAccountingEngine.Application.Dashboard.Dtos;
using FinancialAccountingEngine.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace FinancialAccountingEngine.IntegrationTests;

public class RulesAndDashboardApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public RulesAndDashboardApiTests(CustomWebApplicationFactory factory) => _client = factory.CreateClient();

    [Fact]
    public async Task GetAccountingRules_ReturnsSeededRules()
    {
        var response = await _client.GetAsync("/api/accounting-rules");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var rules = await response.ReadAsAsync<List<AccountingRuleDto>>();
        rules.Should().NotBeEmpty();
        rules.Should().Contain(r => r.IsAccountingOnly); // the accounting-only rule was seeded
    }

    [Fact]
    public async Task CreateAndDeactivateRule_WorksEndToEnd()
    {
        var create = await _client.PostJsonAsync("/api/accounting-rules", new CreateAccountingRuleRequest(
            TransactionType.AccountingAdjustment, "USD",
            "400001", "Accounting Adjustment Account", "500001", "Operational Expense Account",
            RequiresCashFlow: false, IsAccountingOnly: true, CostCenterBehavior.None));

        create.StatusCode.Should().Be(HttpStatusCode.Created);
        var rule = await create.ReadAsAsync<AccountingRuleDto>();
        rule.IsActive.Should().BeTrue();

        var deactivate = await _client.PatchAsync($"/api/accounting-rules/{rule.Id}/deactivate", null);
        deactivate.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await deactivate.ReadAsAsync<AccountingRuleDto>();
        updated.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task CreateRule_WithSameDebitAndCreditAccount_Returns400()
    {
        var create = await _client.PostJsonAsync("/api/accounting-rules", new CreateAccountingRuleRequest(
            TransactionType.AccountingAdjustment, "USD",
            "400001", "Same", "400001", "Same",
            RequiresCashFlow: true, IsAccountingOnly: false, CostCenterBehavior.None));

        create.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetDashboardSummary_ReturnsConsistentTotals()
    {
        var response = await _client.GetAsync("/api/dashboard/summary");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var summary = await response.ReadAsAsync<DashboardSummaryDto>();

        summary.TotalTransactions.Should().Be(
            summary.ProcessedTransactions + summary.FailedTransactions + summary.PendingTransactions);
        summary.TotalAccountingEntries.Should().Be(summary.ProcessedTransactions);
        summary.RecentTransactions.Should().NotBeEmpty();
    }
}
