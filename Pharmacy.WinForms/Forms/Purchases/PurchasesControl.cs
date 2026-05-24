using Pharmacy.WinForms.Controls;
using Pharmacy.WinForms.Controls.Purchases;
using Pharmacy.WinForms.Models;
using Pharmacy.WinForms.Services;
using Pharmacy.WinForms.Ui;

namespace Pharmacy.WinForms.Forms.Purchases;

internal sealed class PurchasesControl : UserControl
{
    private const int WorkspacePadding = 32;
    private const int SectionGap = 16;
    private const int HeaderHeight = 90;
    private const int SearchHeight = 104;
    private const int PaginationHeight = 64;
    private const int DetailsGap = 16;
    private const int OverlayBreakpoint = 1100;
    private const int MinMainWithDetails = 540;

    private readonly PurchaseService _purchaseService;
    private readonly System.Windows.Forms.Timer _searchDebounce = new() { Interval = 300 };

    private readonly List<PurchaseInvoiceListItemView> _allInvoices = new();
    private readonly List<PurchaseInvoiceListItemView> _displayInvoices = new();
    private readonly List<PurPurchaseInvoiceCard> _invoiceCards = new();
    private IReadOnlyList<SupplierOptionView> _suppliers = Array.Empty<SupplierOptionView>();

    private string _searchText = string.Empty;
    private PurchaseStatusFilter _statusFilter = PurchaseStatusFilter.All;
    private SupplierOptionView _supplierFilter = SupplierOptionView.All;
    private PurchaseInvoiceListItemView? _selectedInvoice;
    private bool _detailsOverlay;
    private int _pageNumber = 1;
    private int _pageSize = 20;
    private int _totalCount;
    private CancellationTokenSource? _loadCts;
    private CancellationTokenSource? _detailsCts;

    private Panel _rootPanel = null!;
    private Panel _mainContentPanel = null!;
    private Panel _headerPanel = null!;
    private Label _titleLabel = null!;
    private Label _subtitleLabel = null!;
    private GradientRoundedButton _addPurchaseButton = null!;
    private PurRoundedPanel _searchFilterCard = null!;
    private PurSearchBox _searchBox = null!;
    private PurOutlineButton _supplierFilterButton = null!;
    private PurIconFilterButton _statusFilterButton = null!;
    private Panel _invoicesScrollPanel = null!;
    private Panel _invoicesListPanel = null!;
    private PurPaginationBar _paginationBar = null!;
    private Panel _statePanel = null!;
    private Label _stateTitle = null!;
    private Label _stateDetail = null!;
    private Button _retryButton = null!;
    private PurInvoiceDetailsPanel _detailsPanel = null!;
    private ContextMenuStrip? _statusMenu;
    private ContextMenuStrip? _supplierMenu;

    public PurchasesControl() : this(AppServices.PurchaseService)
    {
    }

    public PurchasesControl(PurchaseService purchaseService)
    {
        _purchaseService = purchaseService;
        _searchDebounce.Tick += (_, _) => _ = ApplySearchFilterAsync();

        SuspendLayout();
        SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        DoubleBuffered = true;
        Dock = DockStyle.Fill;
        BackColor = PharmaTheme.Background;
        RightToLeft = RightToLeft.Yes;

        BuildUi();
        WireEvents();
        BuildMenus();

        ThemeManager.ThemeChanged += HandleThemeChanged;
        FontScaleManager.Changed += HandleThemeChanged;

        ResumeLayout(false);
        Load += async (_, _) => await LoadPageAsync();
        SizeChanged += (_, _) => LayoutPurchasesPage();
    }

    public void ApplyThemeVisuals()
    {
        BackColor = PharmaTheme.Background;
        _rootPanel.BackColor = PharmaTheme.Background;
        _mainContentPanel.BackColor = PharmaTheme.Background;
        _headerPanel.BackColor = PharmaTheme.Background;
        _invoicesScrollPanel.BackColor = PharmaTheme.Background;

        _titleLabel.ForeColor = PharmaTheme.PrimaryDark;
        _titleLabel.Font = PharmaTheme.DashboardHeadlineFont;
        _subtitleLabel.ForeColor = PharmaTheme.OnSurfaceVariant;
        _subtitleLabel.Font = PharmaTheme.DashboardSubtitleFont;
        _addPurchaseButton.ForeColor = PharmaTheme.OnPrimary;
        _addPurchaseButton.Invalidate();

        _searchFilterCard.FillColor = PharmaTheme.SurfaceAlt;
        _searchFilterCard.ApplyThemeVisuals();
        _searchBox.ApplyThemeVisuals();
        _supplierFilterButton.ApplyThemeVisuals();
        _statusFilterButton.ApplyThemeVisuals();
        _paginationBar.ApplyThemeVisuals();
        _detailsPanel.ApplyThemeVisuals();

        foreach (var card in _invoiceCards)
        {
            card.ApplyThemeVisuals();
        }

        LayoutPurchasesPage();
        Invalidate(true);
    }

    private void BuildUi()
    {
        _rootPanel = new Panel { Dock = DockStyle.Fill, BackColor = PharmaTheme.Background };
        _detailsPanel = new PurInvoiceDetailsPanel();
        _mainContentPanel = new Panel { BackColor = PharmaTheme.Background };

        _headerPanel = new Panel { BackColor = PharmaTheme.Background, Height = HeaderHeight };
        _titleLabel = new Label
        {
            Text = "المشتريات",
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleRight,
            Font = PharmaTheme.DashboardHeadlineFont,
            ForeColor = PharmaTheme.PrimaryDark
        };
        _subtitleLabel = new Label
        {
            Text = "إدارة وتتبع فواتير الشراء من الموردين",
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleRight,
            Font = PharmaTheme.DashboardSubtitleFont,
            ForeColor = PharmaTheme.OnSurfaceVariant
        };
        _addPurchaseButton = new GradientRoundedButton
        {
            Text = "إضافة فاتورة شراء",
            IconGlyph = SegoeMdl2Icons.Add,
            Width = 240,
            Height = 52
        };
        _headerPanel.Controls.Add(_addPurchaseButton);
        _headerPanel.Controls.Add(_subtitleLabel);
        _headerPanel.Controls.Add(_titleLabel);

        _searchFilterCard = new PurRoundedPanel(PharmaTheme.PurchasesCardCornerRadius) { FillColor = PharmaTheme.SurfaceAlt };
        _searchBox = new PurSearchBox { PlaceholderText = "ابحث برقم الفاتورة أو المورد..." };
        _supplierFilterButton = new PurOutlineButton { Text = "تصفية بالمورد", Width = 200 };
        _statusFilterButton = new PurIconFilterButton();
        _searchFilterCard.Controls.Add(_statusFilterButton);
        _searchFilterCard.Controls.Add(_supplierFilterButton);
        _searchFilterCard.Controls.Add(_searchBox);

        _invoicesScrollPanel = new Panel { AutoScroll = true, BackColor = PharmaTheme.Background };
        _invoicesListPanel = new Panel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Dock = DockStyle.Top,
            BackColor = Color.Transparent,
            Width = 10
        };
        _invoicesScrollPanel.Controls.Add(_invoicesListPanel);

        _paginationBar = new PurPaginationBar();

        _statePanel = new Panel { Visible = false, BackColor = Color.Transparent };
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
        _mainContentPanel.Controls.Add(_invoicesScrollPanel);
        _mainContentPanel.Controls.Add(_searchFilterCard);
        _mainContentPanel.Controls.Add(_headerPanel);

        _rootPanel.Controls.Add(_mainContentPanel);
        _rootPanel.Controls.Add(_detailsPanel);
        Controls.Add(_rootPanel);
    }

    private async Task OpenCreateInvoiceDialogAsync()
    {
        var owner = FindForm();
        using var dialog = new CreatePurchaseInvoiceDialog(_purchaseService);
        var result = owner is null
            ? dialog.ShowDialog()
            : dialog.ShowDialog(owner);

        if (result != DialogResult.OK)
        {
            return;
        }

        _pageNumber = 1;
        ClearDetails();
        await LoadPageAsync();
    }

    private void WireEvents()
    {
        _searchBox.SearchTextChanged += (_, _) =>
        {
            _searchDebounce.Stop();
            _searchDebounce.Start();
        };
        _addPurchaseButton.Click += async (_, _) => await OpenCreateInvoiceDialogAsync();
        _retryButton.Click += async (_, _) => await LoadPageAsync();
        _paginationBar.PageChangeRequested += async (_, page) => await ChangePageAsync(page);
        _detailsPanel.CloseRequested += (_, _) => ClearDetails();
    }

    private void BuildMenus()
    {
        _statusMenu = new ContextMenuStrip { RightToLeft = RightToLeft.Yes };
        AddStatusMenuItem("الكل", PurchaseStatusFilter.All);
        AddStatusMenuItem("مدفوع", PurchaseStatusFilter.Paid);
        AddStatusMenuItem("متبقي جزئيًا", PurchaseStatusFilter.PartiallyPaid);
        AddStatusMenuItem("غير مدفوع", PurchaseStatusFilter.Unpaid);
        AddStatusMenuItem("ملغي", PurchaseStatusFilter.Cancelled);
        _statusFilterButton.Click += (_, _) => _statusMenu.Show(_statusFilterButton, new Point(0, _statusFilterButton.Height));

        _supplierMenu = new ContextMenuStrip { RightToLeft = RightToLeft.Yes };
        _supplierFilterButton.Click += async (_, _) => await ShowSupplierMenuAsync();
    }

    private void AddStatusMenuItem(string text, PurchaseStatusFilter filter)
    {
        var item = new ToolStripMenuItem(text);
        item.Click += async (_, _) =>
        {
            _statusFilter = filter;
            ApplyFiltersAndRender();
        };
        _statusMenu!.Items.Add(item);
    }

    private async Task ShowSupplierMenuAsync()
    {
        if (_suppliers.Count <= 1)
        {
            _suppliers = await _purchaseService.LoadSuppliersAsync();
            _supplierMenu!.Items.Clear();
            foreach (var supplier in _suppliers)
            {
                var item = new ToolStripMenuItem(supplier.Name);
                var captured = supplier;
                item.Click += (_, _) =>
                {
                    _supplierFilter = captured;
                    _supplierFilterButton.Text = captured.SupplierId is null
                        ? "تصفية بالمورد"
                        : captured.Name;
                    ApplyFiltersAndRender();
                };
                _supplierMenu.Items.Add(item);
            }
        }

        if (_supplierMenu!.Items.Count == 0)
        {
            MessageBox.Show(
                this,
                "تعذر تحميل قائمة الموردين حاليًا.",
                "تصفية بالمورد",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        _supplierMenu.Show(_supplierFilterButton, new Point(0, _supplierFilterButton.Height));
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

    private void LayoutPurchasesPage()
    {
        if (_rootPanel.ClientSize.Width <= 0)
        {
            return;
        }

        var bounds = _rootPanel.ClientRectangle;
        var pad = WorkspacePadding;
        var contentW = Math.Max(320, bounds.Width - pad * 2);
        var contentH = Math.Max(240, bounds.Height - pad * 2);
        var showDetails = _detailsPanel.Visible && _selectedInvoice is not null;
        _detailsOverlay = showDetails && bounds.Width < OverlayBreakpoint;

        var detailsW = showDetails && !_detailsOverlay
            ? Math.Clamp(PharmaTheme.PurchasesDetailsWidth, 320, 380)
            : 0;

        if (showDetails && !_detailsOverlay)
        {
            var tentativeMain = contentW - detailsW - DetailsGap;
            if (tentativeMain < MinMainWithDetails)
            {
                _detailsOverlay = true;
                detailsW = 0;
            }
        }

        if (_detailsOverlay && showDetails)
        {
            _detailsPanel.Bounds = new Rectangle(pad, pad, Math.Min(contentW, 420), contentH);
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
        _headerPanel.SetBounds(0, y, mainW, HeaderHeight);
        if (mainW < 900)
        {
            _titleLabel.SetBounds(0, 0, mainW, 34);
            _subtitleLabel.SetBounds(0, 34, mainW, 22);
            _addPurchaseButton.SetBounds(0, 58, Math.Min(240, mainW), 52);
            _headerPanel.Height = 118;
        }
        else
        {
            _titleLabel.SetBounds(0, 0, mainW - 260, 36);
            _subtitleLabel.SetBounds(0, 38, mainW - 260, 22);
            _addPurchaseButton.SetBounds(mainW - 240, 18, 240, 52);
            _headerPanel.Height = HeaderHeight;
        }

        y = _headerPanel.Height + SectionGap;
        _searchFilterCard.SetBounds(0, y, mainW, SearchHeight);
        var supplierW = Math.Min(200, Math.Max(140, mainW / 5));
        var filterW = 52;
        var searchW = Math.Max(200, mainW - 24 - supplierW - filterW - 16);
        if (mainW < 760)
        {
            _searchBox.SetBounds(12, 12, mainW - 24, 52);
            _supplierFilterButton.SetBounds(12, 72, Math.Min(200, mainW - 24 - filterW - 8), 52);
            _statusFilterButton.SetBounds(mainW - 12 - filterW, 72, filterW, 52);
            _searchFilterCard.Height = 132;
            y += 132 + SectionGap;
        }
        else
        {
            _searchBox.SetBounds(12, 24, searchW, 52);
            _supplierFilterButton.SetBounds(12 + searchW + 8, 24, supplierW, 52);
            _statusFilterButton.SetBounds(12 + searchW + 8 + supplierW + 8, 24, filterW, 52);
            _searchFilterCard.Height = SearchHeight;
            y += SearchHeight + SectionGap;
        }

        var listH = Math.Max(160, contentH - y - PaginationHeight - SectionGap);
        _invoicesScrollPanel.SetBounds(0, y, mainW, listH);
        _statePanel.SetBounds(0, y, mainW, listH);
        _paginationBar.SetBounds(Math.Max(0, (mainW - 360) / 2), contentH - PaginationHeight, Math.Min(360, mainW), PaginationHeight);

        var listW = Math.Max(320, _invoicesScrollPanel.ClientSize.Width);
        _invoicesListPanel.Width = listW;
        foreach (var card in _invoiceCards)
        {
            card.Width = listW;
            card.Invalidate();
        }
    }

    private async Task LoadPageAsync()
    {
        _loadCts?.Cancel();
        _loadCts = new CancellationTokenSource();
        var token = _loadCts.Token;

        ShowLoadingState("جاري تحميل فواتير الشراء...");

        try
        {
            var size = string.IsNullOrWhiteSpace(_searchText) ? _pageSize : PurchaseService.ClampPageSize(100);
            var page = string.IsNullOrWhiteSpace(_searchText) ? _pageNumber : 1;

            var result = await _purchaseService.LoadInvoicesPageAsync(page, size, token).ConfigureAwait(true);
            if (!result.Success)
            {
                ShowErrorState(result.ErrorMessage ?? "تعذر تحميل فواتير الشراء.", result.IsConnectionError);
                return;
            }

            _pageNumber = result.PageNumber;
            _pageSize = result.PageSize;
            _totalCount = result.TotalCount;
            _allInvoices.Clear();
            _allInvoices.AddRange(result.Invoices);
            ApplyFiltersAndRender();
            HideStatePanel();
        }
        catch (OperationCanceledException)
        {
            // Ignore.
        }
    }

    private async Task ChangePageAsync(int newPage)
    {
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

    private Task ApplySearchFilterAsync()
    {
        _searchDebounce.Stop();
        _searchText = (_searchBox.Text ?? string.Empty).Trim();
        _pageNumber = 1;
        return LoadPageAsync();
    }

    private void ApplyFiltersAndRender()
    {
        _displayInvoices.Clear();
        foreach (var invoice in _allInvoices)
        {
            if (!MatchesSearch(invoice) || !MatchesSupplier(invoice) || !MatchesStatus(invoice))
            {
                continue;
            }

            _displayInvoices.Add(invoice);
        }

        RenderInvoiceCards();
        UpdatePagination();
    }

    private bool MatchesSearch(PurchaseInvoiceListItemView invoice)
    {
        if (string.IsNullOrWhiteSpace(_searchText))
        {
            return true;
        }

        var q = _searchText;
        return invoice.InvoiceNumber.Contains(q, StringComparison.OrdinalIgnoreCase)
            || invoice.SupplierName.Contains(q, StringComparison.OrdinalIgnoreCase)
            || invoice.FormattedDate.Contains(q, StringComparison.OrdinalIgnoreCase)
            || invoice.InvoiceDate.ToString("yyyy-MM-dd").Contains(q, StringComparison.Ordinal);
    }

    private bool MatchesSupplier(PurchaseInvoiceListItemView invoice)
    {
        if (_supplierFilter.SupplierId is null)
        {
            return true;
        }

        return invoice.SupplierId == _supplierFilter.SupplierId;
    }

    private bool MatchesStatus(PurchaseInvoiceListItemView invoice)
    {
        return _statusFilter switch
        {
            PurchaseStatusFilter.Paid => invoice.StatusKind == PurchaseInvoiceStatusKind.Paid,
            PurchaseStatusFilter.PartiallyPaid => invoice.StatusKind == PurchaseInvoiceStatusKind.PartiallyPaid,
            PurchaseStatusFilter.Unpaid => invoice.StatusKind == PurchaseInvoiceStatusKind.Unpaid,
            PurchaseStatusFilter.Cancelled => invoice.StatusKind == PurchaseInvoiceStatusKind.Cancelled,
            _ => true
        };
    }

    private void RenderInvoiceCards()
    {
        _invoicesListPanel.Controls.Clear();
        _invoiceCards.Clear();

        if (_displayInvoices.Count == 0)
        {
            var emptyTitle = string.IsNullOrWhiteSpace(_searchText)
                && _statusFilter == PurchaseStatusFilter.All
                && _supplierFilter.SupplierId is null
                ? "لا توجد فواتير شراء"
                : "لا توجد فواتير مطابقة";
            var emptyDetail = string.IsNullOrWhiteSpace(_searchText)
                && _statusFilter == PurchaseStatusFilter.All
                && _supplierFilter.SupplierId is null
                ? "ابدأ بإضافة فاتورة شراء جديدة"
                : "جرّب تغيير البحث أو الفلاتر.";
            ShowEmptyState(emptyTitle, emptyDetail);
            return;
        }

        HideStatePanel();
        foreach (var invoice in _displayInvoices)
        {
            var card = new PurPurchaseInvoiceCard(invoice);
            card.ViewDetailsRequested += async (_, _) => await ShowDetailsAsync(invoice);
            card.PrintRequested += (_, _) =>
                MessageBox.Show(
                    this,
                    "الطباعة غير مفعلة بعد",
                    "طباعة",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            _invoiceCards.Add(card);
            _invoicesListPanel.Controls.Add(card);
        }

        _invoicesListPanel.Width = Math.Max(_invoicesScrollPanel.ClientSize.Width, _invoicesScrollPanel.DisplayRectangle.Width);
        LayoutPurchasesPage();
    }

    private async Task ShowDetailsAsync(PurchaseInvoiceListItemView invoice)
    {
        _selectedInvoice = invoice;
        _detailsPanel.Bind(null);
        _detailsPanel.Visible = true;
        LayoutPurchasesPage();

        _detailsCts?.Cancel();
        _detailsCts = new CancellationTokenSource();
        var details = await _purchaseService.LoadInvoiceDetailsAsync(invoice.Id, _detailsCts.Token)
            .ConfigureAwait(true);

        if (details is null)
        {
            _detailsPanel.Bind(new PurchaseInvoiceDetailsView
            {
                Summary = invoice,
                Lines = Array.Empty<PurchaseInvoiceLineView>()
            });
            return;
        }

        _detailsPanel.Bind(details);
        if (_selectedInvoice is not null && details.Summary.ItemsCount.HasValue)
        {
            var idx = _displayInvoices.FindIndex(i => i.Id == _selectedInvoice.Id);
            if (idx >= 0)
            {
                var updated = details.Summary;
                _displayInvoices[idx] = updated;
                var card = _invoiceCards.FirstOrDefault(c => c.Invoice.Id == updated.Id);
                card?.Invalidate();
            }
        }
    }

    private void ClearDetails()
    {
        _selectedInvoice = null;
        _detailsPanel.Bind(null);
        LayoutPurchasesPage();
    }

    private void UpdatePagination()
    {
        var filtering = !string.IsNullOrWhiteSpace(_searchText)
            || _statusFilter != PurchaseStatusFilter.All
            || _supplierFilter.SupplierId is not null;

        if (filtering)
        {
            _paginationBar.Visible = false;
            return;
        }

        _paginationBar.Visible = true;
        var totalPages = Math.Max(1, (int)Math.Ceiling(_totalCount / (double)_pageSize));
        _paginationBar.Update(_pageNumber, totalPages);
    }

    private void ShowLoadingState(string message)
    {
        _statePanel.Visible = true;
        _stateTitle.Text = message;
        _stateDetail.Text = string.Empty;
        _retryButton.Visible = false;
        _invoicesScrollPanel.Visible = false;
    }

    private void ShowErrorState(string message, bool isConnection)
    {
        _statePanel.Visible = true;
        _stateTitle.Text = "تعذر تحميل فواتير الشراء";
        _stateDetail.Text = isConnection
            ? $"{message}{Environment.NewLine}تحقق من أن API يعمل على {ApiConfiguration.BaseUrl}"
            : message;
        _retryButton.Visible = true;
        _invoicesScrollPanel.Visible = false;
        _displayInvoices.Clear();
        _invoiceCards.Clear();
        _invoicesListPanel.Controls.Clear();
        _paginationBar.Visible = false;
    }

    private void ShowEmptyState(string title, string detail)
    {
        _statePanel.Visible = true;
        _stateTitle.Text = title;
        _stateDetail.Text = detail;
        _retryButton.Visible = false;
        _invoicesScrollPanel.Visible = false;
        _paginationBar.Visible = false;
    }

    private void HideStatePanel()
    {
        _statePanel.Visible = false;
        _invoicesScrollPanel.Visible = true;
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
            _detailsCts?.Cancel();
            _detailsCts?.Dispose();
            _statusMenu?.Dispose();
            _supplierMenu?.Dispose();
        }

        base.Dispose(disposing);
    }
}
