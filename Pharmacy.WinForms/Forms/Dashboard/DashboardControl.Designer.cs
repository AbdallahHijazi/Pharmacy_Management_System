#nullable enable
using Pharmacy.WinForms.Controls;
using Pharmacy.WinForms.Ui;

namespace Pharmacy.WinForms.Forms.Dashboard;

partial class DashboardControl
{
    private System.ComponentModel.IContainer? components;
    private Panel headerPanel = null!;
    private Label titleLabel = null!;
    private Label subtitleLabel = null!;
    private Label errorBannerLabel = null!;
    private Label mockBannerLabel = null!;
    private TableLayoutPanel statsGrid = null!;
    private StatCardControl totalProductsCard = null!;
    private StatCardControl todaySalesCard = null!;
    private StatCardControl todayProfitCard = null!;
    private StatCardControl lowStockCard = null!;
    private StatCardControl expiringCard = null!;
    private StatCardControl todayInvoicesCard = null!;
    private TableLayoutPanel lowerGrid = null!;
    private Panel latestSalesPanel = null!;
    private ListView latestSalesList = null!;
    private Panel stockAlertsPanel = null!;
    private FlowLayoutPanel stockAlertsFlow = null!;
    private Panel quickActionsPanel = null!;
    private FlowLayoutPanel quickActionsFlow = null!;
    private Panel loadingOverlay = null!;
    private Label loadingLabel = null!;

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        SuspendLayout();

        Dock = DockStyle.Fill;
        BackColor = PharmaTheme.SoftGreenBackground;
        Font = PharmaTheme.BodyFont;
        Padding = new Padding(20);
        RightToLeft = RightToLeft.Yes;

        headerPanel = new Panel
        {
            BackColor = PharmaTheme.Background,
            Dock = DockStyle.Top,
            Height = 64,
            Padding = new Padding(0, 0, 0, 8)
        };

        titleLabel = new Label
        {
            AutoSize = false,
            Dock = DockStyle.Top,
            Font = PharmaTheme.TitleFont,
            ForeColor = PharmaTheme.TextDark,
            Height = 32,
            Text = "لوحة التحكم",
            TextAlign = ContentAlignment.MiddleRight
        };

        subtitleLabel = new Label
        {
            AutoSize = false,
            Dock = DockStyle.Top,
            Font = PharmaTheme.SmallFont,
            ForeColor = PharmaTheme.MutedText,
            Height = 24,
            Text = "نظرة عامة على أداء الصيدلية اليوم",
            TextAlign = ContentAlignment.MiddleRight
        };

        headerPanel.Controls.Add(subtitleLabel);
        headerPanel.Controls.Add(titleLabel);

        errorBannerLabel = new Label
        {
            AutoSize = false,
            Dock = DockStyle.Top,
            Font = PharmaTheme.SmallFont,
            ForeColor = PharmaTheme.Danger,
            Height = 0,
            Padding = new Padding(12, 8, 12, 8),
            TextAlign = ContentAlignment.MiddleRight,
            Visible = false
        };

        mockBannerLabel = new Label
        {
            AutoSize = false,
            BackColor = Color.FromArgb(255, 248, 220),
            Dock = DockStyle.Top,
            Font = PharmaTheme.SmallFont,
            ForeColor = PharmaTheme.Warning,
            Height = 0,
            Padding = new Padding(12, 8, 12, 8),
            TextAlign = ContentAlignment.MiddleRight,
            Visible = false
        };

        statsGrid = new TableLayoutPanel
        {
            BackColor = PharmaTheme.Background,
            ColumnCount = 3,
            Dock = DockStyle.Top,
            Height = 240,
            Margin = new Padding(0, 12, 0, 12),
            Padding = new Padding(0),
            RightToLeft = RightToLeft.Yes
        };
        statsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
        statsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
        statsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.34F));
        statsGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
        statsGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));

        totalProductsCard = CreateStatCard("إجمالي المنتجات", "0", "📦");
        todaySalesCard = CreateStatCard("مبيعات اليوم", "0", "💰");
        todayProfitCard = CreateStatCard("أرباح اليوم", "0", "📈");
        lowStockCard = CreateStatCard("منخفض المخزون", "0", "⚠");
        expiringCard = CreateStatCard("قريب الانتهاء", "0", "⏳");
        todayInvoicesCard = CreateStatCard("فواتير اليوم", "0", "🧾");

        statsGrid.Controls.Add(totalProductsCard, 0, 0);
        statsGrid.Controls.Add(todaySalesCard, 1, 0);
        statsGrid.Controls.Add(todayProfitCard, 2, 0);
        statsGrid.Controls.Add(lowStockCard, 0, 1);
        statsGrid.Controls.Add(expiringCard, 1, 1);
        statsGrid.Controls.Add(todayInvoicesCard, 2, 1);

        lowerGrid = new TableLayoutPanel
        {
            ColumnCount = 3,
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 8, 0, 0),
            RightToLeft = RightToLeft.Yes
        };
        lowerGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
        lowerGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));
        lowerGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));

        latestSalesPanel = CreateSectionPanel("آخر المبيعات");
        latestSalesList = new ListView
        {
            BorderStyle = BorderStyle.None,
            Dock = DockStyle.Fill,
            Font = PharmaTheme.SmallFont,
            FullRowSelect = true,
            HeaderStyle = ColumnHeaderStyle.Nonclickable,
            RightToLeft = RightToLeft.Yes,
            RightToLeftLayout = true,
            View = View.Details
        };
        latestSalesList.Columns.Add("الفاتورة", 90);
        latestSalesList.Columns.Add("الزبون", 120);
        latestSalesList.Columns.Add("المبلغ", 80);
        latestSalesList.Columns.Add("الوقت", 110);
        latestSalesPanel.Controls.Add(latestSalesList);

        stockAlertsPanel = CreateSectionPanel("تنبيهات المخزون");
        stockAlertsFlow = new FlowLayoutPanel
        {
            AutoScroll = true,
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            Padding = new Padding(4),
            WrapContents = false
        };
        stockAlertsPanel.Controls.Add(stockAlertsFlow);

        quickActionsPanel = CreateSectionPanel("اختصارات سريعة");
        quickActionsFlow = new FlowLayoutPanel
        {
            AutoScroll = true,
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            Padding = new Padding(4),
            WrapContents = false
        };
        quickActionsPanel.Controls.Add(quickActionsFlow);

        lowerGrid.Controls.Add(latestSalesPanel, 0, 0);
        lowerGrid.Controls.Add(stockAlertsPanel, 1, 0);
        lowerGrid.Controls.Add(quickActionsPanel, 2, 0);

        var scrollHost = new Panel
        {
            AutoScroll = true,
            Dock = DockStyle.Fill
        };
        scrollHost.Controls.Add(lowerGrid);
        lowerGrid.Dock = DockStyle.Top;
        lowerGrid.Height = 420;

        Controls.Add(scrollHost);
        Controls.Add(statsGrid);
        Controls.Add(mockBannerLabel);
        Controls.Add(errorBannerLabel);
        Controls.Add(headerPanel);

        loadingOverlay = new Panel
        {
            BackColor = Color.FromArgb(160, 231, 255, 241),
            Dock = DockStyle.Fill,
            Visible = false
        };
        loadingLabel = new Label
        {
            AutoSize = true,
            Font = PharmaTheme.SectionFont,
            ForeColor = PharmaTheme.PrimaryGreen,
            Text = "جاري تحميل الإحصائيات..."
        };
        loadingOverlay.Controls.Add(loadingLabel);
        Controls.Add(loadingOverlay);
        loadingOverlay.BringToFront();

        ResumeLayout(false);
    }

    private static StatCardControl CreateStatCard(string title, string value, string icon)
    {
        return new StatCardControl
        {
            CardTitle = title,
            CardValue = value,
            Dock = DockStyle.Fill,
            IconText = icon,
            Margin = new Padding(6)
        };
    }

    private static Panel CreateSectionPanel(string title)
    {
        var panel = new Panel
        {
            BackColor = PharmaTheme.CardBackground,
            Dock = DockStyle.Fill,
            Margin = new Padding(6),
            Padding = new Padding(14)
        };

        var header = new Label
        {
            AutoSize = false,
            Dock = DockStyle.Top,
            Font = PharmaTheme.SectionFont,
            ForeColor = PharmaTheme.TextDark,
            Height = 28,
            Text = title,
            TextAlign = ContentAlignment.MiddleRight
        };
        panel.Controls.Add(header);
        return panel;
    }
}
