namespace FinancialAccountingEngine.Infrastructure.Persistence.Seed;

/// <summary>
/// A small, fictional chart of accounts used to seed the engine. Account codes and names
/// are illustrative and do not represent any real institution.
/// </summary>
internal static class ChartOfAccounts
{
    public const string BranchCashCode = "100001";
    public const string BranchCashName = "Branch Cash";

    public const string CustomerLiabilityCode = "200001";
    public const string CustomerLiabilityName = "Customer Liability";

    public const string InternalClearingCode = "300001";
    public const string InternalClearingName = "Internal Clearing Account";

    public const string AdjustmentCode = "400001";
    public const string AdjustmentName = "Accounting Adjustment Account";

    public const string OperationalExpenseCode = "500001";
    public const string OperationalExpenseName = "Operational Expense Account";
}
