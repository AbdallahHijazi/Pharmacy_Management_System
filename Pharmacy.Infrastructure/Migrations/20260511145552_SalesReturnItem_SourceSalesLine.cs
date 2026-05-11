using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pharmacy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SalesReturnItem_SourceSalesLine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SalesInvoiceItemId",
                table: "SalesReturnItems",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalesReturnItems_SalesInvoiceItemId",
                table: "SalesReturnItems",
                column: "SalesInvoiceItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_SalesReturnItems_SalesInvoiceItems_SalesInvoiceItemId",
                table: "SalesReturnItems",
                column: "SalesInvoiceItemId",
                principalTable: "SalesInvoiceItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SalesReturnItems_SalesInvoiceItems_SalesInvoiceItemId",
                table: "SalesReturnItems");

            migrationBuilder.DropIndex(
                name: "IX_SalesReturnItems_SalesInvoiceItemId",
                table: "SalesReturnItems");

            migrationBuilder.DropColumn(
                name: "SalesInvoiceItemId",
                table: "SalesReturnItems");
        }
    }
}
