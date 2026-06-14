namespace FinancialAccountingEngine.Domain.Enums;

/// <summary>Types of events recorded in the audit trail during a transaction's lifecycle.</summary>
public enum AuditEventType
{
    TransactionCreated,
    ProcessingStarted,
    AccountingRuleApplied,
    EntryGenerated,
    BalanceValidated,
    ProcessingFailed,
    ProcessingCompleted
}
