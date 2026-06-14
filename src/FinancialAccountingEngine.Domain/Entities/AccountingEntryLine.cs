namespace FinancialAccountingEngine.Domain.Entities;

/// <summary>
/// A single movement within an <see cref="AccountingEntry"/>. Exactly one of
/// <see cref="Debit"/> / <see cref="Credit"/> is expected to be non-zero.
/// </summary>
public class AccountingEntryLine
{
    // EF Core constructor.
    private AccountingEntryLine() { }

    internal AccountingEntryLine(
        string accountCode,
        string accountName,
        decimal debit,
        decimal credit,
        string? costCenter,
        string description)
    {
        Id = Guid.NewGuid();
        AccountCode = accountCode;
        AccountName = accountName;
        Debit = debit;
        Credit = credit;
        CostCenter = costCenter;
        Description = description;
    }

    public Guid Id { get; private set; }
    public Guid AccountingEntryId { get; private set; }
    public string AccountCode { get; private set; } = string.Empty;
    public string AccountName { get; private set; } = string.Empty;
    public decimal Debit { get; private set; }
    public decimal Credit { get; private set; }
    public string? CostCenter { get; private set; }
    public string Description { get; private set; } = string.Empty;
}
