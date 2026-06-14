using FinancialAccountingEngine.Application.Abstractions.Persistence;
using FinancialAccountingEngine.Application.AccountingEntries.Dtos;
using FinancialAccountingEngine.Application.Common.Mapping;
using FinancialAccountingEngine.Domain.Common;

namespace FinancialAccountingEngine.Application.AccountingEntries;

public sealed class AccountingEntryService : IAccountingEntryService
{
    private readonly IAccountingEntryRepository _entries;

    public AccountingEntryService(IAccountingEntryRepository entries) => _entries = entries;

    public async Task<IReadOnlyList<AccountingEntryDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        var entries = await _entries.ListAsync(cancellationToken);
        return entries.Select(e => e.ToDto()).ToList();
    }

    public async Task<Result<AccountingEntryDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entry = await _entries.GetByIdAsync(id, cancellationToken);
        return entry is null
            ? Result.Failure<AccountingEntryDto>(Error.NotFound($"Accounting entry '{id}' was not found."))
            : Result.Success(entry.ToDto());
    }

    public async Task<Result<AccountingEntryDto>> GetByTransactionAsync(
        Guid transactionId,
        CancellationToken cancellationToken = default)
    {
        var entry = await _entries.GetByTransactionIdAsync(transactionId, cancellationToken);
        return entry is null
            ? Result.Failure<AccountingEntryDto>(
                Error.NotFound($"No accounting entry found for transaction '{transactionId}'."))
            : Result.Success(entry.ToDto());
    }
}
