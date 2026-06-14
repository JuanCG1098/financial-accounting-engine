using FinancialAccountingEngine.Application.Transactions.Dtos;
using FinancialAccountingEngine.Domain.Common;
using FluentValidation;

namespace FinancialAccountingEngine.Application.Transactions.Validators;

public sealed class CreateTransactionRequestValidator : AbstractValidator<CreateTransactionRequest>
{
    public CreateTransactionRequestValidator()
    {
        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("A valid transaction type is required.");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required.")
            .Must(SupportedCurrencies.IsSupported)
            .WithMessage(_ => $"Currency must be one of: {string.Join(", ", SupportedCurrencies.Codes)}.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than zero.");

        RuleFor(x => x.BranchCode)
            .NotEmpty().WithMessage("Branch code is required.")
            .MaximumLength(20);

        RuleFor(x => x.CostCenter)
            .MaximumLength(20);

        RuleFor(x => x.Description)
            .MaximumLength(500);
    }
}
