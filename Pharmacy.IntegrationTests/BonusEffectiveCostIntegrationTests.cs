using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Pharmacy.Application.Common.Accounting;
using Pharmacy.Application.Common.Inventory;
using Pharmacy.Domain.Entities.Catalog;
using Pharmacy.Domain.Enums;
using Pharmacy.Infrastructure.Persistence;
using Xunit;

namespace Pharmacy.IntegrationTests;

/// <summary>
/// End-to-end coverage for bonus quantities, effective unit cost, returns, and branch profit.
/// </summary>
[Collection("inventory-integration")]
public sealed class BonusEffectiveCostIntegrationTests
{
    private const int PaidQty = 50;
    private const int BonusQty = 15;
    private const decimal UnitPurchasePrice = 100m;
    private const decimal SellingPrice = 10m;
    private const int TotalReceived = PaidQty + BonusQty;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private static decimal ExpectedEffectiveUnitCost =>
        UnitPurchasePrice * PaidQty / TotalReceived;

    private readonly PharmacyWebApplicationFactory _factory;

    public BonusEffectiveCostIntegrationTests(PharmacyWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private HttpClient CreateClient() => _factory.CreateClient();

    private async Task<T> QueryDbAsync<T>(Func<AppDbContext, Task<T>> query)
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await query(db);
    }

    private Task<StockBatch> GetBatchAsync(string batchNumber) =>
        QueryDbAsync(db => db.Set<StockBatch>().AsNoTracking().SingleAsync(b => b.BatchNumber == batchNumber));

    private Task<Guid> GetBatchIdAsync(string batchNumber) =>
        QueryDbAsync(db => db.Set<StockBatch>()
            .Where(b => b.BatchNumber == batchNumber)
            .Select(b => b.Id)
            .SingleAsync());

    private static async Task AuthorizeAsync(HttpClient client)
    {
        var loginResp = await client.PostAsJsonAsync("/api/Auth/login", new { email = "admin@pharmacy.com", password = "Admin@123" });
        loginResp.EnsureSuccessStatusCode();
        using var doc = JsonDocument.Parse(await loginResp.Content.ReadAsStringAsync());
        var token = doc.RootElement.GetProperty("token").GetString()!;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    [Fact]
    public async Task Purchase_with_bonus_sets_stock_subtotal_and_effective_cost()
    {
        var uid = NewUid();
        using var client = CreateClient();
        await AuthorizeAsync(client);

        var ctx = await SeedProductAsync(client, uid);
        var batchNumber = $"B-BEC-P-{uid}";

        var purchase = await PostPurchaseWithBonusAsync(client, ctx, uid, batchNumber);
        Assert.Equal(PaidQty * UnitPurchasePrice, purchase.Subtotal);
        Assert.Equal(PaidQty * UnitPurchasePrice, purchase.GrandTotal);

        var linesResp = await client.GetAsync($"/api/v1/purchase-invoices/{purchase.PurchaseInvoiceId}/items");
        linesResp.EnsureSuccessStatusCode();
        var lines = JsonSerializer.Deserialize<List<PurchaseLineDto>>(await linesResp.Content.ReadAsStringAsync(), JsonOptions)!;
        Assert.Single(lines);
        Assert.Equal(PaidQty, lines[0].Quantity);
        Assert.Equal(BonusQty, lines[0].BonusQuantity);

        var batch = await GetBatchAsync(batchNumber);
        Assert.Equal(TotalReceived, batch.ReceivedQuantity);
        Assert.Equal(TotalReceived, batch.AvailableQuantity);
        Assert.Equal(BonusQty, batch.BonusQuantity);
        Assert.Equal(ExpectedEffectiveUnitCost, StockBatchEffectiveUnitCost.Calculate(batch));
        StockBatchManualAdjustment.ValidateInvariants(batch);
    }

    [Fact]
    public async Task Sale_after_bonus_purchase_uses_effective_cost_in_line_and_profit_report()
    {
        var uid = NewUid();
        using var client = CreateClient();
        await AuthorizeAsync(client);
        var periodStart = DateTime.UtcNow;

        var ctx = await SeedProductAsync(client, uid);
        var batchNumber = $"B-BEC-S-{uid}";
        await PostPurchaseWithBonusAsync(client, ctx, uid, batchNumber);

        const int saleQty = 10;
        var sale = await PostSaleAsync(client, ctx.ProductId, saleQty);
        var eff = ExpectedEffectiveUnitCost;

        var itemsResp = await client.GetAsync($"/api/v1/sales-invoices/{sale.SalesInvoiceId}/items");
        itemsResp.EnsureSuccessStatusCode();
        var saleLines = JsonSerializer.Deserialize<List<SalesLineDto>>(await itemsResp.Content.ReadAsStringAsync(), JsonOptions)!;
        Assert.Single(saleLines);
        Assert.Equal(saleQty, saleLines[0].Quantity);
        var unitCostAtSale = saleLines[0].UnitEffectiveCostAtSale!.Value;
        Assert.Equal(eff, unitCostAtSale, precision: 3);
        Assert.Equal(UnitPurchasePrice, saleLines[0].BatchNominalPurchasePriceAtSale);
        Assert.Equal(TotalReceived, saleLines[0].BatchReceivedQuantityAtSale);
        Assert.Equal(BonusQty, saleLines[0].BatchBonusQuantityAtSale);

        var batch = await GetBatchAsync(batchNumber);
        Assert.Equal(TotalReceived - saleQty, batch.AvailableQuantity);

        var report = await GetProfitReportAsync(client, periodStart);
        var expectedCogs = saleQty * unitCostAtSale;
        Assert.Equal(saleQty * SellingPrice, report.NetSalesFromInvoices);
        Assert.Equal(expectedCogs, report.SalesCogsTotal, precision: 3);
        Assert.Equal(report.NetSalesFromInvoices - expectedCogs, report.GrossProfit, precision: 3);
    }

    [Fact]
    public async Task Sales_return_after_bonus_sale_restores_inventory_and_profit()
    {
        var uid = NewUid();
        using var client = CreateClient();
        await AuthorizeAsync(client);
        var periodStart = DateTime.UtcNow;

        var ctx = await SeedProductAsync(client, uid);
        var batchNumber = $"B-BEC-SR-{uid}";
        await PostPurchaseWithBonusAsync(client, ctx, uid, batchNumber);

        const int saleQty = 8;
        const int returnQty = 3;

        var sale = await PostSaleAsync(client, ctx.ProductId, saleQty);
        var batchAfterSale = await GetBatchAsync(batchNumber);
        Assert.Equal(TotalReceived - saleQty, batchAfterSale.AvailableQuantity);

        var itemsResp = await client.GetAsync($"/api/v1/sales-invoices/{sale.SalesInvoiceId}/items");
        itemsResp.EnsureSuccessStatusCode();
        var saleLines = JsonSerializer.Deserialize<List<SalesLineDto>>(await itemsResp.Content.ReadAsStringAsync(), JsonOptions)!;
        var unitCostAtSale = saleLines[0].UnitEffectiveCostAtSale!.Value;
        var lineId = saleLines[0].SalesInvoiceItemId;

        var retResp = await client.PostAsJsonAsync(
            "/api/v1/sales-returns",
            new
            {
                salesInvoiceId = sale.SalesInvoiceId,
                reason = "bonus sale return",
                items = new[] { new { salesInvoiceItemId = lineId, quantity = returnQty } }
            });
        retResp.EnsureSuccessStatusCode();

        var batchAfterReturn = await GetBatchAsync(batchNumber);
        Assert.Equal(TotalReceived - saleQty + returnQty, batchAfterReturn.AvailableQuantity);
        StockBatchManualAdjustment.ValidateInvariants(batchAfterReturn);

        var report = await GetProfitReportAsync(client, periodStart);
        Assert.Equal(saleQty * SellingPrice, report.NetSalesFromInvoices);
        Assert.Equal(returnQty * SellingPrice, report.SalesReturnsRefundTotal);
        Assert.Equal((saleQty - returnQty) * SellingPrice, report.NetSalesAfterReturns);
        Assert.Equal(returnQty * unitCostAtSale, report.SalesReturnCogsRecoveryTotal, precision: 3);
        Assert.Equal((saleQty - returnQty) * unitCostAtSale, report.NetCogs, precision: 3);
        Assert.Equal(report.NetSalesAfterReturns - report.NetCogs, report.GrossProfit, precision: 3);
    }

    [Fact]
    public async Task Purchase_return_after_bonus_purchase_aligns_refund_inventory_and_profit()
    {
        var uid = NewUid();
        using var client = CreateClient();
        await AuthorizeAsync(client);
        var periodStart = DateTime.UtcNow;

        var ctx = await SeedProductAsync(client, uid);
        var batchNumber = $"B-BEC-PR-{uid}";
        var purchase = await PostPurchaseWithBonusAsync(client, ctx, uid, batchNumber);
        var batchId = await GetBatchIdAsync(batchNumber);

        const int returnQty = 10;
        var eff = ExpectedEffectiveUnitCost;
        var expectedRefund = Math.Round(returnQty * eff, 2, MidpointRounding.AwayFromZero);

        var prResp = await client.PostAsJsonAsync(
            "/api/v1/purchase-returns",
            new
            {
                purchaseInvoiceId = purchase.PurchaseInvoiceId,
                reason = "bonus purchase return",
                items = new[] { new { stockBatchId = batchId, quantity = returnQty } }
            });
        prResp.EnsureSuccessStatusCode();
        var pr = JsonSerializer.Deserialize<PurchaseReturnDto>(await prResp.Content.ReadAsStringAsync(), JsonOptions)!;

        var batch = await GetBatchAsync(batchNumber);
        Assert.Equal(expectedRefund, pr.RefundAmount);
        Assert.Equal(TotalReceived - returnQty, batch.AvailableQuantity);
        Assert.Equal(TotalReceived, batch.ReceivedQuantity);
        Assert.Equal(eff, StockBatchEffectiveUnitCost.Calculate(batch));
        StockBatchManualAdjustment.ValidateInvariants(batch);

        var report = await GetProfitReportAsync(client, periodStart);
        Assert.Equal(1, report.PurchaseReturnMovementCount);
        Assert.Equal(returnQty * eff, report.PurchaseReturnCogsRecoveryTotal, precision: 4);
        Assert.Equal(pr.RefundAmount, Math.Round(report.PurchaseReturnCogsRecoveryTotal, 2, MidpointRounding.AwayFromZero));
    }

    [Fact]
    public async Task Manual_adjustment_after_bonus_purchase_preserves_integrity_and_outbound_effective_cost()
    {
        var uid = NewUid();
        using var client = CreateClient();
        await AuthorizeAsync(client);

        var ctx = await SeedProductAsync(client, uid);
        var batchNumber = $"B-BEC-ADJ-{uid}";
        await PostPurchaseWithBonusAsync(client, ctx, uid, batchNumber);
        var batchId = await GetBatchIdAsync(batchNumber);
        var effBefore = ExpectedEffectiveUnitCost;

        var outResp = await client.PostAsJsonAsync(
            "/api/v1/inventory-transactions/adjust-stock",
            new
            {
                stockBatchId = batchId,
                type = nameof(TransactionType.AdjustmentOut),
                quantity = 5,
                reason = "damaged after bonus purchase"
            });
        outResp.EnsureSuccessStatusCode();

        var batch = await GetBatchAsync(batchNumber);
        Assert.Equal(TotalReceived - 5, batch.AvailableQuantity);
        Assert.Equal(TotalReceived, batch.ReceivedQuantity);
        Assert.Equal(BonusQty, batch.BonusQuantity);
        Assert.Equal(effBefore, StockBatchEffectiveUnitCost.Calculate(batch));
        StockBatchManualAdjustment.ValidateInvariants(batch);
        Assert.False(StockBatchEffectiveUnitCost.HasInvalidCostBasis(batch));
    }

    private async Task<BranchProfitReportDto> GetProfitReportAsync(HttpClient client, DateTime periodStart)
    {
        var periodEnd = DateTime.UtcNow;
        var profitResp = await client.GetAsync(
            $"/api/v1/reports/profit/branch?fromDate={Uri.EscapeDataString(periodStart.ToString("O"))}&toDate={Uri.EscapeDataString(periodEnd.ToString("O"))}");
        profitResp.EnsureSuccessStatusCode();
        return JsonSerializer.Deserialize<BranchProfitReportDto>(
            await profitResp.Content.ReadAsStringAsync(),
            JsonOptions)!;
    }

    private async Task<SalesInvoiceIdDto> PostSaleAsync(HttpClient client, Guid productId, int quantity)
    {
        var resp = await client.PostAsJsonAsync(
            "/api/v1/sales-invoices",
            new
            {
                customerId = (Guid?)null,
                discountPercentage = 0m,
                paidAmount = quantity * SellingPrice,
                paymentMethod = "Cash",
                items = new[] { new { productId, quantity } }
            });
        resp.EnsureSuccessStatusCode();
        return JsonSerializer.Deserialize<SalesInvoiceIdDto>(await resp.Content.ReadAsStringAsync(), JsonOptions)!;
    }

    private async Task<PurchaseInvoiceDto> PostPurchaseWithBonusAsync(
        HttpClient client,
        ProductContext ctx,
        string uid,
        string batchNumber)
    {
        var resp = await client.PostAsJsonAsync(
            "/api/v1/purchase-invoices",
            new
            {
                invoiceNumber = $"PI-BEC-{uid}",
                ctx.SupplierId,
                taxRate = 0m,
                paidAmount = PaidQty * UnitPurchasePrice,
                paymentMethod = "Cash",
                items = new[]
                {
                    new
                    {
                        productId = ctx.ProductId,
                        batchNumber,
                        expiryDate = DateTime.UtcNow.AddYears(1),
                        quantity = PaidQty,
                        bonusQuantity = BonusQty,
                        unitPrice = UnitPurchasePrice
                    }
                }
            });
        resp.EnsureSuccessStatusCode();
        return JsonSerializer.Deserialize<PurchaseInvoiceDto>(await resp.Content.ReadAsStringAsync(), JsonOptions)!;
    }

    private static async Task<ProductContext> SeedProductAsync(HttpClient client, string uid)
    {
        var categoryResp = await client.PostAsJsonAsync("/api/v1/categories", new { name = $"bec-c-{uid}" });
        categoryResp.EnsureSuccessStatusCode();
        var categoryId = JsonSerializer.Deserialize<CategoryIdDto>(await categoryResp.Content.ReadAsStringAsync(), JsonOptions)!
            .CategoryId;

        var supplierResp = await client.PostAsJsonAsync(
            "/api/v1/suppliers",
            new { name = $"bec-s-{uid}", contactPerson = "x", phone = "1", address = "a" });
        supplierResp.EnsureSuccessStatusCode();
        var supplierId = JsonSerializer.Deserialize<SupplierIdDto>(await supplierResp.Content.ReadAsStringAsync(), JsonOptions)!
            .SupplierId;

        var productResp = await client.PostAsJsonAsync(
            "/api/v1/products",
            new
            {
                name = $"bec-p-{uid}",
                scientificName = $"bec-p-{uid}",
                barcode = $"bec-bc-{uid}",
                categoryId,
                sellingPrice = SellingPrice,
                defaultSupplierId = supplierId
            });
        productResp.EnsureSuccessStatusCode();
        var productId = JsonSerializer.Deserialize<ProductIdDto>(await productResp.Content.ReadAsStringAsync(), JsonOptions)!
            .ProductId;

        return new ProductContext(categoryId, supplierId, productId);
    }

    private static string NewUid() => Guid.NewGuid().ToString("N")[..10];

    private sealed record ProductContext(Guid CategoryId, Guid SupplierId, Guid ProductId);

    private sealed record PurchaseInvoiceDto(Guid PurchaseInvoiceId, decimal Subtotal, decimal GrandTotal);

    private sealed record PurchaseLineDto(int Quantity, int BonusQuantity);

    private sealed record SalesInvoiceIdDto(Guid SalesInvoiceId);

    private sealed record SalesLineDto(
        Guid SalesInvoiceItemId,
        int Quantity,
        decimal? UnitEffectiveCostAtSale,
        decimal? BatchNominalPurchasePriceAtSale,
        int? BatchReceivedQuantityAtSale,
        int? BatchBonusQuantityAtSale);

    private sealed record PurchaseReturnDto(Guid PurchaseReturnId, decimal RefundAmount);

    private sealed record BranchProfitReportDto(
        decimal NetSalesFromInvoices,
        decimal SalesReturnsRefundTotal,
        decimal NetSalesAfterReturns,
        decimal SalesCogsTotal,
        decimal SalesReturnCogsRecoveryTotal,
        decimal PurchaseReturnCogsRecoveryTotal,
        int PurchaseReturnMovementCount,
        decimal NetCogs,
        decimal GrossProfit);

    private sealed record CategoryIdDto(Guid CategoryId);

    private sealed record ProductIdDto(Guid ProductId);

    private sealed record SupplierIdDto(Guid SupplierId);
}
