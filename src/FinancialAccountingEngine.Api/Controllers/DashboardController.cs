using FinancialAccountingEngine.Application.Dashboard;
using FinancialAccountingEngine.Application.Dashboard.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace FinancialAccountingEngine.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
[Produces("application/json")]
public sealed class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboard;

    public DashboardController(IDashboardService dashboard) => _dashboard = dashboard;

    /// <summary>Returns aggregated metrics for the dashboard landing screen.</summary>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(DashboardSummaryDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Summary(CancellationToken cancellationToken)
        => Ok(await _dashboard.GetSummaryAsync(cancellationToken));
}
