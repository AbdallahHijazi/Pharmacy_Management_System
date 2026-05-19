using System.Drawing;
using Pharmacy.WinForms.Controls;
using Pharmacy.WinForms.Models;
using Pharmacy.WinForms.Services;
using Pharmacy.WinForms.Ui;

namespace Pharmacy.WinForms.Forms.Dashboard;

public sealed partial class DashboardControl : UserControl
{
    private readonly DashboardService _dashboardService;
    private CancellationTokenSource? _loadCts;

    public event EventHandler<string>? QuickActionRequested;

    public DashboardControl() : this(AppServices.DashboardService)
    {
    }

    public DashboardControl(DashboardService dashboardService)
    {
        _dashboardService = dashboardService;
        InitializeComponent();
        ListViewRowHeight.Apply(latestSalesList);
        newInvoiceButton.Click += (_, _) => QuickActionRequested?.Invoke(this, "فاتورة جديدة");
        WireSalesListDrawing();
        BuildQuickActions();
        loadingOverlay.Resize += (_, _) => CenterLoadingLabel();
        SizeChanged += (_, _) => OnChromeSizeChanged();
        stockAlertsFlow.Resize += (_, _) => LayoutStockAlertWidths();
        quickActionsFlow.Resize += (_, _) => LayoutQuickActionWidths();
        latestSalesList.Resize += (_, _) => ResizeLatestSalesColumns();
        OnChromeSizeChanged();
    }

    private void WireSalesListDrawing()
    {
        latestSalesList.DrawColumnHeader += (_, e) =>
        {
            if (e.ColumnIndex < 0)
            {
                return;
            }

            e.DrawBackground();
            var headerText = latestSalesList.Columns[e.ColumnIndex].Text;
            TextRenderer.DrawText(
                e.Graphics,
                headerText,
                PharmaTheme.TableHeaderFont,
                e.Bounds,
                PharmaTheme.MutedText,
                TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        };

        latestSalesList.DrawItem += (_, e) =>
        {
            e.DrawDefault = false;
            if (e.ItemIndex < 0)
            {
                return;
            }

            var back = e.ItemIndex % 2 == 0
                ? PharmaTheme.Surface
                : PharmaTheme.SurfaceContainerLow;
            using var brush = new SolidBrush(back);
            e.Graphics.FillRectangle(brush, e.Bounds);
        };

        latestSalesList.DrawSubItem += (_, e) =>
        {
            if (e.ItemIndex < 0)
            {
                return;
            }

            e.DrawDefault = false;
            if (e.ColumnIndex == 4)
            {
                DrawStatusBadge(e);
                return;
            }

            var font = e.ColumnIndex == 2 ? PharmaTheme.TableAmountFont : PharmaTheme.TableCellFont;
            var color = e.ColumnIndex == 2 ? PharmaTheme.PrimaryGreen : PharmaTheme.TextDark;
            TextRenderer.DrawText(
                e.Graphics,
                e.SubItem?.Text ?? string.Empty,
                font,
                Rectangle.Inflate(e.Bounds, -8, 0),
                color,
                TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        };
    }

    private static void DrawStatusBadge(DrawListViewSubItemEventArgs e)
    {
        var raw = e.Item?.Tag as string;
        var localized = e.SubItem?.Text ?? "—";
        var kind = StatusBadgeControl.FromStatus(raw ?? localized);
        var (back, fore) = kind switch
        {
            InvoiceStatusBadgeKind.Completed => (PharmaTheme.PrimaryFixed, PharmaTheme.PrimaryGreen),
            InvoiceStatusBadgeKind.Returned => (PharmaTheme.ErrorContainer, PharmaTheme.Danger),
            InvoiceStatusBadgeKind.Pending => (PharmaTheme.WarningSurface, PharmaTheme.WarningStrong),
            _ => (PharmaTheme.SurfaceContainerLow, PharmaTheme.OnSurfaceVariant)
        };

        var textSize = TextRenderer.MeasureText(localized, PharmaTheme.ArabicFont(9f, FontStyle.Bold));
        var badgeW = Math.Min(e.Bounds.Width - 8, textSize.Width + 16);
        var badgeH = 22;
        var badgeRect = new Rectangle(
            e.Bounds.Right - badgeW - 6,
            e.Bounds.Y + (e.Bounds.Height - badgeH) / 2,
            badgeW,
            badgeH);
        RoundedDrawing.FillRounded(e.Graphics, badgeRect, badgeH / 2, back);
        TextRenderer.DrawText(
            e.Graphics,
            localized,
            PharmaTheme.ArabicFont(9f, FontStyle.Bold),
            badgeRect,
            fore,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        _ = LoadDashboardAsync();
    }

    public async Task LoadDashboardAsync()
    {
        _loadCts?.Cancel();
        _loadCts = new CancellationTokenSource();
        var token = _loadCts.Token;

        SetLoading(true);
        try
        {
            var result = await _dashboardService.LoadDashboardAsync(token).ConfigureAwait(true);
            if (token.IsCancellationRequested || IsDisposed)
            {
                return;
            }

            ApplyResult(result);
        }
        finally
        {
            if (!IsDisposed)
            {
                SetLoading(false);
            }
        }
    }

    private void OnChromeSizeChanged()
    {
        var w = Math.Max(220, ClientSize.Width - Padding.Horizontal - 8);
        errorBannerLabel.MaximumSize = new Size(w, 0);
        ResizeLatestSalesColumns();
        LayoutStockAlertWidths();
        LayoutQuickActionWidths();
    }

    private void ResizeLatestSalesColumns()
    {
        if (latestSalesList.Columns.Count < 5 || latestSalesList.ClientSize.Width < 40)
        {
            return;
        }

        var inner = Math.Max(80, latestSalesList.ClientSize.Width - SystemInformation.VerticalScrollBarWidth - 4);
        latestSalesList.Columns[0].Width = Math.Max(80, (int)(inner * 0.18));
        latestSalesList.Columns[1].Width = Math.Max(96, (int)(inner * 0.28));
        latestSalesList.Columns[2].Width = Math.Max(80, (int)(inner * 0.16));
        latestSalesList.Columns[3].Width = Math.Max(100, (int)(inner * 0.22));
        latestSalesList.Columns[4].Width = Math.Max(80, inner - latestSalesList.Columns[0].Width - latestSalesList.Columns[1].Width - latestSalesList.Columns[2].Width - latestSalesList.Columns[3].Width);
    }

    private void LayoutStockAlertWidths()
    {
        var inner = Math.Max(140, stockAlertsFlow.ClientSize.Width - stockAlertsFlow.Padding.Horizontal - 4);
        foreach (Control c in stockAlertsFlow.Controls)
        {
            c.Width = inner;
        }
    }

    private void LayoutQuickActionWidths()
    {
        var inner = Math.Max(140, quickActionsFlow.ClientSize.Width - quickActionsFlow.Padding.Horizontal);
        foreach (Control c in quickActionsFlow.Controls)
        {
            c.Width = inner;
        }
    }

    private void ApplyResult(DashboardLoadResult result)
    {
        var summary = result.Summary;

        todaySalesCard.CardValue = FormatCurrency(summary.TodaySales);
        todayInvoicesCard.CardValue = summary.TodayInvoicesCount.ToString("N0");
        expiringCard.CardValue = summary.ExpiringSoonBatchesCount.ToString("N0");
        lowStockCard.CardValue = summary.LowStockProductsCount.ToString("N0");
        todayProfitCard.CardValue = FormatCurrency(summary.TodayProfit);
        totalProductsCard.CardValue = summary.TotalProducts.ToString("N0");

        latestSalesList.BeginUpdate();
        latestSalesList.Items.Clear();
        foreach (var sale in summary.LatestSales)
        {
            var item = new ListViewItem(sale.InvoiceNumber) { Tag = sale.Status };
            item.SubItems.Add(sale.CustomerName);
            item.SubItems.Add(FormatCurrency(sale.GrandTotal));
            item.SubItems.Add(sale.CreatedAt.ToLocalTime().ToString("g"));
            item.SubItems.Add(LocalizeInvoiceStatus(sale.Status));
            latestSalesList.Items.Add(item);
        }

        latestSalesList.EndUpdate();

        var hasSales = summary.LatestSales.Count > 0;
        latestSalesList.Visible = hasSales;
        latestSalesEmptyLabel.Visible = !hasSales;

        stockAlertsFlow.SuspendLayout();
        stockAlertsFlow.Controls.Clear();
        if (summary.StockAlerts.Count == 0)
        {
            stockAlertsFlow.Controls.Add(CreateEmptyAlertsState());
        }
        else
        {
            foreach (var alert in summary.StockAlerts)
            {
                stockAlertsFlow.Controls.Add(CreateAlertCard(alert));
            }
        }

        stockAlertsFlow.ResumeLayout(true);
        LayoutStockAlertWidths();

        if (result.HasError)
        {
            errorBannerLabel.Text = result.ErrorMessage ?? string.Empty;
            errorBannerLabel.Visible = true;
        }
        else
        {
            errorBannerLabel.Visible = false;
            errorBannerLabel.Text = string.Empty;
        }

        ResizeLatestSalesColumns();
    }

    private void BuildQuickActions()
    {
        quickActionsFlow.Controls.Clear();
        AddQuickAction("بيع جديد", "فتح نقطة البيع", SegoeMdl2Icons.PointOfSale, "بيع جديد");
        AddQuickAction("فاتورة شراء", "تسجيل مشتريات جديدة", SegoeMdl2Icons.Purchases, "فاتورة شراء");
        AddQuickAction("إضافة منتج", "إضافة منتج للمخزون", SegoeMdl2Icons.Product, "إضافة منتج");
        AddQuickAction("تقرير اليوم", "عرض التقرير المالي اليومي", SegoeMdl2Icons.Reports, "تقرير اليوم");
        LayoutQuickActionWidths();
    }

    private void AddQuickAction(string title, string description, string iconGlyph, string actionKey)
    {
        var tile = new QuickActionTileControl
        {
            Title = title,
            Description = description,
            IconGlyph = iconGlyph,
            Width = 220
        };
        tile.TileClicked += (_, _) => QuickActionRequested?.Invoke(this, actionKey);
        quickActionsFlow.Controls.Add(tile);
    }

    private Control CreateAlertCard(DashboardStockAlert alert)
    {
        var panel = new RoundedAlertPanel(alert);
        panel.Width = 260;
        return panel;
    }

    private static Control CreateEmptyAlertsState()
    {
        var panel = new Panel
        {
            Height = 88,
            Margin = new Padding(0, 6, 0, 0),
            Padding = new Padding(12, 10, 12, 10)
        };
        panel.Paint += (_, e) =>
        {
            var bounds = panel.ClientRectangle;
            bounds.Inflate(-1, -1);
            RoundedDrawing.FillRounded(e.Graphics, bounds, 14, PharmaTheme.SurfaceContainerLow);
            TextRenderer.DrawText(
                e.Graphics,
                $"{SegoeMdl2Icons.Warning}  لا توجد تنبيهات حالياً",
                PharmaTheme.BodyFont,
                bounds,
                PharmaTheme.MutedText,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        };
        return panel;
    }

    private sealed class RoundedAlertPanel : Panel
    {
        public RoundedAlertPanel(DashboardStockAlert alert)
        {
            Height = 92;
            Margin = new Padding(0, 0, 0, 12);
            Padding = new Padding(16, 12, 16, 12);
            RightToLeft = RightToLeft.Yes;
            var fill = alert.IsExpiryAlert ? PharmaTheme.WarningSurface : PharmaTheme.SurfaceContainerLow;
            BackColor = fill;

            var kind = new Label
            {
                AutoSize = false,
                BackColor = fill,
                Dock = DockStyle.Top,
                Font = PharmaTheme.ArabicFont(9f, FontStyle.Bold),
                ForeColor = alert.IsExpiryAlert ? PharmaTheme.WarningStrong : PharmaTheme.PrimaryGreen,
                Height = 22,
                Text = string.IsNullOrWhiteSpace(alert.AlertKind)
                    ? (alert.IsExpiryAlert ? "قريب الانتهاء" : "مخزون منخفض")
                    : alert.AlertKind,
                TextAlign = ContentAlignment.MiddleRight
            };

            var title = new Label
            {
                AutoSize = false,
                BackColor = fill,
                Dock = DockStyle.Top,
                Font = PharmaTheme.ArabicFont(10.5f, FontStyle.Bold),
                ForeColor = PharmaTheme.TextDark,
                Height = 26,
                Text = alert.Title,
                TextAlign = ContentAlignment.MiddleRight
            };

            var detailText = alert.Detail;
            if (!string.IsNullOrWhiteSpace(alert.BatchNumber))
            {
                detailText = $"رقم التشغيلة: {alert.BatchNumber} — {detailText}";
            }

            var detail = new Label
            {
                AutoSize = false,
                BackColor = fill,
                Dock = DockStyle.Top,
                Font = PharmaTheme.SmallFont,
                ForeColor = PharmaTheme.OnSurfaceVariant,
                Height = 24,
                Text = detailText,
                TextAlign = ContentAlignment.MiddleRight
            };

            Controls.Add(detail);
            Controls.Add(title);
            Controls.Add(kind);

            Paint += (_, e) =>
            {
                var bounds = ClientRectangle;
                bounds.Inflate(-1, -1);
                RoundedDrawing.FillRounded(e.Graphics, bounds, PharmaTheme.DashboardQuickActionRadius, fill);
                RoundedDrawing.DrawRoundedBorder(
                    e.Graphics,
                    bounds,
                    PharmaTheme.DashboardQuickActionRadius,
                    alert.IsExpiryAlert ? Color.FromArgb(70, PharmaTheme.Warning) : PharmaTheme.BorderSoft);
            };
        }
    }

    private static string LocalizeInvoiceStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return "—";
        }

        return status.Trim().ToUpperInvariant() switch
        {
            "PAID" => "مكتمل",
            "COMPLETED" => "مكتمل",
            "RETURNED" or "REFUNDED" => "مرتجع",
            "PENDING" => "معلق",
            _ => status.Trim()
        };
    }

    private void SetLoading(bool isLoading)
    {
        loadingOverlay.Visible = isLoading;
        rootTable.Enabled = !isLoading;
        if (isLoading)
        {
            loadingOverlay.BringToFront();
            CenterLoadingLabel();
        }
    }

    private void CenterLoadingLabel()
    {
        loadingLabel.Left = (loadingOverlay.ClientSize.Width - loadingLabel.Width) / 2;
        loadingLabel.Top = (loadingOverlay.ClientSize.Height - loadingLabel.Height) / 2;
    }

    private static string FormatCurrency(decimal value) => $"{value:N2} ل.س";

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _loadCts?.Cancel();
            _loadCts?.Dispose();
            latestSalesList.SmallImageList?.Dispose();
            latestSalesList.SmallImageList = null;
            components?.Dispose();
        }

        base.Dispose(disposing);
    }
}
