# Business Rules

This document describes the (fictional) accounting logic the engine implements.

## Core idea

The engine turns **financial transactions** into balanced **double-entry accounting entries** by
applying configurable **accounting rules**. Every step is recorded in an **audit trail**.

## Transaction types

| Type | Meaning (illustrative) |
| --- | --- |
| `CASH_DEPOSIT` | Cash received at a branch. |
| `CASH_WITHDRAWAL` | Cash paid out at a branch. |
| `INTERNAL_TRANSFER` | Movement of funds through an internal clearing account. |
| `ACCOUNTING_ADJUSTMENT` | A correction posted against an adjustment account. |
| `ACCOUNTING_ONLY_OPERATION` | A ledger-only entry with **no** cash movement. |

## Transaction lifecycle

```
PENDING ──process──► PROCESSING ──► PROCESSED   (balanced entry generated)
                               └──► FAILED       (no active rule or unbalanced entry)
```

1. A financial transaction is created and stays in `PENDING`.
2. The user triggers processing.
3. The engine searches for an **active** accounting rule matching the transaction type and currency.
4. The engine generates debit and credit movements from that rule.
5. The engine validates that **total debit == total credit**.
6. If valid, the transaction becomes `PROCESSED`; if not, it becomes `FAILED`.
7. Audit events are written during the whole process.

## Accounting rules

A rule defines how a transaction type (optionally scoped to a currency) maps to a debit and a
credit account:

| Field | Purpose |
| --- | --- |
| `TransactionType` | Which transaction type the rule applies to. |
| `Currency` | Optional. `null` means the rule applies to any currency. |
| `DebitAccountCode` / `DebitAccountName` | The account debited. |
| `CreditAccountCode` / `CreditAccountName` | The account credited. |
| `RequiresCashFlow` | Whether processing represents real money movement. |
| `IsAccountingOnly` | Whether the rule produces a ledger-only entry (no cash flow). |
| `CostCenterBehavior` | How the transaction cost center is copied onto the entry lines. |
| `IsActive` | Only active rules are considered by the engine. |

When more than one active rule matches, a **currency-specific** rule takes precedence over a
**currency-agnostic** one.

### Cash flow vs. accounting-only

This distinction is what makes the engine more than a CRUD:

- A rule with `RequiresCashFlow = true` represents an operation that moves money (e.g. a cash
  deposit touching the *Branch Cash* account).
- A rule with `IsAccountingOnly = true` (and `RequiresCashFlow = false`) produces a balanced
  ledger entry that does **not** move cash (e.g. an accrual). The two flags are mutually exclusive
  and this is enforced by validation.

### Cost center behavior

| Value | Effect |
| --- | --- |
| `NONE` | Cost center is not copied to any line. |
| `PROPAGATE` | Copied to both the debit and credit lines. |
| `DEBIT_LINE_ONLY` | Copied to the debit line only. |
| `CREDIT_LINE_ONLY` | Copied to the credit line only. |

## Balance validation

The engine considers an entry valid only when:

- `TotalDebit == TotalCredit`,
- the amount is greater than zero,
- an active rule exists for the transaction type, and
- the currency is supported (`ARS`, `USD`, `EUR`).

An unbalanced entry sends the transaction to `FAILED`.

## Audit events

| Event | When |
| --- | --- |
| `TRANSACTION_CREATED` | A transaction is created. |
| `PROCESSING_STARTED` | Processing begins. |
| `ACCOUNTING_RULE_APPLIED` | A matching rule is found and applied. |
| `ENTRY_GENERATED` | The accounting entry is built. |
| `BALANCE_VALIDATED` | The entry is confirmed balanced. |
| `PROCESSING_FAILED` | Processing fails (no rule or unbalanced). |
| `PROCESSING_COMPLETED` | Processing succeeds. |

## Seeded chart of accounts (fictional)

| Code | Name |
| --- | --- |
| `100001` | Branch Cash |
| `200001` | Customer Liability |
| `300001` | Internal Clearing Account |
| `400001` | Accounting Adjustment Account |
| `500001` | Operational Expense Account |

Seeded cost centers: `CC-001`, `CC-002`, `CC-003`. Seeded currencies: `ARS`, `USD`, `EUR`.
