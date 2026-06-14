# Project Log — Build & Verification

A record of how this project was built, verified end-to-end, and published. Complements the
design docs ([architecture](architecture.md), [decisions](decisions.md)) with the *what was done
and proven* view.

## Scope delivered

A complete, fictional, fully anonymized full-stack **double-entry accounting engine**:

- **Backend (.NET 10, Clean Architecture)** — `Domain / Application / Infrastructure / Api`.
  - Rich domain: `FinancialTransaction` (state machine), `AccountingRule`, `AccountingEntry` +
    `AccountingEntryLine`, `AuditEvent`; pure `AccountingEntryGenerator` domain service.
  - Application: transaction/rule/entry/audit/dashboard services, the orchestrating
    `AccountingEngine`, DTOs, FluentValidation validators, persistence abstractions.
  - Infrastructure: EF Core 10 + PostgreSQL (Npgsql), entity configurations, repositories,
    `InitialCreate` migration, idempotent seeder.
  - Api: controllers, `Result → ProblemDetails` mapping, validation filter, exception middleware,
    Serilog, Swagger.
- **Frontend** — Flutter Web dashboard (dashboard, transactions, entries, rules, audit) consuming
  the API.
- **Infrastructure** — Docker Compose (PostgreSQL + API) with automatic migration and seeding.
- **Tests** — xUnit + FluentAssertions; unit tests over an EF Core InMemory harness, integration
  tests over the real HTTP pipeline via `WebApplicationFactory`.
- **Docs** — README (incl. *Recruiter Notes* + disclaimer), architecture, business rules, API
  examples, development guide, decisions, `CLAUDE.md`.

## Verification performed

**Backend build & tests** — solution builds clean (0 warnings); the full xUnit suite passes
(unit + integration). Run `dotnet test FinancialAccountingEngine.slnx` for the current count.

**Frontend** — `flutter analyze` reports **No issues found!** and `flutter build web` compiles
successfully (Flutter 3.44.2 / Dart 3.12.2).

**Full stack end-to-end (Docker + PostgreSQL)** — `docker compose up --build` brought up Postgres
(healthy) and the API, which migrated and seeded on startup. `GET /api/dashboard/summary` returned
the seeded data: **9 transactions** (6 `PROCESSED`, 1 `FAILED`, 2 `PENDING`), totals by currency,
recent transactions and a full audit trail.

**Live engine smoke test (over HTTP, against the running container):**

| Step | Result |
| --- | --- |
| `POST /api/transactions` (USD 2500.50) | `PENDING` |
| `POST /api/transactions/{id}/process` | `PROCESSED` |
| `GET /api/accounting-entries/by-transaction/{id}` | Entry `JE-...`, **balanced**, debit 2500.50 == credit 2500.50, 2 movements (Branch Cash / Customer Liability), cost center propagated |
| `GET /api/audit/by-transaction/{id}` | Full lifecycle: `TRANSACTION_CREATED → PROCESSING_STARTED → ACCOUNTING_RULE_APPLIED → ENTRY_GENERATED → BALANCE_VALIDATED → PROCESSING_COMPLETED` |
| `POST /api/transactions` (amount = 0) | Rejected with **HTTP 400** |

The seeder also produces one genuinely `FAILED` transaction (by temporarily deactivating a rule)
to exercise the failure path, with audit message *"No active accounting rule found for
CashWithdrawal / ARS."*

## Notable problems solved

- **EF Core change tracking of aggregate children** — new `AuditEvent`s were detected as *Modified*
  instead of *Added*. Fixed by declaring all keys application-assigned (`ValueGeneratedNever()`);
  migration regenerated. See [decisions.md](decisions.md) §6.
- **InMemory provider `GroupBy` limitation** — dashboard aggregations moved to in-memory grouping
  after a minimal DB projection. See [decisions.md](decisions.md) §7.
- **Duplicate EF providers in integration tests** — removed the `DbContextOptions`, the context and
  the `IDbContextOptionsConfiguration<>` registration before adding InMemory. See
  [decisions.md](decisions.md) §11.
- **NuGet audit on a design-time-only transitive dependency** — `NuGetAuditMode=direct` in
  `Directory.Build.props`. See [decisions.md](decisions.md) §9.

## Publish

Initialized as a git repository (`main`), `.gitattributes` added for line-ending normalization,
build artifacts excluded. Published public at
**https://github.com/JuanCG1098/financial-accounting-engine**.

## Status

Functionally complete and verified end-to-end; published. Optional follow-ups: real dashboard
screenshots in the README, and a `LICENSE` file.
