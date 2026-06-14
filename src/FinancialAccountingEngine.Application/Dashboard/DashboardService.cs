using FinancialAccountingEngine.Application.Abstractions.Persistence;
using FinancialAccountingEngine.Application.Common.Mapping;
using FinancialAccountingEngine.Application.Dashboard.Dtos;
using FinancialAccountingEngine.Domain.Enums;

namespace FinancialAccountingEngine.Application.Dashboard;

public sealed class DashboardService : IDashboardService
{
    private const int RecentItemsCount = 10;

    private readonly ITransactionRepository _transactions;
    private readonly IAccountingEntryRepository _entries;
    private readonly IAuditEventRepository _auditEvents;

    public DashboardService(
        ITransactionRepository transactions,
        IAccountingEntryRepository entries,
        IAuditEventRepository auditEvents)
    {
        _transactions = transactions;
        _entries = entries;
        _auditEvents = auditEvents;
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        var countsByStatus = await _transactions.CountByStatusAsync(cancellationToken);
        var amountByCurrency = await _transactions.SumProcessedAmountByCurrencyAsync(cancellationToken);
        var entryCount = await _entries.CountAsync(cancellationToken);
        var recentTransactions = await _transactions.ListRecentAsync(RecentItemsCount, cancellationToken);
        var recentAudit = await _auditEvents.ListRecentAsync(RecentItemsCount, cancellationToken);

        var total = countsByStatus.Values.Sum();

        return new DashboardSummaryDto(
            TotalTransactions: total,
            ProcessedTransactions: countsByStatus.GetValueOrDefault(TransactionStatus.Processed),
            FailedTransactions: countsByStatus.GetValueOrDefault(TransactionStatus.Failed),
            PendingTransactions: countsByStatus.GetValueOrDefault(TransactionStatus.Pending),
            TotalAccountingEntries: entryCount,
            TotalAmountByCurrency: amountByCurrency,
            RecentTransactions: recentTransactions.Select(t => t.ToDto()).ToList(),
            RecentAuditEvents: recentAudit.Select(a => a.ToDto()).ToList());
    }
}
