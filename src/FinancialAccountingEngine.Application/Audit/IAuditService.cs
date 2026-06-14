using FinancialAccountingEngine.Application.Audit.Dtos;

namespace FinancialAccountingEngine.Application.Audit;

public interface IAuditService
{
    Task<IReadOnlyList<AuditEventDto>> ListAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AuditEventDto>> ListByTransactionAsync(Guid transactionId, CancellationToken cancellationToken = default);
}
