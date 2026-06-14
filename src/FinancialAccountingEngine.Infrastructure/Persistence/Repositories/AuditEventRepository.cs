using FinancialAccountingEngine.Application.Abstractions.Persistence;
using FinancialAccountingEngine.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinancialAccountingEngine.Infrastructure.Persistence.Repositories;

public sealed class AuditEventRepository : IAuditEventRepository
{
    private readonly ApplicationDbContext _context;

    public AuditEventRepository(ApplicationDbContext context) => _context = context;

    public async Task<IReadOnlyList<AuditEvent>> ListAsync(CancellationToken cancellationToken = default)
        => await _context.AuditEvents
            .AsNoTracking()
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<AuditEvent>> ListByTransactionAsync(
        Guid transactionId,
        CancellationToken cancellationToken = default)
        => await _context.AuditEvents
            .AsNoTracking()
            .Where(a => a.TransactionId == transactionId)
            .OrderBy(a => a.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<AuditEvent>> ListRecentAsync(
        int count,
        CancellationToken cancellationToken = default)
        => await _context.AuditEvents
            .AsNoTracking()
            .OrderByDescending(a => a.CreatedAt)
            .Take(count)
            .ToListAsync(cancellationToken);
}
