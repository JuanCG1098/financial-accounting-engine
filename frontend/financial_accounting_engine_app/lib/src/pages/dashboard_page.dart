import 'package:flutter/material.dart';

import '../core/formatters.dart';
import '../core/theme.dart';
import '../models/models.dart';
import '../services/api_service.dart';
import '../widgets/async_view.dart';
import '../widgets/metric_card.dart';
import '../widgets/section_card.dart';
import '../widgets/status_chip.dart';

class DashboardPage extends StatefulWidget {
  const DashboardPage({super.key, required this.api, required this.onNavigate});

  final ApiService api;
  final ValueChanged<int> onNavigate;

  @override
  State<DashboardPage> createState() => _DashboardPageState();
}

class _DashboardPageState extends State<DashboardPage> {
  late Future<DashboardSummaryModel> _future = widget.api.getDashboardSummary();

  void _refresh() =>
      setState(() => _future = widget.api.getDashboardSummary());

  @override
  Widget build(BuildContext context) {
    return AsyncView<DashboardSummaryModel>(
      future: _future,
      onRetry: _refresh,
      builder: (context, summary) {
        return RefreshableBody(
          onRefresh: _refresh,
          child: ListView(
            padding: const EdgeInsets.all(28),
            children: [
              _metrics(summary),
              const SizedBox(height: 24),
              _amountByCurrency(summary),
              const SizedBox(height: 24),
              LayoutBuilder(builder: (context, constraints) {
                final wide = constraints.maxWidth > 900;
                final transactions = _recentTransactions(summary);
                final audit = _recentAudit(summary);
                if (!wide) {
                  return Column(children: [
                    transactions,
                    const SizedBox(height: 24),
                    audit,
                  ]);
                }
                return Row(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Expanded(child: transactions),
                    const SizedBox(width: 24),
                    Expanded(child: audit),
                  ],
                );
              }),
            ],
          ),
        );
      },
    );
  }

  Widget _metrics(DashboardSummaryModel s) {
    final cards = [
      MetricCard(
        label: 'Total Transactions',
        value: Formatters.number(s.totalTransactions),
        icon: Icons.receipt_long,
        color: AppTheme.processing,
      ),
      MetricCard(
        label: 'Processed',
        value: Formatters.number(s.processedTransactions),
        icon: Icons.check_circle_outline,
        color: AppTheme.processed,
      ),
      MetricCard(
        label: 'Failed',
        value: Formatters.number(s.failedTransactions),
        icon: Icons.error_outline,
        color: AppTheme.failed,
      ),
      MetricCard(
        label: 'Pending',
        value: Formatters.number(s.pendingTransactions),
        icon: Icons.schedule,
        color: AppTheme.pending,
      ),
      MetricCard(
        label: 'Accounting Entries',
        value: Formatters.number(s.totalAccountingEntries),
        icon: Icons.account_balance,
        color: const Color(0xFF7C3AED),
      ),
    ];

    return LayoutBuilder(builder: (context, constraints) {
      const spacing = 16.0;
      final columns = constraints.maxWidth > 1100
          ? 5
          : constraints.maxWidth > 760
              ? 3
              : constraints.maxWidth > 460
                  ? 2
                  : 1;
      final width =
          (constraints.maxWidth - (columns - 1) * spacing) / columns;
      return Wrap(
        spacing: spacing,
        runSpacing: spacing,
        children: [
          for (final card in cards) SizedBox(width: width, child: card),
        ],
      );
    });
  }

  Widget _amountByCurrency(DashboardSummaryModel s) {
    return SectionCard(
      title: 'Total Processed Amount by Currency',
      child: s.totalAmountByCurrency.isEmpty
          ? const _EmptyHint('No processed amounts yet.')
          : Wrap(
              spacing: 16,
              runSpacing: 16,
              children: [
                for (final c in s.totalAmountByCurrency)
                  _CurrencyTile(currency: c.currency, amount: c.totalAmount),
              ],
            ),
    );
  }

  Widget _recentTransactions(DashboardSummaryModel s) {
    return SectionCard(
      title: 'Recent Transactions',
      trailing: TextButton(
        onPressed: () => widget.onNavigate(1),
        child: const Text('View all'),
      ),
      padding: EdgeInsets.zero,
      child: s.recentTransactions.isEmpty
          ? const Padding(
              padding: EdgeInsets.all(20),
              child: _EmptyHint('No transactions yet.'),
            )
          : Column(
              children: [
                for (final t in s.recentTransactions)
                  ListTile(
                    title: Text(
                      '${Formatters.label(t.type)} · ${Formatters.money(t.amount, t.currency)}',
                      style: const TextStyle(fontWeight: FontWeight.w600),
                    ),
                    subtitle: Text(
                      '${t.branchCode} · ${Formatters.dateTime(t.createdAt)}',
                    ),
                    trailing: StatusChip(t.status),
                  ),
              ],
            ),
    );
  }

  Widget _recentAudit(DashboardSummaryModel s) {
    return SectionCard(
      title: 'Recent Audit Events',
      trailing: TextButton(
        onPressed: () => widget.onNavigate(4),
        child: const Text('View all'),
      ),
      padding: EdgeInsets.zero,
      child: s.recentAuditEvents.isEmpty
          ? const Padding(
              padding: EdgeInsets.all(20),
              child: _EmptyHint('No audit events yet.'),
            )
          : Column(
              children: [
                for (final a in s.recentAuditEvents)
                  ListTile(
                    leading: const Icon(Icons.bolt, size: 18),
                    title: Text(
                      Formatters.label(a.eventType),
                      style: const TextStyle(fontWeight: FontWeight.w600),
                    ),
                    subtitle: Text(
                      a.message,
                      maxLines: 2,
                      overflow: TextOverflow.ellipsis,
                    ),
                    trailing: Text(
                      Formatters.dateTime(a.createdAt),
                      style: const TextStyle(
                        fontSize: 12,
                        color: Color(0xFF64748B),
                      ),
                    ),
                  ),
              ],
            ),
    );
  }
}

class _CurrencyTile extends StatelessWidget {
  const _CurrencyTile({required this.currency, required this.amount});

  final String currency;
  final num amount;

  @override
  Widget build(BuildContext context) {
    return Container(
      width: 220,
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: const Color(0xFFF1F5F9),
        borderRadius: BorderRadius.circular(12),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(
            currency,
            style: const TextStyle(
              fontWeight: FontWeight.w700,
              color: Color(0xFF0D9488),
            ),
          ),
          const SizedBox(height: 6),
          Text(
            Formatters.money(amount, currency),
            style: const TextStyle(
              fontSize: 20,
              fontWeight: FontWeight.w700,
              color: Color(0xFF0F172A),
            ),
          ),
        ],
      ),
    );
  }
}

class _EmptyHint extends StatelessWidget {
  const _EmptyHint(this.text);
  final String text;

  @override
  Widget build(BuildContext context) {
    return Text(text, style: const TextStyle(color: Color(0xFF94A3B8)));
  }
}

/// Adds a small refresh button row above scrollable page content.
class RefreshableBody extends StatelessWidget {
  const RefreshableBody({
    super.key,
    required this.child,
    required this.onRefresh,
  });

  final Widget child;
  final VoidCallback onRefresh;

  @override
  Widget build(BuildContext context) {
    return Stack(
      children: [
        child,
        Positioned(
          top: 16,
          right: 28,
          child: IconButton.filledTonal(
            tooltip: 'Refresh',
            onPressed: onRefresh,
            icon: const Icon(Icons.refresh),
          ),
        ),
      ],
    );
  }
}
