using FinancialAccountingEngine.Api.Common;
using FinancialAccountingEngine.Application.Processing;
using FinancialAccountingEngine.Application.Transactions;
using FinancialAccountingEngine.Application.Transactions.Dtos;
using FinancialAccountingEngine.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace FinancialAccountingEngine.Api.Controllers;

[ApiController]
[Route("api/transactions")]
[Produces("application/json")]
public sealed class TransactionsController : ControllerBase
{
    private readonly ITransactionService _transactions;
    private readonly IAccountingEngine _engine;

    public TransactionsController(ITransactionService transactions, IAccountingEngine engine)
    {
        _transactions = transactions;
        _engine = engine;
    }

    /// <summary>Creates a new financial transaction in PENDING status.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(TransactionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(CreateTransactionRequest request, CancellationToken cancellationToken)
    {
        var result = await _transactions.CreateAsync(request, cancellationToken);
        if (result.IsFailure)
            return this.ToProblem(result.Error);

        return CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value);
    }

    /// <summary>Lists transactions, optionally filtered by status, currency and type.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TransactionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        [FromQuery] TransactionStatus? status,
        [FromQuery] string? currency,
        [FromQuery] TransactionType? type,
        CancellationToken cancellationToken)
    {
        var transactions = await _transactions.ListAsync(new TransactionQuery(status, currency, type), cancellationToken);
        return Ok(transactions);
    }

    /// <summary>Gets a single transaction by its identifier.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TransactionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        => this.ToOkResult(await _transactions.GetByIdAsync(id, cancellationToken));

    /// <summary>Runs the accounting engine against a pending transaction.</summary>
    [HttpPost("{id:guid}/process")]
    [ProducesResponseType(typeof(TransactionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Process(Guid id, CancellationToken cancellationToken)
        => this.ToOkResult(await _engine.ProcessAsync(id, cancellationToken));
}
