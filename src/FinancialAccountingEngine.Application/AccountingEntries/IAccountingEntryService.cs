using FinancialAccountingEngine.Application.AccountingEntries.Dtos;
using FinancialAccountingEngine.Domain.Common;

namespace FinancialAccountingEngine.Application.AccountingEntries;

public interface IAccountingEntryService
{
    Task<IReadOnlyList<AccountingEntryDto>> ListAsync(CancellationToken cancellationToken = default);
    Task<Result<AccountingEntryDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<AccountingEntryDto>> GetByTransactionAsync(Guid transactionId, CancellationToken cancellationToken = default);
}
