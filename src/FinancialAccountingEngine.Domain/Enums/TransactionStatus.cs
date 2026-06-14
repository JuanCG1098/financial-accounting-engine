namespace FinancialAccountingEngine.Domain.Enums;

/// <summary>Lifecycle state of a <c>FinancialTransaction</c>.</summary>
public enum TransactionStatus
{
    /// <summary>Created but not yet processed by the accounting engine.</summary>
    Pending,

    /// <summary>Processing is in progress (transient state during the engine run).</summary>
    Processing,

    /// <summary>Successfully processed; a balanced accounting entry was generated.</summary>
    Processed,

    /// <summary>Processing failed (no active rule, unbalanced entry, etc.).</summary>
    Failed
}
