using FinancialAccountingEngine.Application.AccountingEntries;
using FinancialAccountingEngine.Application.AccountingRules;
using FinancialAccountingEngine.Application.Audit;
using FinancialAccountingEngine.Application.Dashboard;
using FinancialAccountingEngine.Application.Processing;
using FinancialAccountingEngine.Application.Transactions;
using FinancialAccountingEngine.Domain.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace FinancialAccountingEngine.Application;

public static class DependencyInjection
{
    /// <summary>Registers application services, the domain engine and FluentValidation validators.</summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ITransactionService, TransactionService>();
        services.AddScoped<IAccountingEngine, AccountingEngine>();
        services.AddScoped<IAccountingEntryService, AccountingEntryService>();
        services.AddScoped<IAccountingRuleService, AccountingRuleService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IDashboardService, DashboardService>();

        // The accounting entry generator is a stateless domain service.
        services.AddSingleton<IAccountingEntryGenerator, AccountingEntryGenerator>();

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly, ServiceLifetime.Scoped);

        return services;
    }
}
