import 'package:flutter/material.dart';

import '../core/formatters.dart';
import '../core/theme.dart';

/// A colored pill that renders a transaction status (or any labeled state).
class StatusChip extends StatelessWidget {
  const StatusChip(this.status, {super.key});

  final String status;

  Color get _color {
    switch (status.toUpperCase()) {
      case 'PROCESSED':
        return AppTheme.processed;
      case 'FAILED':
        return AppTheme.failed;
      case 'PENDING':
        return AppTheme.pending;
      case 'PROCESSING':
        return AppTheme.processing;
      default:
        return Colors.blueGrey;
    }
  }

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
      decoration: BoxDecoration(
        color: _color.withValues(alpha: 0.12),
        borderRadius: BorderRadius.circular(20),
        border: Border.all(color: _color.withValues(alpha: 0.4)),
      ),
      child: Text(
        Formatters.label(status),
        style: TextStyle(
          color: _color,
          fontWeight: FontWeight.w600,
          fontSize: 12,
        ),
      ),
    );
  }
}

/// A small neutral pill used for boolean/feature tags (e.g. cash flow flags).
class TagChip extends StatelessWidget {
  const TagChip(this.label, {super.key, this.color});

  final String label;
  final Color? color;

  @override
  Widget build(BuildContext context) {
    final c = color ?? Colors.blueGrey;
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
      decoration: BoxDecoration(
        color: c.withValues(alpha: 0.10),
        borderRadius: BorderRadius.circular(20),
        border: Border.all(color: c.withValues(alpha: 0.35)),
      ),
      child: Text(
        label,
        style: TextStyle(color: c, fontWeight: FontWeight.w600, fontSize: 12),
      ),
    );
  }
}
