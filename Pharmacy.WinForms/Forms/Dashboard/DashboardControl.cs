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
        newInvoiceButton.Click += (_, _) => QuickActionRequested?.Invoke(this, "فاتورة جديدة");
        BuildQuickActions();
        loadingOverlay.Resize += (_, _) => CenterLoadingLabel();
        SizeChanged += (_, _) => OnChromeSizeChanged();
        stockAlertsFlow.Resize += (_, _) => LayoutStockAlertWidths();
        quickActionsFlow.Resize += (_, _) => LayoutQuickActionWidths();
        OnChromeSizeChanged();
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
        mockBannerLabel.MaximumSize = new Size(w, 0);
        bannerHost.PerformLayout();
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
        latestSalesList.Columns[0].Width = Math.Max(72, (int)(inner * 0.17));
        latestSalesList.Columns[1].Width = Math.Max(88, (int)(inner * 0.26));
        latestSalesList.Columns[2].Width = Math.Max(72, (int)(inner * 0.16));
        latestSalesList.Columns[3].Width = Math.Max(96, (int)(inner * 0.22));
        latestSalesList.Columns[4].Width = Math.Max(72, inner - latestSalesList.Columns[0].Width - latestSalesList.Columns[1].Width - latestSalesList.Columns[2].Width - latestSalesList.Columns[3].Width);
    }

    private void LayoutStockAlertWidths()
    {
        var inner = Math.Max(80, stockAlertsFlow.ClientSize.Width - stockAlertsFlow.Padding.Horizontal);
        foreach (Control c in stockAlertsFlow.Controls)
        {
            c.Width = inner;
        }
    }

    private void LayoutQuickActionWidths()
    {
        var inner = Math.Max(80, quickActionsFlow.ClientSize.Width - quickActionsFlow.Padding.Horizontal);
        foreach (Control c in quickActionsFlow.Controls)
        {
            if (c is Button b)
            {
                b.Width = inner;
            }
        }
    }

    private void ApplyResult(DashboardLoadResult result)
    {
        var summary = result.Summary;

        totalProductsCard.CardValue = summary.TotalProducts.ToString("N0");
        todaySalesCard.CardValue = FormatCurrency(summary.TodaySales);
        todayProfitCard.CardValue = FormatCurrency(summary.TodayProfit);
        lowStockCard.CardValue = summary.LowStockProductsCount.ToString("N0");
        expiringCard.CardValue = summary.ExpiringSoonBatchesCount.ToString("N0");
        todayInvoicesCard.CardValue = summary.TodayInvoicesCount.ToString("N0");

        latestSalesList.Items.Clear();
        foreach (var sale in summary.LatestSales)
        {
            var item = new ListViewItem(sale.InvoiceNumber);
            item.SubItems.Add(sale.CustomerName);
            item.SubItems.Add(FormatCurrency(sale.GrandTotal));
            item.SubItems.Add(sale.CreatedAt.ToLocalTime().ToString("g"));
            item.SubItems.Add(LocalizeInvoiceStatus(sale.Status));
            latestSalesList.Items.Add(item);
        }

        stockAlertsFlow.Controls.Clear();
        if (summary.StockAlerts.Count == 0)
        {
            stockAlertsFlow.Controls.Add(CreateMutedLabel("لا توجد تنبيهات حالياً."));
        }
        else
        {
            foreach (var alert in summary.StockAlerts)
            {
                stockAlertsFlow.Controls.Add(CreateAlertCard(alert));
            }
        }

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

        if (result.IsMockData)
        {
            mockBannerLabel.Text = "يتم عرض بيانات تجريبية — تحقق من اتصال API.";
            mockBannerLabel.Visible = true;
        }
        else
        {
            mockBannerLabel.Visible = false;
            mockBannerLabel.Text = string.Empty;
        }
    }

    private void BuildQuickActions()
    {
        AddQuickAction("بيع جديد", "فتح نقطة البيع", "₪");
        AddQuickAction("فاتورة شراء", "تسجيل مشتريات جديدة", "↧");
        AddQuickAction("إضافة منتج", "إضافة منتج للمخزون", "＋");
        AddQuickAction("تقرير اليوم", "عرض التقرير المالي اليومي", "📊");
        LayoutQuickActionWidths();
    }

    private void AddQuickAction(string title, string description, string icon)
    {
        var button = new Button
        {
            AutoSize = false,
            BackColor = PharmaTheme.SurfaceContainerLow,
            Cursor = Cursors.Hand,
            FlatStyle = FlatStyle.Flat,
            Font = PharmaTheme.BodyFont,
            Height = 54,
            Margin = new Padding(0, 0, 0, 10),
            RightToLeft = RightToLeft.Yes,
            Text = $"{icon}  {title}\r\n{description}",
            TextAlign = ContentAlignment.MiddleRight,
            UseVisualStyleBackColor = false,
            Width = 220
        };
        button.FlatAppearance.BorderColor = PharmaTheme.OutlineVariant;
        button.FlatAppearance.BorderSize = 1;
        button.Click += (_, _) => QuickActionRequested?.Invoke(this, title);
        quickActionsFlow.Controls.Add(button);
    }

    private Control CreateAlertCard(DashboardStockAlert alert)
    {
        var outer = new TableLayoutPanel
        {
            AutoSize = true,
            ColumnCount = 2,
            Dock = DockStyle.Top,
            Margin = new Padding(0, 0, 0, 10),
            Padding = new Padding(0),
            RightToLeft = RightToLeft.Yes,
            RowCount = 1
        };
        outer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 4F));
        outer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

        var accent = new Panel
        {
            BackColor = alert.IsExpiryAlert ? PharmaTheme.Warning : PharmaTheme.PrimaryContainer,
            Dock = DockStyle.Fill
        };

        var body = new Panel
        {
            AutoSize = true,
            BackColor = alert.IsExpiryAlert ? PharmaTheme.WarningSurface : PharmaTheme.SurfaceContainerLow,
            Dock = DockStyle.Fill,
            Padding = new Padding(12, 10, 12, 10)
        };

        var title = new Label
        {
            AutoSize = true,
            Dock = DockStyle.Top,
            Font = new Font(PharmaTheme.BodyFont, FontStyle.Bold),
            ForeColor = PharmaTheme.TextDark,
            MaximumSize = new Size(360, 0),
            Text = alert.Title,
            TextAlign = ContentAlignment.TopRight
        };
        var detail = new Label
        {
            AutoSize = true,
            Dock = DockStyle.Top,
            Font = PharmaTheme.SmallFont,
            ForeColor = PharmaTheme.OnSurfaceVariant,
            MaximumSize = new Size(360, 0),
            Text = alert.Detail,
            TextAlign = ContentAlignment.TopRight
        };
        body.Controls.Add(detail);
        body.Controls.Add(title);

        outer.Controls.Add(accent, 0, 0);
        outer.Controls.Add(body, 1, 0);
        return outer;
    }

    private static Label CreateMutedLabel(string text) => new()
    {
        AutoSize = true,
        Font = PharmaTheme.SmallFont,
        ForeColor = PharmaTheme.MutedText,
        Margin = new Padding(0, 8, 0, 0),
        Text = text,
        TextAlign = ContentAlignment.TopRight
    };

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

    private static string FormatCurrency(decimal value) => $"{value:N2}";

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _loadCts?.Cancel();
            _loadCts?.Dispose();
            components?.Dispose();
        }

        base.Dispose(disposing);
    }
}
