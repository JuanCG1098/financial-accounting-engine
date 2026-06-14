using FinancialAccountingEngine.Application.Transactions.Dtos;
using FinancialAccountingEngine.Domain.Common;

namespace FinancialAccountingEngine.Application.Processing;

/// <summary>
/// Orchestrates the processing of a pending transaction: it finds the applicable rule,
/// generates the accounting entry, validates the balance and records the audit trail.
/// </summary>
public interface IAccountingEngine
{
    Task<Result<TransactionDto>> ProcessAsync(Guid transactionId, CancellationToken cancellationToken = default);
}
