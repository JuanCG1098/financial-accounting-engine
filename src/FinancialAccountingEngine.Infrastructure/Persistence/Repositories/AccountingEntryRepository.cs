using FinancialAccountingEngine.Application.Abstractions.Persistence;
using FinancialAccountingEngine.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinancialAccountingEngine.Infrastructure.Persistence.Repositories;

public sealed class AccountingEntryRepository : IAccountingEntryRepository
{
    private readonly ApplicationDbContext _context;

    public AccountingEntryRepository(ApplicationDbContext context) => _context = context;

    public async Task<AccountingEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.AccountingEntries
            .AsNoTracking()
            .Include(e => e.Lines)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public async Task<AccountingEntry?> GetByTransactionIdAsync(
        Guid transactionId,
        CancellationToken cancellationToken = default)
        => await _context.AccountingEntries
            .AsNoTracking()
            .Include(e => e.Lines)
            .FirstOrDefaultAsync(e => e.TransactionId == transactionId, cancellationToken);

    public async Task<IReadOnlyList<AccountingEntry>> ListAsync(CancellationToken cancellationToken = default)
        => await _context.AccountingEntries
            .AsNoTracking()
            .Include(e => e.Lines)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
        => await _context.AccountingEntries.CountAsync(cancellationToken);
}
