// Plain data models mirroring the API DTOs. Enum-like fields are kept as their
// raw SCREAMING_SNAKE_CASE strings so unexpected values never crash parsing.

class TransactionModel {
  TransactionModel({
    required this.id,
    required this.type,
    required this.currency,
    required this.amount,
    required this.branchCode,
    required this.costCenter,
    required this.description,
    required this.status,
    required this.createdAt,
    this.processedAt,
  });

  final String id;
  final String type;
  final String currency;
  final num amount;
  final String branchCode;
  final String costCenter;
  final String description;
  final String status;
  final DateTime createdAt;
  final DateTime? processedAt;

  factory TransactionModel.fromJson(Map<String, dynamic> json) =>
      TransactionModel(
        id: json['id'] as String,
        type: json['type'] as String,
        currency: json['currency'] as String,
        amount: json['amount'] as num,
        branchCode: json['branchCode'] as String? ?? '',
        costCenter: json['costCenter'] as String? ?? '',
        description: json['description'] as String? ?? '',
        status: json['status'] as String,
        createdAt: DateTime.parse(json['createdAt'] as String),
        processedAt: json['processedAt'] == null
            ? null
            : DateTime.parse(json['processedAt'] as String),
      );
}

class AccountingEntryLineModel {
  AccountingEntryLineModel({
    required this.id,
    required this.accountCode,
    required this.accountName,
    required this.debit,
    required this.credit,
    required this.costCenter,
    required this.description,
  });

  final String id;
  final String accountCode;
  final String accountName;
  final num debit;
  final num credit;
  final String? costCenter;
  final String description;

  factory AccountingEntryLineModel.fromJson(Map<String, dynamic> json) =>
      AccountingEntryLineModel(
        id: json['id'] as String,
        accountCode: json['accountCode'] as String,
        accountName: json['accountName'] as String,
        debit: json['debit'] as num,
        credit: json['credit'] as num,
        costCenter: json['costCenter'] as String?,
        description: json['description'] as String? ?? '',
      );
}

class AccountingEntryModel {
  AccountingEntryModel({
    required this.id,
    required this.transactionId,
    required this.entryNumber,
    required this.currency,
    required this.totalDebit,
    required this.totalCredit,
    required this.isBalanced,
    required this.createdAt,
    required this.lines,
  });

  final String id;
  final String transactionId;
  final String entryNumber;
  final String currency;
  final num totalDebit;
  final num totalCredit;
  final bool isBalanced;
  final DateTime createdAt;
  final List<AccountingEntryLineModel> lines;

  factory AccountingEntryModel.fromJson(Map<String, dynamic> json) =>
      AccountingEntryModel(
        id: json['id'] as String,
        transactionId: json['transactionId'] as String,
        entryNumber: json['entryNumber'] as String,
        currency: json['currency'] as String,
        totalDebit: json['totalDebit'] as num,
        totalCredit: json['totalCredit'] as num,
        isBalanced: json['isBalanced'] as bool,
        createdAt: DateTime.parse(json['createdAt'] as String),
        lines: (json['lines'] as List<dynamic>)
            .map((e) =>
                AccountingEntryLineModel.fromJson(e as Map<String, dynamic>))
            .toList(),
      );
}

class AccountingRuleModel {
  AccountingRuleModel({
    required this.id,
    required this.transactionType,
    required this.currency,
    required this.debitAccountCode,
    required this.debitAccountName,
    required this.creditAccountCode,
    required this.creditAccountName,
    required this.requiresCashFlow,
    required this.isAccountingOnly,
    required this.costCenterBehavior,
    required this.isActive,
    required this.createdAt,
    required this.updatedAt,
  });

  final String id;
  final String transactionType;
  final String? currency;
  final String debitAccountCode;
  final String debitAccountName;
  final String creditAccountCode;
  final String creditAccountName;
  final bool requiresCashFlow;
  final bool isAccountingOnly;
  final String costCenterBehavior;
  final bool isActive;
  final DateTime createdAt;
  final DateTime updatedAt;

  factory AccountingRuleModel.fromJson(Map<String, dynamic> json) =>
      AccountingRuleModel(
        id: json['id'] as String,
        transactionType: json['transactionType'] as String,
        currency: json['currency'] as String?,
        debitAccountCode: json['debitAccountCode'] as String,
        debitAccountName: json['debitAccountName'] as String,
        creditAccountCode: json['creditAccountCode'] as String,
        creditAccountName: json['creditAccountName'] as String,
        requiresCashFlow: json['requiresCashFlow'] as bool,
        isAccountingOnly: json['isAccountingOnly'] as bool,
        costCenterBehavior: json['costCenterBehavior'] as String,
        isActive: json['isActive'] as bool,
        createdAt: DateTime.parse(json['createdAt'] as String),
        updatedAt: DateTime.parse(json['updatedAt'] as String),
      );
}

class AuditEventModel {
  AuditEventModel({
    required this.id,
    required this.transactionId,
    required this.eventType,
    required this.message,
    required this.previousStatus,
    required this.newStatus,
    required this.createdAt,
  });

  final String id;
  final String transactionId;
  final String eventType;
  final String message;
  final String? previousStatus;
  final String? newStatus;
  final DateTime createdAt;

  factory AuditEventModel.fromJson(Map<String, dynamic> json) =>
      AuditEventModel(
        id: json['id'] as String,
        transactionId: json['transactionId'] as String,
        eventType: json['eventType'] as String,
        message: json['message'] as String? ?? '',
        previousStatus: json['previousStatus'] as String?,
        newStatus: json['newStatus'] as String?,
        createdAt: DateTime.parse(json['createdAt'] as String),
      );
}

class CurrencyAmountModel {
  CurrencyAmountModel({required this.currency, required this.totalAmount});

  final String currency;
  final num totalAmount;

  factory CurrencyAmountModel.fromJson(Map<String, dynamic> json) =>
      CurrencyAmountModel(
        currency: json['currency'] as String,
        totalAmount: json['totalAmount'] as num,
      );
}

class DashboardSummaryModel {
  DashboardSummaryModel({
    required this.totalTransactions,
    required this.processedTransactions,
    required this.failedTransactions,
    required this.pendingTransactions,
    required this.totalAccountingEntries,
    required this.totalAmountByCurrency,
    required this.recentTransactions,
    required this.recentAuditEvents,
  });

  final int totalTransactions;
  final int processedTransactions;
  final int failedTransactions;
  final int pendingTransactions;
  final int totalAccountingEntries;
  final List<CurrencyAmountModel> totalAmountByCurrency;
  final List<TransactionModel> recentTransactions;
  final List<AuditEventModel> recentAuditEvents;

  factory DashboardSummaryModel.fromJson(Map<String, dynamic> json) =>
      DashboardSummaryModel(
        totalTransactions: json['totalTransactions'] as int,
        processedTransactions: json['processedTransactions'] as int,
        failedTransactions: json['failedTransactions'] as int,
        pendingTransactions: json['pendingTransactions'] as int,
        totalAccountingEntries: json['totalAccountingEntries'] as int,
        totalAmountByCurrency: (json['totalAmountByCurrency'] as List<dynamic>)
            .map((e) => CurrencyAmountModel.fromJson(e as Map<String, dynamic>))
            .toList(),
        recentTransactions: (json['recentTransactions'] as List<dynamic>)
            .map((e) => TransactionModel.fromJson(e as Map<String, dynamic>))
            .toList(),
        recentAuditEvents: (json['recentAuditEvents'] as List<dynamic>)
            .map((e) => AuditEventModel.fromJson(e as Map<String, dynamic>))
            .toList(),
      );
}

/// Domain option lists shared by forms and filters (mirrors the backend enums).
class DomainOptions {
  static const transactionTypes = <String>[
    'CASH_DEPOSIT',
    'CASH_WITHDRAWAL',
    'INTERNAL_TRANSFER',
    'ACCOUNTING_ADJUSTMENT',
    'ACCOUNTING_ONLY_OPERATION',
  ];

  static const transactionStatuses = <String>[
    'PENDING',
    'PROCESSING',
    'PROCESSED',
    'FAILED',
  ];

  static const currencies = <String>['ARS', 'USD', 'EUR'];

  static const costCenterBehaviors = <String>[
    'NONE',
    'PROPAGATE',
    'DEBIT_LINE_ONLY',
    'CREDIT_LINE_ONLY',
  ];
}
