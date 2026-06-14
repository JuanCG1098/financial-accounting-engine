import 'package:flutter/material.dart';

import '../core/formatters.dart';
import '../models/models.dart';
import '../services/api_service.dart';
import '../widgets/async_view.dart';
import '../widgets/section_card.dart';
import '../widgets/status_chip.dart';
import 'create_transaction_page.dart';
import 'transaction_detail_page.dart';

class TransactionsPage extends StatefulWidget {
  const TransactionsPage({super.key, required this.api});

  final ApiService api;

  @override
  State<TransactionsPage> createState() => _TransactionsPageState();
}

class _TransactionsPageState extends State<TransactionsPage> {
  String? _status;
  String? _currency;
  String? _type;
  late Future<List<TransactionModel>> _future = _load();

  Future<List<TransactionModel>> _load() => widget.api.listTransactions(
        status: _status,
        currency: _currency,
        type: _type,
      );

  void _refresh() => setState(() => _future = _load());

  Future<void> _openCreate() async {
    final created = await Navigator.of(context).push<bool>(
      MaterialPageRoute(
        builder: (_) => CreateTransactionPage(api: widget.api),
      ),
    );
    if (created == true) _refresh();
  }

  Future<void> _openDetail(String id) async {
    final changed = await Navigator.of(context).push<bool>(
      MaterialPageRoute(
        builder: (_) => TransactionDetailPage(api: widget.api, transactionId: id),
      ),
    );
    if (changed == true) _refresh();
  }

  Future<void> _process(TransactionModel t) async {
    try {
      final updated = await widget.api.processTransaction(t.id);
      if (!mounted) return;
      _showSnack(
        'Transaction ${updated.status == 'PROCESSED' ? 'processed' : 'finished as ${Formatters.label(updated.status)}'}.',
        success: updated.status == 'PROCESSED',
      );
      _refresh();
    } on ApiException catch (e) {
      if (!mounted) return;
      _showSnack(e.message, success: false);
      _refresh();
    }
  }

  void _showSnack(String message, {required bool success}) {
    ScaffoldMessenger.of(context).showSnackBar(SnackBar(
      content: Text(message),
      backgroundColor: success ? null : Colors.red.shade700,
    ));
  }

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.all(28),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          _filters(),
          const SizedBox(height: 16),
          Expanded(
            child: AsyncView<List<TransactionModel>>(
              future: _future,
              onRetry: _refresh,
              builder: (context, transactions) => _table(transactions),
            ),
          ),
        ],
      ),
    );
  }

  Widget _filters() {
    return Wrap(
      spacing: 12,
      runSpacing: 12,
      crossAxisAlignment: WrapCrossAlignment.center,
      children: [
        _dropdown(
          hint: 'Status',
          value: _status,
          items: DomainOptions.transactionStatuses,
          onChanged: (v) => setState(() {
            _status = v;
            _future = _load();
          }),
        ),
        _dropdown(
          hint: 'Currency',
          value: _currency,
          items: DomainOptions.currencies,
          onChanged: (v) => setState(() {
            _currency = v;
            _future = _load();
          }),
        ),
        _dropdown(
          hint: 'Type',
          value: _type,
          items: DomainOptions.transactionTypes,
          onChanged: (v) => setState(() {
            _type = v;
            _future = _load();
          }),
        ),
        const Spacer(),
        FilledButton.icon(
          onPressed: _openCreate,
          icon: const Icon(Icons.add),
          label: const Text('New Transaction'),
        ),
      ],
    );
  }

  Widget _dropdown({
    required String hint,
    required String? value,
    required List<String> items,
    required ValueChanged<String?> onChanged,
  }) {
    return SizedBox(
      width: 220,
      child: DropdownButtonFormField<String?>(
        initialValue: value,
        isExpanded: true,
        decoration: InputDecoration(labelText: hint, isDense: true),
        items: [
          const DropdownMenuItem<String?>(value: null, child: Text('All')),
          for (final item in items)
            DropdownMenuItem<String?>(
              value: item,
              child: Text(Formatters.label(item)),
            ),
        ],
        onChanged: onChanged,
      ),
    );
  }

  Widget _table(List<TransactionModel> transactions) {
    return SectionCard(
      title: 'Transactions (${transactions.length})',
      padding: EdgeInsets.zero,
      child: transactions.isEmpty
          ? const Padding(
              padding: EdgeInsets.all(24),
              child: Text('No transactions match the filters.',
                  style: TextStyle(color: Color(0xFF94A3B8))),
            )
          : SingleChildScrollView(
              scrollDirection: Axis.horizontal,
              child: ConstrainedBox(
                constraints:
                    BoxConstraints(minWidth: MediaQuery.of(context).size.width - 360),
                child: DataTable(
                  columnSpacing: 28,
                  columns: const [
                    DataColumn(label: Text('Type')),
                    DataColumn(label: Text('Currency')),
                    DataColumn(label: Text('Amount'), numeric: true),
                    DataColumn(label: Text('Branch')),
                    DataColumn(label: Text('Cost Center')),
                    DataColumn(label: Text('Status')),
                    DataColumn(label: Text('Created')),
                    DataColumn(label: Text('Actions')),
                  ],
                  rows: [
                    for (final t in transactions)
                      DataRow(
                        onSelectChanged: (_) => _openDetail(t.id),
                        cells: [
                          DataCell(Text(Formatters.label(t.type))),
                          DataCell(Text(t.currency)),
                          DataCell(Text(Formatters.money(t.amount, t.currency))),
                          DataCell(Text(t.branchCode)),
                          DataCell(Text(t.costCenter.isEmpty ? '—' : t.costCenter)),
                          DataCell(StatusChip(t.status)),
                          DataCell(Text(Formatters.dateTime(t.createdAt))),
                          DataCell(
                            t.status == 'PENDING'
                                ? FilledButton.tonal(
                                    onPressed: () => _process(t),
                                    child: const Text('Process'),
                                  )
                                : const SizedBox.shrink(),
                          ),
                        ],
                      ),
                  ],
                ),
              ),
            ),
    );
  }
}
