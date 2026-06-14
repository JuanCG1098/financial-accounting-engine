import 'package:flutter/material.dart';

import 'src/core/theme.dart';
import 'src/services/api_service.dart';
import 'src/widgets/app_shell.dart';

void main() {
  runApp(const FinancialAccountingEngineApp());
}

class FinancialAccountingEngineApp extends StatefulWidget {
  const FinancialAccountingEngineApp({super.key});

  @override
  State<FinancialAccountingEngineApp> createState() =>
      _FinancialAccountingEngineAppState();
}

class _FinancialAccountingEngineAppState
    extends State<FinancialAccountingEngineApp> {
  final ApiService _api = ApiService();

  @override
  void dispose() {
    _api.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'Financial Accounting Engine',
      debugShowCheckedModeBanner: false,
      theme: AppTheme.light,
      home: AppShell(api: _api),
    );
  }
}
