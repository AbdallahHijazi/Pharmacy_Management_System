using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Pharmacy.Application.Common.Accounting;
using Pharmacy.Domain.Entities.Catalog;
using Pharmacy.Domain.Entities.Inventory;
using Pharmacy.Domain.Enums;
using Pharmacy.Infrastructure.Persistence;
using Xunit;

namespace Pharmacy.IntegrationTests;

[Collection("inventory-integration")]
public sealed class StockFlowIntegrationTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly PharmacyWebApplicationFactory _factory;

    public StockFlowIntegrationTests(PharmacyWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private HttpClient CreateClient() => _factory.CreateClient();

    private async Task AuthorizeAsync(HttpClient client, CancellationToken ct = default)
    {
        var login = new { email = "admin@pharmacy.com", password = "Admin@123" };
        var loginResp = await client.PostAsJsonAsync("/api/Auth/login", login, ct);
        loginResp.EnsureSuccessStatusCode();
        var loginJson = await loginResp.Content.ReadAsStringAsync(ct);
        using var doc = JsonDocument.Parse(loginJson);
        var token = doc.RootElement.GetProperty("token").GetString()!;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    private async Task<T> QueryDbAsync<T>(Func<AppDbContext, Task<T>> query)
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await query(db);
    }

    [Fact]
    public async Task Purchase_invoice_increases_available_stock()
    {
        var uid = Guid.NewGuid().ToString("N")[..10];
        using var client = CreateClient();
        await AuthorizeAsync(client);

        var categoryId = await CreateCategoryAsync(client, $"c-{uid}");
        var supplierId = await CreateSupplierAsync(client, $"s-{uid}");
        var productId = await CreateProductAsync(client, $"p-{uid}", $"bc-{uid}", categoryId, supplierId);

        var qtyBefore = await SumAvailableAsync(productId);

        var purchaseResp = await client.PostAsJsonAsync(
            "/api/v1/purchase-invoices",
            new
            {
                invoiceNumber = $"PI-{uid}",
                supplierId,
                taxRate = 0m,
                paidAmount = 60m,
                paymentMethod = "Cash",
                items = new[]
                {
                    new
                    {
                        productId,
                        batchNumber = $"B-{uid}",
                        expiryDate = DateTime.UtcNow.AddYears(2),
                        quantity = 12,
                        unitPrice = 5m
                    }
                }
            });

        purchaseResp.EnsureSuccessStatusCode();

        var qtyAfter = await SumAvailableAsync(productId);
        Assert.Equal(qtyBefore + 12, qtyAfter);
    }

    [Fact]
    public async Task Sale_consumes_earliest_expiring_batch_first()
    {
        var uid = Guid.NewGuid().ToString("N")[..10];
        using var client = CreateClient();
        await AuthorizeAsync(client);

        var categoryId = await CreateCategoryAsync(client, $"c2-{uid}");
        var supplierId = await CreateSupplierAsync(client, $"s2-{uid}");
        var productId = await CreateProductAsync(client, $"p2-{uid}", $"bc2-{uid}", categoryId, supplierId);

        await client.PostAsJsonAsync(
            "/api/v1/purchase-invoices",
            new
            {
                invoiceNumber = $"PI-E-{uid}",
                supplierId,
                taxRate = 0m,
                paidAmount = 10m,
                paymentMethod = "Cash",
                items = new[]
                {
                    new
                    {
                        productId,
                        batchNumber = $"EARLY-{uid}",
                        expiryDate = new DateTime(2030, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                        quantity = 10,
                        unitPrice = 1m
                    }
                }
            });
        await client.PostAsJsonAsync(
            "/api/v1/purchase-invoices",
            new
            {
                invoiceNumber = $"PI-L-{uid}",
                supplierId,
                taxRate = 0m,
                paidAmount = 10m,
                paymentMethod = "Cash",
                items = new[]
                {
                    new
                    {
                        productId,
                        batchNumber = $"LATE-{uid}",
                        expiryDate = new DateTime(2035, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                        quantity = 10,
                        unitPrice = 1m
                    }
                }
            });

        var earlyBatchId = await GetBatchIdByBatchNumberAsync($"EARLY-{uid}");
        var lateBatchId = await GetBatchIdByBatchNumberAsync($"LATE-{uid}");

        var saleResp = await client.PostAsJsonAsync(
            "/api/v1/sales-invoices",
            new
            {
                customerId = (Guid?)null,
                discountPercentage = 0m,
                paidAmount = 50m,
                paymentMethod = "Cash",
                items = new[] { new { productId, quantity = 5 } }
            });
        saleResp.EnsureSuccessStatusCode();
        var sale = JsonSerializer.Deserialize<SalesInvoiceResponse>(await saleResp.Content.ReadAsStringAsync(), JsonOptions)!;

        var itemsResp = await client.GetAsync($"/api/v1/sales-invoices/{sale.SalesInvoiceId}/items");
        itemsResp.EnsureSuccessStatusCode();
        var lines = JsonSerializer.Deserialize<List<SalesLineResponse>>(await itemsResp.Content.ReadAsStringAsync(), JsonOptions)!;

        Assert.Single(lines);
        Assert.Equal(earlyBatchId, lines[0].StockBatchId);
        Assert.Equal(5, lines[0].Quantity);

        var earlyAvail = await GetAvailableAsync(earlyBatchId);
        var lateAvail = await GetAvailableAsync(lateBatchId);
        Assert.Equal(5, earlyAvail);
        Assert.Equal(10, lateAvail);
    }

    [Fact]
    public async Task Sales_return_restores_stock()
    {
        var uid = Guid.NewGuid().ToString("N")[..10];
        using var client = CreateClient();
        await AuthorizeAsync(client);

        var categoryId = await CreateCategoryAsync(client, $"c3-{uid}");
        var supplierId = await CreateSupplierAsync(client, $"s3-{uid}");
        var productId = await CreateProductAsync(client, $"p3-{uid}", $"bc3-{uid}", categoryId, supplierId);

        await client.PostAsJsonAsync(
            "/api/v1/purchase-invoices",
            new
            {
                invoiceNumber = $"PI-SR-{uid}",
                supplierId,
                taxRate = 0m,
                paidAmount = 20m,
                paymentMethod = "Cash",
                items = new[]
                {
                    new
                    {
                        productId,
                        batchNumber = $"B-SR-{uid}",
                        expiryDate = DateTime.UtcNow.AddYears(1),
                        quantity = 10,
                        unitPrice = 2m
                    }
                }
            });

        var batchId = await GetBatchIdByBatchNumberAsync($"B-SR-{uid}");
        var beforeSale = await GetAvailableAsync(batchId);

        var saleResp = await client.PostAsJsonAsync(
            "/api/v1/sales-invoices",
            new
            {
                customerId = (Guid?)null,
                discountPercentage = 0m,
                paidAmount = 40m,
                paymentMethod = "Cash",
                items = new[] { new { productId, quantity = 4 } }
            });
        saleResp.EnsureSuccessStatusCode();
        var sale = JsonSerializer.Deserialize<SalesInvoiceResponse>(await saleResp.Content.ReadAsStringAsync(), JsonOptions)!;

        var afterSale = await GetAvailableAsync(batchId);
        Assert.Equal(beforeSale - 4, afterSale);

        var itemsResp = await client.GetAsync($"/api/v1/sales-invoices/{sale.SalesInvoiceId}/items");
        itemsResp.EnsureSuccessStatusCode();
        var lines = JsonSerializer.Deserialize<List<SalesLineResponse>>(await itemsResp.Content.ReadAsStringAsync(), JsonOptions)!;
        var lineId = lines[0].SalesInvoiceItemId;

        var retResp = await client.PostAsJsonAsync(
            "/api/v1/sales-returns",
            new
            {
                salesInvoiceId = sale.SalesInvoiceId,
                reason = "test return",
                items = new[] { new { salesInvoiceItemId = lineId, quantity = 2 } }
            });
        retResp.EnsureSuccessStatusCode();

        var afterReturn = await GetAvailableAsync(batchId);
        Assert.Equal(afterSale + 2, afterReturn);
    }

    [Fact]
    public async Task Purchase_return_reduces_stock()
    {
        var uid = Guid.NewGuid().ToString("N")[..10];
        using var client = CreateClient();
        await AuthorizeAsync(client);

        var categoryId = await CreateCategoryAsync(client, $"c4-{uid}");
        var supplierId = await CreateSupplierAsync(client, $"s4-{uid}");
        var productId = await CreateProductAsync(client, $"p4-{uid}", $"bc4-{uid}", categoryId, supplierId);

        var purchaseResp = await client.PostAsJsonAsync(
            "/api/v1/purchase-invoices",
            new
            {
                invoiceNumber = $"PI-PR-{uid}",
                supplierId,
                taxRate = 0m,
                paidAmount = 30m,
                paymentMethod = "Cash",
                items = new[]
                {
                    new
                    {
                        productId,
                        batchNumber = $"B-PR-{uid}",
                        expiryDate = DateTime.UtcNow.AddYears(1),
                        quantity = 10,
                        unitPrice = 3m
                    }
                }
            });
        purchaseResp.EnsureSuccessStatusCode();
        var purchase = JsonSerializer.Deserialize<PurchaseInvoiceResponse>(await purchaseResp.Content.ReadAsStringAsync(), JsonOptions)!;

        var batchId = await GetBatchIdByBatchNumberAsync($"B-PR-{uid}");
        var before = await GetAvailableAsync(batchId);

        var prResp = await client.PostAsJsonAsync(
            "/api/v1/purchase-returns",
            new
            {
                purchaseInvoiceId = purchase.PurchaseInvoiceId,
                reason = "supplier return",
                items = new[] { new { stockBatchId = batchId, quantity = 3 } }
            });
        prResp.EnsureSuccessStatusCode();
        var prDetails = JsonSerializer.Deserialize<PurchaseReturnDetailsResponse>(
            await prResp.Content.ReadAsStringAsync(),
            JsonOptions)!;
        Assert.Equal(9m, prDetails.RefundAmount);

        var after = await GetAvailableAsync(batchId);
        Assert.Equal(before - 3, after);
    }

    [Fact]
    public async Task Purchase_return_with_bonus_uses_effective_unit_cost_for_refund()
    {
        var uid = Guid.NewGuid().ToString("N")[..10];
        using var client = CreateClient();
        await AuthorizeAsync(client);

        var categoryId = await CreateCategoryAsync(client, $"c-prb-{uid}");
        var supplierId = await CreateSupplierAsync(client, $"s-prb-{uid}");
        var productId = await CreateProductAsync(client, $"p-prb-{uid}", $"bc-prb-{uid}", categoryId, supplierId);

        var purchaseResp = await client.PostAsJsonAsync(
            "/api/v1/purchase-invoices",
            new
            {
                invoiceNumber = $"PI-PRB-{uid}",
                supplierId,
                taxRate = 0m,
                paidAmount = 5000m,
                paymentMethod = "Cash",
                items = new[]
                {
                    new
                    {
                        productId,
                        batchNumber = $"B-PRB-{uid}",
                        expiryDate = DateTime.UtcNow.AddYears(1),
                        quantity = 50,
                        bonusQuantity = 15,
                        unitPrice = 100m
                    }
                }
            });
        purchaseResp.EnsureSuccessStatusCode();
        var purchase = JsonSerializer.Deserialize<PurchaseInvoiceResponse>(await purchaseResp.Content.ReadAsStringAsync(), JsonOptions)!;

        var batchId = await GetBatchIdByBatchNumberAsync($"B-PRB-{uid}");
        var before = await GetAvailableAsync(batchId);
        Assert.Equal(65, before);

        const int returnQty = 10;
        var expectedRefund = Math.Round(returnQty * 50m * 100m / 65m, 2, MidpointRounding.AwayFromZero);

        var prResp = await client.PostAsJsonAsync(
            "/api/v1/purchase-returns",
            new
            {
                purchaseInvoiceId = purchase.PurchaseInvoiceId,
                reason = "bonus batch return",
                items = new[] { new { stockBatchId = batchId, quantity = returnQty } }
            });
        prResp.EnsureSuccessStatusCode();
        var prDetails = JsonSerializer.Deserialize<PurchaseReturnDetailsResponse>(
            await prResp.Content.ReadAsStringAsync(),
            JsonOptions)!;
        Assert.Equal(expectedRefund, prDetails.RefundAmount);

        var itemsResp = await client.GetAsync($"/api/v1/purchase-returns/{prDetails.PurchaseReturnId}/items");
        itemsResp.EnsureSuccessStatusCode();
        var lines = JsonSerializer.Deserialize<List<PurchaseReturnLineResponse>>(
            await itemsResp.Content.ReadAsStringAsync(),
            JsonOptions)!;
        Assert.Single(lines);
        Assert.Equal(Math.Round(50m * 100m / 65m, 2, MidpointRounding.AwayFromZero), lines[0].UnitPrice);
        Assert.Equal(lines[0].UnitPrice * returnQty, lines[0].Subtotal);

        var after = await GetAvailableAsync(batchId);
        Assert.Equal(55, after);

        var nominalRefund = returnQty * 100m;
        Assert.NotEqual(nominalRefund, prDetails.RefundAmount);
    }

    [Fact]
    public async Task Adjust_stock_creates_inventory_transaction()
    {
        var uid = Guid.NewGuid().ToString("N")[..10];
        using var client = CreateClient();
        await AuthorizeAsync(client);

        var categoryId = await CreateCategoryAsync(client, $"c5-{uid}");
        var supplierId = await CreateSupplierAsync(client, $"s5-{uid}");
        var productId = await CreateProductAsync(client, $"p5-{uid}", $"bc5-{uid}", categoryId, supplierId);

        await client.PostAsJsonAsync(
            "/api/v1/purchase-invoices",
            new
            {
                invoiceNumber = $"PI-ADJ-{uid}",
                supplierId,
                taxRate = 0m,
                paidAmount = 10m,
                paymentMethod = "Cash",
                items = new[]
                {
                    new
                    {
                        productId,
                        batchNumber = $"B-ADJ-{uid}",
                        expiryDate = DateTime.UtcNow.AddYears(1),
                        quantity = 5,
                        unitPrice = 2m
                    }
                }
            });

        var batchId = await GetBatchIdByBatchNumberAsync($"B-ADJ-{uid}");

        var beforeCount = await CountAdjustmentTransactionsAsync(batchId);

        var adjResp = await client.PostAsJsonAsync(
            "/api/v1/inventory-transactions/adjust-stock",
            new
            {
                stockBatchId = batchId,
                type = nameof(TransactionType.AdjustmentIn),
                quantity = 2,
                reason = "integration test adjustment"
            });
        adjResp.EnsureSuccessStatusCode();

        var afterCount = await CountAdjustmentTransactionsAsync(batchId);
        Assert.Equal(beforeCount + 1, afterCount);

        var lastRefType = await QueryDbAsync(async db =>
            await db.Set<InventoryTransaction>()
                .Where(t => t.StockBatchId == batchId && t.Type == TransactionType.AdjustmentIn)
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => t.ReferenceType)
                .FirstAsync());

        Assert.Equal(ReferenceType.StockBatchAdjustment, lastRefType);

        var batch = await QueryDbAsync(db => db.Set<StockBatch>().AsNoTracking().SingleAsync(b => b.Id == batchId));
        Assert.Equal(7, batch.AvailableQuantity);
        Assert.Equal(7, batch.ReceivedQuantity);
        Assert.Equal(2, batch.BonusQuantity);
    }

    [Fact]
    public async Task Manual_adjustment_out_keeps_effective_unit_cost_and_received_quantity()
    {
        var uid = Guid.NewGuid().ToString("N")[..10];
        using var client = CreateClient();
        await AuthorizeAsync(client);

        var categoryId = await CreateCategoryAsync(client, $"c-adj-out-{uid}");
        var supplierId = await CreateSupplierAsync(client, $"s-adj-out-{uid}");
        var productId = await CreateProductAsync(client, $"p-adj-out-{uid}", $"bc-adj-out-{uid}", categoryId, supplierId);

        await client.PostAsJsonAsync(
            "/api/v1/purchase-invoices",
            new
            {
                invoiceNumber = $"PI-AO-{uid}",
                supplierId,
                taxRate = 0m,
                paidAmount = 5000m,
                paymentMethod = "Cash",
                items = new[]
                {
                    new
                    {
                        productId,
                        batchNumber = $"B-AO-{uid}",
                        expiryDate = DateTime.UtcNow.AddYears(1),
                        quantity = 50,
                        bonusQuantity = 15,
                        unitPrice = 100m
                    }
                }
            });

        var batchId = await GetBatchIdByBatchNumberAsync($"B-AO-{uid}");
        var before = await QueryDbAsync(db => db.Set<StockBatch>().AsNoTracking().SingleAsync(b => b.Id == batchId));
        var effectiveBefore = StockBatchEffectiveUnitCost.Calculate(before);

        var adjResp = await client.PostAsJsonAsync(
            "/api/v1/inventory-transactions/adjust-stock",
            new
            {
                stockBatchId = batchId,
                type = nameof(TransactionType.AdjustmentOut),
                quantity = 5,
                reason = "damaged units"
            });
        adjResp.EnsureSuccessStatusCode();

        var after = await QueryDbAsync(db => db.Set<StockBatch>().AsNoTracking().SingleAsync(b => b.Id == batchId));
        Assert.Equal(60, after.AvailableQuantity);
        Assert.Equal(65, after.ReceivedQuantity);
        Assert.Equal(15, after.BonusQuantity);
        Assert.Equal(effectiveBefore, StockBatchEffectiveUnitCost.Calculate(after));
    }

    [Fact]
    public async Task Manual_adjustment_in_syncs_received_quantity_without_bonus_keeps_effective_cost()
    {
        var uid = Guid.NewGuid().ToString("N")[..10];
        using var client = CreateClient();
        await AuthorizeAsync(client);

        var categoryId = await CreateCategoryAsync(client, $"c-adj-in-{uid}");
        var supplierId = await CreateSupplierAsync(client, $"s-adj-in-{uid}");
        var productId = await CreateProductAsync(client, $"p-adj-in-{uid}", $"bc-adj-in-{uid}", categoryId, supplierId);

        await client.PostAsJsonAsync(
            "/api/v1/purchase-invoices",
            new
            {
                invoiceNumber = $"PI-AI-{uid}",
                supplierId,
                taxRate = 0m,
                paidAmount = 20m,
                paymentMethod = "Cash",
                items = new[]
                {
                    new
                    {
                        productId,
                        batchNumber = $"B-AI-{uid}",
                        expiryDate = DateTime.UtcNow.AddYears(1),
                        quantity = 10,
                        unitPrice = 2m
                    }
                }
            });

        var batchId = await GetBatchIdByBatchNumberAsync($"B-AI-{uid}");

        var adjResp = await client.PostAsJsonAsync(
            "/api/v1/inventory-transactions/adjust-stock",
            new
            {
                stockBatchId = batchId,
                type = nameof(TransactionType.AdjustmentIn),
                quantity = 5,
                reason = "stock gain correction"
            });
        adjResp.EnsureSuccessStatusCode();

        var batch = await QueryDbAsync(db => db.Set<StockBatch>().AsNoTracking().SingleAsync(b => b.Id == batchId));
        Assert.Equal(15, batch.ReceivedQuantity);
        Assert.Equal(15, batch.AvailableQuantity);
        Assert.Equal(5, batch.BonusQuantity);
        Assert.Equal(2m * 10m / 15m, StockBatchEffectiveUnitCost.Calculate(batch));
    }

    [Fact]
    public async Task Manual_adjustment_in_with_bonus_dilutes_effective_unit_cost()
    {
        var uid = Guid.NewGuid().ToString("N")[..10];
        using var client = CreateClient();
        await AuthorizeAsync(client);

        var categoryId = await CreateCategoryAsync(client, $"c-adj-bin-{uid}");
        var supplierId = await CreateSupplierAsync(client, $"s-adj-bin-{uid}");
        var productId = await CreateProductAsync(client, $"p-adj-bin-{uid}", $"bc-adj-bin-{uid}", categoryId, supplierId);

        await client.PostAsJsonAsync(
            "/api/v1/purchase-invoices",
            new
            {
                invoiceNumber = $"PI-AIB-{uid}",
                supplierId,
                taxRate = 0m,
                paidAmount = 5000m,
                paymentMethod = "Cash",
                items = new[]
                {
                    new
                    {
                        productId,
                        batchNumber = $"B-AIB-{uid}",
                        expiryDate = DateTime.UtcNow.AddYears(1),
                        quantity = 50,
                        bonusQuantity = 15,
                        unitPrice = 100m
                    }
                }
            });

        var batchId = await GetBatchIdByBatchNumberAsync($"B-AIB-{uid}");
        var effectiveBefore = StockBatchEffectiveUnitCost.Calculate(await QueryDbAsync(db =>
            db.Set<StockBatch>().AsNoTracking().SingleAsync(b => b.Id == batchId)));

        var adjResp = await client.PostAsJsonAsync(
            "/api/v1/inventory-transactions/adjust-stock",
            new
            {
                stockBatchId = batchId,
                type = nameof(TransactionType.AdjustmentIn),
                quantity = 10,
                reason = "found extra units"
            });
        adjResp.EnsureSuccessStatusCode();

        var batch = await QueryDbAsync(db => db.Set<StockBatch>().AsNoTracking().SingleAsync(b => b.Id == batchId));
        Assert.Equal(75, batch.ReceivedQuantity);
        Assert.Equal(75, batch.AvailableQuantity);
        Assert.Equal(25, batch.BonusQuantity);
        Assert.NotEqual(effectiveBefore, StockBatchEffectiveUnitCost.Calculate(batch));
        Assert.Equal(100m * 50m / 75m, StockBatchEffectiveUnitCost.Calculate(batch));
    }

    [Fact]
    public async Task Manual_adjustment_rejects_sale_out_type()
    {
        var uid = Guid.NewGuid().ToString("N")[..10];
        using var client = CreateClient();
        await AuthorizeAsync(client);

        var categoryId = await CreateCategoryAsync(client, $"c-adj-bad-{uid}");
        var supplierId = await CreateSupplierAsync(client, $"s-adj-bad-{uid}");
        var productId = await CreateProductAsync(client, $"p-adj-bad-{uid}", $"bc-adj-bad-{uid}", categoryId, supplierId);

        await client.PostAsJsonAsync(
            "/api/v1/purchase-invoices",
            new
            {
                invoiceNumber = $"PI-AB-{uid}",
                supplierId,
                taxRate = 0m,
                paidAmount = 10m,
                paymentMethod = "Cash",
                items = new[]
                {
                    new
                    {
                        productId,
                        batchNumber = $"B-AB-{uid}",
                        expiryDate = DateTime.UtcNow.AddYears(1),
                        quantity = 5,
                        unitPrice = 2m
                    }
                }
            });

        var batchId = await GetBatchIdByBatchNumberAsync($"B-AB-{uid}");

        var adjResp = await client.PostAsJsonAsync(
            "/api/v1/inventory-transactions/adjust-stock",
            new
            {
                stockBatchId = batchId,
                type = nameof(TransactionType.SaleOut),
                quantity = 1,
                reason = "invalid type"
            });

        Assert.Equal(HttpStatusCode.BadRequest, adjResp.StatusCode);
    }

    [Fact]
    public async Task Manual_adjustment_out_rejects_insufficient_available_quantity()
    {
        var uid = Guid.NewGuid().ToString("N")[..10];
        using var client = CreateClient();
        await AuthorizeAsync(client);

        var categoryId = await CreateCategoryAsync(client, $"c-adj-neg-{uid}");
        var supplierId = await CreateSupplierAsync(client, $"s-adj-neg-{uid}");
        var productId = await CreateProductAsync(client, $"p-adj-neg-{uid}", $"bc-adj-neg-{uid}", categoryId, supplierId);

        await client.PostAsJsonAsync(
            "/api/v1/purchase-invoices",
            new
            {
                invoiceNumber = $"PI-AN-{uid}",
                supplierId,
                taxRate = 0m,
                paidAmount = 10m,
                paymentMethod = "Cash",
                items = new[]
                {
                    new
                    {
                        productId,
                        batchNumber = $"B-AN-{uid}",
                        expiryDate = DateTime.UtcNow.AddYears(1),
                        quantity = 5,
                        unitPrice = 2m
                    }
                }
            });

        var batchId = await GetBatchIdByBatchNumberAsync($"B-AN-{uid}");

        var adjResp = await client.PostAsJsonAsync(
            "/api/v1/inventory-transactions/adjust-stock",
            new
            {
                stockBatchId = batchId,
                type = nameof(TransactionType.AdjustmentOut),
                quantity = 99,
                reason = "too much"
            });

        Assert.Equal(HttpStatusCode.BadRequest, adjResp.StatusCode);
    }

    [Fact]
    public async Task Concurrent_sales_on_same_batch_cannot_oversell()
    {
        var uid = Guid.NewGuid().ToString("N")[..10];
        using var client = CreateClient();
        await AuthorizeAsync(client);

        var categoryId = await CreateCategoryAsync(client, $"c6-{uid}");
        var supplierId = await CreateSupplierAsync(client, $"s6-{uid}");
        var productId = await CreateProductAsync(client, $"p6-{uid}", $"bc6-{uid}", categoryId, supplierId);

        await client.PostAsJsonAsync(
            "/api/v1/purchase-invoices",
            new
            {
                invoiceNumber = $"PI-CC-{uid}",
                supplierId,
                taxRate = 0m,
                paidAmount = 80m,
                paymentMethod = "Cash",
                items = new[]
                {
                    new
                    {
                        productId,
                        batchNumber = $"B-CC-{uid}",
                        expiryDate = DateTime.UtcNow.AddYears(1),
                        quantity = 8,
                        unitPrice = 10m
                    }
                }
            });

        var body = new
        {
            customerId = (Guid?)null,
            discountPercentage = 0m,
            paidAmount = 50m,
            paymentMethod = "Cash",
            items = new[] { new { productId, quantity = 5 } }
        };

        var t1 = client.PostAsJsonAsync("/api/v1/sales-invoices", body);
        var t2 = client.PostAsJsonAsync("/api/v1/sales-invoices", body);
        await Task.WhenAll(t1, t2);

        var r1 = await t1;
        var r2 = await t2;

        var ok = new[] { r1, r2 }.Count(r => r.StatusCode == HttpStatusCode.Created);
        var bad = new[] { r1, r2 }.Count(r => r.StatusCode == HttpStatusCode.BadRequest);

        Assert.Equal(1, ok);
        Assert.Equal(1, bad);

        var batchId = await GetBatchIdByBatchNumberAsync($"B-CC-{uid}");
        var remaining = await GetAvailableAsync(batchId);
        Assert.Equal(3, remaining);
    }

    [Fact]
    public async Task Purchase_bonus_increases_stock_without_inflating_invoice_subtotal()
    {
        var uid = Guid.NewGuid().ToString("N")[..10];
        using var client = CreateClient();
        await AuthorizeAsync(client);

        var categoryId = await CreateCategoryAsync(client, $"cb-{uid}");
        var supplierId = await CreateSupplierAsync(client, $"sb-{uid}");
        var productId = await CreateProductAsync(client, $"pb-{uid}", $"bcb-{uid}", categoryId, supplierId);

        var purchaseResp = await client.PostAsJsonAsync(
            "/api/v1/purchase-invoices",
            new
            {
                invoiceNumber = $"PI-BONUS-{uid}",
                supplierId,
                taxRate = 0m,
                paidAmount = 5000m,
                paymentMethod = "Cash",
                items = new[]
                {
                    new
                    {
                        productId,
                        batchNumber = $"B-BN-{uid}",
                        expiryDate = DateTime.UtcNow.AddYears(2),
                        quantity = 50,
                        bonusQuantity = 15,
                        unitPrice = 100m
                    }
                }
            });
        purchaseResp.EnsureSuccessStatusCode();
        var purchaseDetails = JsonSerializer.Deserialize<PurchaseInvoiceDetailsResponse>(
            await purchaseResp.Content.ReadAsStringAsync(),
            JsonOptions)!;
        Assert.Equal(5000m, purchaseDetails.Subtotal);
        Assert.Equal(5000m, purchaseDetails.GrandTotal);

        var itemsResp = await client.GetAsync($"/api/v1/purchase-invoices/{purchaseDetails.PurchaseInvoiceId}/items");
        itemsResp.EnsureSuccessStatusCode();
        var lines = JsonSerializer.Deserialize<List<PurchaseLineResponse>>(await itemsResp.Content.ReadAsStringAsync(), JsonOptions)!;
        Assert.Single(lines);
        Assert.Equal(50, lines[0].Quantity);
        Assert.Equal(15, lines[0].BonusQuantity);

        var batchId = await GetBatchIdByBatchNumberAsync($"B-BN-{uid}");
        var batch = await QueryDbAsync(db => db.Set<StockBatch>().AsNoTracking().SingleAsync(b => b.Id == batchId));
        Assert.Equal(65, batch.ReceivedQuantity);
        Assert.Equal(65, batch.AvailableQuantity);
        Assert.Equal(15, batch.BonusQuantity);

        var purchaseInQty = await QueryDbAsync(db => db.Set<InventoryTransaction>()
            .Where(t => t.StockBatchId == batchId && t.Type == TransactionType.PurchaseIn)
            .Select(t => t.Quantity)
            .SingleAsync());
        Assert.Equal(65, purchaseInQty);
    }

    private Task<int> CountAdjustmentTransactionsAsync(Guid batchId) =>
        QueryDbAsync(db => db.Set<InventoryTransaction>()
            .CountAsync(t => t.StockBatchId == batchId && t.Type == TransactionType.AdjustmentIn));

    private Task<int> SumAvailableAsync(Guid productId) =>
        QueryDbAsync(db => db.Set<StockBatch>()
            .Where(b => b.ProductId == productId)
            .SumAsync(b => b.AvailableQuantity));

    private Task<int> GetAvailableAsync(Guid batchId) =>
        QueryDbAsync(db => db.Set<StockBatch>().Where(b => b.Id == batchId).Select(b => b.AvailableQuantity).SingleAsync());

    private Task<Guid> GetBatchIdByBatchNumberAsync(string batchNumber) =>
        QueryDbAsync(db => db.Set<StockBatch>()
            .Where(b => b.BatchNumber == batchNumber)
            .Select(b => b.Id)
            .SingleAsync());

    private async Task<Guid> CreateCategoryAsync(HttpClient client, string name)
    {
        var resp = await client.PostAsJsonAsync("/api/v1/categories", new { name });
        resp.EnsureSuccessStatusCode();
        var dto = JsonSerializer.Deserialize<CategoryResponse>(await resp.Content.ReadAsStringAsync(), JsonOptions)!;
        return dto.CategoryId;
    }

    private async Task<Guid> CreateSupplierAsync(HttpClient client, string name)
    {
        var resp = await client.PostAsJsonAsync(
            "/api/v1/suppliers",
            new { name, contactPerson = "x", phone = "1", address = "a" });
        resp.EnsureSuccessStatusCode();
        var dto = JsonSerializer.Deserialize<SupplierResponse>(await resp.Content.ReadAsStringAsync(), JsonOptions)!;
        return dto.SupplierId;
    }

    private async Task<Guid> CreateProductAsync(
        HttpClient client,
        string name,
        string barcode,
        Guid categoryId,
        Guid supplierId)
    {
        var resp = await client.PostAsJsonAsync(
            "/api/v1/products",
            new
            {
                name,
                scientificName = name,
                barcode,
                categoryId,
                sellingPrice = 10m,
                defaultSupplierId = supplierId
            });
        resp.EnsureSuccessStatusCode();
        var dto = JsonSerializer.Deserialize<ProductResponse>(await resp.Content.ReadAsStringAsync(), JsonOptions)!;
        return dto.ProductId;
    }

    private sealed record SalesInvoiceResponse(Guid SalesInvoiceId);
    private sealed record SalesLineResponse(Guid SalesInvoiceItemId, Guid StockBatchId, int Quantity);
    private sealed record PurchaseInvoiceResponse(Guid PurchaseInvoiceId);
    private sealed record PurchaseReturnDetailsResponse(Guid PurchaseReturnId, decimal RefundAmount);
    private sealed record PurchaseReturnLineResponse(decimal UnitPrice, decimal Subtotal);
    private sealed record PurchaseInvoiceDetailsResponse(Guid PurchaseInvoiceId, decimal Subtotal, decimal GrandTotal);
    private sealed record PurchaseLineResponse(int Quantity, int BonusQuantity);
    private sealed record CategoryResponse(Guid CategoryId);
    private sealed record SupplierResponse(Guid SupplierId);
    private sealed record ProductResponse(Guid ProductId);
}
