using Pharmacy.WinForms.Ui;

namespace Pharmacy.WinForms.Models;

internal enum InventoryProductStatus
{
    Available,
    LowStock,
    OutOfStock,
    Expired,
    ExpiringSoon
}

internal enum InventoryListFilter
{
    All,
    LowStock,
    Expired,
    ExpiringSoon,
    Available,
    OutOfStock
}

internal sealed class InventoryProductView
{
    public Guid ProductId { get; init; }
    public string RawName { get; init; } = string.Empty;
    public string ScientificName { get; init; } = string.Empty;
    public string Barcode { get; init; } = string.Empty;
    public string CategoryName { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string Subtitle { get; init; } = string.Empty;
    public decimal SellingPrice { get; init; }
    public decimal? PurchasePrice { get; init; }
    public int PricingType { get; init; }
    public int TotalAvailableQuantity { get; init; }
    public int SellableQuantity { get; init; }
    public int ExpiredQuantity { get; init; }
    public Guid? DefaultSupplierId { get; init; }
    public InventoryProductStatus Status { get; init; }
    public bool IsExpiringSoon { get; init; }

    public string StatusLabel => Status switch
    {
        InventoryProductStatus.LowStock => "نقص مخزون",
        InventoryProductStatus.OutOfStock => "غير متوفر",
        InventoryProductStatus.Expired => "منتهي",
        InventoryProductStatus.ExpiringSoon => "قريب الانتهاء",
        _ => "متوفر"
    };

    public static InventoryProductView FromApi(InventoryProductApiModel api, bool isExpiringSoon = false)
    {
        var displayName = ResolveDisplayName(api.Name, api.ScientificName, api.Barcode);
        var subtitle = ResolveSubtitle(api, displayName);
        var status = ResolveStatus(api, isExpiringSoon);

        return new InventoryProductView
        {
            ProductId = api.ProductId,
            RawName = api.Name,
            ScientificName = api.ScientificName,
            Barcode = api.Barcode,
            CategoryName = api.CategoryName,
            DisplayName = displayName,
            Subtitle = subtitle,
            SellingPrice = api.SellingPrice,
            PurchasePrice = api.PurchasePrice,
            PricingType = api.PricingType,
            TotalAvailableQuantity = api.TotalAvailableQuantity,
            SellableQuantity = api.SellableQuantity,
            ExpiredQuantity = api.ExpiredQuantity,
            DefaultSupplierId = api.DefaultSupplierId,
            Status = status,
            IsExpiringSoon = isExpiringSoon
        };
    }

    public static InventoryProductView FromDetailsApi(InventoryProductDetailsApiModel api, bool isExpiringSoon = false)
    {
        var wrapper = new InventoryProductApiModel
        {
            ProductId = api.ProductId,
            Name = api.Name,
            ScientificName = api.ScientificName,
            Barcode = api.Barcode,
            CategoryId = api.CategoryId,
            CategoryName = api.CategoryName,
            SellingPrice = api.SellingPrice,
            PricingType = api.PricingType,
            PurchasePrice = api.PurchasePrice,
            DefaultSupplierId = api.DefaultSupplierId,
            TotalAvailableQuantity = api.TotalAvailableQuantity,
            SellableQuantity = api.SellableQuantity,
            ExpiredQuantity = api.ExpiredQuantity
        };

        return FromApi(wrapper, isExpiringSoon);
    }

    internal static string ResolveDisplayName(string name, string scientificName, string barcode)
    {
        foreach (var candidate in new[] { name, scientificName, barcode })
        {
            if (!string.IsNullOrWhiteSpace(candidate) && !PosProductView.IsGeneratedTestName(candidate))
            {
                return candidate.Trim();
            }
        }

        return "منتج بدون اسم";
    }

    private static string ResolveSubtitle(InventoryProductApiModel api, string displayName)
    {
        if (string.Equals(displayName, "منتج بدون اسم", StringComparison.Ordinal))
        {
            var code = FirstNonEmpty(api.Name, api.Barcode);
            return string.IsNullOrWhiteSpace(code) ? string.Empty : $"الكود: {code.Trim()}";
        }

        if (!string.IsNullOrWhiteSpace(api.ScientificName)
            && !PosProductView.IsGeneratedTestName(api.ScientificName)
            && !string.Equals(api.ScientificName.Trim(), displayName, StringComparison.OrdinalIgnoreCase))
        {
            return api.ScientificName.Trim();
        }

        if (!string.IsNullOrWhiteSpace(api.CategoryName)
            && !string.Equals(api.CategoryName.Trim(), displayName, StringComparison.OrdinalIgnoreCase))
        {
            return api.CategoryName.Trim();
        }

        if (!string.IsNullOrWhiteSpace(api.Barcode)
            && !PosProductView.IsGeneratedTestName(api.Barcode)
            && !string.Equals(api.Barcode.Trim(), displayName, StringComparison.OrdinalIgnoreCase))
        {
            return api.Barcode.Trim();
        }

        return string.Empty;
    }

    private static InventoryProductStatus ResolveStatus(InventoryProductApiModel api, bool isExpiringSoon)
    {
        if (api.SellableQuantity <= 0 && api.ExpiredQuantity > 0)
        {
            return InventoryProductStatus.Expired;
        }

        if (api.SellableQuantity <= 0)
        {
            return InventoryProductStatus.OutOfStock;
        }

        if (isExpiringSoon)
        {
            return InventoryProductStatus.ExpiringSoon;
        }

        if (api.SellableQuantity <= PharmaTheme.PosLowStockThreshold)
        {
            return InventoryProductStatus.LowStock;
        }

        return InventoryProductStatus.Available;
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

internal sealed class InventoryStatsView
{
    public int TotalProducts { get; init; }
    public int ActiveAvailable { get; init; }
    public int LowStockCount { get; init; }
    public int ExpiringOrExpiredCount { get; init; }
    public string? ActiveAvailableBadge { get; init; }
    public bool HasData { get; init; }
    public string? ErrorMessage { get; init; }

    public static InventoryStatsView Empty(string? error = null) => new()
    {
        HasData = false,
        ErrorMessage = error
    };
}

internal sealed class InventoryProductDetailsView
{
    public InventoryProductView Product { get; init; } = null!;
    public string SupplierName { get; init; } = "غير متوفر";
    public string ShelfLocation { get; init; } = "غير متوفر";
    public DateTime? NearestExpiryDate { get; init; }
    public string? ExpiryWarningText { get; init; }
    public IReadOnlyList<InventoryTransactionView> Transactions { get; init; } = Array.Empty<InventoryTransactionView>();
}

internal sealed class InventoryTransactionView
{
    public DateTime CreatedAt { get; init; }
    public string Type { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public string Reference { get; init; } = string.Empty;

    public static InventoryTransactionView FromApi(InventoryTransactionApiModel api) => new()
    {
        CreatedAt = api.CreatedAt,
        Type = string.IsNullOrWhiteSpace(api.Type) ? "حركة" : api.Type,
        Quantity = api.Quantity,
        Reference = string.IsNullOrWhiteSpace(api.ReferenceType)
            ? api.Reason
            : $"{api.ReferenceType} — {api.Reason}".Trim(' ', '—')
    };
}

internal sealed class InventoryPageState
{
    public IReadOnlyList<InventoryProductView> Products { get; init; } = Array.Empty<InventoryProductView>();
    public int TotalCount { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 25;
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public bool IsConnectionError { get; init; }
}
