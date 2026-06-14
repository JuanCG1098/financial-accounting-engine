# Architecture

The backend follows **Clean Architecture**: dependencies point inwards, the domain has no
infrastructure concerns, and outer layers depend on abstractions defined in inner layers.

```
┌─────────────────────────────────────────────────────────────┐
│                         Api (ASP.NET Core)                    │
│   Controllers · Middleware · Validation filter · Swagger      │
└───────────────┬───────────────────────────────┬──────────────┘
                │ depends on                     │ depends on
                ▼                                 ▼
┌───────────────────────────────┐   ┌──────────────────────────┐
│        Application             │   │      Infrastructure       │
│  Use cases / services          │   │  EF Core DbContext        │
│  DTOs · Validators             │◄──│  Repositories · Seeder    │
│  Persistence abstractions      │   │  (implements abstractions)│
└───────────────┬────────────────┘   └──────────────────────────┘
                │ depends on
                ▼
┌───────────────────────────────────────────────────────────────┐
│                            Domain                               │
│   Entities · Enums · Domain services · Result/Error · Invariants│
│                    (no external dependencies)                   │
└───────────────────────────────────────────────────────────────┘
```

## Projects

| Project | Responsibility |
| --- | --- |
| `FinancialAccountingEngine.Domain` | Entities (`FinancialTransaction`, `AccountingRule`, `AccountingEntry`, `AccountingEntryLine`, `AuditEvent`), enums, the `AccountingEntryGenerator` domain service, the `Result`/`Error` types and business invariants. No framework references. |
| `FinancialAccountingEngine.Application` | Use-case services (`TransactionService`, `AccountingEngine`, `AccountingRuleService`, `AccountingEntryService`, `AuditService`, `DashboardService`), DTOs, FluentValidation validators and the persistence abstractions (repository + unit-of-work interfaces). |
| `FinancialAccountingEngine.Infrastructure` | EF Core `ApplicationDbContext`, entity configurations, repository implementations, the unit of work, migrations and the database seeder. |
| `FinancialAccountingEngine.Api` | Controllers, exception-handling middleware, the FluentValidation action filter, Serilog wiring, Swagger/OpenAPI and dependency injection composition. |

## Key design decisions

- **Rich domain model.** Entities own their invariants. A `FinancialTransaction` can only be
  created through a factory that rejects non-positive amounts and unsupported currencies, and it
  exposes explicit state transitions (`MarkProcessing`, `MarkProcessed`, `MarkFailed`) instead of
  public setters.
- **The accounting engine is split in two.** The pure, side-effect-free entry generation lives in
  the domain (`AccountingEntryGenerator`); the orchestration (load → find rule → generate →
  validate balance → persist → audit) lives in the application layer (`AccountingEngine`). This
  keeps the double-entry logic unit-testable in isolation.
- **Result pattern over exceptions.** Expected business failures (not found, validation, conflict)
  are modelled with `Result`/`Error` and mapped to HTTP status codes in one place. Exceptions are
  reserved for the truly unexpected and handled by a middleware that returns `ProblemDetails`.
- **Repository + Unit of Work.** The application layer depends only on persistence interfaces; the
  EF Core implementation is an infrastructure detail. `ApplicationDbContext` itself implements
  `IUnitOfWork`.
- **Application-assigned identifiers.** Entity keys are GUIDs assigned in the domain and configured
  as `ValueGeneratedNever`, so aggregates can be fully constructed in memory (and audited) before
  they are persisted.
- **Observability.** Serilog provides structured logging and request logging; every processing step
  also writes a durable audit event to the database.

## Request lifecycle (process a transaction)

1. `POST /api/transactions/{id}/process` hits `TransactionsController`.
2. `AccountingEngine.ProcessAsync` loads the transaction through `ITransactionRepository`.
3. It finds an active `AccountingRule` for the transaction type and currency.
4. `AccountingEntryGenerator` builds a balanced debit/credit entry.
5. The engine validates the balance and transitions the transaction to `PROCESSED` or `FAILED`.
6. Audit events are appended throughout and everything is committed via `IUnitOfWork`.
