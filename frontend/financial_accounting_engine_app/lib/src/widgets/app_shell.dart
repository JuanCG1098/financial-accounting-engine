import 'package:flutter/material.dart';

import '../core/theme.dart';
import '../services/api_service.dart';
import '../pages/dashboard_page.dart';
import '../pages/transactions_page.dart';
import '../pages/accounting_entries_page.dart';
import '../pages/accounting_rules_page.dart';
import '../pages/audit_page.dart';

class _Destination {
  const _Destination(this.label, this.icon);
  final String label;
  final IconData icon;
}

/// The main application shell: a fixed sidebar plus a swappable content area.
class AppShell extends StatefulWidget {
  const AppShell({super.key, required this.api});

  final ApiService api;

  @override
  State<AppShell> createState() => _AppShellState();
}

class _AppShellState extends State<AppShell> {
  int _index = 0;

  static const _destinations = <_Destination>[
    _Destination('Dashboard', Icons.dashboard_outlined),
    _Destination('Transactions', Icons.receipt_long_outlined),
    _Destination('Accounting Entries', Icons.account_balance_outlined),
    _Destination('Accounting Rules', Icons.rule_outlined),
    _Destination('Audit Trail', Icons.history_outlined),
  ];

  late final List<Widget> _pages = [
    DashboardPage(api: widget.api, onNavigate: _select),
    TransactionsPage(api: widget.api),
    AccountingEntriesPage(api: widget.api),
    AccountingRulesPage(api: widget.api),
    AuditPage(api: widget.api),
  ];

  void _select(int index) => setState(() => _index = index);

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: Row(
        children: [
          _Sidebar(
            destinations: _destinations,
            selectedIndex: _index,
            onSelected: _select,
          ),
          Expanded(
            child: Column(
              children: [
                _TopBar(title: _destinations[_index].label),
                Expanded(
                  child: IndexedStack(index: _index, children: _pages),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }
}

class _Sidebar extends StatelessWidget {
  const _Sidebar({
    required this.destinations,
    required this.selectedIndex,
    required this.onSelected,
  });

  final List<_Destination> destinations;
  final int selectedIndex;
  final ValueChanged<int> onSelected;

  @override
  Widget build(BuildContext context) {
    return Container(
      width: 248,
      color: AppTheme.sidebarBackground,
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          const Padding(
            padding: EdgeInsets.fromLTRB(20, 28, 20, 24),
            child: Row(
              children: [
                Icon(Icons.account_balance_wallet, color: Colors.white, size: 26),
                SizedBox(width: 12),
                Expanded(
                  child: Text(
                    'Accounting Engine',
                    style: TextStyle(
                      color: Colors.white,
                      fontSize: 16,
                      fontWeight: FontWeight.w700,
                    ),
                  ),
                ),
              ],
            ),
          ),
          for (var i = 0; i < destinations.length; i++)
            _SidebarItem(
              destination: destinations[i],
              selected: i == selectedIndex,
              onTap: () => onSelected(i),
            ),
          const Spacer(),
          const Padding(
            padding: EdgeInsets.all(20),
            child: Text(
              'Fictional & anonymized\nportfolio project',
              style: TextStyle(color: Color(0xFF94A3B8), fontSize: 11),
            ),
          ),
        ],
      ),
    );
  }
}

class _SidebarItem extends StatelessWidget {
  const _SidebarItem({
    required this.destination,
    required this.selected,
    required this.onTap,
  });

  final _Destination destination;
  final bool selected;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    final color = selected ? Colors.white : const Color(0xFF94A3B8);
    return Padding(
      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 2),
      child: Material(
        color: selected ? Colors.white.withValues(alpha: 0.10) : Colors.transparent,
        borderRadius: BorderRadius.circular(10),
        child: InkWell(
          borderRadius: BorderRadius.circular(10),
          onTap: onTap,
          child: Padding(
            padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 12),
            child: Row(
              children: [
                Icon(destination.icon, color: color, size: 20),
                const SizedBox(width: 12),
                Text(
                  destination.label,
                  style: TextStyle(
                    color: color,
                    fontWeight: selected ? FontWeight.w600 : FontWeight.w500,
                    fontSize: 14,
                  ),
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }
}

class _TopBar extends StatelessWidget {
  const _TopBar({required this.title});

  final String title;

  @override
  Widget build(BuildContext context) {
    return Container(
      height: 64,
      decoration: const BoxDecoration(
        color: Colors.white,
        border: Border(bottom: BorderSide(color: Color(0xFFE2E8F0))),
      ),
      padding: const EdgeInsets.symmetric(horizontal: 28),
      alignment: Alignment.centerLeft,
      child: Text(
        title,
        style: const TextStyle(
          fontSize: 20,
          fontWeight: FontWeight.w700,
          color: Color(0xFF0F172A),
        ),
      ),
    );
  }
}
