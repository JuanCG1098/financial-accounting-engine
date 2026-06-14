import 'package:flutter/material.dart';

/// A sober, professional financial-dashboard theme.
class AppTheme {
  static const Color _navy = Color(0xFF0F172A); // slate-900
  static const Color _primary = Color(0xFF1D4ED8); // blue-700
  static const Color _teal = Color(0xFF0D9488); // teal-600
  static const Color _surface = Color(0xFFF8FAFC); // slate-50

  // Semantic status colors.
  static const Color processed = Color(0xFF16A34A); // green-600
  static const Color failed = Color(0xFFDC2626); // red-600
  static const Color pending = Color(0xFFD97706); // amber-600
  static const Color processing = Color(0xFF2563EB); // blue-600

  static const Color sidebarBackground = _navy;

  static ThemeData get light {
    final scheme = ColorScheme.fromSeed(
      seedColor: _primary,
      primary: _primary,
      secondary: _teal,
      surface: Colors.white,
    ).copyWith(surfaceContainerLowest: _surface);

    return ThemeData(
      useMaterial3: true,
      colorScheme: scheme,
      scaffoldBackgroundColor: _surface,
      fontFamily: 'Roboto',
      cardTheme: CardThemeData(
        elevation: 0,
        color: Colors.white,
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(14),
          side: const BorderSide(color: Color(0xFFE2E8F0)),
        ),
        margin: EdgeInsets.zero,
      ),
      appBarTheme: const AppBarTheme(
        backgroundColor: Colors.white,
        foregroundColor: _navy,
        elevation: 0,
        scrolledUnderElevation: 0.5,
        surfaceTintColor: Colors.transparent,
      ),
      dividerTheme: const DividerThemeData(color: Color(0xFFE2E8F0), space: 1),
      inputDecorationTheme: InputDecorationTheme(
        filled: true,
        fillColor: Colors.white,
        border: OutlineInputBorder(
          borderRadius: BorderRadius.circular(10),
          borderSide: const BorderSide(color: Color(0xFFCBD5E1)),
        ),
        enabledBorder: OutlineInputBorder(
          borderRadius: BorderRadius.circular(10),
          borderSide: const BorderSide(color: Color(0xFFCBD5E1)),
        ),
      ),
      filledButtonTheme: FilledButtonThemeData(
        style: FilledButton.styleFrom(
          shape:
              RoundedRectangleBorder(borderRadius: BorderRadius.circular(10)),
          padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 16),
        ),
      ),
    );
  }
}
