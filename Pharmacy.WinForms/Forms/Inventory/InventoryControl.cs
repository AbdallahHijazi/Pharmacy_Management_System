using Pharmacy.WinForms.Controls;
using Pharmacy.WinForms.Controls.Inventory;
using Pharmacy.WinForms.Models;
using Pharmacy.WinForms.Services;
using Pharmacy.WinForms.Ui;

namespace Pharmacy.WinForms.Forms.Inventory;

internal sealed class InventoryControl : UserControl
{
    private const int WorkspacePadding = 24;
    private const int SectionGap = 16;
    private const int StatsGap = 12;
    private const int DetailsGap = 16;
    private const int WideBreakpoint = 1200;
    private const int OverlayBreakpoint = 980;

    private readonly InventoryService _inventoryService;
    private readonly System.Windows.Forms.Timer _searchDebounce = new() { Interval = 300 };

    private readonly List<InventoryProductView> _displayProducts = new();
    private readonly List<InvProductTableRow> _tableRows = new();
    private readonly List<InvFilterChip> _filterChips = new();
    private readonly string[] _filterLabels =
    [
        "الكل",
        "منخفض المخزون",
        "منتهي الصلاحية",
        "قريب الانتهاء",
        "متوفر",
        "غير متوفر"
    ];

    private InventoryListFilter _activeFilter = InventoryListFilter.All;
    private InventoryProductView? _selectedProduct;
    private InventoryStatsView _stats = InventoryStatsView.Empty();
    private string _searchText = string.Empty;
    private bool _isSearchMode;
    private bool _filtersExpanded;
    private bool _detailsOverlay;
    private int _pageNumber = 1;
    private int _pageSize = 25;
    private int _totalCount;
    private CancellationTokenSource? _loadCts;
    private CancellationTokenSource? _detailsCts;

    private Panel _rootPanel = null!;
    private Panel _mainContentPanel = null!;
    private Panel _headerPanel = null!;
    private Label _titleLabel = null!;
    private Label _subtitleLabel = null!;
    private Panel _statsCardsPanel = null!;
    private StatCardControl _totalProductsCard = null!;
    private StatCardControl _activeCard = null!;
    private StatCardControl _lowStockCard = null!;
    private StatCardControl _expiringCard = null!;
    private InvRoundedPanel _searchFilterCard = null!;
    private InvSearchBox _searchBox = null!;
    private InvFilterToggleButton _filterToggle = null!;
    private FlowLayoutPanel _filterChipsPanel = null!;
    private InvRoundedPanel _tableCard = null!;
    private InvProductTableHeader _tableHeader = null!;
    private Panel _tableScrollPanel = null!;
    private Panel _tableRowsHost = null!;
    private InvPaginationBar _paginationBar = null!;
    private Panel _statePanel = null!;
    private Label _stateTitle = null!;
    private Label _stateDetail = null!;
    private Button _retryButton = null!;
    private InvProductDetailsPanel _detailsPanel = null!;

    public InventoryControl() : this(AppServices.InventoryService)
    {
    }

    public InventoryControl(InventoryService inventoryService)
    {
        _inventoryService = inventoryService;
        _searchDebounce.Tick += (_, _) => _ = ApplySearchAsync();

        SuspendLayout();
        SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        DoubleBuffered = true;
        Dock = DockStyle.Fill;
        BackColor = PharmaTheme.Background;
        RightToLeft = RightToLeft.Yes;
        Padding = Padding.Empty;

        BuildUi();
        WireEvents();

        ThemeManager.ThemeChanged += HandleThemeChanged;
        FontScaleManager.Changed += HandleThemeChanged;

        ResumeLayout(false);
        Load += async (_, _) => await LoadPageAsync();
        SizeChanged += (_, _) => LayoutInventoryPage();
    }

    public void ApplyThemeVisuals()
    {
        BackColor = PharmaTheme.Background;
        _rootPanel.BackColor = PharmaTheme.Background;
        _mainContentPanel.BackColor = PharmaTheme.Background;
        _headerPanel.BackColor = PharmaTheme.Background;
        _statsCardsPanel.BackColor = PharmaTheme.Background;

        _titleLabel.ForeColor = PharmaTheme.PrimaryDark;
        _titleLabel.Font = PharmaTheme.DashboardHeadlineFont;
        _subtitleLabel.ForeColor = PharmaTheme.OnSurfaceVariant;
        _subtitleLabel.Font = PharmaTheme.DashboardSubtitleFont;

        _totalProductsCard.Invalidate();
        _activeCard.Invalidate();
        _lowStockCard.Invalidate();
        _expiringCard.Invalidate();

        _searchFilterCard.FillColor = PharmaTheme.Surface;
        _searchFilterCard.ApplyThemeVisuals();
        _searchBox.ApplyThemeVisuals();
        _filterToggle.ApplyThemeVisuals();
        foreach (var chip in _filterChips)
        {
            chip.ApplyThemeVisuals();
        }

        _tableCard.FillColor = PharmaTheme.Surface;
        _tableCard.ApplyThemeVisuals();
        _tableHeader.ApplyThemeVisuals();
        _paginationBar.ApplyThemeVisuals();
        _detailsPanel.ApplyThemeVisuals();

        foreach (var row in _tableRows)
        {
            row.ApplyThemeVisuals();
        }

        LayoutInventoryPage();
        Invalidate(true);
    }

    private void BuildUi()
    {
        _rootPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = PharmaTheme.Background,
            Padding = Padding.Empty
        };

        _detailsPanel = new InvProductDetailsPanel();

        _mainContentPanel = new Panel
        {
            BackColor = PharmaTheme.Background,
            Padding = Padding.Empty
        };

        _headerPanel = new Panel { BackColor = PharmaTheme.Background, Height = 72 };
        _titleLabel = new Label
        {
            Text = "إدارة المخزون",
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleRight,
            Font = PharmaTheme.DashboardHeadlineFont,
            ForeColor = PharmaTheme.PrimaryDark
        };
        _subtitleLabel = new Label
        {
            Text = "مراقبة الكميات، الصلاحية، والطلبات المعلقة.",
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleRight,
            Font = PharmaTheme.DashboardSubtitleFont,
            ForeColor = PharmaTheme.OnSurfaceVariant
        };
        _headerPanel.Controls.Add(_subtitleLabel);
        _headerPanel.Controls.Add(_titleLabel);

        _statsCardsPanel = new Panel { BackColor = PharmaTheme.Background, Height = 148 };
        _totalProductsCard = CreateStatCard("إجمالي الأصناف", SegoeMdl2Icons.Inventory, StatCardVisualTone.Normal);
        _activeCard = CreateStatCard("نشط ومتوفر", SegoeMdl2Icons.Product, StatCardVisualTone.Normal);
        _lowStockCard = CreateStatCard("نقص مخزون", SegoeMdl2Icons.Warning, StatCardVisualTone.Danger);
        _expiringCard = CreateStatCard("منتهي أو قريب الانتهاء", SegoeMdl2Icons.Expiry, StatCardVisualTone.Warning);
        _statsCardsPanel.Controls.AddRange([_totalProductsCard, _activeCard, _lowStockCard, _expiringCard]);

        _searchFilterCard = new InvRoundedPanel(PharmaTheme.InventoryCardCornerRadius)
        {
            FillColor = PharmaTheme.Surface
        };
        _searchBox = new InvSearchBox
        {
            PlaceholderText = "ابحث بالاسم أو الباركود أو المورد..."
        };
        _filterToggle = new InvFilterToggleButton();
        _filterChipsPanel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = true,
            AutoSize = false,
            BackColor = Color.Transparent,
            Visible = false,
            RightToLeft = RightToLeft.Yes,
            Padding = new Padding(0, 8, 0, 0)
        };

        foreach (var label in _filterLabels)
        {
            var chip = new InvFilterChip(label) { Margin = new Padding(0, 0, 8, 8) };
            chip.Click += (_, _) => _ = SetFilterAsync(MapFilter(label));
            _filterChips.Add(chip);
            _filterChipsPanel.Controls.Add(chip);
        }

        _searchFilterCard.Controls.Add(_filterChipsPanel);
        _searchFilterCard.Controls.Add(_filterToggle);
        _searchFilterCard.Controls.Add(_searchBox);

        _tableCard = new InvRoundedPanel(PharmaTheme.InventoryCardCornerRadius)
        {
            FillColor = PharmaTheme.Surface
        };
        _tableHeader = new InvProductTableHeader();
        _tableScrollPanel = new Panel
        {
            AutoScroll = true,
            BackColor = Color.Transparent
        };
        _tableRowsHost = new Panel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            BackColor = Color.Transparent,
            Dock = DockStyle.Top,
            Width = 10
        };
        _tableScrollPanel.Controls.Add(_tableRowsHost);
        _paginationBar = new InvPaginationBar();

        _statePanel = new Panel
        {
            Visible = false,
            BackColor = Color.Transparent
        };
        _stateTitle = new Label
        {
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = PharmaTheme.SectionFont,
            ForeColor = PharmaTheme.TextDark,
            Dock = DockStyle.Top,
            Height = 32
        };
        _stateDetail = new Label
        {
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = PharmaTheme.BodyFont,
            ForeColor = PharmaTheme.OnSurfaceVariant,
            Dock = DockStyle.Top,
            Height = 28
        };
        _retryButton = new Button
        {
            Text = "إعادة المحاولة",
            AutoSize = true,
            FlatStyle = FlatStyle.Flat,
            BackColor = PharmaTheme.Primary,
            ForeColor = PharmaTheme.OnPrimary,
            Font = PharmaTheme.ArabicFont(10f, FontStyle.Bold),
            Cursor = Cursors.Hand,
            Padding = new Padding(16, 8, 16, 8)
        };
        _statePanel.Controls.Add(_retryButton);
        _statePanel.Controls.Add(_stateDetail);
        _statePanel.Controls.Add(_stateTitle);

        _tableCard.Controls.Add(_statePanel);
        _tableCard.Controls.Add(_paginationBar);
        _tableCard.Controls.Add(_tableScrollPanel);
        _tableCard.Controls.Add(_tableHeader);

        _mainContentPanel.Controls.Add(_tableCard);
        _mainContentPanel.Controls.Add(_searchFilterCard);
        _mainContentPanel.Controls.Add(_statsCardsPanel);
        _mainContentPanel.Controls.Add(_headerPanel);

        _rootPanel.Controls.Add(_mainContentPanel);
        _rootPanel.Controls.Add(_detailsPanel);

        Controls.Add(_rootPanel);
        SetFilterChipSelected(InventoryListFilter.All);
    }

    private static StatCardControl CreateStatCard(string title, string icon, StatCardVisualTone tone)
    {
        return new StatCardControl
        {
            CardTitle = title,
            CardValue = "—",
            IconText = icon,
            VisualTone = tone
        };
    }

    private void WireEvents()
    {
        _searchBox.SearchTextChanged += (_, _) =>
        {
            _searchDebounce.Stop();
            _searchDebounce.Start();
        };
        _filterToggle.Click += (_, _) =>
        {
            _filtersExpanded = !_filtersExpanded;
            _filterChipsPanel.Visible = _filtersExpanded;
            LayoutInventoryPage();
        };
        _retryButton.Click += async (_, _) => await LoadPageAsync();
        _paginationBar.PreviousRequested += async (_, _) => await ChangePageAsync(_pageNumber - 1);
        _paginationBar.NextRequested += async (_, _) => await ChangePageAsync(_pageNumber + 1);
        _detailsPanel.CloseRequested += (_, _) => ClearSelection();
        _detailsPanel.PurchaseOrderRequested += (_, _) =>
            MessageBox.Show(this, "سيتم ربط طلب الشراء لاحقًا.", "طلب شراء", MessageBoxButtons.OK, MessageBoxIcon.Information);
        _detailsPanel.EditRequested += (_, _) =>
            MessageBox.Show(this, "سيتم ربط تعديل البيانات لاحقًا.", "تعديل البيانات", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void HandleThemeChanged(object? sender, EventArgs e)
    {
        if (IsDisposed)
        {
            return;
        }

        if (InvokeRequired)
        {
            BeginInvoke(ApplyThemeVisuals);
            return;
        }

        ApplyThemeVisuals();
    }

    private void LayoutInventoryPage()
    {
        if (_rootPanel is null || _rootPanel.ClientSize.Width <= 0)
        {
            return;
        }

        var bounds = _rootPanel.ClientRectangle;
        var pad = WorkspacePadding;
        var contentW = Math.Max(320, bounds.Width - pad * 2);
        var contentH = Math.Max(240, bounds.Height - pad * 2);
        var showDetails = _selectedProduct is not null;
        _detailsOverlay = showDetails && bounds.Width < OverlayBreakpoint;

        var detailsW = showDetails && !_detailsOverlay
            ? Math.Clamp(PharmaTheme.InventoryDetailsWidth, 340, 420)
            : 0;

        if (_detailsOverlay && showDetails)
        {
            _detailsPanel.Bounds = new Rectangle(
                pad,
                pad,
                Math.Min(contentW, 420),
                contentH);
            _detailsPanel.BringToFront();
        }
        else if (showDetails)
        {
            _detailsPanel.SetBounds(pad, pad, detailsW, contentH);
            _detailsPanel.BringToFront();
        }
        else
        {
            _detailsPanel.SetBounds(-500, pad, detailsW, contentH);
        }

        var mainX = showDetails && !_detailsOverlay ? pad + detailsW + DetailsGap : pad;
        var mainW = showDetails && !_detailsOverlay ? contentW - detailsW - DetailsGap : contentW;
        _mainContentPanel.SetBounds(mainX, pad, mainW, contentH);

        var y = 0;
        _headerPanel.SetBounds(0, y, mainW, 72);
        y += _headerPanel.Height + SectionGap;

        LayoutStatsCards(mainW, y);
        y += _statsCardsPanel.Height + SectionGap;

        var searchH = _filtersExpanded ? 132 : 72;
        _searchFilterCard.SetBounds(0, y, mainW, searchH);
        _searchBox.SetBounds(12, 12, mainW - 12 - 12 - 104, 48);
        _filterToggle.SetBounds(mainW - 12 - 96, 16, 96, 40);
        _filterChipsPanel.SetBounds(12, 64, mainW - 24, 56);
        y += searchH + SectionGap;

        var tableH = Math.Max(180, contentH - y);
        _tableCard.SetBounds(0, y, mainW, tableH);
        _tableHeader.SetBounds(0, 0, mainW, 44);
        _paginationBar.SetBounds(0, tableH - 44, mainW, 44);
        _tableScrollPanel.SetBounds(0, 44, mainW, tableH - 44 - 44);
        _statePanel.SetBounds(0, 44, mainW, tableH - 44 - 44);

        _titleLabel.SetBounds(0, 0, mainW, 36);
        _subtitleLabel.SetBounds(0, 38, mainW, 24);
    }

    private void LayoutStatsCards(int mainW, int y)
    {
        _statsCardsPanel.SetBounds(0, y, mainW, 148);
        var cardCount = 4;
        var gap = StatsGap;
        var cardW = Math.Max(150, (mainW - gap * (cardCount - 1)) / cardCount);
        var cards = new[] { _totalProductsCard, _activeCard, _lowStockCard, _expiringCard };

        if (mainW < WideBreakpoint)
        {
            var halfW = Math.Max(150, (mainW - gap) / 2);
            for (var i = 0; i < cards.Length; i++)
            {
                var row = i / 2;
                var col = i % 2;
                cards[i].SetBounds(col * (halfW + gap), row * 70, halfW, 136);
            }

            _statsCardsPanel.Height = mainW < 760 ? 292 : 148;
            return;
        }

        for (var i = 0; i < cards.Length; i++)
        {
            cards[i].SetBounds(i * (cardW + gap), 6, cardW, 136);
        }

        _statsCardsPanel.Height = 148;
    }

    private async Task LoadPageAsync()
    {
        _loadCts?.Cancel();
        _loadCts = new CancellationTokenSource();
        var token = _loadCts.Token;

        ShowLoadingState("جاري تحميل المخزون...");

        try
        {
            var statsTask = _inventoryService.LoadStatsAsync(token);
            var pageTask = LoadProductsForCurrentModeAsync(token);
            await Task.WhenAll(statsTask, pageTask).ConfigureAwait(true);

            _stats = await statsTask.ConfigureAwait(true);
            ApplyStatsCards();

            var page = await pageTask.ConfigureAwait(true);
            if (!page.Success)
            {
                ShowErrorState(page.ErrorMessage ?? "تعذر تحميل المخزون.", page.IsConnectionError);
                return;
            }

            _pageNumber = page.PageNumber;
            _pageSize = page.PageSize;
            _totalCount = page.TotalCount;
            _displayProducts.Clear();
            _displayProducts.AddRange(ApplyClientFilter(page.Products));
            RenderTable();
            UpdatePagination();
            HideStatePanel();
        }
        catch (OperationCanceledException)
        {
            // Ignore.
        }
    }

    private async Task<InventoryPageState> LoadProductsForCurrentModeAsync(CancellationToken token)
    {
        if (_isSearchMode && !string.IsNullOrWhiteSpace(_searchText))
        {
            return await _inventoryService.SearchProductsAsync(_searchText, token).ConfigureAwait(true);
        }

        return _activeFilter switch
        {
            InventoryListFilter.LowStock => WrapList(await _inventoryService.LoadLowStockProductsAsync(token).ConfigureAwait(true)),
            InventoryListFilter.ExpiringSoon => WrapList(await _inventoryService.LoadExpiringProductsAsync(token).ConfigureAwait(true)),
            _ => await _inventoryService.LoadProductsPageAsync(_pageNumber, _pageSize, token).ConfigureAwait(true)
        };
    }

    private static InventoryPageState WrapList(IReadOnlyList<InventoryProductView> products) => new()
    {
        Success = true,
        Products = products,
        TotalCount = products.Count,
        PageNumber = 1,
        PageSize = products.Count > 0 ? products.Count : 25
    };

    private async Task ApplySearchAsync()
    {
        _searchDebounce.Stop();
        _searchText = (_searchBox.Text ?? string.Empty).Trim();
        _isSearchMode = !string.IsNullOrWhiteSpace(_searchText);
        _pageNumber = 1;
        await LoadPageAsync();
    }

    private async Task SetFilterAsync(InventoryListFilter filter)
    {
        _activeFilter = filter;
        SetFilterChipSelected(filter);
        _pageNumber = 1;
        _isSearchMode = false;
        _searchBox.Text = string.Empty;
        await LoadPageAsync();
    }

    private async Task ChangePageAsync(int newPage)
    {
        if (_isSearchMode || _activeFilter is InventoryListFilter.LowStock or InventoryListFilter.ExpiringSoon)
        {
            return;
        }

        if (newPage < 1)
        {
            return;
        }

        var maxPage = Math.Max(1, (int)Math.Ceiling(_totalCount / (double)_pageSize));
        if (newPage > maxPage)
        {
            return;
        }

        _pageNumber = newPage;
        await LoadPageAsync();
    }

    private IEnumerable<InventoryProductView> ApplyClientFilter(IReadOnlyList<InventoryProductView> products)
    {
        return _activeFilter switch
        {
            InventoryListFilter.Expired => products.Where(p =>
                p.Status == InventoryProductStatus.Expired || p.ExpiredQuantity > 0),
            InventoryListFilter.Available => products.Where(p =>
                p.Status == InventoryProductStatus.Available),
            InventoryListFilter.OutOfStock => products.Where(p =>
                p.Status == InventoryProductStatus.OutOfStock),
            InventoryListFilter.ExpiringSoon => products.Where(p =>
                p.IsExpiringSoon || p.Status == InventoryProductStatus.ExpiringSoon),
            InventoryListFilter.LowStock => products.Where(p =>
                p.Status == InventoryProductStatus.LowStock),
            _ => products
        };
    }

    private void ApplyStatsCards()
    {
        if (!_stats.HasData)
        {
            _totalProductsCard.CardValue = "—";
            _activeCard.CardValue = "—";
            _lowStockCard.CardValue = "—";
            _expiringCard.CardValue = "—";
            _activeCard.CardBadge = null;
            return;
        }

        _totalProductsCard.CardValue = _stats.TotalProducts.ToString("N0");
        _activeCard.CardValue = _stats.ActiveAvailable.ToString("N0");
        _activeCard.CardBadge = _stats.ActiveAvailableBadge;
        _lowStockCard.CardValue = _stats.LowStockCount.ToString("N0");
        _expiringCard.CardValue = _stats.ExpiringOrExpiredCount.ToString("N0");
    }

    private void RenderTable()
    {
        _tableRowsHost.Controls.Clear();
        _tableRows.Clear();

        if (_displayProducts.Count == 0)
        {
            ShowEmptyState(_isSearchMode ? "لا توجد منتجات مطابقة" : "لا توجد منتجات");
            return;
        }

        HideStatePanel();
        foreach (var product in _displayProducts)
        {
            var row = new InvProductTableRow(product);
            row.IsRowSelected = _selectedProduct?.ProductId == product.ProductId;
            row.RowClicked += async (_, _) => await SelectProductAsync(product);
            _tableRows.Add(row);
            _tableRowsHost.Controls.Add(row);
        }

        _tableRowsHost.Width = Math.Max(_tableScrollPanel.ClientSize.Width, _tableScrollPanel.DisplayRectangle.Width);
        LayoutInventoryPage();
    }

    private async Task SelectProductAsync(InventoryProductView product)
    {
        _selectedProduct = product;
        foreach (var row in _tableRows)
        {
            row.IsRowSelected = row.Product.ProductId == product.ProductId;
        }

        _detailsPanel.Bind(null);
        _detailsPanel.Visible = true;
        LayoutInventoryPage();

        _detailsCts?.Cancel();
        _detailsCts = new CancellationTokenSource();
        var details = await _inventoryService.LoadProductDetailsAsync(product.ProductId, _detailsCts.Token)
            .ConfigureAwait(true);

        if (details is null)
        {
            _detailsPanel.Bind(new InventoryProductDetailsView { Product = product });
            return;
        }

        _detailsPanel.Bind(details);
    }

    private void ClearSelection()
    {
        _selectedProduct = null;
        foreach (var row in _tableRows)
        {
            row.IsRowSelected = false;
        }

        _detailsPanel.Bind(null);
        LayoutInventoryPage();
    }

    private void UpdatePagination()
    {
        if (_displayProducts.Count == 0)
        {
            _paginationBar.Update(0, 0, _totalCount, false, false);
            return;
        }

        var from = _isSearchMode || _activeFilter is InventoryListFilter.LowStock or InventoryListFilter.ExpiringSoon
            ? 1
            : ((_pageNumber - 1) * _pageSize) + 1;
        var to = _isSearchMode || _activeFilter is InventoryListFilter.LowStock or InventoryListFilter.ExpiringSoon
            ? _displayProducts.Count
            : Math.Min(_pageNumber * _pageSize, _totalCount);
        var maxPage = Math.Max(1, (int)Math.Ceiling(_totalCount / (double)_pageSize));
        _paginationBar.Update(from, to, _totalCount, _pageNumber > 1, _pageNumber < maxPage);
    }

    private void ShowLoadingState(string message)
    {
        _statePanel.Visible = true;
        _stateTitle.Text = message;
        _stateDetail.Text = string.Empty;
        _retryButton.Visible = false;
        _tableScrollPanel.Visible = false;
    }

    private void ShowErrorState(string message, bool isConnection)
    {
        _statePanel.Visible = true;
        _stateTitle.Text = "تعذر تحميل المخزون";
        _stateDetail.Text = isConnection
            ? $"{message}{Environment.NewLine}تحقق من أن API يعمل على {ApiConfiguration.BaseUrl}"
            : message;
        _retryButton.Visible = true;
        _tableScrollPanel.Visible = false;
        _displayProducts.Clear();
        _tableRowsHost.Controls.Clear();
        _tableRows.Clear();
        UpdatePagination();
    }

    private void ShowEmptyState(string message)
    {
        _statePanel.Visible = true;
        _stateTitle.Text = message;
        _stateDetail.Text = _isSearchMode ? "جرّب كلمات بحث مختلفة أو أزل الفلاتر." : string.Empty;
        _retryButton.Visible = false;
        _tableScrollPanel.Visible = false;
        UpdatePagination();
    }

    private void HideStatePanel()
    {
        _statePanel.Visible = false;
        _tableScrollPanel.Visible = true;
    }

    private void SetFilterChipSelected(InventoryListFilter filter)
    {
        for (var i = 0; i < _filterChips.Count; i++)
        {
            _filterChips[i].IsSelected = MapFilter(_filterLabels[i]) == filter;
        }
    }

    private static InventoryListFilter MapFilter(string label) => label switch
    {
        "منخفض المخزون" => InventoryListFilter.LowStock,
        "منتهي الصلاحية" => InventoryListFilter.Expired,
        "قريب الانتهاء" => InventoryListFilter.ExpiringSoon,
        "متوفر" => InventoryListFilter.Available,
        "غير متوفر" => InventoryListFilter.OutOfStock,
        _ => InventoryListFilter.All
    };

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            ThemeManager.ThemeChanged -= HandleThemeChanged;
            FontScaleManager.Changed -= HandleThemeChanged;
            _searchDebounce.Dispose();
            _loadCts?.Cancel();
            _loadCts?.Dispose();
            _detailsCts?.Cancel();
            _detailsCts?.Dispose();
        }

        base.Dispose(disposing);
    }
}
