import 'package:flutter/material.dart';

import '../core/formatters.dart';
import '../models/models.dart';
import '../services/api_service.dart';
import '../widgets/async_view.dart';
import '../widgets/entry_lines_table.dart';
import '../widgets/section_card.dart';
import 'entry_detail_page.dart';

class AccountingEntriesPage extends StatefulWidget {
  const AccountingEntriesPage({super.key, required this.api});

  final ApiService api;

  @override
  State<AccountingEntriesPage> createState() => _AccountingEntriesPageState();
}

class _AccountingEntriesPageState extends State<AccountingEntriesPage> {
  late Future<List<AccountingEntryModel>> _future =
      widget.api.listAccountingEntries();

  void _refresh() =>
      setState(() => _future = widget.api.listAccountingEntries());

  void _openDetail(AccountingEntryModel entry) {
    Navigator.of(context).push(
      MaterialPageRoute(builder: (_) => EntryDetailPage(entry: entry)),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.all(28),
      child: AsyncView<List<AccountingEntryModel>>(
        future: _future,
        onRetry: _refresh,
        builder: (context, entries) {
          return SectionCard(
            title: 'Accounting Entries (${entries.length})',
            trailing: IconButton(
              tooltip: 'Refresh',
              onPressed: _refresh,
              icon: const Icon(Icons.refresh),
            ),
            padding: EdgeInsets.zero,
            child: entries.isEmpty
                ? const Padding(
                    padding: EdgeInsets.all(24),
                    child: Text('No accounting entries yet.',
                        style: TextStyle(color: Color(0xFF94A3B8))),
                  )
                : SingleChildScrollView(
                    scrollDirection: Axis.horizontal,
                    child: ConstrainedBox(
                      constraints: BoxConstraints(
                          minWidth: MediaQuery.of(context).size.width - 360),
                      child: DataTable(
                        columnSpacing: 28,
                        columns: const [
                          DataColumn(label: Text('Entry Number')),
                          DataColumn(label: Text('Currency')),
                          DataColumn(label: Text('Total Debit'), numeric: true),
                          DataColumn(label: Text('Total Credit'), numeric: true),
                          DataColumn(label: Text('Balanced')),
                          DataColumn(label: Text('Created')),
                        ],
                        rows: [
                          for (final e in entries)
                            DataRow(
                              onSelectChanged: (_) => _openDetail(e),
                              cells: [
                                DataCell(Text(e.entryNumber,
                                    style: const TextStyle(
                                        fontWeight: FontWeight.w600))),
                                DataCell(Text(e.currency)),
                                DataCell(Text(
                                    Formatters.money(e.totalDebit, e.currency))),
                                DataCell(Text(
                                    Formatters.money(e.totalCredit, e.currency))),
                                DataCell(BalanceBadge(isBalanced: e.isBalanced)),
                                DataCell(Text(Formatters.dateTime(e.createdAt))),
                              ],
                            ),
                        ],
                      ),
                    ),
                  ),
          );
        },
      ),
    );
  }
}
