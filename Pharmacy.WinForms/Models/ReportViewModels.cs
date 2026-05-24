using Pharmacy.WinForms.Ui;

namespace Pharmacy.WinForms.Models;

internal enum ReportKind
{
    Sales,
    TopSellingProducts,
    ProfitLoss,
    ExpiringMedicines,
    CustomerDebts,
    SupplierPayables
}

internal sealed class ReportCardViewModel
{
    public ReportKind Kind { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string IconGlyph { get; init; } = string.Empty;
    public string BadgeText { get; init; } = string.Empty;
    public bool IsWarning { get; init; }
    public bool IsAvailable { get; init; } = true;
}

internal sealed class ReportDetailsRowView
{
    public IReadOnlyList<string> Cells { get; init; } = Array.Empty<string>();
}

internal sealed class ReportDetailsContentView
{
    public IReadOnlyList<(string Label, string Value)> Summary { get; init; } =
        Array.Empty<(string, string)>();

    public IReadOnlyList<string> TableHeaders { get; init; } = Array.Empty<string>();
    public IReadOnlyList<ReportDetailsRowView> TableRows { get; init; } = Array.Empty<ReportDetailsRowView>();
    public string EmptyMessage { get; init; } = string.Empty;
}

internal sealed class ReportLoadResult
{
    public bool Success { get; init; }
    public bool IsAvailable { get; init; } = true;
    public string? ErrorMessage { get; init; }
    public bool IsConnectionError { get; init; }
    public string PeriodText { get; init; } = string.Empty;
    public ReportDetailsContentView? Content { get; init; }
}

internal static class ReportCatalog
{
    public static IReadOnlyList<ReportCardViewModel> CreateCards() =>
    [
        new ReportCardViewModel
        {
            Kind = ReportKind.Sales,
            Title = "تقرير المبيعات",
            Description = "تحليل تفصيلي لحركة المبيعات اليومية والشهرية.",
            IconGlyph = SegoeMdl2Icons.Receipt,
            BadgeText = "آخر تحديث: اليوم",
            IsAvailable = true
        },
        new ReportCardViewModel
        {
            Kind = ReportKind.TopSellingProducts,
            Title = "الأدوية الأكثر مبيعًا",
            Description = "إحصائيات الأصناف الأعلى طلبًا لتوجيه المشتريات.",
            IconGlyph = SegoeMdl2Icons.Chart,
            BadgeText = "أسبوعي",
            IsAvailable = true
        },
        new ReportCardViewModel
        {
            Kind = ReportKind.ProfitLoss,
            Title = "الأرباح والخسائر",
            Description = "بيان شامل للإيرادات والمصروفات وصافي الدخل.",
            IconGlyph = SegoeMdl2Icons.Profit,
            BadgeText = "شهري",
            IsAvailable = true
        },
        new ReportCardViewModel
        {
            Kind = ReportKind.ExpiringMedicines,
            Title = "أدوية منتهية الصلاحية",
            Description = "قائمة بالأدوية التالفة أو القريبة من انتهاء الصلاحية.",
            IconGlyph = SegoeMdl2Icons.Expiry,
            BadgeText = "تنبيه عالي",
            IsWarning = true,
            IsAvailable = true
        },
        new ReportCardViewModel
        {
            Kind = ReportKind.CustomerDebts,
            Title = "ديون الزبائن",
            Description = "سجل المدفوعات الآجلة والذمم المالية للعملاء.",
            IconGlyph = SegoeMdl2Icons.Customers,
            BadgeText = "محدث",
            IsAvailable = true
        },
        new ReportCardViewModel
        {
            Kind = ReportKind.SupplierPayables,
            Title = "مستحقات الموردين",
            Description = "كشف حساب الالتزامات المالية للشركات والموزعين.",
            IconGlyph = SegoeMdl2Icons.Suppliers,
            BadgeText = "ربع سنوي",
            IsAvailable = true
        }
    ];
}
