using FinancialAccountingEngine.Application.Abstractions.Persistence;
using FinancialAccountingEngine.Application.Audit.Dtos;
using FinancialAccountingEngine.Application.Common.Mapping;

namespace FinancialAccountingEngine.Application.Audit;

public sealed class AuditService : IAuditService
{
    private readonly IAuditEventRepository _auditEvents;

    public AuditService(IAuditEventRepository auditEvents) => _auditEvents = auditEvents;

    public async Task<IReadOnlyList<AuditEventDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        var events = await _auditEvents.ListAsync(cancellationToken);
        return events.Select(e => e.ToDto()).ToList();
    }

    public async Task<IReadOnlyList<AuditEventDto>> ListByTransactionAsync(
        Guid transactionId,
        CancellationToken cancellationToken = default)
    {
        var events = await _auditEvents.ListByTransactionAsync(transactionId, cancellationToken);
        return events.Select(e => e.ToDto()).ToList();
    }
}
