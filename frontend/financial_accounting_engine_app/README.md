# Financial Accounting Engine — Flutter Web Dashboard

A Flutter Web front-end for the Financial Accounting Engine API. It presents the domain in a
professional, sidebar-based financial dashboard:

- **Dashboard** — KPI cards (total / processed / failed / pending transactions, accounting
  entries), total processed amount by currency, recent transactions and recent audit events.
- **Transactions** — filter by status, currency and type; create new transactions; process pending
  ones; open a transaction detail view.
- **Transaction detail** — full transaction data, the generated accounting entry (with a clear
  *Balanced / Unbalanced* badge) and the audit trail as a timeline.
- **Accounting entries** — list of generated entries and a detail view with the debit/credit
  movements and totals.
- **Accounting rules** — list rules, create rules, activate/deactivate them, and see at a glance
  whether each rule requires cash flow or is accounting-only.
- **Audit** — a table of audit events, optionally filtered by transaction id.

## Configuration

The API base URL is read from a Dart define and defaults to `http://localhost:8080`:

```dart
const String.fromEnvironment('API_BASE_URL', defaultValue: 'http://localhost:8080');
```

## Running

Requires the Flutter SDK with web support enabled and the API running (see the root README).

```bash
flutter pub get
flutter run -d chrome --dart-define=API_BASE_URL=http://localhost:8080
```

Build a static bundle for hosting:

```bash
flutter build web --dart-define=API_BASE_URL=https://your-api-host
```

> CORS is enabled on the API for any origin, so the web app can call it directly during
> development.

## Structure

```
lib/
├── main.dart
└── src/
    ├── config/app_config.dart       # API base URL
    ├── core/                        # theme, formatters
    ├── models/models.dart           # API DTO models + domain option lists
    ├── services/api_service.dart    # typed REST client
    ├── widgets/                     # shell/sidebar, cards, tables, chips
    └── pages/                       # dashboard, transactions, entries, rules, audit
```

## Notes

- State management is intentionally lightweight (`StatefulWidget` + `FutureBuilder`) to keep the
  example readable; the only external dependencies are `http` and `intl`.
- Targets a recent Flutter stable channel (Material 3).
