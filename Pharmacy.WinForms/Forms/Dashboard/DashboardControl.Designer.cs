#nullable enable
using Pharmacy.WinForms.Controls;
using Pharmacy.WinForms.Ui;

namespace Pharmacy.WinForms.Forms.Dashboard;

partial class DashboardControl
{
    private System.ComponentModel.IContainer? components;
    private TableLayoutPanel rootTable = null!;
    private TableLayoutPanel headerLayout = null!;
    private Panel titleStack = null!;
    private Label titleLabel = null!;
    private Label subtitleLabel = null!;
    private GradientRoundedButton newInvoiceButton = null!;
    private Label errorBannerLabel = null!;
    private TableLayoutPanel statsGrid = null!;
    private StatCardControl todaySalesCard = null!;
    private StatCardControl todayInvoicesCard = null!;
    private StatCardControl expiringCard = null!;
    private StatCardControl lowStockCard = null!;
    private StatCardControl todayProfitCard = null!;
    private StatCardControl totalProductsCard = null!;
    private TableLayoutPanel chartsRow = null!;
    private PharmaCardPanel salesChartCard = null!;
    private SalesChartPlaceholderControl salesChartPlaceholder = null!;
    private TableLayoutPanel lowerOuter = null!;
    private PharmaCardPanel latestSalesCard = null!;
    private ListView latestSalesList = null!;
    private Label latestSalesEmptyLabel = null!;
    private PharmaCardPanel stockAlertsCard = null!;
    private FlowLayoutPanel stockAlertsFlow = null!;
    private PharmaCardPanel quickActionsCard = null!;
    private FlowLayoutPanel quickActionsFlow = null!;
    private Panel loadingOverlay = null!;
    private Label loadingLabel = null!;

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        SuspendLayout();

        Dock = DockStyle.Fill;
        AutoScroll = true;
        BackColor = PharmaTheme.SoftGreenBackground;
        Font = PharmaTheme.BodyFont;
        Padding = new Padding(28, 12, 28, 20);
        RightToLeft = RightToLeft.Yes;

        headerLayout = new TableLayoutPanel
        {
            ColumnCount = 2,
            Dock = DockStyle.Top,
            Height = 82,
            Margin = new Padding(0, 0, 0, 12),
            Padding = new Padding(0, 4, 0, 4),
            RightToLeft = RightToLeft.Yes,
            RowCount = 1
        };
        headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 156F));

        titleStack = new Panel { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 16, 0) };

        titleLabel = new Label
        {
            AutoSize = false,
            Dock = DockStyle.Top,
            Font = PharmaTheme.DashboardHeadlineFont,
            ForeColor = PharmaTheme.PrimaryGreen,
            Height = 38,
            Text = "نظرة عامة على الصيدلية",
            TextAlign = ContentAlignment.MiddleRight
        };

        subtitleLabel = new Label
        {
            AutoSize = false,
            Dock = DockStyle.Top,
            Font = PharmaTheme.DashboardSubtitleFont,
            ForeColor = PharmaTheme.OnSurfaceVariant,
            Height = 28,
            Text = "مرحباً بك مجدداً، إليك ملخص أداء اليوم.",
            TextAlign = ContentAlignment.MiddleRight
        };

        titleStack.Controls.Add(subtitleLabel);
        titleStack.Controls.Add(titleLabel);

        newInvoiceButton = new GradientRoundedButton
        {
            Anchor = AnchorStyles.Left | AnchorStyles.Top,
            Margin = new Padding(0, 18, 0, 0),
            Text = "فاتورة جديدة",
            Width = 148
        };
        newInvoiceButton.IconGlyph = SegoeMdl2Icons.Add;

        headerLayout.Controls.Add(titleStack, 0, 0);
        headerLayout.Controls.Add(newInvoiceButton, 1, 0);

        errorBannerLabel = new Label
        {
            AutoSize = true,
            Dock = DockStyle.Top,
            Font = PharmaTheme.SmallFont,
            ForeColor = PharmaTheme.Danger,
            Margin = new Padding(0, 0, 0, 10),
            MaximumSize = new Size(1100, 0),
            Padding = new Padding(14, 10, 14, 10),
            TextAlign = ContentAlignment.TopRight,
            Visible = false
        };

        statsGrid = new TableLayoutPanel
        {
            BackColor = PharmaTheme.Background,
            ColumnCount = 3,
            Dock = DockStyle.Top,
            Height = 268,
            Margin = new Padding(0, 0, 0, 16),
            RightToLeft = RightToLeft.Yes,
            RowCount = 2
        };
        statsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
        statsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
        statsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.34F));
        statsGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 128F));
        statsGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 128F));

        todaySalesCard = CreateStatCard("إجمالي مبيعات اليوم", "0", SegoeMdl2Icons.Payments);
        todayInvoicesCard = CreateStatCard("عدد الفواتير اليوم", "0", SegoeMdl2Icons.Receipt);
        expiringCard = CreateStatCard("أدوية تنتهي قريباً", "0", SegoeMdl2Icons.Expiry, StatCardVisualTone.Warning);
        lowStockCard = CreateStatCard("منخفض المخزون", "0", SegoeMdl2Icons.Warning, StatCardVisualTone.Danger);
        todayProfitCard = CreateStatCard("أرباح اليوم", "0", SegoeMdl2Icons.Chart);
        totalProductsCard = CreateStatCard("إجمالي المنتجات", "0", SegoeMdl2Icons.Product);

        statsGrid.Controls.Add(todaySalesCard, 0, 0);
        statsGrid.Controls.Add(todayInvoicesCard, 1, 0);
        statsGrid.Controls.Add(expiringCard, 2, 0);
        statsGrid.Controls.Add(lowStockCard, 0, 1);
        statsGrid.Controls.Add(todayProfitCard, 1, 1);
        statsGrid.Controls.Add(totalProductsCard, 2, 1);

        chartsRow = new TableLayoutPanel
        {
            ColumnCount = 1,
            Dock = DockStyle.Top,
            Height = 228,
            Margin = new Padding(0, 0, 0, 16),
            RightToLeft = RightToLeft.Yes,
            RowCount = 1
        };
        chartsRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        chartsRow.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        salesChartCard = new PharmaCardPanel
        {
            CornerRadius = PharmaTheme.DashboardSectionCornerRadius,
            Dock = DockStyle.Fill,
            Padding = new Padding(20, 16, 20, 16)
        };

        var chartHeader = new Label
        {
            AutoSize = false,
            Dock = DockStyle.Top,
            Font = PharmaTheme.SectionFont,
            ForeColor = PharmaTheme.TextDark,
            Height = 32,
            Margin = new Padding(0, 0, 0, 8),
            Text = "المبيعات خلال 30 يوم",
            TextAlign = ContentAlignment.MiddleRight
        };

        salesChartPlaceholder = new SalesChartPlaceholderControl { Dock = DockStyle.Fill };

        salesChartCard.Controls.Add(salesChartPlaceholder);
        salesChartCard.Controls.Add(chartHeader);
        chartsRow.Controls.Add(salesChartCard, 0, 0);

        lowerOuter = new TableLayoutPanel
        {
            ColumnCount = 2,
            Dock = DockStyle.Fill,
            MinimumSize = new Size(0, 340),
            RightToLeft = RightToLeft.Yes,
            RowCount = 1
        };
        lowerOuter.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58F));
        lowerOuter.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42F));
        lowerOuter.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        latestSalesCard = CreateSalesCard();
        stockAlertsCard = CreateAlertsCard();
        quickActionsCard = CreateQuickActionsCard();

        var rightColumn = new TableLayoutPanel
        {
            ColumnCount = 1,
            Dock = DockStyle.Fill,
            Margin = new Padding(10, 0, 0, 0),
            RightToLeft = RightToLeft.Yes,
            RowCount = 2
        };
        rightColumn.RowStyles.Add(new RowStyle(SizeType.Percent, 58F));
        rightColumn.RowStyles.Add(new RowStyle(SizeType.Percent, 42F));
        rightColumn.Controls.Add(stockAlertsCard, 0, 0);
        rightColumn.Controls.Add(quickActionsCard, 0, 1);

        lowerOuter.Controls.Add(latestSalesCard, 0, 0);
        lowerOuter.Controls.Add(rightColumn, 1, 0);

        rootTable = new TableLayoutPanel
        {
            BackColor = PharmaTheme.Background,
            ColumnCount = 1,
            Dock = DockStyle.Fill,
            RightToLeft = RightToLeft.Yes,
            RowCount = 5
        };
        rootTable.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        rootTable.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        rootTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 268F));
        rootTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 228F));
        rootTable.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        rootTable.Controls.Add(headerLayout, 0, 0);
        rootTable.Controls.Add(errorBannerLabel, 0, 1);
        rootTable.Controls.Add(statsGrid, 0, 2);
        rootTable.Controls.Add(chartsRow, 0, 3);
        rootTable.Controls.Add(lowerOuter, 0, 4);

        Controls.Add(rootTable);

        loadingOverlay = new Panel
        {
            BackColor = Color.FromArgb(150, PharmaTheme.SoftGreenBackground),
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

    private PharmaCardPanel CreateSalesCard()
    {
        var card = new PharmaCardPanel
        {
            CornerRadius = PharmaTheme.DashboardSectionCornerRadius,
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, 10, 0),
            MinimumSize = new Size(280, 280),
            Padding = new Padding(20, 16, 20, 16)
        };

        var header = new TableLayoutPanel
        {
            ColumnCount = 2,
            Dock = DockStyle.Top,
            Height = 36,
            Margin = new Padding(0, 0, 0, 10),
            RightToLeft = RightToLeft.Yes,
            RowCount = 1
        };
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 88F));

        var title = new Label
        {
            AutoSize = false,
            Dock = DockStyle.Fill,
            Font = PharmaTheme.SectionFont,
            ForeColor = PharmaTheme.TextDark,
            Text = "أحدث الفواتير",
            TextAlign = ContentAlignment.MiddleRight
        };

        var viewAll = new LinkLabel
        {
            ActiveLinkColor = PharmaTheme.PrimaryGreen,
            AutoSize = true,
            Dock = DockStyle.Fill,
            Font = PharmaTheme.SmallFont,
            LinkBehavior = LinkBehavior.HoverUnderline,
            LinkColor = PharmaTheme.PrimaryGreen,
            Text = "عرض الكل",
            TextAlign = ContentAlignment.MiddleLeft,
            VisitedLinkColor = PharmaTheme.PrimaryGreen
        };
        viewAll.LinkClicked += (_, _) => QuickActionRequested?.Invoke(this, "عرض الكل");

        header.Controls.Add(title, 0, 0);
        header.Controls.Add(viewAll, 1, 0);

        var listHost = new Panel { Dock = DockStyle.Fill };

        latestSalesList = new ListView
        {
            BackColor = PharmaTheme.SurfaceContainerLowest,
            BorderStyle = BorderStyle.None,
            Dock = DockStyle.Fill,
            Font = PharmaTheme.TableCellFont,
            ForeColor = PharmaTheme.TextDark,
            FullRowSelect = true,
            GridLines = false,
            HeaderStyle = ColumnHeaderStyle.Nonclickable,
            HideSelection = false,
            OwnerDraw = true,
            RightToLeft = RightToLeft.Yes,
            RightToLeftLayout = true,
            View = View.Details,
            UseCompatibleStateImageBehavior = false
        };
        latestSalesList.Columns.Add("رقم الفاتورة", 110);
        latestSalesList.Columns.Add("الزبون", 130);
        latestSalesList.Columns.Add("القيمة", 96);
        latestSalesList.Columns.Add("الوقت", 118);
        latestSalesList.Columns.Add("الحالة", 96);

        latestSalesEmptyLabel = new Label
        {
            Dock = DockStyle.Fill,
            Font = PharmaTheme.BodyFont,
            ForeColor = PharmaTheme.MutedText,
            Text = "لا توجد فواتير حديثة",
            TextAlign = ContentAlignment.MiddleCenter,
            Visible = false
        };

        listHost.Controls.Add(latestSalesEmptyLabel);
        listHost.Controls.Add(latestSalesList);

        card.Controls.Add(listHost);
        card.Controls.Add(header);
        return card;
    }

    private PharmaCardPanel CreateAlertsCard()
    {
        var card = new PharmaCardPanel
        {
            CornerRadius = PharmaTheme.DashboardSectionCornerRadius,
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, 0, 10),
            MinimumSize = new Size(220, 170),
            Padding = new Padding(18, 16, 18, 16)
        };

        var header = new Label
        {
            AutoSize = false,
            Dock = DockStyle.Top,
            Font = PharmaTheme.SectionFont,
            ForeColor = PharmaTheme.TextDark,
            Height = 32,
            Margin = new Padding(0, 0, 0, 10),
            Text = "تنبيهات المخزون والصلاحية",
            TextAlign = ContentAlignment.MiddleRight
        };

        stockAlertsFlow = new FlowLayoutPanel
        {
            AutoScroll = true,
            BackColor = PharmaTheme.SurfaceContainerLowest,
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false
        };

        card.Controls.Add(stockAlertsFlow);
        card.Controls.Add(header);
        return card;
    }

    private PharmaCardPanel CreateQuickActionsCard()
    {
        var card = new PharmaCardPanel
        {
            CornerRadius = PharmaTheme.DashboardSectionCornerRadius,
            Dock = DockStyle.Fill,
            MinimumSize = new Size(220, 140),
            Padding = new Padding(18, 16, 18, 16)
        };

        var header = new Label
        {
            AutoSize = false,
            Dock = DockStyle.Top,
            Font = PharmaTheme.SectionFont,
            ForeColor = PharmaTheme.TextDark,
            Height = 32,
            Margin = new Padding(0, 0, 0, 10),
            Text = "اختصارات سريعة",
            TextAlign = ContentAlignment.MiddleRight
        };

        quickActionsFlow = new FlowLayoutPanel
        {
            AutoScroll = true,
            BackColor = PharmaTheme.SurfaceContainerLowest,
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false
        };

        card.Controls.Add(quickActionsFlow);
        card.Controls.Add(header);
        return card;
    }

    private static StatCardControl CreateStatCard(string title, string value, string iconGlyph, StatCardVisualTone tone = StatCardVisualTone.Normal)
    {
        return new StatCardControl
        {
            CardTitle = title,
            CardValue = value,
            Dock = DockStyle.Fill,
            IconText = iconGlyph,
            VisualTone = tone
        };
    }
}
