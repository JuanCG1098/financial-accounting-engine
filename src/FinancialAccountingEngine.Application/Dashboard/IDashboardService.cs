using FinancialAccountingEngine.Application.Dashboard.Dtos;

namespace FinancialAccountingEngine.Application.Dashboard;

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default);
}
