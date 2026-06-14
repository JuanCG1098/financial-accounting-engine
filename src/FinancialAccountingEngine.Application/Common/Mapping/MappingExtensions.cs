using FinancialAccountingEngine.Application.AccountingEntries.Dtos;
using FinancialAccountingEngine.Application.AccountingRules.Dtos;
using FinancialAccountingEngine.Application.Audit.Dtos;
using FinancialAccountingEngine.Application.Transactions.Dtos;
using FinancialAccountingEngine.Domain.Entities;

namespace FinancialAccountingEngine.Application.Common.Mapping;

/// <summary>Pure projection helpers from domain entities to API-facing DTOs.</summary>
public static class MappingExtensions
{
    public static TransactionDto ToDto(this FinancialTransaction t) => new(
        t.Id, t.Type, t.Currency, t.Amount, t.BranchCode, t.CostCenter,
        t.Description, t.Status, t.CreatedAt, t.ProcessedAt);

    public static AccountingEntryDto ToDto(this AccountingEntry e) => new(
        e.Id, e.TransactionId, e.EntryNumber, e.Currency, e.TotalDebit, e.TotalCredit,
        e.IsBalanced, e.CreatedAt,
        e.Lines.Select(l => l.ToDto()).ToList());

    public static AccountingEntryLineDto ToDto(this AccountingEntryLine l) => new(
        l.Id, l.AccountCode, l.AccountName, l.Debit, l.Credit, l.CostCenter, l.Description);

    public static AccountingRuleDto ToDto(this AccountingRule r) => new(
        r.Id, r.TransactionType, r.Currency, r.DebitAccountCode, r.DebitAccountName,
        r.CreditAccountCode, r.CreditAccountName, r.RequiresCashFlow, r.IsAccountingOnly,
        r.CostCenterBehavior, r.IsActive, r.CreatedAt, r.UpdatedAt);

    public static AuditEventDto ToDto(this AuditEvent a) => new(
        a.Id, a.TransactionId, a.EventType, a.Message, a.PreviousStatus, a.NewStatus, a.CreatedAt);
}
