using FinancialAccountingEngine.Api.Common;
using FinancialAccountingEngine.Application.AccountingRules;
using FinancialAccountingEngine.Application.AccountingRules.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace FinancialAccountingEngine.Api.Controllers;

[ApiController]
[Route("api/accounting-rules")]
[Produces("application/json")]
public sealed class AccountingRulesController : ControllerBase
{
    private readonly IAccountingRuleService _rules;

    public AccountingRulesController(IAccountingRuleService rules) => _rules = rules;

    /// <summary>Lists all accounting rules.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<AccountingRuleDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
        => Ok(await _rules.ListAsync(cancellationToken));

    /// <summary>Creates a new accounting rule.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(AccountingRuleDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(CreateAccountingRuleRequest request, CancellationToken cancellationToken)
    {
        var result = await _rules.CreateAsync(request, cancellationToken);
        if (result.IsFailure)
            return this.ToProblem(result.Error);

        return CreatedAtAction(nameof(List), new { id = result.Value.Id }, result.Value);
    }

    /// <summary>Updates an existing accounting rule.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(AccountingRuleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, UpdateAccountingRuleRequest request, CancellationToken cancellationToken)
        => this.ToOkResult(await _rules.UpdateAsync(id, request, cancellationToken));

    /// <summary>Activates a rule so the engine can apply it.</summary>
    [HttpPatch("{id:guid}/activate")]
    [ProducesResponseType(typeof(AccountingRuleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken)
        => this.ToOkResult(await _rules.ActivateAsync(id, cancellationToken));

    /// <summary>Deactivates a rule so the engine ignores it.</summary>
    [HttpPatch("{id:guid}/deactivate")]
    [ProducesResponseType(typeof(AccountingRuleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
        => this.ToOkResult(await _rules.DeactivateAsync(id, cancellationToken));
}
