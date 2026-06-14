using FinancialAccountingEngine.Api.Common;
using FinancialAccountingEngine.Application.AccountingEntries;
using FinancialAccountingEngine.Application.AccountingEntries.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace FinancialAccountingEngine.Api.Controllers;

[ApiController]
[Route("api/accounting-entries")]
[Produces("application/json")]
public sealed class AccountingEntriesController : ControllerBase
{
    private readonly IAccountingEntryService _entries;

    public AccountingEntriesController(IAccountingEntryService entries) => _entries = entries;

    /// <summary>Lists all generated accounting entries.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<AccountingEntryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
        => Ok(await _entries.ListAsync(cancellationToken));

    /// <summary>Gets a single accounting entry with its debit/credit movements.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AccountingEntryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        => this.ToOkResult(await _entries.GetByIdAsync(id, cancellationToken));

    /// <summary>Gets the accounting entry generated for a given transaction.</summary>
    [HttpGet("by-transaction/{transactionId:guid}")]
    [ProducesResponseType(typeof(AccountingEntryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByTransaction(Guid transactionId, CancellationToken cancellationToken)
        => this.ToOkResult(await _entries.GetByTransactionAsync(transactionId, cancellationToken));
}
