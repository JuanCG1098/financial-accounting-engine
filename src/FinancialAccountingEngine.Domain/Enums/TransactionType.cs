namespace FinancialAccountingEngine.Domain.Enums;

/// <summary>
/// The kind of financial operation being recorded. Serialized as SCREAMING_SNAKE_CASE
/// over the API (e.g. <c>CASH_DEPOSIT</c>).
/// </summary>
public enum TransactionType
{
    /// <summary>Cash entering a branch, increasing branch cash and customer liability.</summary>
    CashDeposit,

    /// <summary>Cash leaving a branch, decreasing branch cash and customer liability.</summary>
    CashWithdrawal,

    /// <summary>Movement of funds between internal accounts through a clearing account.</summary>
    InternalTransfer,

    /// <summary>A correction posted against an adjustment account.</summary>
    AccountingAdjustment,

    /// <summary>An entry that affects the ledger only, with no associated cash flow.</summary>
    AccountingOnlyOperation
}
