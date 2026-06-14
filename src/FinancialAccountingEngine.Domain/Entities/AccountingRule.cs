using FinancialAccountingEngine.Domain.Enums;

namespace FinancialAccountingEngine.Domain.Entities;

/// <summary>
/// A configurable rule that tells the engine how to translate a transaction of a given
/// <see cref="TransactionType"/> (optionally scoped to a currency) into balanced
/// debit/credit movements.
/// </summary>
public class AccountingRule
{
    // EF Core constructor.
    private AccountingRule() { }

    public AccountingRule(
        TransactionType transactionType,
        string? currency,
        string debitAccountCode,
        string debitAccountName,
        string creditAccountCode,
        string creditAccountName,
        bool requiresCashFlow,
        bool isAccountingOnly,
        CostCenterBehavior costCenterBehavior,
        bool isActive = true)
    {
        Id = Guid.NewGuid();
        TransactionType = transactionType;
        Currency = string.IsNullOrWhiteSpace(currency) ? null : currency.ToUpperInvariant();
        DebitAccountCode = debitAccountCode;
        DebitAccountName = debitAccountName;
        CreditAccountCode = creditAccountCode;
        CreditAccountName = creditAccountName;
        RequiresCashFlow = requiresCashFlow;
        IsAccountingOnly = isAccountingOnly;
        CostCenterBehavior = costCenterBehavior;
        IsActive = isActive;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public Guid Id { get; private set; }
    public TransactionType TransactionType { get; private set; }

    /// <summary>When null the rule applies to any currency; otherwise it is currency-specific.</summary>
    public string? Currency { get; private set; }

    public string DebitAccountCode { get; private set; } = string.Empty;
    public string DebitAccountName { get; private set; } = string.Empty;
    public string CreditAccountCode { get; private set; } = string.Empty;
    public string CreditAccountName { get; private set; } = string.Empty;

    /// <summary>Whether processing this rule represents real money movement.</summary>
    public bool RequiresCashFlow { get; private set; }

    /// <summary>Whether the rule produces a ledger-only entry with no cash flow.</summary>
    public bool IsAccountingOnly { get; private set; }

    public CostCenterBehavior CostCenterBehavior { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    /// <summary>True when this rule can be applied to the given transaction type and currency.</summary>
    public bool Matches(TransactionType transactionType, string currency) =>
        IsActive
        && TransactionType == transactionType
        && (Currency is null || string.Equals(Currency, currency, StringComparison.OrdinalIgnoreCase));

    public void Update(
        string? currency,
        string debitAccountCode,
        string debitAccountName,
        string creditAccountCode,
        string creditAccountName,
        bool requiresCashFlow,
        bool isAccountingOnly,
        CostCenterBehavior costCenterBehavior)
    {
        Currency = string.IsNullOrWhiteSpace(currency) ? null : currency.ToUpperInvariant();
        DebitAccountCode = debitAccountCode;
        DebitAccountName = debitAccountName;
        CreditAccountCode = creditAccountCode;
        CreditAccountName = creditAccountName;
        RequiresCashFlow = requiresCashFlow;
        IsAccountingOnly = isAccountingOnly;
        CostCenterBehavior = costCenterBehavior;
        Touch();
    }

    public void Activate()
    {
        IsActive = true;
        Touch();
    }

    public void Deactivate()
    {
        IsActive = false;
        Touch();
    }

    private void Touch() => UpdatedAt = DateTime.UtcNow;
}
