# Technical Decisions

A lightweight log of the notable design and implementation decisions, and the reasoning behind
them — including a few non-obvious problems that were solved along the way.

## 1. Clean Architecture with four layers

`Domain → Application → Infrastructure → Api`, dependencies pointing inwards. The domain is
framework-free; outer layers depend on abstractions defined inwards. This keeps the accounting
logic isolated and testable, and makes infrastructure (EF Core, PostgreSQL) a replaceable detail.

## 2. Result pattern instead of exceptions for expected failures

Expected outcomes — not found, validation failure, illegal state transition — are modelled with
`Result`/`Result<T>` and an `Error(code, message)`. The API maps error codes to HTTP status codes
in one place (`ResultExtensions`). Exceptions are reserved for genuinely unexpected faults, handled
by `ExceptionHandlingMiddleware` which returns `ProblemDetails`. This keeps control flow explicit
and avoids using exceptions for ordinary business decisions.

## 3. Rich domain model

Entities own their invariants and expose behavior, not setters. A `FinancialTransaction` can only
exist via `Create` (which rejects non-positive amounts and unsupported currencies) and transitions
through `MarkProcessing` / `MarkProcessed` / `MarkFailed`. This prevents invalid states regardless
of which caller (API, seeder, tests) constructs them.

## 4. Split engine: pure generation vs. orchestration

Double-entry generation is a pure, side-effect-free domain service
(`Domain/Services/AccountingEntryGenerator`). The workflow that loads data, finds a rule, validates
the balance, persists and writes the audit trail lives in `Application/Processing/AccountingEngine`.
This lets the core accounting math be unit-tested in isolation.

## 5. Repository + Unit of Work over EF Core

The application layer depends only on persistence interfaces; EF Core implements them in
Infrastructure. `ApplicationDbContext` implements `IUnitOfWork`, so a use case commits all changes
of an aggregate as one transaction. This was preferred over exposing `DbContext`/`IQueryable` to the
application, to keep the layering honest and the use cases testable.

## 6. Application-assigned GUID keys + `ValueGeneratedNever`  *(non-obvious)*

Entity ids are GUIDs assigned in the domain constructors. EF Core is configured with
`Property(x => x.Id).ValueGeneratedNever()` for every entity.

**Why:** when new child entities (e.g. `AuditEvent`s) are added to an already-tracked aggregate and
those children carry a store-generated key that already has a value, EF's change detector tags them
as **Modified** instead of **Added**, causing `DbUpdateConcurrencyException: Attempted to update or
delete an entity that does not exist`. Declaring the keys as application-assigned
(`ValueGeneratedNever`) makes EF treat freshly-discovered graph entities as inserts. This affects
the relational provider too, not just tests.

## 7. Dashboard aggregation computed in memory  *(non-obvious)*

`CountByStatusAsync` / `SumProcessedAmountByCurrencyAsync` pull a minimal projection from the
database (a filtered column set) and group/sum with LINQ-to-Objects, rather than translating
`GroupBy` + aggregate to SQL.

**Why:** the EF Core **InMemory** provider used by the unit/integration tests cannot translate
`GroupBy` followed by an aggregate projection (it throws "could not be translated"). Computing the
aggregation in memory keeps the same code working across providers, and the dashboard's data volume
makes this perfectly acceptable. (For a high-volume system this would move back to SQL-side
aggregation against a relational provider.)

## 8. Enums as `SCREAMING_SNAKE_CASE`

C# enums are PascalCase; over the wire and in the database they are strings such as `CASH_DEPOSIT`
/ `PROCESSED`. Achieved with `JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseUpper)` for JSON and
`HasConversion<string>()` for EF. String storage keeps the database and API payloads readable and
stable against enum reordering.

## 9. `NuGetAuditMode=direct`  *(non-obvious)*

`Directory.Build.props` sets `NuGetAuditMode=direct`. The EF Core design-time tooling pulls in a
transitive dependency that the NuGet audit flags, but it is a build-time-only tool dependency that
is never shipped at runtime. Auditing only **direct** dependencies keeps the build warning-free
while still surfacing vulnerabilities in packages the solution actually references and deploys.

## 10. Seeder runs through the real engine

On startup the seeder creates transactions and processes them via the actual `AccountingEngine`, so
the resulting entries and audit events are produced by the same code path as live traffic. It also
produces one genuinely `FAILED` transaction by temporarily deactivating a rule, processing a
withdrawal (which then finds no active rule), and reactivating the rule — exercising the failure
path and giving the dashboard realistic mixed data. The seeder is idempotent.

## 11. Testing strategy

- **Unit tests** exercise domain entities, the entry generator and the application services/engine,
  wiring the real repositories over an EF Core InMemory database (`TestHarness`).
- **Integration tests** use `WebApplicationFactory<Program>` to run the real HTTP pipeline,
  replacing the PostgreSQL registration with InMemory. Removing the provider requires deleting the
  `DbContextOptions`, the context, and the `IDbContextOptionsConfiguration<>` service that carries
  the `UseNpgsql` call — otherwise EF complains that two providers are registered.
