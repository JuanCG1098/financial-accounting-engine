using FinancialAccountingEngine.Application.Audit;
using FinancialAccountingEngine.Application.Audit.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace FinancialAccountingEngine.Api.Controllers;

[ApiController]
[Route("api/audit")]
[Produces("application/json")]
public sealed class AuditController : ControllerBase
{
    private readonly IAuditService _audit;

    public AuditController(IAuditService audit) => _audit = audit;

    /// <summary>Lists all audit-trail events, most recent first.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<AuditEventDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
        => Ok(await _audit.ListAsync(cancellationToken));

    /// <summary>Lists audit-trail events for a single transaction, in chronological order.</summary>
    [HttpGet("by-transaction/{transactionId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<AuditEventDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListByTransaction(Guid transactionId, CancellationToken cancellationToken)
        => Ok(await _audit.ListByTransactionAsync(transactionId, cancellationToken));
}
