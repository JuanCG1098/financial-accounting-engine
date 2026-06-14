using FinancialAccountingEngine.Application.Dashboard.Dtos;
using FinancialAccountingEngine.Application.Transactions.Dtos;
using FinancialAccountingEngine.Domain.Entities;
using FinancialAccountingEngine.Domain.Enums;

namespace FinancialAccountingEngine.Application.Abstractions.Persistence;

public interface ITransactionRepository
{
    Task AddAsync(FinancialTransaction transaction, CancellationToken cancellationToken = default);

    /// <summary>Loads a transaction including its audit events and accounting entry.</summary>
    Task<FinancialTransaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FinancialTransaction>> ListAsync(
        TransactionQuery query,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FinancialTransaction>> ListRecentAsync(
        int count,
        CancellationToken cancellationToken = default);

    /// <summary>Number of transactions grouped by status.</summary>
    Task<IReadOnlyDictionary<TransactionStatus, int>> CountByStatusAsync(
        CancellationToken cancellationToken = default);

    /// <summary>Total processed amount grouped by currency.</summary>
    Task<IReadOnlyList<CurrencyAmountDto>> SumProcessedAmountByCurrencyAsync(
        CancellationToken cancellationToken = default);
}
