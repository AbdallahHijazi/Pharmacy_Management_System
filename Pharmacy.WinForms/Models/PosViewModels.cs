using System.Text.RegularExpressions;

namespace Pharmacy.WinForms.Models;

internal sealed class PosProductView
{
    private static readonly string[] GeneratedNamePrefixes = ["mt-p-", "mt2-p-", "test-", "seed-"];
    private static readonly Regex GeneratedNamePattern = new(@"^mt\d*-p-", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public Guid ProductId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string ScientificName { get; init; } = string.Empty;
    public string Barcode { get; init; } = string.Empty;
    public string CategoryName { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string Subtitle { get; init; } = string.Empty;
    public decimal SellingPrice { get; init; }
    public int PricingType { get; init; }
    public int SellableQuantity { get; init; }

    public bool IsLowStock => SellableQuantity > 0 && SellableQuantity <= 5;
    public bool IsOutOfStock => SellableQuantity <= 0;
    public bool ShowRxBadge => PricingType == 1;

    public static PosProductView FromApi(PosProductApiModel api)
    {
        var displayName = ResolveDisplayName(api);
        return new PosProductView
        {
            ProductId = api.ProductId,
            Name = api.Name,
            ScientificName = api.ScientificName,
            Barcode = api.Barcode,
            CategoryName = api.CategoryName,
            DisplayName = displayName,
            Subtitle = ResolveSubtitle(api, displayName),
            SellingPrice = api.SellingPrice,
            PricingType = api.PricingType,
            SellableQuantity = api.SellableQuantity
        };
    }

    internal static bool IsGeneratedTestName(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var normalized = value.Trim();
        if (GeneratedNamePrefixes.Any(p => normalized.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        return GeneratedNamePattern.IsMatch(normalized);
    }

    internal static string ResolveDisplayName(PosProductApiModel api)
    {
        foreach (var candidate in new[]
                 {
                     api.TradeName,
                     api.CommercialName,
                     api.ArabicName,
                     api.Name,
                     api.ProductName,
                     api.ScientificName
                 })
        {
            if (!string.IsNullOrWhiteSpace(candidate) && !IsGeneratedTestName(candidate))
            {
                return candidate.Trim();
            }
        }

        foreach (var candidate in new[] { api.Barcode, api.Sku, api.Code })
        {
            if (!string.IsNullOrWhiteSpace(candidate) && !IsGeneratedTestName(candidate))
            {
                return candidate.Trim();
            }
        }

        return "منتج بدون اسم";
    }

    internal static string ResolveSubtitle(PosProductApiModel api, string displayName)
    {
        if (string.Equals(displayName, "منتج بدون اسم", StringComparison.Ordinal))
        {
            var code = FirstNonEmpty(api.Name, api.ProductName, api.Barcode, api.Sku, api.Code);
            return string.IsNullOrWhiteSpace(code) ? string.Empty : $"الكود: {code.Trim()}";
        }

        var displayKey = displayName.Trim();

        if (!string.IsNullOrWhiteSpace(api.ScientificName)
            && !IsGeneratedTestName(api.ScientificName)
            && !string.Equals(api.ScientificName.Trim(), displayKey, StringComparison.OrdinalIgnoreCase))
        {
            return api.ScientificName.Trim();
        }

        if (!string.IsNullOrWhiteSpace(api.CategoryName)
            && !string.Equals(api.CategoryName.Trim(), displayKey, StringComparison.OrdinalIgnoreCase))
        {
            return api.CategoryName.Trim();
        }

        var barcodeOrSku = FirstNonEmpty(api.Barcode, api.Sku, api.Code);
        if (!string.IsNullOrWhiteSpace(barcodeOrSku)
            && !IsGeneratedTestName(barcodeOrSku)
            && !string.Equals(barcodeOrSku, displayKey, StringComparison.OrdinalIgnoreCase))
        {
            return barcodeOrSku;
        }

        return string.Empty;
    }

    private static string FirstNonEmpty(params string?[] values)
    {
        foreach (var value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value.Trim();
            }
        }

        return string.Empty;
    }
}

internal sealed class PosCartLine
{
    public required PosProductView Product { get; init; }
    public int Quantity { get; set; }

    public decimal LineTotal => Product.SellingPrice * Quantity;
}

internal enum PosPaymentUiMode
{
    Cash,
    Credit,
    Card
}
