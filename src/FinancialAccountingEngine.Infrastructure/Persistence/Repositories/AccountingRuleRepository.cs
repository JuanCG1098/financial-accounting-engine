using FinancialAccountingEngine.Application.Abstractions.Persistence;
using FinancialAccountingEngine.Domain.Entities;
using FinancialAccountingEngine.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FinancialAccountingEngine.Infrastructure.Persistence.Repositories;

public sealed class AccountingRuleRepository : IAccountingRuleRepository
{
    private readonly ApplicationDbContext _context;

    public AccountingRuleRepository(ApplicationDbContext context) => _context = context;

    public async Task AddAsync(AccountingRule rule, CancellationToken cancellationToken = default)
        => await _context.AccountingRules.AddAsync(rule, cancellationToken);

    public async Task<AccountingRule?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.AccountingRules.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public async Task<IReadOnlyList<AccountingRule>> ListAsync(CancellationToken cancellationToken = default)
        => await _context.AccountingRules
            .AsNoTracking()
            .OrderBy(r => r.TransactionType)
            .ThenBy(r => r.Currency)
            .ToListAsync(cancellationToken);

    public async Task<AccountingRule?> FindActiveRuleAsync(
        TransactionType transactionType,
        string currency,
        CancellationToken cancellationToken = default)
    {
        var normalizedCurrency = currency.ToUpperInvariant();

        var candidates = await _context.AccountingRules
            .Where(r => r.IsActive
                        && r.TransactionType == transactionType
                        && (r.Currency == null || r.Currency == normalizedCurrency))
            .ToListAsync(cancellationToken);

        // Prefer a currency-specific rule over a currency-agnostic one.
        return candidates
            .OrderByDescending(r => r.Currency != null)
            .FirstOrDefault();
    }
}
