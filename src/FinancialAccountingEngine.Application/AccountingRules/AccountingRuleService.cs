using FinancialAccountingEngine.Application.Abstractions.Persistence;
using FinancialAccountingEngine.Application.AccountingRules.Dtos;
using FinancialAccountingEngine.Application.Common.Mapping;
using FinancialAccountingEngine.Domain.Common;
using FinancialAccountingEngine.Domain.Entities;

namespace FinancialAccountingEngine.Application.AccountingRules;

public sealed class AccountingRuleService : IAccountingRuleService
{
    private readonly IAccountingRuleRepository _rules;
    private readonly IUnitOfWork _unitOfWork;

    public AccountingRuleService(IAccountingRuleRepository rules, IUnitOfWork unitOfWork)
    {
        _rules = rules;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<AccountingRuleDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        var rules = await _rules.ListAsync(cancellationToken);
        return rules.Select(r => r.ToDto()).ToList();
    }

    public async Task<Result<AccountingRuleDto>> CreateAsync(
        CreateAccountingRuleRequest request,
        CancellationToken cancellationToken = default)
    {
        var rule = new AccountingRule(
            request.TransactionType,
            request.Currency,
            request.DebitAccountCode,
            request.DebitAccountName,
            request.CreditAccountCode,
            request.CreditAccountName,
            request.RequiresCashFlow,
            request.IsAccountingOnly,
            request.CostCenterBehavior);

        await _rules.AddAsync(rule, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(rule.ToDto());
    }

    public async Task<Result<AccountingRuleDto>> UpdateAsync(
        Guid id,
        UpdateAccountingRuleRequest request,
        CancellationToken cancellationToken = default)
    {
        var rule = await _rules.GetByIdAsync(id, cancellationToken);
        if (rule is null)
            return Result.Failure<AccountingRuleDto>(Error.NotFound($"Accounting rule '{id}' was not found."));

        rule.Update(
            request.Currency,
            request.DebitAccountCode,
            request.DebitAccountName,
            request.CreditAccountCode,
            request.CreditAccountName,
            request.RequiresCashFlow,
            request.IsAccountingOnly,
            request.CostCenterBehavior);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(rule.ToDto());
    }

    public async Task<Result<AccountingRuleDto>> ActivateAsync(Guid id, CancellationToken cancellationToken = default)
        => await SetActiveAsync(id, activate: true, cancellationToken);

    public async Task<Result<AccountingRuleDto>> DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
        => await SetActiveAsync(id, activate: false, cancellationToken);

    private async Task<Result<AccountingRuleDto>> SetActiveAsync(
        Guid id,
        bool activate,
        CancellationToken cancellationToken)
    {
        var rule = await _rules.GetByIdAsync(id, cancellationToken);
        if (rule is null)
            return Result.Failure<AccountingRuleDto>(Error.NotFound($"Accounting rule '{id}' was not found."));

        if (activate)
            rule.Activate();
        else
            rule.Deactivate();

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(rule.ToDto());
    }
}
