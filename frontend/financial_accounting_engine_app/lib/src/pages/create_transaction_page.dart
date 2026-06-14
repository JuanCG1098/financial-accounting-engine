import 'package:flutter/material.dart';

import '../core/formatters.dart';
import '../models/models.dart';
import '../services/api_service.dart';
import '../widgets/section_card.dart';

class CreateTransactionPage extends StatefulWidget {
  const CreateTransactionPage({super.key, required this.api});

  final ApiService api;

  @override
  State<CreateTransactionPage> createState() => _CreateTransactionPageState();
}

class _CreateTransactionPageState extends State<CreateTransactionPage> {
  final _formKey = GlobalKey<FormState>();
  String _type = DomainOptions.transactionTypes.first;
  String _currency = DomainOptions.currencies.first;
  final _amount = TextEditingController();
  final _branchCode = TextEditingController(text: 'BR-001');
  final _costCenter = TextEditingController(text: 'CC-001');
  final _description = TextEditingController();
  bool _submitting = false;

  @override
  void dispose() {
    _amount.dispose();
    _branchCode.dispose();
    _costCenter.dispose();
    _description.dispose();
    super.dispose();
  }

  Future<void> _submit() async {
    if (!_formKey.currentState!.validate()) return;
    setState(() => _submitting = true);
    try {
      await widget.api.createTransaction(
        type: _type,
        currency: _currency,
        amount: num.parse(_amount.text.trim()),
        branchCode: _branchCode.text.trim(),
        costCenter: _costCenter.text.trim(),
        description: _description.text.trim(),
      );
      if (!mounted) return;
      Navigator.of(context).pop(true);
    } on ApiException catch (e) {
      if (!mounted) return;
      setState(() => _submitting = false);
      ScaffoldMessenger.of(context).showSnackBar(SnackBar(
        content: Text(e.message),
        backgroundColor: Colors.red.shade700,
      ));
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('New Transaction')),
      body: Center(
        child: ConstrainedBox(
          constraints: const BoxConstraints(maxWidth: 640),
          child: SingleChildScrollView(
            padding: const EdgeInsets.all(28),
            child: SectionCard(
              title: 'Transaction details',
              child: Form(
                key: _formKey,
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.stretch,
                  children: [
                    _dropdown(
                      label: 'Transaction Type',
                      value: _type,
                      items: DomainOptions.transactionTypes,
                      onChanged: (v) => setState(() => _type = v!),
                    ),
                    const SizedBox(height: 16),
                    _dropdown(
                      label: 'Currency',
                      value: _currency,
                      items: DomainOptions.currencies,
                      onChanged: (v) => setState(() => _currency = v!),
                    ),
                    const SizedBox(height: 16),
                    TextFormField(
                      controller: _amount,
                      keyboardType:
                          const TextInputType.numberWithOptions(decimal: true),
                      decoration: const InputDecoration(labelText: 'Amount'),
                      validator: (v) {
                        final value = num.tryParse((v ?? '').trim());
                        if (value == null) return 'Enter a valid number.';
                        if (value <= 0) return 'Amount must be greater than zero.';
                        return null;
                      },
                    ),
                    const SizedBox(height: 16),
                    TextFormField(
                      controller: _branchCode,
                      decoration: const InputDecoration(labelText: 'Branch Code'),
                      validator: (v) => (v == null || v.trim().isEmpty)
                          ? 'Branch code is required.'
                          : null,
                    ),
                    const SizedBox(height: 16),
                    TextFormField(
                      controller: _costCenter,
                      decoration: const InputDecoration(labelText: 'Cost Center'),
                    ),
                    const SizedBox(height: 16),
                    TextFormField(
                      controller: _description,
                      maxLines: 3,
                      decoration: const InputDecoration(labelText: 'Description'),
                    ),
                    const SizedBox(height: 24),
                    Row(
                      mainAxisAlignment: MainAxisAlignment.end,
                      children: [
                        TextButton(
                          onPressed: _submitting
                              ? null
                              : () => Navigator.of(context).pop(false),
                          child: const Text('Cancel'),
                        ),
                        const SizedBox(width: 12),
                        FilledButton(
                          onPressed: _submitting ? null : _submit,
                          child: _submitting
                              ? const SizedBox(
                                  width: 18,
                                  height: 18,
                                  child:
                                      CircularProgressIndicator(strokeWidth: 2),
                                )
                              : const Text('Create'),
                        ),
                      ],
                    ),
                  ],
                ),
              ),
            ),
          ),
        ),
      ),
    );
  }

  Widget _dropdown({
    required String label,
    required String value,
    required List<String> items,
    required ValueChanged<String?> onChanged,
  }) {
    return DropdownButtonFormField<String>(
      initialValue: value,
      isExpanded: true,
      decoration: InputDecoration(labelText: label),
      items: [
        for (final item in items)
          DropdownMenuItem<String>(
            value: item,
            child: Text(Formatters.label(item)),
          ),
      ],
      onChanged: onChanged,
    );
  }
}
