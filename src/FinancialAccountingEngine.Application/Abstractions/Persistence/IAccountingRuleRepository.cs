using FinancialAccountingEngine.Domain.Entities;
using FinancialAccountingEngine.Domain.Enums;

namespace FinancialAccountingEngine.Application.Abstractions.Persistence;

public interface IAccountingRuleRepository
{
    Task AddAsync(AccountingRule rule, CancellationToken cancellationToken = default);

    Task<AccountingRule?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AccountingRule>> ListAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds the active rule that applies to a transaction type and currency. A currency-specific
    /// rule is preferred over a currency-agnostic one when both exist.
    /// </summary>
    Task<AccountingRule?> FindActiveRuleAsync(
        TransactionType transactionType,
        string currency,
        CancellationToken cancellationToken = default);
}
