using FinancialAccountingEngine.Domain.Entities;

namespace FinancialAccountingEngine.Application.Abstractions.Persistence;

public interface IAccountingEntryRepository
{
    Task<AccountingEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<AccountingEntry?> GetByTransactionIdAsync(Guid transactionId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AccountingEntry>> ListAsync(CancellationToken cancellationToken = default);

    Task<int> CountAsync(CancellationToken cancellationToken = default);
}
