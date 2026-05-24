using Pharmacy.WinForms.Controls;
using Pharmacy.WinForms.Controls.Customers;
using Pharmacy.WinForms.Models;
using Pharmacy.WinForms.Services;
using Pharmacy.WinForms.Ui;

namespace Pharmacy.WinForms.Forms.Customers;

internal sealed class CustomersControl : UserControl
{
    private const int WorkspacePadding = 32;
    private const int SectionGap = 16;
    private const int HeaderHeight = 96;
    private const int PaginationHeight = 60;
    private const int DetailsGap = 16;
    private const int OverlayBreakpoint = 980;
    private const int CardGap = 24;

    private readonly CustomerService _customerService;
    private readonly System.Windows.Forms.Timer _searchDebounce = new() { Interval = 300 };

    private readonly List<CustomerListItemView> _allCustomers = new();
    private readonly List<CustomerListItemView> _displayCustomers = new();
    private readonly List<Control> _customerViews = new();

    private string _searchText = string.Empty;
    private CustomerListItemView? _selectedCustomer;
    private CustomerViewMode _viewMode = CustomerViewMode.Grid;
    private bool _detailsOverlay;
    private int _pageNumber = 1;
    private int _pageSize = 20;
    private int _totalCount;
    private CancellationTokenSource? _loadCts;

    private Panel _rootPanel = null!;
    private Panel _mainContentPanel = null!;
    private Panel _headerPanel = null!;
    private Label _titleLabel = null!;
    private Label _subtitleLabel = null!;
    private GradientRoundedButton _addCustomerButton = null!;
    private CusSearchBox _searchBox = null!;
    private CusViewToggle _viewToggle = null!;
    private Panel _customersScrollPanel = null!;
    private Panel _customersHostPanel = null!;
    private CusPaginationBar _paginationBar = null!;
    private Panel _statePanel = null!;
    private Label _stateTitle = null!;
    private Label _stateDetail = null!;
    private Button _retryButton = null!;
    private CusCustomerDetailsPanel _detailsPanel = null!;
    private ContextMenuStrip? _cardMenu;

    public CustomersControl() : this(AppServices.CustomerService)
    {
    }

    public CustomersControl(CustomerService customerService)
    {
        _customerService = customerService;
        _searchDebounce.Tick += (_, _) => _ = ApplySearchAsync();

        SuspendLayout();
        SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        DoubleBuffered = true;
        Dock = DockStyle.Fill;
        BackColor = PharmaTheme.Background;
        RightToLeft = RightToLeft.Yes;

        BuildUi();
        WireEvents();
        BuildCardMenu();

        ThemeManager.ThemeChanged += HandleThemeChanged;
        FontScaleManager.Changed += HandleThemeChanged;

        ResumeLayout(false);
        Load += async (_, _) => await LoadPageAsync();
        SizeChanged += (_, _) => LayoutCustomersPage();
    }

    public void ApplyThemeVisuals()
    {
        BackColor = PharmaTheme.Background;
        _rootPanel.BackColor = PharmaTheme.Background;
        _mainContentPanel.BackColor = PharmaTheme.Background;
        _headerPanel.BackColor = PharmaTheme.Background;
        _customersScrollPanel.BackColor = PharmaTheme.Background;
        _customersHostPanel.BackColor = PharmaTheme.Background;

        _titleLabel.ForeColor = PharmaTheme.PrimaryDark;
        _titleLabel.Font = PharmaTheme.DashboardHeadlineFont;
        _subtitleLabel.ForeColor = PharmaTheme.OnSurfaceVariant;
        _subtitleLabel.Font = PharmaTheme.DashboardSubtitleFont;
        _addCustomerButton.ForeColor = PharmaTheme.OnPrimary;
        _addCustomerButton.Invalidate();
        _searchBox.ApplyThemeVisuals();
        _viewToggle.ApplyThemeVisuals();
        _paginationBar.ApplyThemeVisuals();
        _detailsPanel.ApplyThemeVisuals();

        foreach (var view in _customerViews)
        {
            switch (view)
            {
                case CusCustomerCard card:
                    card.ApplyThemeVisuals();
                    break;
                case CusCustomerListRow row:
                    row.ApplyThemeVisuals();
                    break;
            }
        }

        LayoutCustomersPage();
        Invalidate(true);
    }

    private void BuildUi()
    {
        _rootPanel = new Panel { Dock = DockStyle.Fill, BackColor = PharmaTheme.Background };
        _detailsPanel = new CusCustomerDetailsPanel();
        _mainContentPanel = new Panel { BackColor = PharmaTheme.Background };

        _headerPanel = new Panel { BackColor = PharmaTheme.Background, Height = HeaderHeight };
        _titleLabel = new Label
        {
            Text = "الزبائن",
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleRight,
            Font = PharmaTheme.DashboardHeadlineFont,
            ForeColor = PharmaTheme.PrimaryDark
        };
        _subtitleLabel = new Label
        {
            Text = "إدارة سجلات الزبائن وحالة الديون",
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleRight,
            Font = PharmaTheme.DashboardSubtitleFont,
            ForeColor = PharmaTheme.OnSurfaceVariant
        };
        _addCustomerButton = new GradientRoundedButton
        {
            Text = "إضافة زبون",
            IconGlyph = SegoeMdl2Icons.PersonAdd,
            Width = 200,
            Height = 52
        };
        _searchBox = new CusSearchBox { PlaceholderText = "بحث عن زبون..." };
        _viewToggle = new CusViewToggle();

        _headerPanel.Controls.Add(_viewToggle);
        _headerPanel.Controls.Add(_searchBox);
        _headerPanel.Controls.Add(_addCustomerButton);
        _headerPanel.Controls.Add(_subtitleLabel);
        _headerPanel.Controls.Add(_titleLabel);

        _customersScrollPanel = new Panel { AutoScroll = true, BackColor = PharmaTheme.Background };
        _customersHostPanel = new Panel { BackColor = PharmaTheme.Background };
        _customersScrollPanel.Controls.Add(_customersHostPanel);

        _paginationBar = new CusPaginationBar();

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
        _mainContentPanel.Controls.Add(_customersScrollPanel);
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
        _addCustomerButton.Click += async (_, _) => await OpenAddCustomerDialogAsync();
        _retryButton.Click += async (_, _) => await LoadPageAsync();
        _paginationBar.PageChangeRequested += async (_, page) => await ChangePageAsync(page);
        _viewToggle.ModeChanged += (_, _) =>
        {
            _viewMode = _viewToggle.Mode;
            RenderCustomerViews();
            LayoutCustomersPage();
        };
        _detailsPanel.CloseRequested += (_, _) => ClearDetails();
    }

    private void BuildCardMenu()
    {
        _cardMenu = new ContextMenuStrip { RightToLeft = RightToLeft.Yes };
        var detailsItem = new ToolStripMenuItem("عرض التفاصيل");
        detailsItem.Click += async (_, _) =>
        {
            if (_cardMenu?.Tag is CustomerListItemView customer)
            {
                await ShowDetailsAsync(customer);
            }
        };
        _cardMenu.Items.Add(detailsItem);
    }

    private async Task OpenAddCustomerDialogAsync()
    {
        var owner = FindForm();
        using var dialog = new AddCustomerDialog(_customerService);
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

    private void LayoutCustomersPage()
    {
        if (_rootPanel.ClientSize.Width <= 0)
        {
            return;
        }

        var bounds = _rootPanel.ClientRectangle;
        var pad = WorkspacePadding;
        var contentW = Math.Max(320, bounds.Width - pad * 2);
        var contentH = Math.Max(240, bounds.Height - pad * 2);
        var showDetails = _detailsPanel.Visible && _selectedCustomer is not null;
        _detailsOverlay = showDetails && bounds.Width < OverlayBreakpoint;

        var detailsW = showDetails && !_detailsOverlay
            ? Math.Clamp(PharmaTheme.CustomersDetailsWidth, 320, 380)
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
        _headerPanel.SetBounds(0, y, mainW, HeaderHeight);
        if (mainW < 900)
        {
            _titleLabel.SetBounds(0, 0, mainW, 34);
            _subtitleLabel.SetBounds(0, 34, mainW, 22);
            _searchBox.SetBounds(0, 62, Math.Max(180, mainW - 220), 48);
            _viewToggle.SetBounds(mainW - 92, 64, 92, 44);
            _addCustomerButton.SetBounds(0, 116, Math.Min(200, mainW), 52);
            _headerPanel.Height = 176;
        }
        else
        {
            _titleLabel.SetBounds(0, 0, mainW - 460, 36);
            _subtitleLabel.SetBounds(0, 38, mainW - 460, 22);
            _addCustomerButton.SetBounds(mainW - 200, 18, 200, 52);
            _viewToggle.SetBounds(mainW - 420, 22, 92, 44);
            _searchBox.SetBounds(mainW - 640, 20, 200, 48);
            _headerPanel.Height = HeaderHeight;
        }

        y = _headerPanel.Height + SectionGap;
        var listH = Math.Max(160, contentH - y - PaginationHeight - SectionGap);
        _customersScrollPanel.SetBounds(0, y, mainW, listH);
        _statePanel.SetBounds(0, y, mainW, listH);
        _paginationBar.SetBounds(Math.Max(0, (mainW - 360) / 2), contentH - PaginationHeight, Math.Min(360, mainW), PaginationHeight);

        LayoutCustomerGrid();
    }

    private void LayoutCustomerGrid()
    {
        if (_customersHostPanel.Controls.Count == 0)
        {
            _customersHostPanel.Size = new Size(_customersScrollPanel.ClientSize.Width, 0);
            return;
        }

        var availableW = Math.Max(280, _customersScrollPanel.ClientSize.Width);
        var cols = availableW >= 1180 ? 3 : availableW >= 760 ? 2 : 1;
        var cardH = _viewMode == CustomerViewMode.Grid ? PharmaTheme.CustomersCardHeight : 64;
        var gap = _viewMode == CustomerViewMode.Grid ? CardGap : 12;

        if (_viewMode == CustomerViewMode.List)
        {
            cols = 1;
        }

        var cardW = cols == 1 ? availableW : (availableW - gap * (cols - 1)) / cols;
        var x = 0;
        var y = 0;
        var col = 0;
        foreach (Control view in _customersHostPanel.Controls)
        {
            view.SetBounds(x, y, cardW, cardH);
            col++;
            if (col >= cols)
            {
                col = 0;
                x = 0;
                y += cardH + gap;
            }
            else
            {
                x += cardW + gap;
            }
        }

        if (col > 0)
        {
            y += cardH + gap;
        }

        _customersHostPanel.Size = new Size(availableW, Math.Max(y, cardH));
    }

    private async Task LoadPageAsync()
    {
        _loadCts?.Cancel();
        _loadCts = new CancellationTokenSource();
        var token = _loadCts.Token;

        ShowLoadingState("جاري تحميل الزبائن...");

        try
        {
            var result = await _customerService.LoadCustomersPageAsync(_pageNumber, _pageSize, token).ConfigureAwait(true);
            if (!result.Success)
            {
                ShowErrorState(result.ErrorMessage ?? "تعذر تحميل الزبائن.", result.IsConnectionError);
                return;
            }

            _pageNumber = result.PageNumber;
            _pageSize = result.PageSize;
            _totalCount = result.TotalCount;
            _allCustomers.Clear();
            _allCustomers.AddRange(result.Customers);
            if (_allCustomers.Count == 0)
            {
                ShowEmptyState("لا يوجد زبائن", "ابدأ بإضافة زبون جديد");
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
            var results = await _customerService.SearchCustomersAsync(_searchText).ConfigureAwait(true);
            _displayCustomers.Clear();
            _displayCustomers.AddRange(results);
            _paginationBar.Visible = false;
            RenderCustomerViews();
            HideStatePanel();
            if (_displayCustomers.Count == 0)
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
        _displayCustomers.Clear();
        _displayCustomers.AddRange(_allCustomers);
        RenderCustomerViews();
        UpdatePagination();
    }

    private void RenderCustomerViews()
    {
        _customersHostPanel.Controls.Clear();
        _customerViews.Clear();

        if (_displayCustomers.Count == 0)
        {
            return;
        }

        foreach (var customer in _displayCustomers)
        {
            Control view = _viewMode == CustomerViewMode.Grid
                ? CreateCard(customer)
                : CreateListRow(customer);
            _customerViews.Add(view);
            _customersHostPanel.Controls.Add(view);
        }

        LayoutCustomerGrid();
    }

    private CusCustomerCard CreateCard(CustomerListItemView customer)
    {
        var card = new CusCustomerCard(customer);
        card.ViewDetailsRequested += async (_, _) => await ShowDetailsAsync(customer);
        card.MenuRequested += (_, _) =>
        {
            if (_cardMenu is null)
            {
                return;
            }

            _cardMenu.Tag = customer;
            _cardMenu.Show(card, new Point(0, card.Height));
        };
        return card;
    }

    private CusCustomerListRow CreateListRow(CustomerListItemView customer)
    {
        var row = new CusCustomerListRow(customer);
        row.ViewDetailsRequested += async (_, _) => await ShowDetailsAsync(customer);
        return row;
    }

    private async Task ShowDetailsAsync(CustomerListItemView customer)
    {
        _selectedCustomer = customer;
        _detailsPanel.Bind(customer);
        LayoutCustomersPage();

        var refreshed = await _customerService.LoadCustomerDetailsAsync(customer.Id).ConfigureAwait(true);
        if (refreshed is not null)
        {
            _selectedCustomer = refreshed;
            _detailsPanel.Bind(refreshed);
        }
    }

    private void ClearDetails()
    {
        _selectedCustomer = null;
        _detailsPanel.Bind(null);
        LayoutCustomersPage();
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
        _paginationBar.Update(_pageNumber, totalPages);
    }

    private void ShowLoadingState(string message)
    {
        _statePanel.Visible = true;
        _stateTitle.Text = message;
        _stateDetail.Text = string.Empty;
        _retryButton.Visible = false;
        _customersScrollPanel.Visible = false;
    }

    private void ShowErrorState(string message, bool isConnection)
    {
        _statePanel.Visible = true;
        _stateTitle.Text = "تعذر تحميل الزبائن";
        _stateDetail.Text = isConnection
            ? $"{message}{Environment.NewLine}تحقق من أن API يعمل على {ApiConfiguration.BaseUrl}"
            : message;
        _retryButton.Visible = true;
        _customersScrollPanel.Visible = false;
        _displayCustomers.Clear();
        _customerViews.Clear();
        _customersHostPanel.Controls.Clear();
        _paginationBar.Visible = false;
    }

    private void ShowEmptyState(string title, string detail)
    {
        _statePanel.Visible = true;
        _stateTitle.Text = title;
        _stateDetail.Text = detail;
        _retryButton.Visible = false;
        _customersScrollPanel.Visible = false;
        _paginationBar.Visible = false;
    }

    private void HideStatePanel()
    {
        _statePanel.Visible = false;
        _customersScrollPanel.Visible = true;
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
            _cardMenu?.Dispose();
        }

        base.Dispose(disposing);
    }
}
