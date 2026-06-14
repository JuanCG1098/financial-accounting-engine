using System.Linq;
using FinancialAccountingEngine.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FinancialAccountingEngine.IntegrationTests;

/// <summary>
/// Boots the real API in-process but swaps the PostgreSQL context for an isolated EF Core
/// in-memory database, so integration tests run without external infrastructure. The seeder
/// configured in <c>Program.cs</c> still runs, so each test server starts with fictional data.
/// </summary>
public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"fae-integration-{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureTestServices(services =>
        {
            // Remove every registration tied to the application's PostgreSQL DbContext, including
            // the options-configuration service that carries the UseNpgsql() call.
            var toRemove = services.Where(d =>
                    d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>)
                    || d.ServiceType == typeof(DbContextOptions)
                    || d.ServiceType == typeof(ApplicationDbContext)
                    || (d.ServiceType.IsGenericType
                        && d.ServiceType.GetGenericTypeDefinition().Name == "IDbContextOptionsConfiguration`1"))
                .ToList();

            foreach (var descriptor in toRemove)
                services.Remove(descriptor);

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName));
        });
    }
}
