namespace FinancialAccountingEngine.Domain.Enums;

/// <summary>
/// Controls how a transaction's cost center is propagated onto the generated
/// accounting entry lines when a rule is applied.
/// </summary>
public enum CostCenterBehavior
{
    /// <summary>The cost center is not copied onto any line.</summary>
    None,

    /// <summary>The cost center is copied onto both the debit and the credit line.</summary>
    Propagate,

    /// <summary>The cost center is copied onto the debit line only.</summary>
    DebitLineOnly,

    /// <summary>The cost center is copied onto the credit line only.</summary>
    CreditLineOnly
}
