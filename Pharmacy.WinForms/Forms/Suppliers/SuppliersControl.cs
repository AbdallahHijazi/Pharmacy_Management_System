using Pharmacy.WinForms.Controls;
using Pharmacy.WinForms.Controls.Suppliers;
using Pharmacy.WinForms.Models;
using Pharmacy.WinForms.Services;
using Pharmacy.WinForms.Ui;

namespace Pharmacy.WinForms.Forms.Suppliers;

internal sealed class SuppliersControl : UserControl
{
    private const int WorkspacePadding = 32;
    private const int SectionGap = 16;
    private const int HeaderHeight = 104;
    private const int StatsHeight = 136;
    private const int PaginationHeight = 64;
    private const int DetailsGap = 16;
    private const int OverlayBreakpoint = 980;
    private const int RowGap = 10;

    private readonly SupplierService _supplierService;
    private readonly System.Windows.Forms.Timer _searchDebounce = new() { Interval = 300 };

    private readonly List<SupplierListItemView> _allSuppliers = new();
    private readonly List<SupplierListItemView> _displaySuppliers = new();
    private readonly List<SupSupplierRow> _supplierRows = new();

    private string _searchText = string.Empty;
    private SupplierListItemView? _selectedSupplier;
    private bool _detailsOverlay;
    private int _pageNumber = 1;
    private int _pageSize = 10;
    private int _totalCount;
    private CancellationTokenSource? _loadCts;

    private Panel _rootPanel = null!;
    private Panel _mainContentPanel = null!;
    private Panel _headerPanel = null!;
    private Label _titleLabel = null!;
    private Label _subtitleLabel = null!;
    private GradientRoundedButton _addSupplierButton = null!;
    private SupSearchBox _searchBox = null!;
    private Panel _statsPanel = null!;
    private SupStatCard _totalSuppliersCard = null!;
    private SupStatCard _monthlyPurchasesCard = null!;
    private SupStatCard _unpaidDuesCard = null!;
    private Panel _tablePanel = null!;
    private SupTableHeader _tableHeader = null!;
    private Panel _rowsScrollPanel = null!;
    private Panel _rowsHostPanel = null!;
    private SupPaginationBar _paginationBar = null!;
    private Panel _statePanel = null!;
    private Label _stateTitle = null!;
    private Label _stateDetail = null!;
    private Button _retryButton = null!;
    private SupSupplierDetailsPanel _detailsPanel = null!;

    public SuppliersControl() : this(AppServices.SupplierService)
    {
    }

    public SuppliersControl(SupplierService supplierService)
    {
        _supplierService = supplierService;
        _searchDebounce.Tick += (_, _) => _ = ApplySearchAsync();

        SuspendLayout();
        SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        DoubleBuffered = true;
        Dock = DockStyle.Fill;
        BackColor = PharmaTheme.Background;
        RightToLeft = RightToLeft.Yes;

        BuildUi();
        WireEvents();

        ThemeManager.ThemeChanged += HandleThemeChanged;
        FontScaleManager.Changed += HandleThemeChanged;

        ResumeLayout(false);
        Load += async (_, _) => await LoadPageAsync();
        SizeChanged += (_, _) => LayoutSuppliersPage();
    }

    public void ApplyThemeVisuals()
    {
        BackColor = PharmaTheme.Background;
        _rootPanel.BackColor = PharmaTheme.Background;
        _mainContentPanel.BackColor = PharmaTheme.Background;
        _headerPanel.BackColor = PharmaTheme.Background;
        _statsPanel.BackColor = PharmaTheme.Background;
        _tablePanel.BackColor = PharmaTheme.Background;
        _rowsScrollPanel.BackColor = PharmaTheme.Background;
        _rowsHostPanel.BackColor = PharmaTheme.Background;
        _statePanel.BackColor = PharmaTheme.Background;

        _titleLabel.ForeColor = PharmaTheme.PrimaryDark;
        _titleLabel.Font = PharmaTheme.DashboardHeadlineFont;
        _subtitleLabel.ForeColor = PharmaTheme.OnSurfaceVariant;
        _subtitleLabel.Font = PharmaTheme.DashboardSubtitleFont;
        _addSupplierButton.Invalidate();
        _searchBox.ApplyThemeVisuals();
        _totalSuppliersCard.ApplyThemeVisuals();
        _monthlyPurchasesCard.ApplyThemeVisuals();
        _unpaidDuesCard.ApplyThemeVisuals();
        _tableHeader.ApplyThemeVisuals();
        _paginationBar.ApplyThemeVisuals();
        _detailsPanel.ApplyThemeVisuals();

        foreach (var row in _supplierRows)
        {
            row.ApplyThemeVisuals();
        }

        LayoutSuppliersPage();
        Invalidate(true);
    }

    private void BuildUi()
    {
        _rootPanel = new Panel { Dock = DockStyle.Fill, BackColor = PharmaTheme.Background };
        _detailsPanel = new SupSupplierDetailsPanel();
        _mainContentPanel = new Panel { BackColor = PharmaTheme.Background };

        _headerPanel = new Panel { BackColor = PharmaTheme.Background, Height = HeaderHeight };
        _titleLabel = new Label
        {
            Text = "إدارة الموردين",
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleRight,
            Font = PharmaTheme.DashboardHeadlineFont,
            ForeColor = PharmaTheme.PrimaryDark,
            RightToLeft = RightToLeft.Yes,
            BackColor = PharmaTheme.Background
        };
        _subtitleLabel = new Label
        {
            Text = "قائمة شاملة لشركات الأدوية والموزعين المعتمدين.",
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleRight,
            Font = PharmaTheme.DashboardSubtitleFont,
            ForeColor = PharmaTheme.OnSurfaceVariant,
            RightToLeft = RightToLeft.Yes,
            BackColor = PharmaTheme.Background
        };
        _addSupplierButton = new GradientRoundedButton
        {
            Text = "إضافة مورد",
            IconGlyph = SegoeMdl2Icons.Add,
            Width = 190,
            Height = 52
        };
        _searchBox = new SupSearchBox { PlaceholderText = "البحث عن مورد..." };

        _headerPanel.Controls.Add(_searchBox);
        _headerPanel.Controls.Add(_addSupplierButton);
        _headerPanel.Controls.Add(_subtitleLabel);
        _headerPanel.Controls.Add(_titleLabel);

        _statsPanel = new Panel { BackColor = PharmaTheme.Background, Height = StatsHeight };
        _totalSuppliersCard = new SupStatCard
        {
            CardTitle = "إجمالي الموردين",
            IconGlyph = SegoeMdl2Icons.Suppliers,
            CardValue = "0"
        };
        _monthlyPurchasesCard = new SupStatCard
        {
            CardTitle = "إجمالي المشتريات هذا الشهر",
            IconGlyph = SegoeMdl2Icons.Purchases,
            CardValue = "غير متوفر"
        };
        _unpaidDuesCard = new SupStatCard
        {
            CardTitle = "المستحقات غير المدفوعة",
            IconGlyph = SegoeMdl2Icons.Currency,
            CardValue = "غير متوفر",
            DangerTone = true
        };
        _statsPanel.Controls.Add(_unpaidDuesCard);
        _statsPanel.Controls.Add(_monthlyPurchasesCard);
        _statsPanel.Controls.Add(_totalSuppliersCard);

        _tablePanel = new Panel { BackColor = PharmaTheme.Background };
        _tableHeader = new SupTableHeader();
        _rowsScrollPanel = new Panel
        {
            AutoScroll = true,
            BackColor = PharmaTheme.Background
        };
        _rowsHostPanel = new Panel { BackColor = PharmaTheme.Background };
        _rowsScrollPanel.Controls.Add(_rowsHostPanel);
        _tablePanel.Controls.Add(_rowsScrollPanel);
        _tablePanel.Controls.Add(_tableHeader);
        _tablePanel.Resize += (_, _) => LayoutTableInternals();
        _rowsScrollPanel.Resize += (_, _) => LayoutSupplierRows();

        _paginationBar = new SupPaginationBar();

        _statePanel = new Panel { Visible = false, BackColor = PharmaTheme.Background };
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
            Cursor = Cursors.Hand
        };
        _statePanel.Controls.Add(_retryButton);
        _statePanel.Controls.Add(_stateDetail);
        _statePanel.Controls.Add(_stateTitle);

        _mainContentPanel.Controls.Add(_statePanel);
        _mainContentPanel.Controls.Add(_paginationBar);
        _mainContentPanel.Controls.Add(_tablePanel);
        _mainContentPanel.Controls.Add(_statsPanel);
        _mainContentPanel.Controls.Add(_headerPanel);

        _rootPanel.Controls.Add(_mainContentPanel);
        _rootPanel.Controls.Add(_detailsPanel);
        Controls.Add(_rootPanel);
    }

    private void WireEvents()
    {
        _searchBox.SearchTextChanged += (_, _) =>
        {
            _searchDebounce.Stop();
            _searchDebounce.Start();
        };
        _addSupplierButton.Click += async (_, _) => await OpenAddSupplierDialogAsync();
        _retryButton.Click += async (_, _) => await LoadPageAsync();
        _paginationBar.PageChangeRequested += async (_, page) => await ChangePageAsync(page);
        _detailsPanel.CloseRequested += (_, _) => ClearDetails();
    }

    private async Task OpenAddSupplierDialogAsync()
    {
        var owner = FindForm();
        using var dialog = new AddSupplierDialog(_supplierService);
        var result = owner is null ? dialog.ShowDialog() : dialog.ShowDialog(owner);
        if (result != DialogResult.OK)
        {
            return;
        }

        _pageNumber = 1;
        _searchBox.Text = string.Empty;
        _searchText = string.Empty;
        ClearDetails();
        await LoadPageAsync();
    }

    private async Task OpenEditSupplierDialogAsync(SupplierListItemView supplier)
    {
        var owner = FindForm();
        using var dialog = new EditSupplierDialog(supplier, _supplierService);
        var result = owner is null ? dialog.ShowDialog() : dialog.ShowDialog(owner);
        if (result != DialogResult.OK)
        {
            return;
        }

        ClearDetails();
        await LoadPageAsync();
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

    private void LayoutSuppliersPage()
    {
        if (_rootPanel.ClientSize.Width <= 0)
        {
            return;
        }

        var bounds = _rootPanel.ClientRectangle;
        var pad = WorkspacePadding;
        var contentW = Math.Max(320, bounds.Width - pad * 2);
        var contentH = Math.Max(240, bounds.Height - pad * 2);
        var showDetails = _detailsPanel.Visible && _selectedSupplier is not null;
        _detailsOverlay = showDetails && bounds.Width < OverlayBreakpoint;

        var detailsW = showDetails && !_detailsOverlay
            ? Math.Clamp(PharmaTheme.SuppliersDetailsWidth, 320, 380)
            : 0;

        if (showDetails && !_detailsOverlay)
        {
            var tentativeMain = contentW - detailsW - DetailsGap;
            if (tentativeMain < 520)
            {
                _detailsOverlay = true;
                detailsW = 0;
            }
        }

        if (_detailsOverlay && showDetails)
        {
            _detailsPanel.Bounds = new Rectangle(pad, pad, Math.Min(contentW, 380), contentH);
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
        LayoutHeader(mainW);
        _headerPanel.SetBounds(0, y, mainW, _headerPanel.Height);

        y = _headerPanel.Height + SectionGap;
        _statsPanel.SetBounds(0, y, mainW, _statsPanel.Height);
        LayoutStatsCards(mainW);

        y += _statsPanel.Height + SectionGap;
        var tableH = Math.Max(160, contentH - y - PaginationHeight - SectionGap);
        _tablePanel.SetBounds(0, y, mainW, tableH);
        _statePanel.SetBounds(0, y, mainW, tableH);
        _paginationBar.SetBounds(0, contentH - PaginationHeight, mainW, PaginationHeight);

        LayoutTableInternals();
    }

    private void LayoutHeader(int mainW)
    {
        const int buttonW = 190;
        const int searchW = 260;
        const int gap = 16;
        const int actionBlockW = buttonW + gap + searchW;

        if (mainW < 900)
        {
            _headerPanel.Height = 176;
            _titleLabel.SetBounds(0, 0, mainW, 38);
            _subtitleLabel.SetBounds(0, 40, mainW, 24);
            _searchBox.SetBounds(0, 72, mainW, 50);
            _addSupplierButton.SetBounds(0, 128, Math.Min(buttonW, mainW), 52);
            return;
        }

        _headerPanel.Height = HeaderHeight;
        var textW = Math.Max(360, mainW - actionBlockW - gap);
        _titleLabel.SetBounds(0, 0, textW, 38);
        _subtitleLabel.SetBounds(0, 40, textW, 24);
        _addSupplierButton.SetBounds(mainW - buttonW, 16, buttonW, 52);
        _searchBox.SetBounds(mainW - buttonW - gap - searchW, 20, searchW, 50);
    }

    private void LayoutStatsCards(int mainW)
    {
        var gap = 16;
        var cols = mainW >= 1100 ? 3 : mainW >= 720 ? 2 : 1;
        var cardW = cols == 1 ? mainW : (mainW - gap * (cols - 1)) / cols;
        var cardH = 128;
        var cards = new[] { _totalSuppliersCard, _monthlyPurchasesCard, _unpaidDuesCard };

        for (var i = 0; i < cards.Length; i++)
        {
            if (cols == 1)
            {
                cards[i].SetBounds(0, i * (cardH + gap), cardW, cardH);
            }
            else if (cols == 2)
            {
                var row = i / 2;
                var col = i % 2;
                if (i == 2)
                {
                    cards[i].SetBounds(0, row * (cardH + gap), cardW, cardH);
                }
                else
                {
                    cards[i].SetBounds(col * (cardW + gap), row * (cardH + gap), cardW, cardH);
                }
            }
            else
            {
                cards[i].SetBounds(i * (cardW + gap), 0, cardW, cardH);
            }
        }

        _statsPanel.Height = cols switch
        {
            1 => cards.Length * (cardH + gap) - gap,
            2 => 2 * (cardH + gap) - gap,
            _ => cardH
        };
    }

    private void LayoutTableInternals()
    {
        if (_tablePanel.ClientSize.Width <= 0 || _tablePanel.ClientSize.Height <= 0)
        {
            return;
        }

        var tableW = _tablePanel.ClientSize.Width;
        var tableH = _tablePanel.ClientSize.Height;
        var headerH = _tableHeader.Height;
        _tableHeader.SetBounds(0, 0, tableW, headerH);
        _rowsScrollPanel.SetBounds(0, headerH, tableW, Math.Max(0, tableH - headerH));
        _tableHeader.Invalidate();
        LayoutSupplierRows();
    }

    private void LayoutSupplierRows()
    {
        var tableW = _tablePanel.ClientSize.Width;
        if (tableW <= 0)
        {
            return;
        }

        var y = 0;
        foreach (Control row in _rowsHostPanel.Controls)
        {
            row.SetBounds(0, y, tableW, PharmaTheme.SuppliersRowHeight);
            row.Invalidate();
            y += PharmaTheme.SuppliersRowHeight + RowGap;
        }

        _rowsHostPanel.Size = new Size(tableW, Math.Max(y, 0));
        _rowsHostPanel.MinimumSize = new Size(tableW, 0);
    }

    private async Task LoadPageAsync()
    {
        _loadCts?.Cancel();
        _loadCts = new CancellationTokenSource();
        var token = _loadCts.Token;

        ShowLoadingState("جاري تحميل الموردين...");

        try
        {
            var pageTask = _supplierService.LoadSuppliersPageAsync(_pageNumber, _pageSize, token);
            var statsTask = _supplierService.LoadSupplierStatsAsync(token);
            await Task.WhenAll(pageTask, statsTask).ConfigureAwait(true);

            var result = await pageTask.ConfigureAwait(true);
            var stats = await statsTask.ConfigureAwait(true);

            if (!result.Success)
            {
                ShowErrorState(result.ErrorMessage ?? "تعذر تحميل الموردين.", result.IsConnectionError);
                return;
            }

            _pageNumber = result.PageNumber;
            _pageSize = result.PageSize;
            _totalCount = result.TotalCount;
            _allSuppliers.Clear();
            _allSuppliers.AddRange(result.Suppliers);

            _totalSuppliersCard.CardValue = SupplierDisplayHelper.FormatCount(stats.TotalSuppliers);
            _monthlyPurchasesCard.CardValue = stats.MonthlyPurchasesText;
            _unpaidDuesCard.CardValue = stats.UnpaidDuesText;

            if (_allSuppliers.Count == 0)
            {
                ShowEmptyState("لا يوجد موردون", "ابدأ بإضافة مورد جديد");
                _paginationBar.Visible = false;
            }
            else
            {
                ApplyFiltersAndRender();
                HideStatePanel();
            }
        }
        catch (OperationCanceledException)
        {
            // Ignore.
        }
    }

    private async Task ChangePageAsync(int newPage)
    {
        if (newPage < 1 || !string.IsNullOrWhiteSpace(_searchText))
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

    private async Task ApplySearchAsync()
    {
        _searchDebounce.Stop();
        _searchText = (_searchBox.Text ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(_searchText))
        {
            await LoadPageAsync();
            return;
        }

        ShowLoadingState("جاري البحث...");
        try
        {
            var results = await _supplierService.SearchSuppliersAsync(_searchText).ConfigureAwait(true);
            _displaySuppliers.Clear();
            _displaySuppliers.AddRange(results);
            _paginationBar.Visible = false;
            RenderSupplierRows();
            HideStatePanel();
            if (_displaySuppliers.Count == 0)
            {
                ShowEmptyState("لا توجد نتائج مطابقة", "جرّب اسمًا أو رقم هاتف مختلفًا.");
            }
        }
        catch (Exception ex)
        {
            ShowErrorState($"تعذر البحث: {ex.Message}", false);
        }
    }

    private void ApplyFiltersAndRender()
    {
        _displaySuppliers.Clear();
        _displaySuppliers.AddRange(_allSuppliers);
        RenderSupplierRows();
        UpdatePagination();
    }

    private void RenderSupplierRows()
    {
        _rowsHostPanel.Controls.Clear();
        _supplierRows.Clear();

        if (_displaySuppliers.Count == 0)
        {
            return;
        }

        foreach (var supplier in _displaySuppliers)
        {
            var row = new SupSupplierRow(supplier);
            row.EditRequested += async (_, _) => await OpenEditSupplierDialogAsync(supplier);
            row.DetailsRequested += async (_, _) => await ShowDetailsAsync(supplier);
            _supplierRows.Add(row);
            _rowsHostPanel.Controls.Add(row);
        }

        LayoutSupplierRows();
    }

    private async Task ShowDetailsAsync(SupplierListItemView supplier)
    {
        _selectedSupplier = supplier;
        _detailsPanel.Bind(supplier);
        LayoutSuppliersPage();

        var refreshed = await _supplierService.LoadSupplierDetailsAsync(supplier.Id).ConfigureAwait(true);
        if (refreshed is not null)
        {
            _selectedSupplier = refreshed;
            _detailsPanel.Bind(refreshed);
        }
    }

    private void ClearDetails()
    {
        _selectedSupplier = null;
        _detailsPanel.Bind(null);
        LayoutSuppliersPage();
    }

    private void UpdatePagination()
    {
        if (!string.IsNullOrWhiteSpace(_searchText))
        {
            _paginationBar.Visible = false;
            return;
        }

        _paginationBar.Visible = true;
        var totalPages = Math.Max(1, (int)Math.Ceiling(_totalCount / (double)_pageSize));
        var fromIndex = _totalCount == 0 ? 0 : ((_pageNumber - 1) * _pageSize) + 1;
        var toIndex = Math.Min(_pageNumber * _pageSize, _totalCount);
        _paginationBar.Update(_pageNumber, totalPages, fromIndex, toIndex, _totalCount);
    }

    private void ShowLoadingState(string message)
    {
        _statePanel.Visible = true;
        _stateTitle.Text = message;
        _stateDetail.Text = string.Empty;
        _retryButton.Visible = false;
        _tablePanel.Visible = false;
    }

    private void ShowErrorState(string message, bool isConnection)
    {
        _statePanel.Visible = true;
        _stateTitle.Text = "تعذر تحميل الموردين";
        _stateDetail.Text = isConnection
            ? $"{message}{Environment.NewLine}تحقق من أن API يعمل على {ApiConfiguration.BaseUrl}"
            : message;
        _retryButton.Visible = true;
        _tablePanel.Visible = false;
        _displaySuppliers.Clear();
        _supplierRows.Clear();
        _rowsHostPanel.Controls.Clear();
        _paginationBar.Visible = false;
    }

    private void ShowEmptyState(string title, string detail)
    {
        _statePanel.Visible = true;
        _stateTitle.Text = title;
        _stateDetail.Text = detail;
        _retryButton.Visible = false;
        _tablePanel.Visible = false;
        _paginationBar.Visible = false;
    }

    private void HideStatePanel()
    {
        _statePanel.Visible = false;
        _tablePanel.Visible = true;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            ThemeManager.ThemeChanged -= HandleThemeChanged;
            FontScaleManager.Changed -= HandleThemeChanged;
            _searchDebounce.Dispose();
            _loadCts?.Cancel();
            _loadCts?.Dispose();
        }

        base.Dispose(disposing);
    }
}
