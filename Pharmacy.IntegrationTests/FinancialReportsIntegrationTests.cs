using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace Pharmacy.IntegrationTests;

[Collection("inventory-integration")]
public sealed class FinancialReportsIntegrationTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly PharmacyWebApplicationFactory _factory;

    public FinancialReportsIntegrationTests(PharmacyWebApplicationFactory factory)
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
    public async Task Daily_financial_report_aligns_with_branch_profit_for_same_utc_day()
    {
        var uid = Guid.NewGuid().ToString("N")[..10];
        using var client = CreateClient();
        await AuthorizeAsync(client);

        var day = DateTime.UtcNow.Date;
        var dayStart = DateTime.SpecifyKind(day, DateTimeKind.Utc);
        var dayEnd = dayStart.AddDays(1).AddTicks(-1);

        var categoryId = await CreateCategoryAsync(client, $"fin-c-{uid}");
        var supplierId = await CreateSupplierAsync(client, $"fin-s-{uid}");
        var productId = await CreateProductAsync(client, $"fin-p-{uid}", $"fin-bc-{uid}", categoryId, supplierId);

        await client.PostAsJsonAsync(
            "/api/v1/purchase-invoices",
            new
            {
                invoiceNumber = $"PI-FIN-{uid}",
                supplierId,
                taxRate = 0m,
                paidAmount = 20m,
                paymentMethod = "Cash",
                items = new[]
                {
                    new
                    {
                        productId,
                        batchNumber = $"B-FIN-{uid}",
                        expiryDate = DateTime.UtcNow.AddYears(1),
                        quantity = 10,
                        unitPrice = 2m
                    }
                }
            });

        await client.PostAsJsonAsync(
            "/api/v1/sales-invoices",
            new
            {
                customerId = (Guid?)null,
                discountPercentage = 0m,
                paidAmount = 30m,
                paymentMethod = "Cash",
                items = new[] { new { productId, quantity = 3 } }
            });

        var branchResp = await client.GetAsync(
            $"/api/v1/reports/profit/branch?fromDate={Uri.EscapeDataString(dayStart.ToString("O"))}&toDate={Uri.EscapeDataString(dayEnd.ToString("O"))}");
        branchResp.EnsureSuccessStatusCode();
        var branch = JsonSerializer.Deserialize<BranchProfitJson>(await branchResp.Content.ReadAsStringAsync(), JsonOptions)!;

        var dailyResp = await client.GetAsync(
            $"/api/v1/reports/financial/daily?date={Uri.EscapeDataString(dayStart.ToString("O"))}");
        dailyResp.EnsureSuccessStatusCode();
        var daily = JsonSerializer.Deserialize<DailyFinancialJson>(await dailyResp.Content.ReadAsStringAsync(), JsonOptions)!;

        Assert.Equal(branch.NetSalesAfterReturns, daily.Profit.NetSalesAfterReturns);
        Assert.Equal(branch.NetCogs, daily.Profit.NetCogs);
        Assert.Equal(branch.GrossProfit, daily.Profit.GrossProfit);
        Assert.Equal(branch.NetSalesAfterReturns, daily.NetSalesAfterReturns);
        Assert.Equal(branch.NetCogs, daily.NetCostOfGoodsSold);
    }

    [Fact]
    public async Task Product_profit_ranking_lists_product_after_sale()
    {
        var uid = Guid.NewGuid().ToString("N")[..10];
        using var client = CreateClient();
        await AuthorizeAsync(client);
        var from = DateTime.UtcNow;

        var categoryId = await CreateCategoryAsync(client, $"rk-c-{uid}");
        var supplierId = await CreateSupplierAsync(client, $"rk-s-{uid}");
        var productId = await CreateProductAsync(client, $"rk-p-{uid}", $"rk-bc-{uid}", categoryId, supplierId);

        await client.PostAsJsonAsync(
            "/api/v1/purchase-invoices",
            new
            {
                invoiceNumber = $"PI-RK-{uid}",
                supplierId,
                taxRate = 0m,
                paidAmount = 20m,
                paymentMethod = "Cash",
                items = new[]
                {
                    new
                    {
                        productId,
                        batchNumber = $"B-RK-{uid}",
                        expiryDate = DateTime.UtcNow.AddYears(1),
                        quantity = 10,
                        unitPrice = 2m
                    }
                }
            });

        await client.PostAsJsonAsync(
            "/api/v1/sales-invoices",
            new
            {
                customerId = (Guid?)null,
                discountPercentage = 0m,
                paidAmount = 20m,
                paymentMethod = "Cash",
                items = new[] { new { productId, quantity = 2 } }
            });

        var to = DateTime.UtcNow;
        var rankResp = await client.GetAsync(
            $"/api/v1/reports/financial/products/profit-ranking?fromDate={Uri.EscapeDataString(from.ToString("O"))}&toDate={Uri.EscapeDataString(to.ToString("O"))}&take=5&rank=BestProfit");
        rankResp.EnsureSuccessStatusCode();
        var rank = JsonSerializer.Deserialize<ProductRankJson>(await rankResp.Content.ReadAsStringAsync(), JsonOptions)!;

        Assert.Contains(rank.Rows, r => r.ProductId == productId && r.GrossProfit > 0m);
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

    private sealed record BranchProfitJson(decimal NetSalesAfterReturns, decimal NetCogs, decimal GrossProfit);

    private sealed record ProfitNested(decimal NetSalesAfterReturns, decimal NetCogs, decimal GrossProfit);

    private sealed record DailyFinancialJson(ProfitNested Profit, decimal NetSalesAfterReturns, decimal NetCostOfGoodsSold);
    private sealed record ProductRankJson(List<ProductRowJson> Rows);
    private sealed record ProductRowJson(Guid ProductId, decimal GrossProfit);
    private sealed record CategoryResponse(Guid CategoryId);
    private sealed record SupplierResponse(Guid SupplierId);
    private sealed record ProductResponse(Guid ProductId);
}
