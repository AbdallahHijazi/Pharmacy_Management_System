using Pharmacy.WinForms.Controls;
using Pharmacy.WinForms.Controls.Users;
using Pharmacy.WinForms.Models;
using Pharmacy.WinForms.Services;
using Pharmacy.WinForms.Ui;

namespace Pharmacy.WinForms.Forms.Users;

internal sealed class UsersControl : UserControl
{
    private const int WorkspacePadding = 32;
    private const int SectionGap = 16;
    private const int HeaderHeight = 112;
    private const int StatsHeight = 148;
    private const int PaginationHeight = 64;
    private const int DetailsGap = 16;
    private const int OverlayBreakpoint = 980;
    private const int RowGap = 8;
    private const int PageSize = 20;

    private readonly UserService _userService;
    private readonly System.Windows.Forms.Timer _searchDebounce = new() { Interval = 300 };

    private readonly List<UserListItemView> _allUsers = new();
    private readonly List<UserListItemView> _filteredUsers = new();
    private readonly List<UsrUserRow> _userRows = new();

    private string _searchText = string.Empty;
    private UserListItemView? _selectedUser;
    private bool _detailsOverlay;
    private int _pageNumber = 1;
    private CancellationTokenSource? _loadCts;

    private Panel _rootPanel = null!;
    private Panel _mainContentPanel = null!;
    private Panel _headerPanel = null!;
    private Label _titleLabel = null!;
    private Label _subtitleLabel = null!;
    private GradientRoundedButton _addUserButton = null!;
    private UsrSearchBox _searchBox = null!;
    private Panel _statsPanel = null!;
    private UsrStatCard _totalUsersCard = null!;
    private UsrStatCard _activePharmacistsCard = null!;
    private UsrStatCard _systemAdminsCard = null!;
    private Panel _tablePanel = null!;
    private UsrTableHeader _tableHeader = null!;
    private Panel _rowsScrollPanel = null!;
    private Panel _rowsHostPanel = null!;
    private UsrPaginationBar _paginationBar = null!;
    private Panel _statePanel = null!;
    private Label _stateTitle = null!;
    private Label _stateDetail = null!;
    private Button _retryButton = null!;
    private UsrUserDetailsPanel _detailsPanel = null!;

    public UsersControl() : this(AppServices.UserService)
    {
    }

    public UsersControl(UserService userService)
    {
        _userService = userService;
        _searchDebounce.Tick += (_, _) => ApplySearchAndPagination(resetPage: true);

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
        Load += async (_, _) => await LoadUsersAsync();
        SizeChanged += (_, _) => LayoutUsersPage();
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
        _addUserButton.Invalidate();
        _searchBox.ApplyThemeVisuals();
        _totalUsersCard.ApplyThemeVisuals();
        _activePharmacistsCard.ApplyThemeVisuals();
        _systemAdminsCard.ApplyThemeVisuals();
        _tableHeader.ApplyThemeVisuals();
        _paginationBar.ApplyThemeVisuals();
        _detailsPanel.ApplyThemeVisuals();

        foreach (var row in _userRows)
        {
            row.ApplyThemeVisuals();
        }

        LayoutUsersPage();
        Invalidate(true);
    }

    private void BuildUi()
    {
        _rootPanel = new Panel { Dock = DockStyle.Fill, BackColor = PharmaTheme.Background };
        _detailsPanel = new UsrUserDetailsPanel();
        _mainContentPanel = new Panel { BackColor = PharmaTheme.Background };

        _headerPanel = new Panel { BackColor = PharmaTheme.Background, Height = HeaderHeight };
        _titleLabel = new Label
        {
            Text = "المستخدمين",
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleRight,
            Font = PharmaTheme.DashboardHeadlineFont,
            ForeColor = PharmaTheme.PrimaryDark,
            BackColor = PharmaTheme.Background
        };
        _subtitleLabel = new Label
        {
            Text = "إدارة حسابات وصلاحيات فريق العمل في الصيدلية",
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleRight,
            Font = PharmaTheme.DashboardSubtitleFont,
            ForeColor = PharmaTheme.OnSurfaceVariant,
            BackColor = PharmaTheme.Background
        };
        _addUserButton = new GradientRoundedButton
        {
            Text = "إضافة مستخدم",
            IconGlyph = SegoeMdl2Icons.Add,
            Width = 200,
            Height = 52
        };
        _searchBox = new UsrSearchBox { PlaceholderText = "البحث عن مستخدم..." };

        _headerPanel.Controls.Add(_searchBox);
        _headerPanel.Controls.Add(_addUserButton);
        _headerPanel.Controls.Add(_subtitleLabel);
        _headerPanel.Controls.Add(_titleLabel);

        _statsPanel = new Panel { BackColor = PharmaTheme.Background, Height = StatsHeight };
        _totalUsersCard = new UsrStatCard
        {
            CardTitle = "إجمالي المستخدمين",
            IconGlyph = SegoeMdl2Icons.Users,
            CardValue = "0"
        };
        _activePharmacistsCard = new UsrStatCard
        {
            CardTitle = "الصيادلة النشطون",
            IconGlyph = SegoeMdl2Icons.Pharmacy,
            CardValue = "0",
            Subtitle = string.Empty
        };
        _systemAdminsCard = new UsrStatCard
        {
            CardTitle = "مدراء النظام",
            IconGlyph = SegoeMdl2Icons.Account,
            CardValue = "0"
        };
        _statsPanel.Controls.Add(_systemAdminsCard);
        _statsPanel.Controls.Add(_activePharmacistsCard);
        _statsPanel.Controls.Add(_totalUsersCard);

        _tablePanel = new Panel { BackColor = PharmaTheme.Background };
        _tableHeader = new UsrTableHeader();
        _rowsScrollPanel = new Panel { AutoScroll = true, BackColor = PharmaTheme.Background };
        _rowsHostPanel = new Panel { BackColor = PharmaTheme.Background };
        _rowsScrollPanel.Controls.Add(_rowsHostPanel);
        _tablePanel.Controls.Add(_rowsScrollPanel);
        _tablePanel.Controls.Add(_tableHeader);
        _tablePanel.Resize += (_, _) => LayoutTableInternals();
        _rowsScrollPanel.Resize += (_, _) => LayoutUserRows();

        _paginationBar = new UsrPaginationBar();

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
        _addUserButton.Click += async (_, _) => await OpenAddUserDialogAsync();
        _retryButton.Click += async (_, _) => await LoadUsersAsync();
        _paginationBar.PageChangeRequested += (_, page) => ChangePage(page);
        _detailsPanel.CloseRequested += (_, _) => ClearDetails();
    }

    private async Task LoadUsersAsync()
    {
        _loadCts?.Cancel();
        _loadCts = new CancellationTokenSource();
        var token = _loadCts.Token;

        ShowLoadingState();
        try
        {
            var state = await _userService.LoadUsersAsync(token).ConfigureAwait(true);
            if (token.IsCancellationRequested)
            {
                return;
            }

            if (!state.Success)
            {
                ShowErrorState(
                    state.ErrorMessage ?? "تعذر تحميل المستخدمين.",
                    state.IsConnectionError);
                return;
            }

            _allUsers.Clear();
            _allUsers.AddRange(state.Users);
            UpdateStats();
            ApplySearchAndPagination(resetPage: true);
            HideStatePanel();
        }
        catch (OperationCanceledException)
        {
            // Ignore.
        }
    }

    private void ApplySearchAndPagination(bool resetPage)
    {
        _searchText = _searchBox.Text?.Trim() ?? string.Empty;
        _filteredUsers.Clear();
        _filteredUsers.AddRange(_allUsers.Where(u => UserDisplayHelper.MatchesSearch(u, _searchText)));

        if (resetPage)
        {
            _pageNumber = 1;
        }

        var totalPages = Math.Max(1, (int)Math.Ceiling(_filteredUsers.Count / (double)PageSize));
        _pageNumber = Math.Clamp(_pageNumber, 1, totalPages);

        RenderRows();
        UpdatePaginationBar();
        LayoutUsersPage();
    }

    private void RenderRows()
    {
        foreach (var row in _userRows)
        {
            row.EditRequested -= OnRowEditRequested;
            row.ToggleActiveRequested -= OnRowToggleRequested;
            row.DeleteRequested -= OnRowDeleteRequested;
            row.DetailsRequested -= OnRowDetailsRequested;
            row.Dispose();
        }

        _userRows.Clear();
        _rowsHostPanel.Controls.Clear();

        if (_filteredUsers.Count == 0)
        {
            ShowEmptyState();
            return;
        }

        HideStatePanel();

        var skip = (_pageNumber - 1) * PageSize;
        var pageItems = _filteredUsers.Skip(skip).Take(PageSize).ToList();
        var currentUserId = SessionManager.CurrentUser?.UserId;

        foreach (var user in pageItems)
        {
            var row = new UsrUserRow(user);
            row.IsCurrentUser = currentUserId.HasValue && currentUserId.Value == user.Id;
            row.EditRequested += OnRowEditRequested;
            row.ToggleActiveRequested += OnRowToggleRequested;
            row.DeleteRequested += OnRowDeleteRequested;
            row.DetailsRequested += OnRowDetailsRequested;
            _userRows.Add(row);
            _rowsHostPanel.Controls.Add(row);
        }

        LayoutUserRows();
    }

    private void UpdateStats()
    {
        var stats = UserDisplayHelper.ComputeStats(_allUsers);
        _totalUsersCard.CardValue = stats.TotalUsers.ToString("N0");
        _totalUsersCard.Subtitle = string.Empty;
        _activePharmacistsCard.CardValue = stats.HasPharmacistRole
            ? stats.ActivePharmacists.ToString("N0")
            : "غير متوفر";
        _activePharmacistsCard.Subtitle = stats.HasPharmacistRole
            ? string.Empty
            : "لا يوجد دور صيدلي في البيانات";
        _systemAdminsCard.CardValue = stats.SystemAdmins.ToString("N0");
    }

    private void UpdatePaginationBar()
    {
        var total = _filteredUsers.Count;
        if (total <= 0)
        {
            _paginationBar.Update(1, 1, 0, 0, 0);
            return;
        }

        var from = ((_pageNumber - 1) * PageSize) + 1;
        var to = Math.Min(total, _pageNumber * PageSize);
        var totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)PageSize));
        _paginationBar.Update(_pageNumber, totalPages, from, to, total);
    }

    private void ChangePage(int page)
    {
        _pageNumber = Math.Max(1, page);
        RenderRows();
        UpdatePaginationBar();
    }

    private async Task OpenAddUserDialogAsync()
    {
        var owner = FindForm();
        using var dialog = new AddUserDialog(_userService);
        var result = owner is null ? dialog.ShowDialog() : dialog.ShowDialog(owner);
        if (result != DialogResult.OK)
        {
            return;
        }

        _searchBox.Text = string.Empty;
        _searchText = string.Empty;
        ClearDetails();
        await LoadUsersAsync();
    }

    private async void OnRowEditRequested(object? sender, EventArgs e)
    {
        if (sender is not UsrUserRow row)
        {
            return;
        }

        var owner = FindForm();
        using var dialog = new EditUserDialog(row.User, _userService);
        var result = owner is null ? dialog.ShowDialog() : dialog.ShowDialog(owner);
        if (result != DialogResult.OK)
        {
            return;
        }

        ClearDetails();
        await LoadUsersAsync();
    }

    private async void OnRowToggleRequested(object? sender, EventArgs e)
    {
        if (sender is not UsrUserRow row)
        {
            return;
        }

        if (IsCurrentSessionUser(row.User.Id))
        {
            MessageBox.Show(FindForm(), "لا يمكن تعطيل المستخدم الحالي.", "تعذر التنفيذ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var action = row.User.IsActive ? "تعطيل" : "تفعيل";
        var confirm = MessageBox.Show(
            FindForm(),
            $"هل تريد {action} المستخدم \"{row.User.DisplayName}\"؟",
            "تأكيد",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question,
            MessageBoxDefaultButton.Button2);
        if (confirm != DialogResult.Yes)
        {
            return;
        }

        var result = await _userService.SetUserActiveAsync(row.User, !row.User.IsActive).ConfigureAwait(true);
        if (!result.Success)
        {
            MessageBox.Show(
                FindForm(),
                result.ErrorMessage ?? $"تعذر {action} المستخدم.",
                "فشل العملية",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        ClearDetails();
        await LoadUsersAsync();
    }

    private async void OnRowDeleteRequested(object? sender, EventArgs e)
    {
        if (sender is not UsrUserRow row)
        {
            return;
        }

        if (IsCurrentSessionUser(row.User.Id))
        {
            MessageBox.Show(FindForm(), "لا يمكن حذف المستخدم الحالي.", "تعذر التنفيذ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var confirm = MessageBox.Show(
            FindForm(),
            $"هل تريد حذف المستخدم \"{row.User.DisplayName}\"؟",
            "تأكيد الحذف",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning,
            MessageBoxDefaultButton.Button2);
        if (confirm != DialogResult.Yes)
        {
            return;
        }

        var result = await _userService.DeleteUserAsync(row.User.Id).ConfigureAwait(true);
        if (!result.Success)
        {
            MessageBox.Show(
                FindForm(),
                result.ErrorMessage ?? "تعذر حذف المستخدم.",
                "فشل الحذف",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        ClearDetails();
        await LoadUsersAsync();
    }

    private void OnRowDetailsRequested(object? sender, EventArgs e)
    {
        if (sender is not UsrUserRow row)
        {
            return;
        }

        _selectedUser = row.User;
        _detailsPanel.Bind(row.User);
        LayoutUsersPage();
    }

    private static bool IsCurrentSessionUser(Guid userId) =>
        SessionManager.CurrentUser?.UserId == userId;

    private void ClearDetails()
    {
        _selectedUser = null;
        _detailsPanel.Bind(null);
        LayoutUsersPage();
    }

    private void ShowLoadingState()
    {
        _statePanel.Visible = true;
        _stateTitle.Text = "جاري تحميل المستخدمين...";
        _stateDetail.Text = string.Empty;
        _retryButton.Visible = false;
        _tablePanel.Visible = false;
        _paginationBar.Visible = false;
    }

    private void ShowErrorState(string message, bool isConnectionError)
    {
        _statePanel.Visible = true;
        _stateTitle.Text = "تعذر تحميل المستخدمين";
        _stateDetail.Text = isConnectionError
            ? $"{message}{Environment.NewLine}تحقق من أن API يعمل على {ApiConfiguration.BaseUrl}"
            : message;
        _retryButton.Visible = true;
        _tablePanel.Visible = false;
        _paginationBar.Visible = false;
    }

    private void ShowEmptyState()
    {
        _statePanel.Visible = true;
        _stateTitle.Text = string.IsNullOrWhiteSpace(_searchText)
            ? "لا يوجد مستخدمون"
            : "لا توجد نتائج";
        _stateDetail.Text = string.IsNullOrWhiteSpace(_searchText)
            ? "ابدأ بإضافة مستخدم جديد"
            : "جرّب كلمات بحث مختلفة";
        _retryButton.Visible = false;
        _tablePanel.Visible = false;
        _paginationBar.Visible = false;
    }

    private void HideStatePanel()
    {
        _statePanel.Visible = false;
        _tablePanel.Visible = true;
        _paginationBar.Visible = true;
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

    private void LayoutUsersPage()
    {
        if (_rootPanel.ClientSize.Width <= 0)
        {
            return;
        }

        var bounds = _rootPanel.ClientRectangle;
        var pad = WorkspacePadding;
        var contentW = Math.Max(320, bounds.Width - pad * 2);
        var contentH = Math.Max(240, bounds.Height - pad * 2);
        var showDetails = _detailsPanel.Visible && _selectedUser is not null;
        _detailsOverlay = showDetails && bounds.Width < OverlayBreakpoint;

        var detailsW = showDetails && !_detailsOverlay
            ? Math.Clamp(PharmaTheme.UsersDetailsWidth, 320, 400)
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
            _detailsPanel.Bounds = new Rectangle(pad, pad, Math.Min(contentW, PharmaTheme.UsersDetailsWidth), contentH);
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

        LayoutHeader(mainW);
        LayoutStats(mainW);

        var y = _headerPanel.Height + SectionGap + _statsPanel.Height + SectionGap;
        var bottomH = PaginationHeight + SectionGap;
        var tableH = Math.Max(160, contentH - y - bottomH);
        _tablePanel.SetBounds(0, y, mainW, tableH);
        LayoutTableInternals();

        _paginationBar.SetBounds(0, y + tableH + SectionGap, mainW, PaginationHeight);

        var stateY = y + 40;
        _statePanel.SetBounds(0, stateY, mainW, Math.Max(120, tableH - 40));
        if (_retryButton.Visible)
        {
            _retryButton.Location = new Point((mainW - _retryButton.Width) / 2, 72);
        }

        LayoutUserRows();
    }

    private void LayoutHeader(int mainW)
    {
        const int buttonW = 200;
        const int searchW = 280;
        const int gap = 16;

        if (mainW < 900)
        {
            _headerPanel.Height = 168;
            _titleLabel.SetBounds(0, 0, mainW, 38);
            _subtitleLabel.SetBounds(0, 40, mainW, 24);
            _addUserButton.SetBounds(0, 108, Math.Min(buttonW, mainW), 52);
            _searchBox.SetBounds(0, 108, Math.Min(searchW, mainW), 48);
            if (mainW >= 520)
            {
                _addUserButton.SetBounds(mainW - buttonW, 108, buttonW, 52);
                _searchBox.SetBounds(0, 108, Math.Max(220, mainW - buttonW - gap), 48);
            }

            return;
        }

        _headerPanel.Height = HeaderHeight;
        var textW = Math.Max(360, mainW - buttonW - searchW - gap * 2);
        _titleLabel.SetBounds(0, 0, textW, 38);
        _subtitleLabel.SetBounds(0, 40, textW, 24);
        _addUserButton.SetBounds(0, 24, buttonW, 52);
        _searchBox.SetBounds(buttonW + gap, 26, searchW, 48);
    }

    private void LayoutStats(int mainW)
    {
        var y = _headerPanel.Height + SectionGap;
        _statsPanel.SetBounds(0, y, mainW, StatsHeight);

        var gap = 20;
        var cols = mainW >= 1100 ? 3 : mainW >= 720 ? 2 : 1;
        var cardW = cols == 1 ? mainW : (mainW - gap * (cols - 1)) / cols;
        var cardH = 136;
        var x = 0;
        var rowY = 0;

        void PlaceCard(Control card, int col)
        {
            card.SetBounds(x + col * (cardW + gap), rowY, cardW, cardH);
        }

        if (cols == 3)
        {
            PlaceCard(_totalUsersCard, 0);
            PlaceCard(_activePharmacistsCard, 1);
            PlaceCard(_systemAdminsCard, 2);
            return;
        }

        if (cols == 2)
        {
            PlaceCard(_totalUsersCard, 0);
            PlaceCard(_activePharmacistsCard, 1);
            PlaceCard(_systemAdminsCard, 0);
            _systemAdminsCard.SetBounds(0, cardH + gap, cardW, cardH);
            _statsPanel.Height = cardH * 2 + gap;
            return;
        }

        _totalUsersCard.SetBounds(0, 0, cardW, cardH);
        _activePharmacistsCard.SetBounds(0, cardH + gap, cardW, cardH);
        _systemAdminsCard.SetBounds(0, (cardH + gap) * 2, cardW, cardH);
        _statsPanel.Height = cardH * 3 + gap * 2;
    }

    private void LayoutTableInternals()
    {
        var w = Math.Max(280, _tablePanel.ClientSize.Width);
        _tableHeader.SetBounds(0, 0, w, 50);
        _rowsScrollPanel.SetBounds(0, 54, w, Math.Max(80, _tablePanel.ClientSize.Height - 54));
        LayoutUserRows();
    }

    private void LayoutUserRows()
    {
        if (_userRows.Count == 0)
        {
            _rowsHostPanel.Size = new Size(Math.Max(280, _rowsScrollPanel.ClientSize.Width), 0);
            return;
        }

        var w = Math.Max(280, _rowsScrollPanel.ClientSize.Width);
        var y = 0;
        foreach (var row in _userRows)
        {
            row.SetBounds(0, y, w, PharmaTheme.UsersRowHeight);
            y += PharmaTheme.UsersRowHeight + RowGap;
        }

        _rowsHostPanel.Size = new Size(w, y);
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
