namespace FinancialAccountingEngine.Application.AccountingEntries.Dtos;

/// <summary>Read model for an accounting entry, including its debit/credit movements.</summary>
public sealed record AccountingEntryDto(
    Guid Id,
    Guid TransactionId,
    string EntryNumber,
    string Currency,
    decimal TotalDebit,
    decimal TotalCredit,
    bool IsBalanced,
    DateTime CreatedAt,
    IReadOnlyList<AccountingEntryLineDto> Lines);

/// <summary>Read model for a single movement of an accounting entry.</summary>
public sealed record AccountingEntryLineDto(
    Guid Id,
    string AccountCode,
    string AccountName,
    decimal Debit,
    decimal Credit,
    string? CostCenter,
    string Description);
