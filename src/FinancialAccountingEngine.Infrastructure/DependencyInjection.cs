using FinancialAccountingEngine.Application.Abstractions.Persistence;
using FinancialAccountingEngine.Infrastructure.Persistence;
using FinancialAccountingEngine.Infrastructure.Persistence.Repositories;
using FinancialAccountingEngine.Infrastructure.Persistence.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FinancialAccountingEngine.Infrastructure;

public static class DependencyInjection
{
    /// <summary>Registers the EF Core database context, repositories, unit of work and seeder.</summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Connection string 'Default' is not configured.");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ApplicationDbContext>());

        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IAccountingRuleRepository, AccountingRuleRepository>();
        services.AddScoped<IAccountingEntryRepository, AccountingEntryRepository>();
        services.AddScoped<IAuditEventRepository, AuditEventRepository>();

        services.AddScoped<DatabaseSeeder>();

        return services;
    }
}
