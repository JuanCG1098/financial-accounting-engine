using FinancialAccountingEngine.Application.Abstractions.Persistence;
using FinancialAccountingEngine.Application.Common.Mapping;
using FinancialAccountingEngine.Application.Transactions.Dtos;
using FinancialAccountingEngine.Domain.Common;
using FinancialAccountingEngine.Domain.Entities;
using FinancialAccountingEngine.Domain.Enums;
using FinancialAccountingEngine.Domain.Services;
using Microsoft.Extensions.Logging;

namespace FinancialAccountingEngine.Application.Processing;

/// <summary>
/// Implements the engine's processing workflow. Every meaningful step appends an audit event,
/// and the transaction always ends in either <see cref="TransactionStatus.Processed"/> or
/// <see cref="TransactionStatus.Failed"/>.
/// </summary>
public sealed class AccountingEngine : IAccountingEngine
{
    private readonly ITransactionRepository _transactions;
    private readonly IAccountingRuleRepository _rules;
    private readonly IAccountingEntryRepository _entries;
    private readonly IAccountingEntryGenerator _entryGenerator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AccountingEngine> _logger;

    public AccountingEngine(
        ITransactionRepository transactions,
        IAccountingRuleRepository rules,
        IAccountingEntryRepository entries,
        IAccountingEntryGenerator entryGenerator,
        IUnitOfWork unitOfWork,
        ILogger<AccountingEngine> logger)
    {
        _transactions = transactions;
        _rules = rules;
        _entries = entries;
        _entryGenerator = entryGenerator;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<TransactionDto>> ProcessAsync(
        Guid transactionId,
        CancellationToken cancellationToken = default)
    {
        var transaction = await _transactions.GetByIdAsync(transactionId, cancellationToken);
        if (transaction is null)
            return Result.Failure<TransactionDto>(Error.NotFound($"Transaction '{transactionId}' was not found."));

        var previousStatus = transaction.Status;
        var startResult = transaction.MarkProcessing();
        if (startResult.IsFailure)
            return Result.Failure<TransactionDto>(startResult.Error);

        transaction.RecordAudit(
            AuditEventType.ProcessingStarted,
            "Processing started.",
            previousStatus,
            TransactionStatus.Processing);

        // 1. Find an active accounting rule for this transaction type and currency.
        var rule = await _rules.FindActiveRuleAsync(transaction.Type, transaction.Currency, cancellationToken);
        if (rule is null)
        {
            return await FailAsync(
                transaction,
                $"No active accounting rule found for {transaction.Type} / {transaction.Currency}.",
                cancellationToken);
        }

        transaction.RecordAudit(
            AuditEventType.AccountingRuleApplied,
            $"Applied rule {rule.Id}: debit {rule.DebitAccountCode}, credit {rule.CreditAccountCode} " +
            $"(cash flow: {rule.RequiresCashFlow}, accounting-only: {rule.IsAccountingOnly}).",
            TransactionStatus.Processing,
            TransactionStatus.Processing);

        // 2. Generate the accounting entry.
        var entryNumber = await BuildEntryNumberAsync(cancellationToken);
        var entry = _entryGenerator.Generate(transaction, rule, entryNumber);

        transaction.RecordAudit(
            AuditEventType.EntryGenerated,
            $"Generated entry {entry.EntryNumber} with {entry.Lines.Count} movements.",
            TransactionStatus.Processing,
            TransactionStatus.Processing);

        // 3. Validate the balance (total debit must equal total credit).
        if (!entry.IsBalanced)
        {
            return await FailAsync(
                transaction,
                $"Generated entry is not balanced (debit {entry.TotalDebit} != credit {entry.TotalCredit}).",
                cancellationToken);
        }

        transaction.RecordAudit(
            AuditEventType.BalanceValidated,
            $"Balance validated: debit {entry.TotalDebit} == credit {entry.TotalCredit}.",
            TransactionStatus.Processing,
            TransactionStatus.Processing);

        // 4. Commit success.
        transaction.MarkProcessed(entry);
        transaction.RecordAudit(
            AuditEventType.ProcessingCompleted,
            "Processing completed successfully.",
            TransactionStatus.Processing,
            TransactionStatus.Processed);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Processed transaction {TransactionId} -> entry {EntryNumber}.", transaction.Id, entry.EntryNumber);

        return Result.Success(transaction.ToDto());
    }

    private async Task<Result<TransactionDto>> FailAsync(
        FinancialTransaction transaction,
        string reason,
        CancellationToken cancellationToken)
    {
        transaction.MarkFailed();
        transaction.RecordAudit(
            AuditEventType.ProcessingFailed,
            reason,
            TransactionStatus.Processing,
            TransactionStatus.Failed);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogWarning("Processing failed for transaction {TransactionId}: {Reason}", transaction.Id, reason);
        return Result.Failure<TransactionDto>(Error.Validation(reason));
    }

    /// <summary>Builds a human-readable, sequential-ish entry number such as <c>JE-20260613-000042</c>.</summary>
    private async Task<string> BuildEntryNumberAsync(CancellationToken cancellationToken)
    {
        var sequence = await _entries.CountAsync(cancellationToken) + 1;
        return $"JE-{DateTime.UtcNow:yyyyMMdd}-{sequence:D6}";
    }
}
