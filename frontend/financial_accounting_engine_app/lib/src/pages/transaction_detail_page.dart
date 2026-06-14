import 'package:flutter/material.dart';

import '../core/formatters.dart';
import '../models/models.dart';
import '../services/api_service.dart';
import '../widgets/async_view.dart';
import '../widgets/entry_lines_table.dart';
import '../widgets/section_card.dart';
import '../widgets/status_chip.dart';

class _DetailData {
  _DetailData(this.transaction, this.entry, this.audit);
  final TransactionModel transaction;
  final AccountingEntryModel? entry;
  final List<AuditEventModel> audit;
}

class TransactionDetailPage extends StatefulWidget {
  const TransactionDetailPage({
    super.key,
    required this.api,
    required this.transactionId,
  });

  final ApiService api;
  final String transactionId;

  @override
  State<TransactionDetailPage> createState() => _TransactionDetailPageState();
}

class _TransactionDetailPageState extends State<TransactionDetailPage> {
  late Future<_DetailData> _future = _load();
  bool _changed = false;
  bool _processing = false;

  Future<_DetailData> _load() async {
    final results = await Future.wait([
      widget.api.getTransaction(widget.transactionId),
      widget.api.getEntryByTransaction(widget.transactionId),
      widget.api.listAuditByTransaction(widget.transactionId),
    ]);
    return _DetailData(
      results[0] as TransactionModel,
      results[1] as AccountingEntryModel?,
      results[2] as List<AuditEventModel>,
    );
  }

  void _refresh() => setState(() => _future = _load());

  Future<void> _process() async {
    setState(() => _processing = true);
    try {
      final updated = await widget.api.processTransaction(widget.transactionId);
      _changed = true;
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(SnackBar(
        content: Text('Finished as ${Formatters.label(updated.status)}.'),
        backgroundColor: updated.status == 'PROCESSED' ? null : Colors.red.shade700,
      ));
    } on ApiException catch (e) {
      _changed = true;
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(SnackBar(
        content: Text(e.message),
        backgroundColor: Colors.red.shade700,
      ));
    } finally {
      if (mounted) setState(() => _processing = false);
      _refresh();
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Transaction Detail'),
        leading: IconButton(
          icon: const Icon(Icons.arrow_back),
          onPressed: () => Navigator.of(context).pop(_changed),
        ),
      ),
      body: AsyncView<_DetailData>(
        future: _future,
        onRetry: _refresh,
        builder: (context, data) {
          return SingleChildScrollView(
            padding: const EdgeInsets.all(28),
            child: Center(
              child: ConstrainedBox(
                constraints: const BoxConstraints(maxWidth: 920),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.stretch,
                  children: [
                    _summary(data.transaction),
                    const SizedBox(height: 24),
                    _entrySection(data.entry),
                    const SizedBox(height: 24),
                    _auditSection(data.audit),
                  ],
                ),
              ),
            ),
          );
        },
      ),
    );
  }

  Widget _summary(TransactionModel t) {
    return SectionCard(
      title: 'Transaction',
      trailing: t.status == 'PENDING'
          ? FilledButton.icon(
              onPressed: _processing ? null : _process,
              icon: _processing
                  ? const SizedBox(
                      width: 16,
                      height: 16,
                      child: CircularProgressIndicator(strokeWidth: 2))
                  : const Icon(Icons.play_arrow),
              label: const Text('Process'),
            )
          : StatusChip(t.status),
      child: Wrap(
        spacing: 32,
        runSpacing: 16,
        children: [
          _field('Type', Formatters.label(t.type)),
          _field('Amount', Formatters.money(t.amount, t.currency)),
          _field('Currency', t.currency),
          _field('Branch', t.branchCode),
          _field('Cost Center', t.costCenter.isEmpty ? '—' : t.costCenter),
          _field('Status', Formatters.label(t.status)),
          _field('Created', Formatters.dateTime(t.createdAt)),
          _field('Processed',
              t.processedAt == null ? '—' : Formatters.dateTime(t.processedAt!)),
          _field('Description', t.description.isEmpty ? '—' : t.description),
        ],
      ),
    );
  }

  Widget _entrySection(AccountingEntryModel? entry) {
    if (entry == null) {
      return const SectionCard(
        title: 'Accounting Entry',
        child: Text(
          'No accounting entry has been generated for this transaction yet.',
          style: TextStyle(color: Color(0xFF94A3B8)),
        ),
      );
    }
    return SectionCard(
      title: 'Accounting Entry · ${entry.entryNumber}',
      trailing: BalanceBadge(isBalanced: entry.isBalanced),
      child: EntryLinesTable(entry: entry),
    );
  }

  Widget _auditSection(List<AuditEventModel> audit) {
    return SectionCard(
      title: 'Audit Trail',
      padding: EdgeInsets.zero,
      child: Column(
        children: [
          for (var i = 0; i < audit.length; i++)
            _AuditTile(event: audit[i], isLast: i == audit.length - 1),
        ],
      ),
    );
  }

  Widget _field(String label, String value) {
    return SizedBox(
      width: 200,
      child: Column(
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
      ),
    );
  }
}

class _AuditTile extends StatelessWidget {
  const _AuditTile({required this.event, required this.isLast});

  final AuditEventModel event;
  final bool isLast;

  @override
  Widget build(BuildContext context) {
    return IntrinsicHeight(
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          const SizedBox(width: 20),
          Column(
            children: [
              Container(
                margin: const EdgeInsets.only(top: 18),
                width: 10,
                height: 10,
                decoration: const BoxDecoration(
                  color: Color(0xFF1D4ED8),
                  shape: BoxShape.circle,
                ),
              ),
              if (!isLast)
                const Expanded(
                  child: VerticalDivider(width: 1, color: Color(0xFFCBD5E1)),
                ),
            ],
          ),
          const SizedBox(width: 16),
          Expanded(
            child: Padding(
              padding: const EdgeInsets.symmetric(vertical: 12),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Row(
                    children: [
                      Expanded(
                        child: Text(
                          Formatters.label(event.eventType),
                          style: const TextStyle(fontWeight: FontWeight.w700),
                        ),
                      ),
                      Text(
                        Formatters.dateTime(event.createdAt),
                        style: const TextStyle(
                            fontSize: 12, color: Color(0xFF64748B)),
                      ),
                      const SizedBox(width: 20),
                    ],
                  ),
                  const SizedBox(height: 2),
                  Padding(
                    padding: const EdgeInsets.only(right: 20),
                    child: Text(
                      event.message,
                      style: const TextStyle(color: Color(0xFF475569)),
                    ),
                  ),
                ],
              ),
            ),
          ),
        ],
      ),
    );
  }
}
