using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pharmacy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InventoryTransaction_ReferenceTypeDataFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Data-only, idempotent updates. Enum columns use string conversion (Type / ReferenceType).
            //
            // NOT auto-corrected (review manually if reports look wrong):
            // - SaleOut / ReturnIn with ReferenceType inconsistent with ReferenceId (needs join to SalesInvoices / SalesReturns).
            // - PurchaseIn where ReferenceType is not PurchaseInvoice (may be valid custom data).
            // - Rows where ReferenceId does not match the implied parent after any future logic changes.

            // 1) Legacy purchase returns: stock movement stored as AdjustmentOut but ReferenceId is a PurchaseReturns row.
            migrationBuilder.Sql(
                """
                UPDATE it
                SET
                    [Type] = N'PurchaseReturnOut',
                    [ReferenceType] = N'PurchaseReturn'
                FROM [InventoryTransactions] AS it
                WHERE it.[IsDeleted] = CAST(0 AS bit)
                  AND it.[Type] = N'AdjustmentOut'
                  AND it.[ReferenceId] IS NOT NULL
                  AND EXISTS (
                      SELECT 1
                      FROM [PurchaseReturns] AS pr
                      WHERE pr.[Id] = it.[ReferenceId]);
                """);

            // 2) Movement already PurchaseReturnOut but reference label wrong; only when ReferenceId resolves to PurchaseReturns.
            migrationBuilder.Sql(
                """
                UPDATE it
                SET [ReferenceType] = N'PurchaseReturn'
                FROM [InventoryTransactions] AS it
                WHERE it.[IsDeleted] = CAST(0 AS bit)
                  AND it.[Type] = N'PurchaseReturnOut'
                  AND it.[ReferenceType] <> N'PurchaseReturn'
                  AND it.[ReferenceId] IS NOT NULL
                  AND EXISTS (
                      SELECT 1
                      FROM [PurchaseReturns] AS pr
                      WHERE pr.[Id] = it.[ReferenceId]);
                """);

            // 3a) Adjustments / write-offs mislabeled as PurchaseInvoice (ReferenceType only; Type left unchanged).
            migrationBuilder.Sql(
                """
                UPDATE it
                SET [ReferenceType] = N'StockBatchAdjustment'
                FROM [InventoryTransactions] AS it
                WHERE it.[IsDeleted] = CAST(0 AS bit)
                  AND it.[ReferenceType] = N'PurchaseInvoice'
                  AND it.[Type] IN (N'AdjustmentIn', N'AdjustmentOut', N'ExpiredWriteOff');
                """);

            // 3b) Manual batch receipt mislabeled as PurchaseInvoice.
            migrationBuilder.Sql(
                """
                UPDATE it
                SET [ReferenceType] = N'StockBatchManualEntry'
                FROM [InventoryTransactions] AS it
                WHERE it.[IsDeleted] = CAST(0 AS bit)
                  AND it.[ReferenceType] = N'PurchaseInvoice'
                  AND it.[Type] = N'ManualBatchIn';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Irreversible data correction; no-op.
        }
    }
}
