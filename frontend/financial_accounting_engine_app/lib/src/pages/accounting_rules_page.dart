import 'package:flutter/material.dart';

import '../core/formatters.dart';
import '../core/theme.dart';
import '../models/models.dart';
import '../services/api_service.dart';
import '../widgets/async_view.dart';
import '../widgets/section_card.dart';
import '../widgets/status_chip.dart';

class AccountingRulesPage extends StatefulWidget {
  const AccountingRulesPage({super.key, required this.api});

  final ApiService api;

  @override
  State<AccountingRulesPage> createState() => _AccountingRulesPageState();
}

class _AccountingRulesPageState extends State<AccountingRulesPage> {
  late Future<List<AccountingRuleModel>> _future = widget.api.listRules();

  void _refresh() => setState(() => _future = widget.api.listRules());

  Future<void> _toggle(AccountingRuleModel rule) async {
    try {
      if (rule.isActive) {
        await widget.api.deactivateRule(rule.id);
      } else {
        await widget.api.activateRule(rule.id);
      }
      _refresh();
    } on ApiException catch (e) {
      _snack(e.message);
    }
  }

  Future<void> _openCreate() async {
    final payload = await showDialog<Map<String, dynamic>>(
      context: context,
      builder: (_) => const _CreateRuleDialog(),
    );
    if (payload == null) return;
    try {
      await widget.api.createRule(payload);
      _refresh();
    } on ApiException catch (e) {
      _snack(e.message);
    }
  }

  void _snack(String message) {
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(content: Text(message), backgroundColor: Colors.red.shade700),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.all(28),
      child: AsyncView<List<AccountingRuleModel>>(
        future: _future,
        onRetry: _refresh,
        builder: (context, rules) {
          return SectionCard(
            title: 'Accounting Rules (${rules.length})',
            trailing: FilledButton.icon(
              onPressed: _openCreate,
              icon: const Icon(Icons.add),
              label: const Text('New Rule'),
            ),
            padding: EdgeInsets.zero,
            child: SingleChildScrollView(
              scrollDirection: Axis.horizontal,
              child: ConstrainedBox(
                constraints: BoxConstraints(
                    minWidth: MediaQuery.of(context).size.width - 360),
                child: DataTable(
                  columnSpacing: 24,
                  columns: const [
                    DataColumn(label: Text('Transaction Type')),
                    DataColumn(label: Text('Currency')),
                    DataColumn(label: Text('Debit')),
                    DataColumn(label: Text('Credit')),
                    DataColumn(label: Text('Flags')),
                    DataColumn(label: Text('Active')),
                  ],
                  rows: [
                    for (final r in rules)
                      DataRow(cells: [
                        DataCell(Text(Formatters.label(r.transactionType))),
                        DataCell(Text(r.currency ?? 'Any')),
                        DataCell(Text('${r.debitAccountCode} · ${r.debitAccountName}')),
                        DataCell(Text('${r.creditAccountCode} · ${r.creditAccountName}')),
                        DataCell(Row(
                          children: [
                            if (r.requiresCashFlow)
                              const TagChip('Cash Flow', color: AppTheme.processing),
                            if (r.isAccountingOnly)
                              const Padding(
                                padding: EdgeInsets.only(left: 6),
                                child: TagChip('Accounting Only',
                                    color: Color(0xFF7C3AED)),
                              ),
                          ],
                        )),
                        DataCell(Switch(
                          value: r.isActive,
                          onChanged: (_) => _toggle(r),
                        )),
                      ]),
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

class _CreateRuleDialog extends StatefulWidget {
  const _CreateRuleDialog();

  @override
  State<_CreateRuleDialog> createState() => _CreateRuleDialogState();
}

class _CreateRuleDialogState extends State<_CreateRuleDialog> {
  final _formKey = GlobalKey<FormState>();
  String _type = DomainOptions.transactionTypes.first;
  String? _currency; // null = Any
  String _behavior = DomainOptions.costCenterBehaviors.first;
  final _debitCode = TextEditingController();
  final _debitName = TextEditingController();
  final _creditCode = TextEditingController();
  final _creditName = TextEditingController();
  bool _requiresCashFlow = true;
  bool _isAccountingOnly = false;

  @override
  void dispose() {
    _debitCode.dispose();
    _debitName.dispose();
    _creditCode.dispose();
    _creditName.dispose();
    super.dispose();
  }

  void _submit() {
    if (!_formKey.currentState!.validate()) return;
    if (_isAccountingOnly && _requiresCashFlow) {
      ScaffoldMessenger.of(context).showSnackBar(const SnackBar(
        content: Text('An accounting-only rule cannot also require cash flow.'),
      ));
      return;
    }
    Navigator.of(context).pop(<String, dynamic>{
      'transactionType': _type,
      'currency': _currency,
      'debitAccountCode': _debitCode.text.trim(),
      'debitAccountName': _debitName.text.trim(),
      'creditAccountCode': _creditCode.text.trim(),
      'creditAccountName': _creditName.text.trim(),
      'requiresCashFlow': _requiresCashFlow,
      'isAccountingOnly': _isAccountingOnly,
      'costCenterBehavior': _behavior,
    });
  }

  @override
  Widget build(BuildContext context) {
    return AlertDialog(
      title: const Text('New Accounting Rule'),
      content: SizedBox(
        width: 520,
        child: SingleChildScrollView(
          child: Form(
            key: _formKey,
            child: Column(
              mainAxisSize: MainAxisSize.min,
              children: [
                DropdownButtonFormField<String>(
                  initialValue: _type,
                  isExpanded: true,
                  decoration: const InputDecoration(labelText: 'Transaction Type'),
                  items: [
                    for (final t in DomainOptions.transactionTypes)
                      DropdownMenuItem(value: t, child: Text(Formatters.label(t))),
                  ],
                  onChanged: (v) => setState(() => _type = v!),
                ),
                const SizedBox(height: 12),
                DropdownButtonFormField<String?>(
                  initialValue: _currency,
                  isExpanded: true,
                  decoration: const InputDecoration(labelText: 'Currency'),
                  items: [
                    const DropdownMenuItem<String?>(
                        value: null, child: Text('Any')),
                    for (final c in DomainOptions.currencies)
                      DropdownMenuItem<String?>(value: c, child: Text(c)),
                  ],
                  onChanged: (v) => setState(() => _currency = v),
                ),
                const SizedBox(height: 12),
                Row(children: [
                  Expanded(child: _text(_debitCode, 'Debit Account Code')),
                  const SizedBox(width: 12),
                  Expanded(flex: 2, child: _text(_debitName, 'Debit Account Name')),
                ]),
                const SizedBox(height: 12),
                Row(children: [
                  Expanded(child: _text(_creditCode, 'Credit Account Code')),
                  const SizedBox(width: 12),
                  Expanded(flex: 2, child: _text(_creditName, 'Credit Account Name')),
                ]),
                const SizedBox(height: 12),
                DropdownButtonFormField<String>(
                  initialValue: _behavior,
                  isExpanded: true,
                  decoration:
                      const InputDecoration(labelText: 'Cost Center Behavior'),
                  items: [
                    for (final b in DomainOptions.costCenterBehaviors)
                      DropdownMenuItem(value: b, child: Text(Formatters.label(b))),
                  ],
                  onChanged: (v) => setState(() => _behavior = v!),
                ),
                SwitchListTile(
                  contentPadding: EdgeInsets.zero,
                  title: const Text('Requires cash flow'),
                  value: _requiresCashFlow,
                  onChanged: (v) => setState(() {
                    _requiresCashFlow = v;
                    if (v) _isAccountingOnly = false;
                  }),
                ),
                SwitchListTile(
                  contentPadding: EdgeInsets.zero,
                  title: const Text('Accounting only (no cash flow)'),
                  value: _isAccountingOnly,
                  onChanged: (v) => setState(() {
                    _isAccountingOnly = v;
                    if (v) _requiresCashFlow = false;
                  }),
                ),
              ],
            ),
          ),
        ),
      ),
      actions: [
        TextButton(
          onPressed: () => Navigator.of(context).pop(),
          child: const Text('Cancel'),
        ),
        FilledButton(onPressed: _submit, child: const Text('Create')),
      ],
    );
  }

  Widget _text(TextEditingController controller, String label) {
    return TextFormField(
      controller: controller,
      decoration: InputDecoration(labelText: label),
      validator: (v) =>
          (v == null || v.trim().isEmpty) ? 'Required' : null,
    );
  }
}
