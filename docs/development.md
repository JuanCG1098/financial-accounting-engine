# Development Guide

How to set up, run, test and troubleshoot the project.

## Prerequisites

| Tool | Version | Needed for |
| --- | --- | --- |
| .NET SDK | 10.x | Build/run/test the backend |
| Docker Desktop | recent | Run the full stack (Postgres + API) |
| Flutter SDK | 3.44+ (Dart 3.12+) | Build/run the web frontend |
| Chrome | any recent | `flutter run -d chrome` |
| Git | any recent | Version control |

Verify:

```bash
dotnet --version
docker version
flutter --version
```

## Running the stack with Docker (recommended)

From the repository root:

```bash
docker compose up --build      # build image, start Postgres + API
```

- API + Swagger UI: <http://localhost:8080>
- PostgreSQL: `localhost:5432` (`postgres` / `postgres`, db `financial_accounting`)

The API applies EF Core migrations and seeds fictional data on startup (idempotent).

Lifecycle:

```bash
docker compose stop     # pause containers (keeps data + volume)
docker compose start    # resume
docker compose down     # remove containers (add -v to also drop the DB volume)
```

## Running the backend manually

Requires a reachable PostgreSQL. Configure the connection string via the
`ConnectionStrings__Default` environment variable (or edit
`src/FinancialAccountingEngine.Api/appsettings.json`):

```bash
# bash
export ConnectionStrings__Default="Host=localhost;Port=5432;Database=financial_accounting;Username=postgres;Password=postgres"
dotnet run --project src/FinancialAccountingEngine.Api
```

```powershell
# PowerShell
$env:ConnectionStrings__Default = "Host=localhost;Port=5432;Database=financial_accounting;Username=postgres;Password=postgres"
dotnet run --project src/FinancialAccountingEngine.Api
```

## Running the frontend

```bash
cd frontend/financial_accounting_engine_app
flutter pub get
flutter run -d chrome --dart-define=API_BASE_URL=http://localhost:8080
```

Build a static bundle for hosting:

```bash
flutter build web --dart-define=API_BASE_URL=https://your-api-host
```

The API enables permissive CORS, so the web app can call it from a different origin in development.

## Tests

```bash
dotnet test FinancialAccountingEngine.slnx        # all tests
dotnet test tests/FinancialAccountingEngine.UnitTests
dotnet test tests/FinancialAccountingEngine.IntegrationTests

# Frontend static analysis + build check
cd frontend/financial_accounting_engine_app
flutter analyze
flutter build web
```

- **Unit tests** wire the real services over an EF Core **InMemory** database (`TestHarness`).
- **Integration tests** boot the API in-process with `WebApplicationFactory`, swapping PostgreSQL
  for InMemory; the startup seeder still runs, so each test server starts with data.

## EF Core migrations

```bash
dotnet ef migrations add <Name> \
  --project src/FinancialAccountingEngine.Infrastructure \
  --startup-project src/FinancialAccountingEngine.Api \
  --output-dir Persistence/Migrations
```

A design-time factory (`DesignTimeDbContextFactory`) lets the EF tools build the context without
booting the full host (so the startup seeder does not run during scaffolding).

## Troubleshooting

**`dotnet` can't find the solution** — pass the `.slnx` explicitly:
`dotnet build FinancialAccountingEngine.slnx`.

**Docker error: "cannot connect to the Docker daemon / pipe"** — Docker Desktop is not running.
Start Docker Desktop and wait until it reports *running*, then retry.

**`flutter` not recognized** — the SDK's `bin` directory is not on `PATH`. Add `<flutter>/bin` to
`PATH` (new terminal required), or invoke Flutter by its full path.

**Flutter commands loop with "Building flutter tool... / The system cannot find the path
specified"** — the SDK checkout is incomplete (missing tracked files such as `packages/`). If the
SDK is a git clone, restore it offline from the local objects:
`git -C <flutter-dir> checkout -- .` then re-run `flutter --version`.

**Frontend can't reach the API** — confirm the backend is up at the URL passed via
`--dart-define=API_BASE_URL`, and that CORS is enabled (it is, by default).
