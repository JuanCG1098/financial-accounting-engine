using FinancialAccountingEngine.Application.AccountingRules.Dtos;
using FinancialAccountingEngine.Domain.Common;
using FluentValidation;

namespace FinancialAccountingEngine.Application.AccountingRules.Validators;

public sealed class CreateAccountingRuleRequestValidator : AbstractValidator<CreateAccountingRuleRequest>
{
    public CreateAccountingRuleRequestValidator()
    {
        RuleFor(x => x.TransactionType).IsInEnum();
        RuleFor(x => x.CostCenterBehavior).IsInEnum();

        RuleFor(x => x.Currency)
            .Must(c => string.IsNullOrWhiteSpace(c) || SupportedCurrencies.IsSupported(c))
            .WithMessage(_ => $"Currency must be empty (any) or one of: {string.Join(", ", SupportedCurrencies.Codes)}.");

        RuleFor(x => x.DebitAccountCode).NotEmpty().MaximumLength(20);
        RuleFor(x => x.DebitAccountName).NotEmpty().MaximumLength(120);
        RuleFor(x => x.CreditAccountCode).NotEmpty().MaximumLength(20);
        RuleFor(x => x.CreditAccountName).NotEmpty().MaximumLength(120);

        RuleFor(x => x)
            .Must(x => x.DebitAccountCode != x.CreditAccountCode)
            .WithMessage("Debit and credit accounts must be different.");

        RuleFor(x => x)
            .Must(x => !(x.IsAccountingOnly && x.RequiresCashFlow))
            .WithMessage("An accounting-only rule cannot also require cash flow.");
    }
}

public sealed class UpdateAccountingRuleRequestValidator : AbstractValidator<UpdateAccountingRuleRequest>
{
    public UpdateAccountingRuleRequestValidator()
    {
        RuleFor(x => x.CostCenterBehavior).IsInEnum();

        RuleFor(x => x.Currency)
            .Must(c => string.IsNullOrWhiteSpace(c) || SupportedCurrencies.IsSupported(c))
            .WithMessage(_ => $"Currency must be empty (any) or one of: {string.Join(", ", SupportedCurrencies.Codes)}.");

        RuleFor(x => x.DebitAccountCode).NotEmpty().MaximumLength(20);
        RuleFor(x => x.DebitAccountName).NotEmpty().MaximumLength(120);
        RuleFor(x => x.CreditAccountCode).NotEmpty().MaximumLength(20);
        RuleFor(x => x.CreditAccountName).NotEmpty().MaximumLength(120);

        RuleFor(x => x)
            .Must(x => x.DebitAccountCode != x.CreditAccountCode)
            .WithMessage("Debit and credit accounts must be different.");

        RuleFor(x => x)
            .Must(x => !(x.IsAccountingOnly && x.RequiresCashFlow))
            .WithMessage("An accounting-only rule cannot also require cash flow.");
    }
}
