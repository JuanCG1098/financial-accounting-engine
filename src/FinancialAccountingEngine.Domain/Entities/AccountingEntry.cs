namespace FinancialAccountingEngine.Domain.Entities;

/// <summary>
/// A double-entry accounting record generated for a processed transaction. It owns a
/// collection of <see cref="AccountingEntryLine"/> movements and keeps its debit/credit
/// totals and <see cref="IsBalanced"/> flag consistent with those lines.
/// </summary>
public class AccountingEntry
{
    private readonly List<AccountingEntryLine> _lines = new();

    // EF Core constructor.
    private AccountingEntry() { }

    private AccountingEntry(Guid transactionId, string entryNumber, string currency)
    {
        Id = Guid.NewGuid();
        TransactionId = transactionId;
        EntryNumber = entryNumber;
        Currency = currency;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid TransactionId { get; private set; }
    public string EntryNumber { get; private set; } = string.Empty;
    public string Currency { get; private set; } = string.Empty;
    public decimal TotalDebit { get; private set; }
    public decimal TotalCredit { get; private set; }
    public bool IsBalanced { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public IReadOnlyCollection<AccountingEntryLine> Lines => _lines.AsReadOnly();

    public static AccountingEntry Create(Guid transactionId, string entryNumber, string currency) =>
        new(transactionId, entryNumber, currency);

    /// <summary>Adds a movement and recomputes the entry totals and balance flag.</summary>
    public void AddLine(
        string accountCode,
        string accountName,
        decimal debit,
        decimal credit,
        string? costCenter,
        string description)
    {
        _lines.Add(new AccountingEntryLine(accountCode, accountName, debit, credit, costCenter, description));
        Recalculate();
    }

    private void Recalculate()
    {
        TotalDebit = _lines.Sum(l => l.Debit);
        TotalCredit = _lines.Sum(l => l.Credit);
        // Compare on rounded values to avoid spurious imbalance from decimal noise.
        IsBalanced = decimal.Round(TotalDebit, 2) == decimal.Round(TotalCredit, 2)
                     && TotalDebit > 0;
    }
}
