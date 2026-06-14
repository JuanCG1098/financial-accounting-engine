import 'package:intl/intl.dart';

/// Formatting helpers for money and dates used across the dashboard.
class Formatters {
  static final _dateTime = DateFormat('yyyy-MM-dd HH:mm');
  static final _date = DateFormat('yyyy-MM-dd');

  static String money(num value, String currency) {
    final formatter = NumberFormat.currency(
      symbol: '$currency ',
      decimalDigits: 2,
    );
    return formatter.format(value);
  }

  static String number(num value) =>
      NumberFormat.decimalPattern().format(value);

  static String dateTime(DateTime value) => _dateTime.format(value.toLocal());

  static String date(DateTime value) => _date.format(value.toLocal());

  /// Turns SCREAMING_SNAKE_CASE enum values into a readable label, e.g.
  /// `CASH_DEPOSIT` -> `Cash Deposit`.
  static String label(String enumValue) {
    return enumValue
        .split('_')
        .map((w) => w.isEmpty
            ? w
            : '${w[0].toUpperCase()}${w.substring(1).toLowerCase()}')
        .join(' ');
  }
}
