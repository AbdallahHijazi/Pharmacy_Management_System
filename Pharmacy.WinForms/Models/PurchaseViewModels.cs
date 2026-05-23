using System.Globalization;
using Pharmacy.WinForms.Ui;

namespace Pharmacy.WinForms.Models;

internal enum PurchaseInvoiceStatusKind
{
    Paid,
    PartiallyPaid,
    Unpaid,
    Cancelled,
    Unknown
}

internal enum PurchaseStatusFilter
{
    All,
    Paid,
    PartiallyPaid,
    Unpaid,
    Cancelled
}

internal sealed class PurchaseInvoiceListItemView
{
    public Guid Id { get; init; }
    public string InvoiceNumber { get; init; } = string.Empty;
    public string SupplierName { get; init; } = string.Empty;
    public Guid SupplierId { get; init; }
    public DateTime InvoiceDate { get; init; }
    public string FormattedDate { get; init; } = string.Empty;
    public int? ItemsCount { get; init; }
    public decimal GrandTotal { get; init; }
    public decimal PaidAmount { get; init; }
    public decimal RemainingAmount { get; init; }
    public string RawStatus { get; init; } = string.Empty;
    public string DisplayStatus { get; init; } = string.Empty;
    public PurchaseInvoiceStatusKind StatusKind { get; init; }

    public static PurchaseInvoiceListItemView FromApi(PurchaseInvoiceApiModel api, int? itemsCount = null)
    {
        var kind = ResolveStatusKind(api.Status, api.PaidAmount, api.RemainingAmount, api.GrandTotal);
        return new PurchaseInvoiceListItemView
        {
            Id = api.PurchaseInvoiceId,
            InvoiceNumber = string.IsNullOrWhiteSpace(api.InvoiceNumber) ? "بدون رقم" : api.InvoiceNumber.Trim(),
            SupplierName = string.IsNullOrWhiteSpace(api.SupplierName) ? "مورد غير معروف" : api.SupplierName.Trim(),
            SupplierId = api.SupplierId,
            InvoiceDate = api.CreatedAt,
            FormattedDate = FormatInvoiceDate(api.CreatedAt),
            ItemsCount = itemsCount,
            GrandTotal = api.GrandTotal,
            PaidAmount = api.PaidAmount,
            RemainingAmount = api.RemainingAmount,
            RawStatus = api.Status ?? string.Empty,
            DisplayStatus = ToDisplayStatus(kind),
            StatusKind = kind
        };
    }

    public static PurchaseInvoiceListItemView FromDetails(PurchaseInvoiceDetailsApiModel api, int? itemsCount = null)
    {
        var kind = ResolveStatusKind(api.Status, api.PaidAmount, api.RemainingAmount, api.GrandTotal);
        return new PurchaseInvoiceListItemView
        {
            Id = api.PurchaseInvoiceId,
            InvoiceNumber = string.IsNullOrWhiteSpace(api.InvoiceNumber) ? "بدون رقم" : api.InvoiceNumber.Trim(),
            SupplierName = string.IsNullOrWhiteSpace(api.SupplierName) ? "مورد غير معروف" : api.SupplierName.Trim(),
            SupplierId = api.SupplierId,
            InvoiceDate = api.CreatedAt,
            FormattedDate = FormatInvoiceDate(api.CreatedAt),
            ItemsCount = itemsCount,
            GrandTotal = api.GrandTotal,
            PaidAmount = api.PaidAmount,
            RemainingAmount = api.RemainingAmount,
            RawStatus = api.Status ?? string.Empty,
            DisplayStatus = ToDisplayStatus(kind),
            StatusKind = kind
        };
    }

    internal static PurchaseInvoiceStatusKind ResolveStatusKind(
        string? status,
        decimal paidAmount,
        decimal remainingAmount,
        decimal grandTotal)
    {
        var normalized = (status ?? string.Empty).Trim();
        if (normalized.Equals("Cancelled", StringComparison.OrdinalIgnoreCase))
        {
            return PurchaseInvoiceStatusKind.Cancelled;
        }

        if (normalized.Equals("Completed", StringComparison.OrdinalIgnoreCase)
            || (grandTotal > 0 && remainingAmount <= 0))
        {
            return PurchaseInvoiceStatusKind.Paid;
        }

        if (normalized.Equals("PartiallyPaid", StringComparison.OrdinalIgnoreCase)
            || (paidAmount > 0 && remainingAmount > 0))
        {
            return PurchaseInvoiceStatusKind.PartiallyPaid;
        }

        if (normalized.Equals("Pending", StringComparison.OrdinalIgnoreCase)
            || normalized.Equals("Received", StringComparison.OrdinalIgnoreCase)
            || remainingAmount > 0)
        {
            return PurchaseInvoiceStatusKind.Unpaid;
        }

        return PurchaseInvoiceStatusKind.Unknown;
    }

    private static string ToDisplayStatus(PurchaseInvoiceStatusKind kind) => kind switch
    {
        PurchaseInvoiceStatusKind.Paid => "مدفوع",
        PurchaseInvoiceStatusKind.PartiallyPaid => "متبقي جزئيًا",
        PurchaseInvoiceStatusKind.Unpaid => "غير مدفوع",
        PurchaseInvoiceStatusKind.Cancelled => "ملغي",
        _ => "غير محدد"
    };

    private static string FormatInvoiceDate(DateTime date)
    {
        try
        {
            var culture = CultureInfo.GetCultureInfo("ar-SA");
            return date.ToString("dd MMM, yyyy", culture);
        }
        catch
        {
            return date.ToString("dd MMM, yyyy", CultureInfo.CurrentCulture);
        }
    }
}

internal sealed class PurchaseInvoiceDetailsView
{
    public PurchaseInvoiceListItemView Summary { get; init; } = null!;
    public IReadOnlyList<PurchaseInvoiceLineView> Lines { get; init; } = Array.Empty<PurchaseInvoiceLineView>();
}

internal sealed class PurchaseInvoiceLineView
{
    public string ProductName { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal LineTotal => UnitPrice * Quantity;

    public static PurchaseInvoiceLineView FromApi(PurchaseInvoiceItemApiModel api) => new()
    {
        ProductName = string.IsNullOrWhiteSpace(api.ProductName) ? "صنف غير معروف" : api.ProductName.Trim(),
        Quantity = api.Quantity,
        UnitPrice = api.UnitPrice
    };
}

internal sealed class PurchaseInvoicesPageState
{
    public IReadOnlyList<PurchaseInvoiceListItemView> Invoices { get; init; } = Array.Empty<PurchaseInvoiceListItemView>();
    public int TotalCount { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public bool IsConnectionError { get; init; }
}

internal sealed class SupplierOptionView
{
    public Guid? SupplierId { get; init; }
    public string Name { get; init; } = "كل الموردين";
    public string DisplayName { get; init; } = "كل الموردين";
    public string Subtitle { get; init; } = string.Empty;

    public static SupplierOptionView All { get; } = new();
}
