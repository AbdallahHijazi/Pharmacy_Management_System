using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pharmacy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SalesInvoiceItem_CostTrackingSnapshots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BatchBonusQuantityAtSale",
                table: "SalesInvoiceItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "BatchNominalPurchasePriceAtSale",
                table: "SalesInvoiceItems",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BatchReceivedQuantityAtSale",
                table: "SalesInvoiceItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "UnitEffectiveCostAtSale",
                table: "SalesInvoiceItems",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BatchBonusQuantityAtSale",
                table: "SalesInvoiceItems");

            migrationBuilder.DropColumn(
                name: "BatchNominalPurchasePriceAtSale",
                table: "SalesInvoiceItems");

            migrationBuilder.DropColumn(
                name: "BatchReceivedQuantityAtSale",
                table: "SalesInvoiceItems");

            migrationBuilder.DropColumn(
                name: "UnitEffectiveCostAtSale",
                table: "SalesInvoiceItems");
        }
    }
}
