using FinancialAccountingEngine.Domain.Enums;

namespace FinancialAccountingEngine.Application.Audit.Dtos;

/// <summary>Read model for an audit-trail event.</summary>
public sealed record AuditEventDto(
    Guid Id,
    Guid TransactionId,
    AuditEventType EventType,
    string Message,
    TransactionStatus? PreviousStatus,
    TransactionStatus? NewStatus,
    DateTime CreatedAt);
