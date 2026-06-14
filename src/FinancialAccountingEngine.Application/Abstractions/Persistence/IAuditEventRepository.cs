using FinancialAccountingEngine.Domain.Entities;

namespace FinancialAccountingEngine.Application.Abstractions.Persistence;

public interface IAuditEventRepository
{
    Task<IReadOnlyList<AuditEvent>> ListAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AuditEvent>> ListByTransactionAsync(
        Guid transactionId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AuditEvent>> ListRecentAsync(int count, CancellationToken cancellationToken = default);
}
