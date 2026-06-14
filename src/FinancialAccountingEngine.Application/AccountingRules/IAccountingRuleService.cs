using FinancialAccountingEngine.Application.AccountingRules.Dtos;
using FinancialAccountingEngine.Domain.Common;

namespace FinancialAccountingEngine.Application.AccountingRules;

public interface IAccountingRuleService
{
    Task<IReadOnlyList<AccountingRuleDto>> ListAsync(CancellationToken cancellationToken = default);
    Task<Result<AccountingRuleDto>> CreateAsync(CreateAccountingRuleRequest request, CancellationToken cancellationToken = default);
    Task<Result<AccountingRuleDto>> UpdateAsync(Guid id, UpdateAccountingRuleRequest request, CancellationToken cancellationToken = default);
    Task<Result<AccountingRuleDto>> ActivateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<AccountingRuleDto>> DeactivateAsync(Guid id, CancellationToken cancellationToken = default);
}
