# API Examples

Base URL (Docker): `http://localhost:8080`. Interactive docs (Swagger UI) are served at the root
(`/`). Enums are serialized as `SCREAMING_SNAKE_CASE`.

> The examples use `curl`. On Windows PowerShell, replace line continuations (`\`) with backticks
> (`` ` ``) or put the command on a single line.

## Transactions

### Create a transaction

```bash
curl -X POST http://localhost:8080/api/transactions \
  -H "Content-Type: application/json" \
  -d '{
    "type": "CASH_DEPOSIT",
    "currency": "USD",
    "amount": 1500.00,
    "branchCode": "BR-001",
    "costCenter": "CC-001",
    "description": "Branch cash deposit"
  }'
```

Response `201 Created`:

```json
{
  "id": "0d8c1c6e-2a4b-4f9e-9b1a-6f0d2b7c9a11",
  "type": "CASH_DEPOSIT",
  "currency": "USD",
  "amount": 1500.00,
  "branchCode": "BR-001",
  "costCenter": "CC-001",
  "description": "Branch cash deposit",
  "status": "PENDING",
  "createdAt": "2026-06-13T12:00:00Z",
  "processedAt": null
}
```

### List / filter transactions

```bash
curl "http://localhost:8080/api/transactions?status=PROCESSED&currency=USD&type=CASH_DEPOSIT"
```

### Get a transaction by id

```bash
curl http://localhost:8080/api/transactions/{id}
```

### Process a pending transaction

```bash
curl -X POST http://localhost:8080/api/transactions/{id}/process
```

Returns the transaction with `status` set to `PROCESSED` (or `FAILED`).

## Accounting entries

```bash
curl http://localhost:8080/api/accounting-entries
curl http://localhost:8080/api/accounting-entries/{id}
curl http://localhost:8080/api/accounting-entries/by-transaction/{transactionId}
```

Example entry:

```json
{
  "id": "…",
  "transactionId": "…",
  "entryNumber": "JE-20260613-000007",
  "currency": "USD",
  "totalDebit": 1500.00,
  "totalCredit": 1500.00,
  "isBalanced": true,
  "createdAt": "2026-06-13T12:00:05Z",
  "lines": [
    { "accountCode": "100001", "accountName": "Branch Cash",        "debit": 1500.00, "credit": 0.00,    "costCenter": "CC-001", "description": "CashDeposit (cash-flow) — Branch cash deposit" },
    { "accountCode": "200001", "accountName": "Customer Liability",  "debit": 0.00,    "credit": 1500.00, "costCenter": "CC-001", "description": "CashDeposit (cash-flow) — Branch cash deposit" }
  ]
}
```

## Accounting rules

```bash
# List
curl http://localhost:8080/api/accounting-rules

# Create
curl -X POST http://localhost:8080/api/accounting-rules \
  -H "Content-Type: application/json" \
  -d '{
    "transactionType": "ACCOUNTING_ONLY_OPERATION",
    "currency": null,
    "debitAccountCode": "500001",
    "debitAccountName": "Operational Expense Account",
    "creditAccountCode": "400001",
    "creditAccountName": "Accounting Adjustment Account",
    "requiresCashFlow": false,
    "isAccountingOnly": true,
    "costCenterBehavior": "NONE"
  }'

# Update
curl -X PUT http://localhost:8080/api/accounting-rules/{id} -H "Content-Type: application/json" -d '{ … }'

# Activate / deactivate
curl -X PATCH http://localhost:8080/api/accounting-rules/{id}/activate
curl -X PATCH http://localhost:8080/api/accounting-rules/{id}/deactivate
```

## Audit

```bash
curl http://localhost:8080/api/audit
curl http://localhost:8080/api/audit/by-transaction/{transactionId}
```

## Dashboard

```bash
curl http://localhost:8080/api/dashboard/summary
```

```json
{
  "totalTransactions": 9,
  "processedTransactions": 6,
  "failedTransactions": 1,
  "pendingTransactions": 2,
  "totalAccountingEntries": 6,
  "totalAmountByCurrency": [
    { "currency": "ARS", "totalAmount": 153200.00 },
    { "currency": "EUR", "totalAmount": 12000.00 },
    { "currency": "USD", "totalAmount": 5800.00 }
  ],
  "recentTransactions": [ … ],
  "recentAuditEvents": [ … ]
}
```
