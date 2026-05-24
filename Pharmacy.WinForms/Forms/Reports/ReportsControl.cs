using Pharmacy.WinForms.Controls;
using Pharmacy.WinForms.Controls.Reports;
using Pharmacy.WinForms.Models;
using Pharmacy.WinForms.Services;
using Pharmacy.WinForms.Ui;

namespace Pharmacy.WinForms.Forms.Reports;

internal sealed class ReportsControl : UserControl
{
    private const int WorkspacePadding = 32;
    private const int SectionGap = 16;
    private const int HeaderHeight = 104;
    private const int CardGap = 24;
    private const int CardHeight = 240;
    private const int DetailsGap = 16;
    private const int OverlayBreakpoint = 980;

    private readonly ReportsService _reportsService;
    private readonly ReportsExportService _exportService;
    private readonly List<ReportCardControl> _cards = new();
    private CancellationTokenSource? _loadCts;
    private ReportKind? _activeKind;
    private bool _isExporting;

    private Panel _rootPanel = null!;
    private Panel _mainContentPanel = null!;
    private Panel _headerPanel = null!;
    private Label _titleLabel = null!;
    private Label _subtitleLabel = null!;
    private GradientRoundedButton _exportAllButton = null!;
    private Panel _reportsScrollPanel = null!;
    private Panel _reportsGridPanel = null!;
    private ReportDetailsPanel _detailsPanel = null!;

    public ReportsControl() : this(AppServices.ReportsService, AppServices.ReportsExportService)
    {
    }

    public ReportsControl(ReportsService reportsService, ReportsExportService exportService)
    {
        _reportsService = reportsService;
        _exportService = exportService;

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
        SizeChanged += (_, _) => LayoutReportsPage();
    }

    public void ApplyThemeVisuals()
    {
        BackColor = PharmaTheme.Background;
        _rootPanel.BackColor = PharmaTheme.Background;
        _mainContentPanel.BackColor = PharmaTheme.Background;
        _headerPanel.BackColor = PharmaTheme.Background;
        _reportsScrollPanel.BackColor = PharmaTheme.Background;
        _reportsGridPanel.BackColor = PharmaTheme.Background;

        _titleLabel.ForeColor = PharmaTheme.PrimaryDark;
        _titleLabel.Font = PharmaTheme.DashboardHeadlineFont;
        _subtitleLabel.ForeColor = PharmaTheme.OnSurfaceVariant;
        _subtitleLabel.Font = PharmaTheme.DashboardSubtitleFont;
        _exportAllButton.Invalidate();
        _detailsPanel.ApplyThemeVisuals();

        foreach (var card in _cards)
        {
            card.ApplyThemeVisuals();
        }

        LayoutReportsPage();
        Invalidate(true);
    }

    private void BuildUi()
    {
        _rootPanel = new Panel { Dock = DockStyle.Fill, BackColor = PharmaTheme.Background };
        _detailsPanel = new ReportDetailsPanel();
        _mainContentPanel = new Panel { BackColor = PharmaTheme.Background };

        _headerPanel = new Panel { BackColor = PharmaTheme.Background, Height = HeaderHeight };
        _titleLabel = new Label
        {
            Text = "التقارير التحليلية",
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleRight,
            Font = PharmaTheme.DashboardHeadlineFont,
            ForeColor = PharmaTheme.PrimaryDark,
            RightToLeft = RightToLeft.Yes,
            BackColor = PharmaTheme.Background
        };
        _subtitleLabel = new Label
        {
            Text = "نظرة شاملة على الأداء المالي والتشغيلي للصيدلية.",
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleRight,
            Font = PharmaTheme.DashboardSubtitleFont,
            ForeColor = PharmaTheme.OnSurfaceVariant,
            RightToLeft = RightToLeft.Yes,
            BackColor = PharmaTheme.Background
        };
        _exportAllButton = new GradientRoundedButton
        {
            Text = "تصدير التقرير الشامل",
            IconGlyph = SegoeMdl2Icons.Download,
            Width = 250,
            Height = 52
        };

        _headerPanel.Controls.Add(_exportAllButton);
        _headerPanel.Controls.Add(_subtitleLabel);
        _headerPanel.Controls.Add(_titleLabel);

        _reportsScrollPanel = new Panel { AutoScroll = true, BackColor = PharmaTheme.Background };
        _reportsGridPanel = new Panel { BackColor = PharmaTheme.Background };
        _reportsScrollPanel.Controls.Add(_reportsGridPanel);

        foreach (var cardModel in ReportCatalog.CreateCards())
        {
            var card = new ReportCardControl(cardModel);
            card.DetailsClicked += async (_, _) => await OpenDetailsAsync(cardModel);
            card.ExportClicked += async (_, _) => await ExportSingleReportAsync(cardModel.Kind);
            _cards.Add(card);
            _reportsGridPanel.Controls.Add(card);
        }

        _mainContentPanel.Controls.Add(_reportsScrollPanel);
        _mainContentPanel.Controls.Add(_headerPanel);

        _rootPanel.Controls.Add(_mainContentPanel);
        _rootPanel.Controls.Add(_detailsPanel);
        Controls.Add(_rootPanel);
    }

    private void WireEvents()
    {
        _exportAllButton.Click += async (_, _) => await ExportAllReportsAsync();
        _detailsPanel.CloseRequested += (_, _) => CloseDetails();
        _detailsPanel.RefreshRequested += async (_, _) =>
        {
            if (_activeKind.HasValue)
            {
                await LoadDetailsAsync(_activeKind.Value);
            }
        };
        _detailsPanel.ExportRequested += async (_, _) =>
        {
            if (_activeKind.HasValue)
            {
                await ExportSingleReportAsync(_activeKind.Value, useCachedDetails: true);
            }
        };
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

    private void LayoutReportsPage()
    {
        if (_rootPanel.ClientSize.Width <= 0)
        {
            return;
        }

        var bounds = _rootPanel.ClientRectangle;
        var pad = WorkspacePadding;
        var contentW = Math.Max(320, bounds.Width - pad * 2);
        var contentH = Math.Max(240, bounds.Height - pad * 2);
        var showDetails = _detailsPanel.Visible && _activeKind.HasValue;
        var detailsOverlay = showDetails && bounds.Width < OverlayBreakpoint;

        var detailsW = showDetails && !detailsOverlay
            ? Math.Clamp(PharmaTheme.ReportsDetailsWidth, 340, 460)
            : 0;

        if (showDetails && !detailsOverlay)
        {
            var tentativeMain = contentW - detailsW - DetailsGap;
            if (tentativeMain < 520)
            {
                detailsOverlay = true;
                detailsW = 0;
            }
        }

        if (detailsOverlay && showDetails)
        {
            _detailsPanel.Bounds = new Rectangle(pad, pad, Math.Min(contentW, PharmaTheme.ReportsDetailsWidth), contentH);
            _detailsPanel.BringToFront();
        }
        else if (showDetails)
        {
            _detailsPanel.SetBounds(pad, pad, detailsW, contentH);
            _detailsPanel.BringToFront();
        }
        else
        {
            _detailsPanel.SetBounds(-600, pad, detailsW, contentH);
        }

        var mainX = showDetails && !detailsOverlay ? pad + detailsW + DetailsGap : pad;
        var mainW = showDetails && !detailsOverlay ? contentW - detailsW - DetailsGap : contentW;
        _mainContentPanel.SetBounds(mainX, pad, mainW, contentH);

        LayoutHeader(mainW);

        var y = _headerPanel.Height + SectionGap;
        _reportsScrollPanel.SetBounds(0, y, mainW, Math.Max(160, contentH - y));

        LayoutReportCards(mainW);
    }

    private void LayoutHeader(int mainW)
    {
        const int buttonW = 250;
        const int gap = 16;

        if (mainW < 900)
        {
            _headerPanel.Height = 168;
            _titleLabel.SetBounds(0, 0, mainW, 38);
            _subtitleLabel.SetBounds(0, 40, mainW, 24);
            _exportAllButton.SetBounds(0, 108, Math.Min(buttonW, mainW), 52);
            return;
        }

        _headerPanel.Height = HeaderHeight;
        var textW = Math.Max(360, mainW - buttonW - gap);
        _titleLabel.SetBounds(0, 0, textW, 38);
        _subtitleLabel.SetBounds(0, 40, textW, 24);
        _exportAllButton.SetBounds(mainW - buttonW, 16, buttonW, 52);
    }

    private void LayoutReportCards(int mainW)
    {
        var availableW = Math.Max(280, _reportsScrollPanel.ClientSize.Width);
        var cols = availableW >= 1200 ? 3 : availableW >= 780 ? 2 : 1;
        var cardW = cols == 1 ? availableW : (availableW - CardGap * (cols - 1)) / cols;

        var x = 0;
        var y = 0;
        var col = 0;
        foreach (var card in _cards)
        {
            card.SetBounds(x, y, cardW, CardHeight);
            col++;
            if (col >= cols)
            {
                col = 0;
                x = 0;
                y += CardHeight + CardGap;
            }
            else
            {
                x += cardW + CardGap;
            }
        }

        if (col > 0)
        {
            y += CardHeight + CardGap;
        }

        _reportsGridPanel.Size = new Size(availableW, Math.Max(y, CardHeight));
    }

    private async Task OpenDetailsAsync(ReportCardViewModel card)
    {
        _activeKind = card.Kind;
        _detailsPanel.BindCard(card);
        LayoutReportsPage();
        await LoadDetailsAsync(card.Kind);
    }

    private async Task LoadDetailsAsync(ReportKind kind)
    {
        _loadCts?.Cancel();
        _loadCts = new CancellationTokenSource();
        var token = _loadCts.Token;

        _detailsPanel.ShowLoading("جاري تحميل التقرير...");

        try
        {
            var result = await _reportsService.LoadReportAsync(kind, token).ConfigureAwait(true);
            if (!result.IsAvailable)
            {
                _detailsPanel.ShowError(
                    result.ErrorMessage ?? "هذا التقرير غير متاح بعد من الـ API الحالي.",
                    showRetry: false);
                return;
            }

            if (!result.Success)
            {
                _detailsPanel.ShowError(
                    result.IsConnectionError
                        ? $"{result.ErrorMessage}{Environment.NewLine}تحقق من أن API يعمل على {ApiConfiguration.BaseUrl}"
                        : result.ErrorMessage ?? "تعذر تحميل التقرير.",
                    showRetry: true);
                return;
            }

            _detailsPanel.ShowContent(result);
        }
        catch (OperationCanceledException)
        {
            // Ignore.
        }
    }

    private void CloseDetails()
    {
        _activeKind = null;
        _detailsPanel.BindCard(null);
        LayoutReportsPage();
    }

    private async Task ExportSingleReportAsync(ReportKind kind, bool useCachedDetails = false)
    {
        if (_isExporting)
        {
            return;
        }

        using var dialog = new SaveFileDialog
        {
            Filter = "CSV files (*.csv)|*.csv",
            FileName = ReportDisplayHelper.GetSingleExportDefaultFileName(kind),
            Title = "تصدير التقرير"
        };

        if (dialog.ShowDialog(FindForm()) != DialogResult.OK)
        {
            return;
        }

        _isExporting = true;
        SetExportBusy(true);
        try
        {
            ReportLoadResult? cached = useCachedDetails && _detailsPanel.ActiveKind == kind
                ? _detailsPanel.LastLoadedResult
                : null;

            var result = await _exportService.ExportSingleReportAsync(
                kind,
                dialog.FileName,
                cached).ConfigureAwait(true);

            if (result.IsCancelled)
            {
                return;
            }

            if (!result.Success)
            {
                MessageBox.Show(
                    FindForm(),
                    result.ErrorMessage ?? "تعذر تصدير التقرير.",
                    "فشل التصدير",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            MessageBox.Show(
                FindForm(),
                $"تم تصدير التقرير بنجاح.{Environment.NewLine}{result.FilePath}",
                "تم التصدير",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        finally
        {
            _isExporting = false;
            SetExportBusy(false);
        }
    }

    private async Task ExportAllReportsAsync()
    {
        if (_isExporting)
        {
            return;
        }

        using var dialog = new SaveFileDialog
        {
            Filter = "ZIP files (*.zip)|*.zip",
            FileName = ReportDisplayHelper.GetBulkExportDefaultFileName(),
            Title = "تصدير التقرير الشامل"
        };

        if (dialog.ShowDialog(FindForm()) != DialogResult.OK)
        {
            return;
        }

        _isExporting = true;
        SetExportBusy(true, bulkExport: true);
        try
        {
            var result = await _exportService.ExportAllReportsAsync(dialog.FileName).ConfigureAwait(true);
            if (result.IsCancelled)
            {
                return;
            }

            if (!result.Success)
            {
                MessageBox.Show(
                    FindForm(),
                    result.ErrorMessage ?? "تعذر إنشاء التصدير الشامل.",
                    "فشل التصدير",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            var unavailableNote = result.UnavailableCount > 0
                ? $"{Environment.NewLine}تقارير غير متاحة: {result.UnavailableCount} (راجع unavailable-reports.txt داخل الأرشيف)."
                : string.Empty;

            MessageBox.Show(
                FindForm(),
                $"تم إنشاء التصدير الشامل بنجاح.{Environment.NewLine}عدد التقارير: {result.ExportedCount}{unavailableNote}{Environment.NewLine}{result.FilePath}",
                "تم التصدير",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        finally
        {
            _isExporting = false;
            SetExportBusy(false);
        }
    }

    private void SetExportBusy(bool busy, bool bulkExport = false)
    {
        _exportAllButton.Enabled = !busy;
        if (bulkExport && busy)
        {
            _exportAllButton.Text = "جارٍ تجهيز التصدير...";
        }
        else if (!busy)
        {
            _exportAllButton.Text = "تصدير التقرير الشامل";
        }

        _detailsPanel.SetExportBusy(busy);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            ThemeManager.ThemeChanged -= HandleThemeChanged;
            FontScaleManager.Changed -= HandleThemeChanged;
            _loadCts?.Cancel();
            _loadCts?.Dispose();
        }

        base.Dispose(disposing);
    }
}
