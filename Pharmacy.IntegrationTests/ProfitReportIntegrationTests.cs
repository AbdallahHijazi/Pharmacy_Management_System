using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace Pharmacy.IntegrationTests;

[Collection("inventory-integration")]
public sealed class ProfitReportIntegrationTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly PharmacyWebApplicationFactory _factory;

    public ProfitReportIntegrationTests(PharmacyWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private HttpClient CreateClient() => _factory.CreateClient();

    private static async Task AuthorizeAsync(HttpClient client, CancellationToken ct = default)
    {
        var login = new { email = "admin@pharmacy.com", password = "Admin@123" };
        var loginResp = await client.PostAsJsonAsync("/api/Auth/login", login, ct);
        loginResp.EnsureSuccessStatusCode();
        var loginJson = await loginResp.Content.ReadAsStringAsync(ct);
        using var doc = JsonDocument.Parse(loginJson);
        var token = doc.RootElement.GetProperty("token").GetString()!;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    [Fact]
    public async Task Branch_profit_respects_invoice_discount_and_batch_cogs()
    {
        var uid = Guid.NewGuid().ToString("N")[..10];
        using var client = CreateClient();
        await AuthorizeAsync(client);
        var periodStart = DateTime.UtcNow;

        var categoryId = await CreateCategoryAsync(client, $"pf-c-{uid}");
        var supplierId = await CreateSupplierAsync(client, $"pf-s-{uid}");
        var productId = await CreateProductAsync(client, $"pf-p-{uid}", $"pf-bc-{uid}", categoryId, supplierId);

        await client.PostAsJsonAsync(
            "/api/v1/purchase-invoices",
            new
            {
                invoiceNumber = $"PI-PF-{uid}",
                supplierId,
                taxRate = 0m,
                paidAmount = 40m,
                paymentMethod = "Cash",
                items = new[]
                {
                    new
                    {
                        productId,
                        batchNumber = $"B-PF-{uid}",
                        expiryDate = DateTime.UtcNow.AddYears(1),
                        quantity = 10,
                        unitPrice = 4m
                    }
                }
            });

        var saleResp = await client.PostAsJsonAsync(
            "/api/v1/sales-invoices",
            new
            {
                customerId = (Guid?)null,
                discountPercentage = 10m,
                paidAmount = 90m,
                paymentMethod = "Cash",
                items = new[] { new { productId, quantity = 10 } }
            });
        saleResp.EnsureSuccessStatusCode();

        var periodEnd = DateTime.UtcNow;
        var profitResp = await client.GetAsync(
            $"/api/v1/reports/profit/branch?fromDate={Uri.EscapeDataString(periodStart.ToString("O"))}&toDate={Uri.EscapeDataString(periodEnd.ToString("O"))}");
        profitResp.EnsureSuccessStatusCode();
        var report = JsonSerializer.Deserialize<BranchProfitReportResponse>(
            await profitResp.Content.ReadAsStringAsync(),
            JsonOptions)!;

        Assert.Equal(100m, report.GrossSalesBeforeDiscount);
        Assert.Equal(10m, report.InvoiceDiscountTotal);
        Assert.Equal(90m, report.NetSalesFromInvoices);
        Assert.Equal(0m, report.SalesReturnsRefundTotal);
        Assert.Equal(40m, report.SalesCogsTotal);
        Assert.Equal(40m, report.NetCogs);
        Assert.Equal(50m, report.GrossProfit);
        Assert.Equal(55.5556m, report.GrossProfitMarginPercent!.Value);
    }

    [Fact]
    public async Task Branch_profit_bonus_dilutes_purchase_cost_per_unit()
    {
        var uid = Guid.NewGuid().ToString("N")[..10];
        using var client = CreateClient();
        await AuthorizeAsync(client);
        var periodStart = DateTime.UtcNow;

        var categoryId = await CreateCategoryAsync(client, $"pf2-c-{uid}");
        var supplierId = await CreateSupplierAsync(client, $"pf2-s-{uid}");
        var productId = await CreateProductAsync(client, $"pf2-p-{uid}", $"pf2-bc-{uid}", categoryId, supplierId);

        await client.PostAsJsonAsync(
            "/api/v1/purchase-invoices",
            new
            {
                invoiceNumber = $"PI-PF2-{uid}",
                supplierId,
                taxRate = 0m,
                paidAmount = 40m,
                paymentMethod = "Cash",
                items = new[]
                {
                    new
                    {
                        productId,
                        batchNumber = $"B-PF2-{uid}",
                        expiryDate = DateTime.UtcNow.AddYears(1),
                        quantity = 8,
                        bonusQuantity = 2,
                        unitPrice = 5m
                    }
                }
            });

        await client.PostAsJsonAsync(
            "/api/v1/sales-invoices",
            new
            {
                customerId = (Guid?)null,
                discountPercentage = 0m,
                paidAmount = 100m,
                paymentMethod = "Cash",
                items = new[] { new { productId, quantity = 10 } }
            });

        var periodEnd = DateTime.UtcNow;
        var profitResp = await client.GetAsync(
            $"/api/v1/reports/profit/branch?fromDate={Uri.EscapeDataString(periodStart.ToString("O"))}&toDate={Uri.EscapeDataString(periodEnd.ToString("O"))}");
        profitResp.EnsureSuccessStatusCode();
        var report = JsonSerializer.Deserialize<BranchProfitReportResponse>(
            await profitResp.Content.ReadAsStringAsync(),
            JsonOptions)!;

        var expectedCogs = 10m * (8m * 5m / 10m);
        Assert.Equal(100m, report.NetSalesFromInvoices);
        Assert.Equal(expectedCogs, report.SalesCogsTotal);
        Assert.Equal(100m - expectedCogs, report.GrossProfit);
    }

    [Fact]
    public async Task Branch_profit_sales_return_reduces_net_sales_and_restores_cogs()
    {
        var uid = Guid.NewGuid().ToString("N")[..10];
        using var client = CreateClient();
        await AuthorizeAsync(client);
        var periodStart = DateTime.UtcNow;

        var categoryId = await CreateCategoryAsync(client, $"pf3-c-{uid}");
        var supplierId = await CreateSupplierAsync(client, $"pf3-s-{uid}");
        var productId = await CreateProductAsync(client, $"pf3-p-{uid}", $"pf3-bc-{uid}", categoryId, supplierId);

        await client.PostAsJsonAsync(
            "/api/v1/purchase-invoices",
            new
            {
                invoiceNumber = $"PI-PF3-{uid}",
                supplierId,
                taxRate = 0m,
                paidAmount = 40m,
                paymentMethod = "Cash",
                items = new[]
                {
                    new
                    {
                        productId,
                        batchNumber = $"B-PF3-{uid}",
                        expiryDate = DateTime.UtcNow.AddYears(1),
                        quantity = 10,
                        unitPrice = 4m
                    }
                }
            });

        var saleResp = await client.PostAsJsonAsync(
            "/api/v1/sales-invoices",
            new
            {
                customerId = (Guid?)null,
                discountPercentage = 0m,
                paidAmount = 100m,
                paymentMethod = "Cash",
                items = new[] { new { productId, quantity = 10 } }
            });
        saleResp.EnsureSuccessStatusCode();
        var sale = JsonSerializer.Deserialize<SalesInvoiceIdResponse>(await saleResp.Content.ReadAsStringAsync(), JsonOptions)!;

        var itemsResp = await client.GetAsync($"/api/v1/sales-invoices/{sale.SalesInvoiceId}/items");
        itemsResp.EnsureSuccessStatusCode();
        var lines = JsonSerializer.Deserialize<List<SalesLineIdOnly>>(await itemsResp.Content.ReadAsStringAsync(), JsonOptions)!;
        var lineId = lines[0].SalesInvoiceItemId;

        var retResp = await client.PostAsJsonAsync(
            "/api/v1/sales-returns",
            new
            {
                salesInvoiceId = sale.SalesInvoiceId,
                reason = "profit test",
                items = new[] { new { salesInvoiceItemId = lineId, quantity = 2 } }
            });
        retResp.EnsureSuccessStatusCode();

        var periodEnd = DateTime.UtcNow;
        var profitResp = await client.GetAsync(
            $"/api/v1/reports/profit/branch?fromDate={Uri.EscapeDataString(periodStart.ToString("O"))}&toDate={Uri.EscapeDataString(periodEnd.ToString("O"))}");
        profitResp.EnsureSuccessStatusCode();
        var report = JsonSerializer.Deserialize<BranchProfitReportResponse>(
            await profitResp.Content.ReadAsStringAsync(),
            JsonOptions)!;

        Assert.Equal(100m, report.NetSalesFromInvoices);
        Assert.Equal(20m, report.SalesReturnsRefundTotal);
        Assert.Equal(80m, report.NetSalesAfterReturns);
        Assert.Equal(8m, report.SalesReturnCogsRecoveryTotal);
        Assert.Equal(32m, report.NetCogs);
        Assert.Equal(48m, report.GrossProfit);
    }

    private static async Task<Guid> CreateCategoryAsync(HttpClient client, string name)
    {
        var resp = await client.PostAsJsonAsync("/api/v1/categories", new { name });
        resp.EnsureSuccessStatusCode();
        var dto = JsonSerializer.Deserialize<CategoryResponse>(await resp.Content.ReadAsStringAsync(), JsonOptions)!;
        return dto.CategoryId;
    }

    private static async Task<Guid> CreateSupplierAsync(HttpClient client, string name)
    {
        var resp = await client.PostAsJsonAsync(
            "/api/v1/suppliers",
            new { name, contactPerson = "x", phone = "1", address = "a" });
        resp.EnsureSuccessStatusCode();
        var dto = JsonSerializer.Deserialize<SupplierResponse>(await resp.Content.ReadAsStringAsync(), JsonOptions)!;
        return dto.SupplierId;
    }

    private static async Task<Guid> CreateProductAsync(
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

    private sealed record BranchProfitReportResponse(
        decimal GrossSalesBeforeDiscount,
        decimal InvoiceDiscountTotal,
        decimal NetSalesFromInvoices,
        decimal SalesReturnsRefundTotal,
        decimal NetSalesAfterReturns,
        decimal SalesCogsTotal,
        decimal SalesReturnCogsRecoveryTotal,
        decimal NetCogs,
        decimal GrossProfit,
        decimal? GrossProfitMarginPercent);

    private sealed record SalesInvoiceIdResponse(Guid SalesInvoiceId);
    private sealed record SalesLineIdOnly(Guid SalesInvoiceItemId);
    private sealed record CategoryResponse(Guid CategoryId);
    private sealed record SupplierResponse(Guid SupplierId);
    private sealed record ProductResponse(Guid ProductId);
}
