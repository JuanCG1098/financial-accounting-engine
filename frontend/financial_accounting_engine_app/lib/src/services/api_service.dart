import 'dart:convert';

import 'package:http/http.dart' as http;

import '../config/app_config.dart';
import '../models/models.dart';

/// Thrown when the API returns a non-success status code.
class ApiException implements Exception {
  ApiException(this.message, {this.statusCode});
  final String message;
  final int? statusCode;

  @override
  String toString() => message;
}

/// Typed wrapper around the Financial Accounting Engine REST API.
class ApiService {
  ApiService({http.Client? client, String? baseUrl})
      : _client = client ?? http.Client(),
        _baseUrl = baseUrl ?? AppConfig.apiBaseUrl;

  final http.Client _client;
  final String _baseUrl;

  Uri _uri(String path, [Map<String, String>? query]) {
    final base = Uri.parse('$_baseUrl$path');
    if (query == null || query.isEmpty) return base;
    return base.replace(queryParameters: {...base.queryParameters, ...query});
  }

  // ---- Dashboard ----
  Future<DashboardSummaryModel> getDashboardSummary() async {
    final json = await _getJson('/api/dashboard/summary') as Map<String, dynamic>;
    return DashboardSummaryModel.fromJson(json);
  }

  // ---- Transactions ----
  Future<List<TransactionModel>> listTransactions({
    String? status,
    String? currency,
    String? type,
  }) async {
    final query = <String, String>{
      if (status != null) 'status': status,
      if (currency != null) 'currency': currency,
      if (type != null) 'type': type,
    };
    final json = await _getJson('/api/transactions', query) as List<dynamic>;
    return json
        .map((e) => TransactionModel.fromJson(e as Map<String, dynamic>))
        .toList();
  }

  Future<TransactionModel> getTransaction(String id) async {
    final json = await _getJson('/api/transactions/$id') as Map<String, dynamic>;
    return TransactionModel.fromJson(json);
  }

  Future<TransactionModel> createTransaction({
    required String type,
    required String currency,
    required num amount,
    required String branchCode,
    required String costCenter,
    required String description,
  }) async {
    final json = await _postJson('/api/transactions', {
      'type': type,
      'currency': currency,
      'amount': amount,
      'branchCode': branchCode,
      'costCenter': costCenter,
      'description': description,
    }) as Map<String, dynamic>;
    return TransactionModel.fromJson(json);
  }

  Future<TransactionModel> processTransaction(String id) async {
    final json =
        await _postJson('/api/transactions/$id/process', null) as Map<String, dynamic>;
    return TransactionModel.fromJson(json);
  }

  // ---- Accounting entries ----
  Future<List<AccountingEntryModel>> listAccountingEntries() async {
    final json = await _getJson('/api/accounting-entries') as List<dynamic>;
    return json
        .map((e) => AccountingEntryModel.fromJson(e as Map<String, dynamic>))
        .toList();
  }

  Future<AccountingEntryModel?> getEntryByTransaction(String transactionId) async {
    final response = await _client.get(
      _uri('/api/accounting-entries/by-transaction/$transactionId'),
      headers: _jsonHeaders,
    );
    if (response.statusCode == 404) return null;
    final json = _decode(response) as Map<String, dynamic>;
    return AccountingEntryModel.fromJson(json);
  }

  // ---- Accounting rules ----
  Future<List<AccountingRuleModel>> listRules() async {
    final json = await _getJson('/api/accounting-rules') as List<dynamic>;
    return json
        .map((e) => AccountingRuleModel.fromJson(e as Map<String, dynamic>))
        .toList();
  }

  Future<AccountingRuleModel> createRule(Map<String, dynamic> payload) async {
    final json = await _postJson('/api/accounting-rules', payload)
        as Map<String, dynamic>;
    return AccountingRuleModel.fromJson(json);
  }

  Future<AccountingRuleModel> activateRule(String id) =>
      _patchRule('/api/accounting-rules/$id/activate');

  Future<AccountingRuleModel> deactivateRule(String id) =>
      _patchRule('/api/accounting-rules/$id/deactivate');

  Future<AccountingRuleModel> _patchRule(String path) async {
    final response = await _client.patch(_uri(path), headers: _jsonHeaders);
    final json = _decode(response) as Map<String, dynamic>;
    return AccountingRuleModel.fromJson(json);
  }

  // ---- Audit ----
  Future<List<AuditEventModel>> listAudit() async {
    final json = await _getJson('/api/audit') as List<dynamic>;
    return json
        .map((e) => AuditEventModel.fromJson(e as Map<String, dynamic>))
        .toList();
  }

  Future<List<AuditEventModel>> listAuditByTransaction(String transactionId) async {
    final json =
        await _getJson('/api/audit/by-transaction/$transactionId') as List<dynamic>;
    return json
        .map((e) => AuditEventModel.fromJson(e as Map<String, dynamic>))
        .toList();
  }

  // ---- Internals ----
  static const _jsonHeaders = {'Content-Type': 'application/json'};

  Future<dynamic> _getJson(String path, [Map<String, String>? query]) async {
    final response = await _client.get(_uri(path, query), headers: _jsonHeaders);
    return _decode(response);
  }

  Future<dynamic> _postJson(String path, Object? body) async {
    final response = await _client.post(
      _uri(path),
      headers: _jsonHeaders,
      body: body == null ? null : jsonEncode(body),
    );
    return _decode(response);
  }

  dynamic _decode(http.Response response) {
    if (response.statusCode >= 200 && response.statusCode < 300) {
      if (response.body.isEmpty) return null;
      return jsonDecode(response.body);
    }
    throw ApiException(_extractError(response), statusCode: response.statusCode);
  }

  String _extractError(http.Response response) {
    try {
      final body = jsonDecode(response.body);
      if (body is Map<String, dynamic>) {
        // ProblemDetails: prefer detail, then title, then validation errors.
        if (body['detail'] is String) return body['detail'] as String;
        if (body['errors'] is Map<String, dynamic>) {
          final errors = (body['errors'] as Map<String, dynamic>)
              .values
              .expand((v) => v is List ? v : [v])
              .join('\n');
          if (errors.isNotEmpty) return errors;
        }
        if (body['title'] is String) return body['title'] as String;
      }
    } catch (_) {
      // fall through to a generic message
    }
    return 'Request failed (${response.statusCode}).';
  }

  void dispose() => _client.close();
}
