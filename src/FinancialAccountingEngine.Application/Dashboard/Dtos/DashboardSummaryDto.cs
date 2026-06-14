using FinancialAccountingEngine.Application.Audit.Dtos;
using FinancialAccountingEngine.Application.Transactions.Dtos;

namespace FinancialAccountingEngine.Application.Dashboard.Dtos;

/// <summary>Aggregated metrics powering the dashboard landing screen.</summary>
public sealed record DashboardSummaryDto(
    int TotalTransactions,
    int ProcessedTransactions,
    int FailedTransactions,
    int PendingTransactions,
    int TotalAccountingEntries,
    IReadOnlyList<CurrencyAmountDto> TotalAmountByCurrency,
    IReadOnlyList<TransactionDto> RecentTransactions,
    IReadOnlyList<AuditEventDto> RecentAuditEvents);

/// <summary>Total processed amount for a single currency.</summary>
public sealed record CurrencyAmountDto(string Currency, decimal TotalAmount);
