using FinancialAccountingEngine.Domain.Entities;
using FinancialAccountingEngine.Domain.Enums;

namespace FinancialAccountingEngine.Domain.Services;

/// <summary>
/// Default double-entry generator. For a transaction of amount <c>A</c> it produces two
/// movements: a debit of <c>A</c> against the rule's debit account and a credit of <c>A</c>
/// against the rule's credit account, so the entry is balanced by construction. The rule's
/// <see cref="CostCenterBehavior"/> decides how the transaction cost center flows onto the lines.
/// </summary>
public sealed class AccountingEntryGenerator : IAccountingEntryGenerator
{
    public AccountingEntry Generate(FinancialTransaction transaction, AccountingRule rule, string entryNumber)
    {
        ArgumentNullException.ThrowIfNull(transaction);
        ArgumentNullException.ThrowIfNull(rule);

        var entry = AccountingEntry.Create(transaction.Id, entryNumber, transaction.Currency);

        var (debitCostCenter, creditCostCenter) = ResolveCostCenters(rule.CostCenterBehavior, transaction.CostCenter);
        var flow = rule.RequiresCashFlow ? "cash-flow" : "accounting-only";
        var description = $"{transaction.Type} ({flow}) — {transaction.Description}".Trim();

        // Debit line.
        entry.AddLine(
            rule.DebitAccountCode,
            rule.DebitAccountName,
            debit: transaction.Amount,
            credit: 0m,
            costCenter: debitCostCenter,
            description: description);

        // Credit line.
        entry.AddLine(
            rule.CreditAccountCode,
            rule.CreditAccountName,
            debit: 0m,
            credit: transaction.Amount,
            costCenter: creditCostCenter,
            description: description);

        return entry;
    }

    private static (string? Debit, string? Credit) ResolveCostCenters(
        CostCenterBehavior behavior,
        string transactionCostCenter)
    {
        var costCenter = string.IsNullOrWhiteSpace(transactionCostCenter) ? null : transactionCostCenter;

        return behavior switch
        {
            CostCenterBehavior.Propagate => (costCenter, costCenter),
            CostCenterBehavior.DebitLineOnly => (costCenter, null),
            CostCenterBehavior.CreditLineOnly => (null, costCenter),
            _ => (null, null)
        };
    }
}
