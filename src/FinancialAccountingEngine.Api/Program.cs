using System.Text.Json.Serialization;
using FinancialAccountingEngine.Api.Common;
using FinancialAccountingEngine.Api.Middleware;
using FinancialAccountingEngine.Application;
using FinancialAccountingEngine.Infrastructure;
using FinancialAccountingEngine.Infrastructure.Persistence;
using FinancialAccountingEngine.Infrastructure.Persistence.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Structured logging with Serilog, configured from appsettings.
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext());

const string CorsPolicy = "FrontendCors";
builder.Services.AddCors(options => options.AddPolicy(CorsPolicy, policy =>
    policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

builder.Services.AddControllers(options => options.Filters.Add<ValidationFilter>())
    .AddJsonOptions(options =>
    {
        // Serialize enums as SCREAMING_SNAKE_CASE (e.g. CASH_DEPOSIT, PROCESSED).
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter(System.Text.Json.JsonNamingPolicy.SnakeCaseUpper));
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Financial Accounting Engine API",
        Version = "v1",
        Description = "A fictional, fully anonymized double-entry accounting engine for portfolio purposes."
    });

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

await InitializeDatabaseAsync(app);

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseSerilogRequestLogging();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Financial Accounting Engine API v1");
    options.RoutePrefix = string.Empty; // Serve Swagger UI at the application root.
});

app.UseCors(CorsPolicy);
app.MapControllers();

app.Run();

// Applies migrations (or creates the schema) and seeds fictional data on startup.
static async Task InitializeDatabaseAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;

    var context = services.GetRequiredService<ApplicationDbContext>();
    if (context.Database.IsRelational())
        await context.Database.MigrateAsync();

    var seeder = services.GetRequiredService<DatabaseSeeder>();
    await seeder.SeedAsync();
}

/// <summary>Exposed so the integration test host (WebApplicationFactory) can reference the entry point.</summary>
public partial class Program;
