import 'package:flutter/material.dart';

import '../core/formatters.dart';
import '../models/models.dart';
import '../services/api_service.dart';
import '../widgets/async_view.dart';
import '../widgets/section_card.dart';

class AuditPage extends StatefulWidget {
  const AuditPage({super.key, required this.api});

  final ApiService api;

  @override
  State<AuditPage> createState() => _AuditPageState();
}

class _AuditPageState extends State<AuditPage> {
  final _transactionId = TextEditingController();
  String? _activeFilter;
  late Future<List<AuditEventModel>> _future = widget.api.listAudit();

  Future<List<AuditEventModel>> _load() {
    final id = _activeFilter;
    return (id == null || id.isEmpty)
        ? widget.api.listAudit()
        : widget.api.listAuditByTransaction(id);
  }

  void _apply() => setState(() {
        _activeFilter = _transactionId.text.trim();
        _future = _load();
      });

  void _clear() => setState(() {
        _transactionId.clear();
        _activeFilter = null;
        _future = _load();
      });

  @override
  void dispose() {
    _transactionId.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.all(28),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          Row(
            children: [
              SizedBox(
                width: 420,
                child: TextField(
                  controller: _transactionId,
                  decoration: const InputDecoration(
                    labelText: 'Filter by Transaction ID',
                    isDense: true,
                    prefixIcon: Icon(Icons.search),
                  ),
                  onSubmitted: (_) => _apply(),
                ),
              ),
              const SizedBox(width: 12),
              FilledButton(onPressed: _apply, child: const Text('Filter')),
              const SizedBox(width: 8),
              TextButton(onPressed: _clear, child: const Text('Clear')),
            ],
          ),
          const SizedBox(height: 16),
          Expanded(
            child: AsyncView<List<AuditEventModel>>(
              future: _future,
              onRetry: () => setState(() => _future = _load()),
              builder: (context, events) => _table(events),
            ),
          ),
        ],
      ),
    );
  }

  Widget _table(List<AuditEventModel> events) {
    return SectionCard(
      title: 'Audit Events (${events.length})',
      padding: EdgeInsets.zero,
      child: events.isEmpty
          ? const Padding(
              padding: EdgeInsets.all(24),
              child: Text('No audit events found.',
                  style: TextStyle(color: Color(0xFF94A3B8))),
            )
          : SingleChildScrollView(
              scrollDirection: Axis.horizontal,
              child: ConstrainedBox(
                constraints: BoxConstraints(
                    minWidth: MediaQuery.of(context).size.width - 360),
                child: DataTable(
                  columnSpacing: 24,
                  columns: const [
                    DataColumn(label: Text('Event')),
                    DataColumn(label: Text('Message')),
                    DataColumn(label: Text('From')),
                    DataColumn(label: Text('To')),
                    DataColumn(label: Text('Timestamp')),
                  ],
                  rows: [
                    for (final e in events)
                      DataRow(cells: [
                        DataCell(Text(Formatters.label(e.eventType),
                            style: const TextStyle(fontWeight: FontWeight.w600))),
                        DataCell(ConstrainedBox(
                          constraints: const BoxConstraints(maxWidth: 420),
                          child: Text(e.message, overflow: TextOverflow.ellipsis),
                        )),
                        DataCell(Text(
                            e.previousStatus == null
                                ? '—'
                                : Formatters.label(e.previousStatus!))),
                        DataCell(Text(e.newStatus == null
                            ? '—'
                            : Formatters.label(e.newStatus!))),
                        DataCell(Text(Formatters.dateTime(e.createdAt))),
                      ]),
                  ],
                ),
              ),
            ),
    );
  }
}
