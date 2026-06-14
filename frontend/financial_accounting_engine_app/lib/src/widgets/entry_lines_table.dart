import 'package:flutter/material.dart';

import '../core/formatters.dart';
import '../core/theme.dart';
import '../models/models.dart';

/// Renders the debit/credit movements of an accounting entry plus its totals row.
class EntryLinesTable extends StatelessWidget {
  const EntryLinesTable({super.key, required this.entry});

  final AccountingEntryModel entry;

  @override
  Widget build(BuildContext context) {
    return SingleChildScrollView(
      scrollDirection: Axis.horizontal,
      child: ConstrainedBox(
        constraints: const BoxConstraints(minWidth: 700),
        child: DataTable(
          columnSpacing: 28,
          headingRowColor:
              const WidgetStatePropertyAll(Color(0xFFF1F5F9)),
          columns: const [
            DataColumn(label: Text('Account')),
            DataColumn(label: Text('Name')),
            DataColumn(label: Text('Cost Center')),
            DataColumn(label: Text('Debit'), numeric: true),
            DataColumn(label: Text('Credit'), numeric: true),
          ],
          rows: [
            for (final line in entry.lines)
              DataRow(cells: [
                DataCell(Text(line.accountCode,
                    style: const TextStyle(fontWeight: FontWeight.w600))),
                DataCell(Text(line.accountName)),
                DataCell(Text(line.costCenter ?? '—')),
                DataCell(Text(line.debit == 0
                    ? '—'
                    : Formatters.money(line.debit, entry.currency))),
                DataCell(Text(line.credit == 0
                    ? '—'
                    : Formatters.money(line.credit, entry.currency))),
              ]),
            DataRow(
              color: const WidgetStatePropertyAll(Color(0xFFF8FAFC)),
              cells: [
                const DataCell(Text('TOTAL',
                    style: TextStyle(fontWeight: FontWeight.w700))),
                const DataCell(Text('')),
                const DataCell(Text('')),
                DataCell(Text(
                  Formatters.money(entry.totalDebit, entry.currency),
                  style: const TextStyle(fontWeight: FontWeight.w700),
                )),
                DataCell(Text(
                  Formatters.money(entry.totalCredit, entry.currency),
                  style: const TextStyle(fontWeight: FontWeight.w700),
                )),
              ],
            ),
          ],
        ),
      ),
    );
  }
}

/// A badge that clearly states whether an accounting entry is balanced.
class BalanceBadge extends StatelessWidget {
  const BalanceBadge({super.key, required this.isBalanced});

  final bool isBalanced;

  @override
  Widget build(BuildContext context) {
    final color = isBalanced ? AppTheme.processed : AppTheme.failed;
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
      decoration: BoxDecoration(
        color: color.withValues(alpha: 0.12),
        borderRadius: BorderRadius.circular(20),
        border: Border.all(color: color.withValues(alpha: 0.4)),
      ),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(isBalanced ? Icons.check_circle : Icons.error,
              color: color, size: 16),
          const SizedBox(width: 6),
          Text(
            isBalanced ? 'Balanced' : 'Unbalanced',
            style: TextStyle(color: color, fontWeight: FontWeight.w600, fontSize: 13),
          ),
        ],
      ),
    );
  }
}
