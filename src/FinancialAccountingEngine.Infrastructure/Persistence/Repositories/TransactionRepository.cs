using FinancialAccountingEngine.Application.Abstractions.Persistence;
using FinancialAccountingEngine.Application.Dashboard.Dtos;
using FinancialAccountingEngine.Application.Transactions.Dtos;
using FinancialAccountingEngine.Domain.Entities;
using FinancialAccountingEngine.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FinancialAccountingEngine.Infrastructure.Persistence.Repositories;

public sealed class TransactionRepository : ITransactionRepository
{
    private readonly ApplicationDbContext _context;

    public TransactionRepository(ApplicationDbContext context) => _context = context;

    public async Task AddAsync(FinancialTransaction transaction, CancellationToken cancellationToken = default)
        => await _context.Transactions.AddAsync(transaction, cancellationToken);

    public async Task<FinancialTransaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Transactions
            .Include(t => t.AccountingEntry!)
                .ThenInclude(e => e.Lines)
            .Include(t => t.AuditEvents)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task<IReadOnlyList<FinancialTransaction>> ListAsync(
        TransactionQuery query,
        CancellationToken cancellationToken = default)
    {
        var transactions = _context.Transactions.AsNoTracking();

        if (query.Status is not null)
            transactions = transactions.Where(t => t.Status == query.Status);

        if (!string.IsNullOrWhiteSpace(query.Currency))
        {
            var currency = query.Currency.ToUpperInvariant();
            transactions = transactions.Where(t => t.Currency == currency);
        }

        if (query.Type is not null)
            transactions = transactions.Where(t => t.Type == query.Type);

        return await transactions
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<FinancialTransaction>> ListRecentAsync(
        int count,
        CancellationToken cancellationToken = default)
        => await _context.Transactions
            .AsNoTracking()
            .OrderByDescending(t => t.CreatedAt)
            .Take(count)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyDictionary<TransactionStatus, int>> CountByStatusAsync(
        CancellationToken cancellationToken = default)
    {
        // Only the status column is pulled from the database; grouping is done in memory.
        var statuses = await _context.Transactions
            .Select(t => t.Status)
            .ToListAsync(cancellationToken);

        return statuses
            .GroupBy(s => s)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    public async Task<IReadOnlyList<CurrencyAmountDto>> SumProcessedAmountByCurrencyAsync(
        CancellationToken cancellationToken = default)
    {
        // The currency/amount columns of processed transactions are pulled and summed in memory.
        var rows = await _context.Transactions
            .Where(t => t.Status == TransactionStatus.Processed)
            .Select(t => new { t.Currency, t.Amount })
            .ToListAsync(cancellationToken);

        return rows
            .GroupBy(r => r.Currency)
            .Select(g => new CurrencyAmountDto(g.Key, g.Sum(x => x.Amount)))
            .OrderBy(x => x.Currency)
            .ToList();
    }
}
