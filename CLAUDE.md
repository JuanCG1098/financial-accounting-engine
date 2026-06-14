# CLAUDE.md

Context for AI assistants (and humans) working in this repository.

## What this is

**Financial Accounting Engine** — a fictional, fully anonymized double-entry accounting engine.
Financial transactions are processed against configurable accounting rules to produce balanced
debit/credit journal entries, with balance validation, an audit trail and a dashboard. Portfolio
project; no proprietary code, data or business rules from any company.

## Stack

- **Backend:** .NET 10, ASP.NET Core Web API (controllers), EF Core 10 + PostgreSQL (Npgsql),
  FluentValidation, Serilog, Swagger/OpenAPI (Swashbuckle).
- **Frontend:** Flutter Web (Material 3); deps limited to `http` + `intl`.
- **Infra:** Docker Compose (PostgreSQL + API).
- **Tests:** xUnit + FluentAssertions; EF Core InMemory; `WebApplicationFactory` for integration.

## Layout (Clean Architecture)

```
src/
  *.Domain          # entities, enums, domain services, Result/Error — no framework deps
  *.Application     # use-case services, DTOs, validators, persistence abstractions
  *.Infrastructure  # EF Core DbContext, configs, repositories, migrations, seeder
  *.Api             # controllers, middleware, validation filter, Swagger, DI
tests/
  *.UnitTests       # domain + engine + dashboard (InMemory harness)
  *.IntegrationTests# full HTTP pipeline via WebApplicationFactory
frontend/financial_accounting_engine_app/   # Flutter Web dashboard
docs/               # architecture, business-rules, api-examples, development, decisions
```

Dependencies point inwards: `Api → Application/Infrastructure → Domain`. See
[docs/architecture.md](docs/architecture.md).

## Common commands

The solution file is **`FinancialAccountingEngine.slnx`** (new XML solution format — pass it
explicitly to `dotnet`).

```bash
dotnet build FinancialAccountingEngine.slnx
dotnet test  FinancialAccountingEngine.slnx          # 28 tests (18 unit + 10 integration *)
docker compose up --build                            # Postgres + API, auto-migrate + seed
dotnet run --project src/FinancialAccountingEngine.Api   # needs ConnectionStrings__Default

# EF Core migrations
dotnet ef migrations add <Name> \
  --project src/FinancialAccountingEngine.Infrastructure \
  --startup-project src/FinancialAccountingEngine.Api \
  --output-dir Persistence/Migrations

# Frontend
cd frontend/financial_accounting_engine_app
flutter pub get && flutter analyze
flutter run -d chrome --dart-define=API_BASE_URL=http://localhost:8080
```

\* test count grows as scenarios are added — run `dotnet test` for the current number.

Full setup, run and troubleshooting: [docs/development.md](docs/development.md).

## Conventions (follow these when editing)

- **Rich domain model.** Entities enforce their own invariants; no public setters. Create/transition
  via factory methods (`FinancialTransaction.Create`, `MarkProcessing/Processed/Failed`).
- **Result pattern, not exceptions, for expected failures.** Application code returns
  `Result`/`Result<T>` with an `Error(code, message)`; the API maps codes to HTTP status
  (`not_found→404`, `conflict→409`, `validation→400`). Exceptions are for the unexpected only.
- **Repository + Unit of Work.** Application depends on persistence interfaces in
  `Application/Abstractions/Persistence`; EF Core implements them. `ApplicationDbContext` itself
  implements `IUnitOfWork`.
- **Application-assigned GUID keys**, configured `ValueGeneratedNever()` (see decisions doc — this
  matters for change tracking of aggregate children).
- **Enums** are PascalCase in C#, persisted and serialized as `SCREAMING_SNAKE_CASE`
  (`JsonNamingPolicy.SnakeCaseUpper` + `HasConversion<string>()`).
- **Validation** at the edge with FluentValidation (`ValidationFilter`); domain invariants are the
  last line of defence.
- **Feature-folder organization** in Application (Transactions/, AccountingRules/, ...).
- The accounting **engine is split**: pure entry generation in `Domain/Services`
  (`AccountingEntryGenerator`), orchestration in `Application/Processing` (`AccountingEngine`).

## Gotchas

- Reference the `.slnx` explicitly in `dotnet` commands.
- Dashboard aggregations are computed in memory (the EF InMemory provider used by tests can't
  translate `GroupBy` + aggregate); keep them provider-agnostic.
- The seeder runs through the **real** engine and intentionally produces one genuinely `FAILED`
  transaction by temporarily deactivating a rule. It is idempotent.
- Keep everything **fictional/anonymized** — no real institution names, accounts or rules.
