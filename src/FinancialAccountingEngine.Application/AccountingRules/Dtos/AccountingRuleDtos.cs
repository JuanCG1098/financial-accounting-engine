using FinancialAccountingEngine.Domain.Enums;

namespace FinancialAccountingEngine.Application.AccountingRules.Dtos;

/// <summary>Read model for a configurable accounting rule.</summary>
public sealed record AccountingRuleDto(
    Guid Id,
    TransactionType TransactionType,
    string? Currency,
    string DebitAccountCode,
    string DebitAccountName,
    string CreditAccountCode,
    string CreditAccountName,
    bool RequiresCashFlow,
    bool IsAccountingOnly,
    CostCenterBehavior CostCenterBehavior,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt);

/// <summary>Payload for creating an accounting rule.</summary>
public sealed record CreateAccountingRuleRequest(
    TransactionType TransactionType,
    string? Currency,
    string DebitAccountCode,
    string DebitAccountName,
    string CreditAccountCode,
    string CreditAccountName,
    bool RequiresCashFlow,
    bool IsAccountingOnly,
    CostCenterBehavior CostCenterBehavior);

/// <summary>Payload for updating an existing accounting rule.</summary>
public sealed record UpdateAccountingRuleRequest(
    string? Currency,
    string DebitAccountCode,
    string DebitAccountName,
    string CreditAccountCode,
    string CreditAccountName,
    bool RequiresCashFlow,
    bool IsAccountingOnly,
    CostCenterBehavior CostCenterBehavior);
