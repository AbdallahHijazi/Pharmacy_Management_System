namespace Pharmacy.WinForms.Models;

internal sealed class PosProductView
{
    public Guid ProductId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string ScientificName { get; init; } = string.Empty;
    public string Barcode { get; init; } = string.Empty;
    public string CategoryName { get; init; } = string.Empty;
    public decimal SellingPrice { get; init; }
    public int PricingType { get; init; }
    public int SellableQuantity { get; init; }

    public bool IsLowStock => SellableQuantity > 0 && SellableQuantity <= 5;
    public bool IsOutOfStock => SellableQuantity <= 0;
    public bool ShowRxBadge => PricingType == 1;

    public static PosProductView FromApi(PosProductApiModel api) => new()
    {
        ProductId = api.ProductId,
        Name = api.Name,
        ScientificName = api.ScientificName,
        Barcode = api.Barcode,
        CategoryName = api.CategoryName,
        SellingPrice = api.SellingPrice,
        PricingType = api.PricingType,
        SellableQuantity = api.SellableQuantity
    };
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
