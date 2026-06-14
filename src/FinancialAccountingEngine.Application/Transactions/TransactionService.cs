using FinancialAccountingEngine.Application.Abstractions.Persistence;
using FinancialAccountingEngine.Application.Common.Mapping;
using FinancialAccountingEngine.Application.Transactions.Dtos;
using FinancialAccountingEngine.Domain.Common;
using FinancialAccountingEngine.Domain.Entities;
using FinancialAccountingEngine.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace FinancialAccountingEngine.Application.Transactions;

/// <summary>
/// Handles the creation and querying of transactions. Processing (entry generation and
/// balance validation) is delegated to <see cref="IAccountingEngine"/>.
/// </summary>
public sealed class TransactionService : ITransactionService
{
    private readonly ITransactionRepository _transactions;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TransactionService> _logger;

    public TransactionService(
        ITransactionRepository transactions,
        IUnitOfWork unitOfWork,
        ILogger<TransactionService> logger)
    {
        _transactions = transactions;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<TransactionDto>> CreateAsync(
        CreateTransactionRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = FinancialTransaction.Create(
            request.Type,
            request.Currency,
            request.Amount,
            request.BranchCode,
            request.CostCenter,
            request.Description);

        if (result.IsFailure)
        {
            _logger.LogWarning("Rejected transaction creation: {Error}", result.Error.Message);
            return Result.Failure<TransactionDto>(result.Error);
        }

        var transaction = result.Value;
        transaction.RecordAudit(
            AuditEventType.TransactionCreated,
            $"Transaction created with amount {transaction.Amount} {transaction.Currency}.",
            previousStatus: null,
            newStatus: TransactionStatus.Pending);

        await _transactions.AddAsync(transaction, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created transaction {TransactionId} ({Type}).", transaction.Id, transaction.Type);
        return Result.Success(transaction.ToDto());
    }

    public async Task<Result<TransactionDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var transaction = await _transactions.GetByIdAsync(id, cancellationToken);
        return transaction is null
            ? Result.Failure<TransactionDto>(Error.NotFound($"Transaction '{id}' was not found."))
            : Result.Success(transaction.ToDto());
    }

    public async Task<IReadOnlyList<TransactionDto>> ListAsync(
        TransactionQuery query,
        CancellationToken cancellationToken = default)
    {
        var transactions = await _transactions.ListAsync(query, cancellationToken);
        return transactions.Select(t => t.ToDto()).ToList();
    }
}
