namespace FinancialAccountingEngine.Application.Abstractions.Persistence;

/// <summary>Commits pending changes tracked across the repositories as a single unit.</summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
