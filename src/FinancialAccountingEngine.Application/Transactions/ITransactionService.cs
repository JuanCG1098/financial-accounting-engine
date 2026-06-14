using FinancialAccountingEngine.Application.Transactions.Dtos;
using FinancialAccountingEngine.Domain.Common;

namespace FinancialAccountingEngine.Application.Transactions;

public interface ITransactionService
{
    Task<Result<TransactionDto>> CreateAsync(CreateTransactionRequest request, CancellationToken cancellationToken = default);
    Task<Result<TransactionDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TransactionDto>> ListAsync(TransactionQuery query, CancellationToken cancellationToken = default);
}
