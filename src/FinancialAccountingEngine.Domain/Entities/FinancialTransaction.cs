using FinancialAccountingEngine.Domain.Common;
using FinancialAccountingEngine.Domain.Enums;

namespace FinancialAccountingEngine.Domain.Entities;

/// <summary>
/// A financial operation submitted to the engine. It starts in <see cref="TransactionStatus.Pending"/>
/// and transitions to <see cref="TransactionStatus.Processed"/> or <see cref="TransactionStatus.Failed"/>
/// once the accounting engine runs.
/// </summary>
public class FinancialTransaction
{
    private readonly List<AuditEvent> _auditEvents = new();

    // EF Core constructor.
    private FinancialTransaction() { }

    private FinancialTransaction(
        TransactionType type,
        string currency,
        decimal amount,
        string branchCode,
        string costCenter,
        string description)
    {
        Id = Guid.NewGuid();
        Type = type;
        Currency = currency;
        Amount = amount;
        BranchCode = branchCode;
        CostCenter = costCenter;
        Description = description;
        Status = TransactionStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }
    public TransactionType Type { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public decimal Amount { get; private set; }
    public string BranchCode { get; private set; } = string.Empty;
    public string CostCenter { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public TransactionStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ProcessedAt { get; private set; }

    public AccountingEntry? AccountingEntry { get; private set; }
    public IReadOnlyCollection<AuditEvent> AuditEvents => _auditEvents.AsReadOnly();

    /// <summary>
    /// Creates a new pending transaction. Invariants that depend only on the transaction
    /// itself (positive amount, supported currency) are enforced here so an invalid
    /// transaction can never be constructed.
    /// </summary>
    public static Result<FinancialTransaction> Create(
        TransactionType type,
        string currency,
        decimal amount,
        string branchCode,
        string costCenter,
        string description)
    {
        if (amount <= 0)
            return Result.Failure<FinancialTransaction>(Error.Validation("Amount must be greater than zero."));

        if (!SupportedCurrencies.IsSupported(currency))
            return Result.Failure<FinancialTransaction>(
                Error.Validation($"Currency '{currency}' is not supported."));

        if (string.IsNullOrWhiteSpace(branchCode))
            return Result.Failure<FinancialTransaction>(Error.Validation("Branch code is required."));

        var transaction = new FinancialTransaction(
            type,
            currency.ToUpperInvariant(),
            amount,
            branchCode.Trim(),
            string.IsNullOrWhiteSpace(costCenter) ? string.Empty : costCenter.Trim(),
            description?.Trim() ?? string.Empty);

        return Result.Success(transaction);
    }

    /// <summary>Marks the transaction as being processed. Only valid from <see cref="TransactionStatus.Pending"/>.</summary>
    public Result MarkProcessing()
    {
        if (Status != TransactionStatus.Pending)
            return Result.Failure(Error.Conflict(
                $"Only pending transactions can be processed. Current status: {Status}."));

        Status = TransactionStatus.Processing;
        return Result.Success();
    }

    /// <summary>Attaches a balanced accounting entry and marks the transaction processed.</summary>
    public void MarkProcessed(AccountingEntry entry)
    {
        AccountingEntry = entry;
        Status = TransactionStatus.Processed;
        ProcessedAt = DateTime.UtcNow;
    }

    /// <summary>Marks the transaction as failed (e.g. no rule found or unbalanced entry).</summary>
    public void MarkFailed()
    {
        Status = TransactionStatus.Failed;
        ProcessedAt = DateTime.UtcNow;
    }

    /// <summary>Appends an audit event capturing a step of the processing lifecycle.</summary>
    public AuditEvent RecordAudit(
        AuditEventType eventType,
        string message,
        TransactionStatus? previousStatus,
        TransactionStatus? newStatus)
    {
        var auditEvent = AuditEvent.Create(Id, eventType, message, previousStatus, newStatus);
        _auditEvents.Add(auditEvent);
        return auditEvent;
    }
}
