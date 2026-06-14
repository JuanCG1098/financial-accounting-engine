using FinancialAccountingEngine.Domain.Entities;

namespace FinancialAccountingEngine.Domain.Services;

/// <summary>
/// Translates a transaction into a balanced accounting entry by applying an accounting rule.
/// This is the pure, side-effect-free core of the accounting engine.
/// </summary>
public interface IAccountingEntryGenerator
{
    AccountingEntry Generate(FinancialTransaction transaction, AccountingRule rule, string entryNumber);
}
