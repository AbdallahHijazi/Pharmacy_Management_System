namespace Pharmacy.WinForms.Models;

internal sealed class PosProductView
{
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

    internal static string ResolveDisplayName(PosProductApiModel api)
    {
        foreach (var candidate in new[]
                 {
                     api.TradeName,
                     api.CommercialName,
                     api.Name,
                     api.ProductName,
                     api.ScientificName,
                     api.Barcode,
                     api.Sku,
                     api.Code
                 })
        {
            if (!string.IsNullOrWhiteSpace(candidate))
            {
                return candidate.Trim();
            }
        }

        return "منتج بدون اسم";
    }

    internal static string ResolveSubtitle(PosProductApiModel api, string displayName)
    {
        var displayKey = displayName.Trim();

        if (!string.IsNullOrWhiteSpace(api.ScientificName)
            && !string.Equals(api.ScientificName.Trim(), displayKey, StringComparison.OrdinalIgnoreCase))
        {
            return api.ScientificName.Trim();
        }

        if (!string.IsNullOrWhiteSpace(api.CategoryName)
            && !string.Equals(api.CategoryName.Trim(), displayKey, StringComparison.OrdinalIgnoreCase))
        {
            return api.CategoryName.Trim();
        }

        var barcodeOrSku = !string.IsNullOrWhiteSpace(api.Barcode)
            ? api.Barcode.Trim()
            : !string.IsNullOrWhiteSpace(api.Sku)
                ? api.Sku.Trim()
                : !string.IsNullOrWhiteSpace(api.Code)
                    ? api.Code.Trim()
                    : string.Empty;

        if (!string.IsNullOrWhiteSpace(barcodeOrSku)
            && !string.Equals(barcodeOrSku, displayKey, StringComparison.OrdinalIgnoreCase))
        {
            return barcodeOrSku;
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
