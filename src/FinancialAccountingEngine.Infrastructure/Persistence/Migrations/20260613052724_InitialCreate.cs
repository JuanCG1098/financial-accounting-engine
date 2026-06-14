using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinancialAccountingEngine.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "accounting_rules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TransactionType = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    DebitAccountCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DebitAccountName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    CreditAccountCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreditAccountName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    RequiresCashFlow = table.Column<bool>(type: "boolean", nullable: false),
                    IsAccountingOnly = table.Column<bool>(type: "boolean", nullable: false),
                    CostCenterBehavior = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_accounting_rules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "transactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    BranchCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CostCenter = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transactions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "accounting_entries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TransactionId = table.Column<Guid>(type: "uuid", nullable: false),
                    EntryNumber = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    TotalDebit = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalCredit = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    IsBalanced = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_accounting_entries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_accounting_entries_transactions_TransactionId",
                        column: x => x.TransactionId,
                        principalTable: "transactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "audit_events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TransactionId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    PreviousStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    NewStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_events", x => x.Id);
                    table.ForeignKey(
                        name: "FK_audit_events_transactions_TransactionId",
                        column: x => x.TransactionId,
                        principalTable: "transactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "accounting_entry_lines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountingEntryId = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    AccountName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Debit = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Credit = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CostCenter = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_accounting_entry_lines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_accounting_entry_lines_accounting_entries_AccountingEntryId",
                        column: x => x.AccountingEntryId,
                        principalTable: "accounting_entries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_accounting_entries_EntryNumber",
                table: "accounting_entries",
                column: "EntryNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_accounting_entries_TransactionId",
                table: "accounting_entries",
                column: "TransactionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_accounting_entry_lines_AccountingEntryId",
                table: "accounting_entry_lines",
                column: "AccountingEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_accounting_rules_TransactionType_Currency_IsActive",
                table: "accounting_rules",
                columns: new[] { "TransactionType", "Currency", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_audit_events_CreatedAt",
                table: "audit_events",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_audit_events_TransactionId",
                table: "audit_events",
                column: "TransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_transactions_CreatedAt",
                table: "transactions",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_transactions_Currency",
                table: "transactions",
                column: "Currency");

            migrationBuilder.CreateIndex(
                name: "IX_transactions_Status",
                table: "transactions",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "accounting_entry_lines");

            migrationBuilder.DropTable(
                name: "accounting_rules");

            migrationBuilder.DropTable(
                name: "audit_events");

            migrationBuilder.DropTable(
                name: "accounting_entries");

            migrationBuilder.DropTable(
                name: "transactions");
        }
    }
}
