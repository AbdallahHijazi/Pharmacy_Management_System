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
        BuildQuickActions();
        loadingOverlay.Resize += (_, _) => CenterLoadingLabel();
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
            item.SubItems.Add(sale.CreatedAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm"));
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

        if (result.HasError)
        {
            errorBannerLabel.Text = result.ErrorMessage;
            errorBannerLabel.Height = 40;
            errorBannerLabel.Visible = true;
        }
        else
        {
            errorBannerLabel.Visible = false;
            errorBannerLabel.Height = 0;
        }

        if (result.IsMockData)
        {
            mockBannerLabel.Text = "يتم عرض بيانات تجريبية — تحقق من اتصال API.";
            mockBannerLabel.Height = 36;
            mockBannerLabel.Visible = true;
        }
        else
        {
            mockBannerLabel.Visible = false;
            mockBannerLabel.Height = 0;
        }
    }

    private void BuildQuickActions()
    {
        AddQuickAction("بيع جديد", "فتح نقطة البيع", "₪");
        AddQuickAction("فاتورة شراء", "تسجيل مشتريات جديدة", "↧");
        AddQuickAction("إضافة منتج", "إضافة منتج للمخزون", "＋");
        AddQuickAction("تقرير اليوم", "عرض التقرير المالي اليومي", "📊");
    }

    private void AddQuickAction(string title, string description, string icon)
    {
        var button = new Button
        {
            AutoSize = false,
            BackColor = PharmaTheme.SoftGreenBackground,
            FlatStyle = FlatStyle.Flat,
            Font = PharmaTheme.BodyFont,
            Height = 56,
            Margin = new Padding(0, 0, 0, 8),
            RightToLeft = RightToLeft.Yes,
            Text = $"{icon}  {title}\r\n{description}",
            TextAlign = ContentAlignment.MiddleRight,
            UseVisualStyleBackColor = false,
            Width = quickActionsFlow.ClientSize.Width > 0 ? quickActionsFlow.ClientSize.Width - 12 : 220
        };
        button.FlatAppearance.BorderColor = PharmaTheme.BorderLight;
        button.FlatAppearance.BorderSize = 1;
        button.Click += (_, _) => QuickActionRequested?.Invoke(this, title);
        quickActionsFlow.Controls.Add(button);
    }

    private static Control CreateAlertCard(DashboardStockAlert alert)
    {
        var panel = new Panel
        {
            BackColor = alert.IsExpiryAlert
                ? Color.FromArgb(255, 246, 232)
                : Color.FromArgb(245, 252, 248),
            Height = 58,
            Margin = new Padding(0, 0, 0, 8),
            Padding = new Padding(10, 8, 10, 8),
            Width = 280
        };

        var title = new Label
        {
            AutoSize = false,
            Dock = DockStyle.Top,
            Font = PharmaTheme.BodyFont,
            ForeColor = PharmaTheme.TextDark,
            Height = 22,
            Text = alert.Title,
            TextAlign = ContentAlignment.MiddleRight
        };
        var detail = new Label
        {
            AutoSize = false,
            Dock = DockStyle.Top,
            Font = PharmaTheme.SmallFont,
            ForeColor = PharmaTheme.MutedText,
            Height = 20,
            Text = alert.Detail,
            TextAlign = ContentAlignment.MiddleRight
        };
        panel.Controls.Add(detail);
        panel.Controls.Add(title);
        return panel;
    }

    private static Label CreateMutedLabel(string text) => new()
    {
        AutoSize = true,
        Font = PharmaTheme.SmallFont,
        ForeColor = PharmaTheme.MutedText,
        Text = text,
        TextAlign = ContentAlignment.MiddleRight
    };

    private void SetLoading(bool isLoading)
    {
        loadingOverlay.Visible = isLoading;
        statsGrid.Enabled = !isLoading;
        lowerGrid.Enabled = !isLoading;
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
