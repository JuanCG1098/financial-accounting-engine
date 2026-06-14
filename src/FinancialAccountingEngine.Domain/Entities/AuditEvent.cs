using FinancialAccountingEngine.Domain.Enums;

namespace FinancialAccountingEngine.Domain.Entities;

/// <summary>
/// An immutable record of a single step in a transaction's processing lifecycle,
/// forming the audit trail / journal.
/// </summary>
public class AuditEvent
{
    // EF Core constructor.
    private AuditEvent() { }

    private AuditEvent(
        Guid transactionId,
        AuditEventType eventType,
        string message,
        TransactionStatus? previousStatus,
        TransactionStatus? newStatus)
    {
        Id = Guid.NewGuid();
        TransactionId = transactionId;
        EventType = eventType;
        Message = message;
        PreviousStatus = previousStatus;
        NewStatus = newStatus;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid TransactionId { get; private set; }
    public AuditEventType EventType { get; private set; }
    public string Message { get; private set; } = string.Empty;
    public TransactionStatus? PreviousStatus { get; private set; }
    public TransactionStatus? NewStatus { get; private set; }
    public DateTime CreatedAt { get; private set; }

    internal static AuditEvent Create(
        Guid transactionId,
        AuditEventType eventType,
        string message,
        TransactionStatus? previousStatus,
        TransactionStatus? newStatus) =>
        new(transactionId, eventType, message, previousStatus, newStatus);
}
