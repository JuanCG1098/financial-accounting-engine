import 'package:flutter/material.dart';

import '../core/formatters.dart';
import '../models/models.dart';
import '../widgets/entry_lines_table.dart';
import '../widgets/section_card.dart';

class EntryDetailPage extends StatelessWidget {
  const EntryDetailPage({super.key, required this.entry});

  final AccountingEntryModel entry;

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: Text('Entry ${entry.entryNumber}')),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(28),
        child: Center(
          child: ConstrainedBox(
            constraints: const BoxConstraints(maxWidth: 920),
            child: SectionCard(
              title: 'Movements',
              trailing: BalanceBadge(isBalanced: entry.isBalanced),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Wrap(
                    spacing: 32,
                    runSpacing: 12,
                    children: [
                      _field('Entry Number', entry.entryNumber),
                      _field('Currency', entry.currency),
                      _field('Total Debit',
                          Formatters.money(entry.totalDebit, entry.currency)),
                      _field('Total Credit',
                          Formatters.money(entry.totalCredit, entry.currency)),
                      _field('Created', Formatters.dateTime(entry.createdAt)),
                    ],
                  ),
                  const SizedBox(height: 20),
                  EntryLinesTable(entry: entry),
                ],
              ),
            ),
          ),
        ),
      ),
    );
  }

  Widget _field(String label, String value) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text(label,
            style: const TextStyle(
                fontSize: 12, color: Color(0xFF64748B), fontWeight: FontWeight.w500)),
        const SizedBox(height: 4),
        Text(value,
            style: const TextStyle(
                fontSize: 15, fontWeight: FontWeight.w600, color: Color(0xFF0F172A))),
      ],
    );
  }
}
