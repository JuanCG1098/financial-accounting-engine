using FinancialAccountingEngine.Domain.Enums;

namespace FinancialAccountingEngine.Application.Transactions.Dtos;

/// <summary>Payload for creating a new financial transaction.</summary>
public sealed record CreateTransactionRequest(
    TransactionType Type,
    string Currency,
    decimal Amount,
    string BranchCode,
    string CostCenter,
    string Description);

/// <summary>Read model returned for a financial transaction.</summary>
public sealed record TransactionDto(
    Guid Id,
    TransactionType Type,
    string Currency,
    decimal Amount,
    string BranchCode,
    string CostCenter,
    string Description,
    TransactionStatus Status,
    DateTime CreatedAt,
    DateTime? ProcessedAt);

/// <summary>Optional filters for listing transactions.</summary>
public sealed record TransactionQuery(
    TransactionStatus? Status = null,
    string? Currency = null,
    TransactionType? Type = null);
