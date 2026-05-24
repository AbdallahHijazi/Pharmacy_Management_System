using System.ComponentModel;
using System.Drawing.Drawing2D;
using Pharmacy.WinForms.Controls;
using Pharmacy.WinForms.Models;
using Pharmacy.WinForms.Ui;

namespace Pharmacy.WinForms.Controls.Reports;

internal sealed class ReportCardControl : Control
{
    private ReportCardViewModel _card = null!;
    private bool _hover;

    public ReportCardControl(ReportCardViewModel card)
    {
        _card = card;
        MinimumSize = new Size(280, 240);
        Height = 240;
        Cursor = Cursors.Hand;
        RightToLeft = RightToLeft.Yes;
        SetStyle(
            ControlStyles.AllPaintingInWmPaint
                | ControlStyles.UserPaint
                | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.ResizeRedraw
                | ControlStyles.StandardClick,
            true);
        MouseClick += OnCardMouseClick;
    }

    private void OnCardMouseClick(object? sender, MouseEventArgs e)
    {
        if (GetDetailsButtonRect().Contains(e.Location))
        {
            DetailsClicked?.Invoke(this, EventArgs.Empty);
            return;
        }

        if (GetExportButtonRect().Contains(e.Location))
        {
            ExportClicked?.Invoke(this, EventArgs.Empty);
        }
    }

    public event EventHandler? DetailsClicked;
    public event EventHandler? ExportClicked;

    public ReportKind Kind => _card.Kind;

    public void Bind(ReportCardViewModel card)
    {
        _card = card;
        Invalidate();
    }

    public void ApplyThemeVisuals() => Invalidate();

    protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        _hover = true;
        Invalidate();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        _hover = false;
        Invalidate();
    }

    protected override void OnPaintBackground(PaintEventArgs e) =>
        e.Graphics.Clear(Parent?.BackColor ?? PharmaTheme.Background);

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        var bounds = ClientRectangle;
        bounds.Inflate(-1, -1);
        var fill = _hover ? PharmaTheme.SurfaceContainerLow : PharmaTheme.Surface;
        RoundedDrawing.FillRounded(g, bounds, PharmaTheme.ReportsCardCornerRadius, fill);
        RoundedDrawing.DrawRoundedBorder(g, bounds, PharmaTheme.ReportsCardCornerRadius, PharmaTheme.BorderSoft, 1f);

        if (_card.IsWarning)
        {
            var accent = new Rectangle(bounds.Right - 4, bounds.Y + 12, 4, bounds.Height - 24);
            using var accentBrush = new SolidBrush(PharmaTheme.Danger);
            g.FillRectangle(accentBrush, accent);
        }

        const int pad = 24;
        var exportRect = GetExportButtonRect();
        RoundedDrawing.FillRounded(g, exportRect, 8, PharmaTheme.SurfaceContainerHigh);
        TextRenderer.DrawText(
            g,
            SegoeMdl2Icons.Download,
            PharmaTheme.IconFont(11f),
            exportRect,
            PharmaTheme.OnSurfaceVariant,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

        var iconSize = 52;
        var iconRect = new Rectangle(bounds.X + pad, bounds.Y + pad, iconSize, iconSize);
        var iconBack = _card.IsWarning
            ? PharmaTheme.WithAlpha(PharmaTheme.ErrorContainer, 180)
            : PharmaTheme.WithAlpha(PharmaTheme.PrimaryContainer, 160);
        RoundedDrawing.FillRounded(g, iconRect, 14, iconBack);
        TextRenderer.DrawText(
            g,
            _card.IconGlyph,
            PharmaTheme.IconFont(22f),
            iconRect,
            _card.IsWarning ? PharmaTheme.Danger : PharmaTheme.Primary,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

        var textX = bounds.X + pad + iconSize + 16;
        var textW = Math.Max(120, bounds.Width - textX - pad);
        TextRenderer.DrawText(
            g,
            _card.Title,
            PharmaTheme.ArabicFont(13f, FontStyle.Bold),
            new Rectangle(textX, bounds.Y + pad, textW, 28),
            PharmaTheme.TextDark,
            TextFormatFlags.Right | TextFormatFlags.Top | TextFormatFlags.EndEllipsis | TextFormatFlags.WordBreak);

        TextRenderer.DrawText(
            g,
            _card.Description,
            PharmaTheme.SmallFont,
            new Rectangle(bounds.X + pad, bounds.Y + pad + 64, bounds.Width - pad * 2, 48),
            PharmaTheme.OnSurfaceVariant,
            TextFormatFlags.Right | TextFormatFlags.Top | TextFormatFlags.WordBreak | TextFormatFlags.EndEllipsis);

        var dividerY = bounds.Bottom - pad - 72;
        using var dividerPen = new Pen(PharmaTheme.BorderSoft);
        g.DrawLine(dividerPen, bounds.X + pad, dividerY, bounds.Right - pad, dividerY);

        var badgeBack = _card.IsWarning ? PharmaTheme.ErrorContainer : PharmaTheme.PrimaryContainer;
        var badgeFore = _card.IsWarning ? PharmaTheme.Danger : PharmaTheme.PrimaryDark;
        var badgeSize = TextRenderer.MeasureText(_card.BadgeText, PharmaTheme.SmallFont);
        var badgeW = Math.Min(bounds.Width - pad * 2, badgeSize.Width + 16);
        var badgeH = 24;
        var badgeRect = new Rectangle(bounds.Right - pad - badgeW, dividerY + 10, badgeW, badgeH);
        RoundedDrawing.FillRounded(g, badgeRect, badgeH / 2, badgeBack);
        TextRenderer.DrawText(
            g,
            _card.BadgeText,
            PharmaTheme.SmallFont,
            badgeRect,
            badgeFore,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

        var detailsRect = GetDetailsButtonRect();
        var detailsBack = _card.IsWarning ? PharmaTheme.WithAlpha(PharmaTheme.ErrorContainer, 140) : PharmaTheme.PrimaryContainer;
        var detailsFore = _card.IsWarning ? PharmaTheme.Danger : PharmaTheme.PrimaryDark;
        RoundedDrawing.FillRounded(g, detailsRect, 10, detailsBack);
        TextRenderer.DrawText(
            g,
            "عرض التفاصيل",
            PharmaTheme.ArabicFont(10f, FontStyle.Bold),
            detailsRect,
            detailsFore,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
    }

    private Rectangle GetExportButtonRect()
    {
        const int pad = 24;
        return new Rectangle(Width - pad - 36, pad, 32, 32);
    }

    private Rectangle GetDetailsButtonRect()
    {
        const int pad = 24;
        var w = 120;
        var h = 36;
        return new Rectangle(pad, Height - pad - h, w, h);
    }
}

internal sealed class ReportDetailsPanel : Panel
{
    private readonly Panel _headerPanel = new();
    private readonly ReportDetailsBackButton _backButton = new();
    private readonly Label _titleLabel = new();
    private readonly Label _periodLabel = new();
    private readonly Panel _contentPanel = new();
    private readonly Label _statusLabel = new();
    private readonly Panel _footerPanel = new();
    private readonly ReportDetailsOutlineButton _refreshButton = new();
    private readonly GradientRoundedButton _exportButton = new();
    private ReportCardViewModel? _card;
    private ReportLoadResult? _lastLoadedResult;

    public ReportDetailsPanel()
    {
        DoubleBuffered = true;
        Visible = false;
        Width = PharmaTheme.ReportsDetailsWidth;
        BackColor = PharmaTheme.Surface;
        RightToLeft = RightToLeft.Yes;
        Padding = new Padding(0);

        _headerPanel.BackColor = PharmaTheme.Surface;
        _headerPanel.Dock = DockStyle.Top;
        _headerPanel.Height = 96;
        _headerPanel.Padding = new Padding(16, 14, 16, 8);

        _backButton.Click += (_, _) => CloseRequested?.Invoke(this, EventArgs.Empty);

        _titleLabel.Font = PharmaTheme.ArabicFont(15f, FontStyle.Bold);
        _titleLabel.ForeColor = PharmaTheme.TextDark;
        _titleLabel.AutoSize = false;
        _titleLabel.Height = 30;
        _titleLabel.TextAlign = ContentAlignment.MiddleRight;

        _periodLabel.Font = PharmaTheme.SmallFont;
        _periodLabel.ForeColor = PharmaTheme.OnSurfaceVariant;
        _periodLabel.AutoSize = false;
        _periodLabel.Height = 22;
        _periodLabel.TextAlign = ContentAlignment.MiddleRight;

        _headerPanel.Controls.Add(_periodLabel);
        _headerPanel.Controls.Add(_titleLabel);
        _headerPanel.Controls.Add(_backButton);
        _headerPanel.Resize += (_, _) => LayoutHeader();

        _statusLabel.Font = PharmaTheme.BodyFont;
        _statusLabel.ForeColor = PharmaTheme.OnSurfaceVariant;
        _statusLabel.AutoSize = false;
        _statusLabel.Height = 48;
        _statusLabel.TextAlign = ContentAlignment.MiddleCenter;
        _statusLabel.Dock = DockStyle.Top;
        _statusLabel.Visible = false;

        _contentPanel.AutoScroll = true;
        _contentPanel.BackColor = PharmaTheme.Surface;
        _contentPanel.Dock = DockStyle.Fill;
        _contentPanel.Padding = new Padding(16, 8, 16, 8);

        _refreshButton.Text = "تحديث";
        _refreshButton.Click += (_, _) => RefreshRequested?.Invoke(this, EventArgs.Empty);

        _exportButton.Text = "تصدير التقرير";
        _exportButton.IconGlyph = SegoeMdl2Icons.Download;
        _exportButton.Height = 44;
        _exportButton.Click += (_, _) => ExportRequested?.Invoke(this, EventArgs.Empty);

        _footerPanel.BackColor = PharmaTheme.Surface;
        _footerPanel.Dock = DockStyle.Bottom;
        _footerPanel.Height = 64;
        _footerPanel.Padding = new Padding(16, 10, 16, 10);
        _footerPanel.Controls.Add(_exportButton);
        _footerPanel.Controls.Add(_refreshButton);
        _footerPanel.Resize += (_, _) => LayoutFooter();

        Controls.Add(_contentPanel);
        Controls.Add(_statusLabel);
        Controls.Add(_footerPanel);
        Controls.Add(_headerPanel);
    }

    public event EventHandler? CloseRequested;
    public event EventHandler? RefreshRequested;
    public event EventHandler? ExportRequested;

    public ReportKind? ActiveKind => _card?.Kind;

    public ReportLoadResult? LastLoadedResult => _lastLoadedResult;

    public void SetExportBusy(bool busy)
    {
        _exportButton.Enabled = !busy;
        _exportButton.Text = busy ? "جارٍ التصدير..." : "تصدير التقرير";
    }

    public void BindCard(ReportCardViewModel? card)
    {
        _card = card;
        _lastLoadedResult = null;
        Visible = card is not null;
        _titleLabel.Text = card?.Title ?? string.Empty;
        _periodLabel.Text = string.Empty;
        _statusLabel.Visible = false;
        _contentPanel.Controls.Clear();
    }

    public void ShowLoading(string message)
    {
        _statusLabel.Text = message;
        _statusLabel.Visible = true;
        _contentPanel.Controls.Clear();
    }

    public void ShowError(string message, bool showRetry)
    {
        _statusLabel.Text = message;
        _statusLabel.Visible = true;
        _contentPanel.Controls.Clear();
        _refreshButton.Visible = showRetry;
    }

    public void ShowContent(ReportLoadResult result)
    {
        _lastLoadedResult = result;
        _periodLabel.Text = result.PeriodText;
        _statusLabel.Visible = false;
        _contentPanel.Controls.Clear();

        if (result.Content is null)
        {
            return;
        }

        var host = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            BackColor = PharmaTheme.Surface,
            RightToLeft = RightToLeft.Yes,
            Width = Math.Max(300, _contentPanel.ClientSize.Width - 8),
            Padding = new Padding(4, 8, 4, 12)
        };

        foreach (var (label, value) in result.Content.Summary)
        {
            host.Controls.Add(MakeSummaryRow(label, value, host.Width));
        }

        if (result.Content.TableHeaders.Count > 0)
        {
            host.Controls.Add(MakeTable(result.Content, host.Width));
        }
        else if (!string.IsNullOrWhiteSpace(result.Content.EmptyMessage))
        {
            host.Controls.Add(new Label
            {
                Text = result.Content.EmptyMessage,
                AutoSize = true,
                Font = PharmaTheme.BodyFont,
                ForeColor = PharmaTheme.OnSurfaceVariant,
                Margin = new Padding(0, 12, 0, 0)
            });
        }

        _contentPanel.Controls.Add(host);
    }

    public void ApplyThemeVisuals()
    {
        BackColor = PharmaTheme.Surface;
        _headerPanel.BackColor = PharmaTheme.Surface;
        _footerPanel.BackColor = PharmaTheme.Surface;
        _titleLabel.ForeColor = PharmaTheme.TextDark;
        _periodLabel.ForeColor = PharmaTheme.OnSurfaceVariant;
        _contentPanel.BackColor = PharmaTheme.Surface;
        _backButton.ApplyThemeVisuals();
        _refreshButton.ApplyThemeVisuals();
        _exportButton.Invalidate();
        Invalidate(true);
    }

    private void LayoutHeader()
    {
        const int pad = 16;
        var w = Math.Max(240, _headerPanel.ClientSize.Width - pad * 2);
        _backButton.SetBounds(_headerPanel.ClientSize.Width - pad - _backButton.Width, 0, _backButton.Width, _backButton.Height);
        _titleLabel.SetBounds(pad, _backButton.Bottom + 10, w, 30);
        _periodLabel.SetBounds(pad, _titleLabel.Bottom + 2, w, 22);
    }

    private void LayoutFooter()
    {
        const int gap = 12;
        const int buttonH = 44;
        const int exportW = 168;
        const int refreshW = 120;
        var y = Math.Max(0, (_footerPanel.ClientSize.Height - buttonH) / 2);
        var right = _footerPanel.ClientSize.Width - _footerPanel.Padding.Right;
        _exportButton.SetBounds(right - exportW, y, exportW, buttonH);
        _refreshButton.SetBounds(_exportButton.Left - gap - refreshW, y, refreshW, buttonH);
    }

    private static Control MakeSummaryRow(string label, string value, int width)
    {
        var panel = new Panel { Height = 32, Width = width, BackColor = PharmaTheme.Surface, Margin = new Padding(0, 0, 0, 4) };
        var cap = new Label
        {
            Text = label,
            Dock = DockStyle.Right,
            Width = 140,
            ForeColor = PharmaTheme.OnSurfaceVariant,
            Font = PharmaTheme.SmallFont,
            TextAlign = ContentAlignment.MiddleRight
        };
        var val = new Label
        {
            Text = value,
            Dock = DockStyle.Fill,
            ForeColor = PharmaTheme.TextDark,
            Font = PharmaTheme.BodyFont,
            TextAlign = ContentAlignment.MiddleRight,
            AutoEllipsis = true
        };
        panel.Controls.Add(val);
        panel.Controls.Add(cap);
        return panel;
    }

    private static Control MakeTable(ReportDetailsContentView content, int width)
    {
        var panel = new Panel
        {
            Width = width,
            AutoSize = true,
            BackColor = PharmaTheme.Surface,
            Margin = new Padding(0, 16, 0, 0)
        };

        var y = 0;
        var colCount = content.TableHeaders.Count;
        var colW = Math.Max(80, (width - 16) / colCount);

        for (var c = 0; c < colCount; c++)
        {
            var headerRect = new Panel
            {
                Bounds = new Rectangle(c * colW, y, colW, 32),
                BackColor = PharmaTheme.SurfaceAlt
            };
            headerRect.Controls.Add(new Label
            {
                Text = content.TableHeaders[c],
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleRight,
                Font = PharmaTheme.TableHeaderFont,
                ForeColor = PharmaTheme.MutedText,
                Padding = new Padding(4)
            });
            panel.Controls.Add(headerRect);
        }

        y += 34;
        foreach (var row in content.TableRows)
        {
            for (var c = 0; c < colCount; c++)
            {
                var cellText = c < row.Cells.Count ? row.Cells[c] : string.Empty;
                var cellPanel = new Panel
                {
                    Bounds = new Rectangle(c * colW, y, colW, 30),
                    BackColor = PharmaTheme.Surface
                };
                cellPanel.Controls.Add(new Label
                {
                    Text = cellText,
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleRight,
                    Font = PharmaTheme.TableCellFont,
                    ForeColor = PharmaTheme.TextDark,
                    AutoEllipsis = true,
                    Padding = new Padding(4)
                });
                panel.Controls.Add(cellPanel);
            }

            y += 32;
        }

        panel.Height = y + 4;
        return panel;
    }
}

internal sealed class ReportDetailsBackButton : Control
{
    private bool _hover;
    private bool _pressed;

    public ReportDetailsBackButton()
    {
        Size = new Size(104, 40);
        MinimumSize = new Size(96, 38);
        Cursor = Cursors.Hand;
        RightToLeft = RightToLeft.Yes;
        SetStyle(
            ControlStyles.AllPaintingInWmPaint
                | ControlStyles.UserPaint
                | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.StandardClick,
            true);
    }

    public void ApplyThemeVisuals() => Invalidate();

    protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        _hover = true;
        Invalidate();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        _hover = false;
        _pressed = false;
        Invalidate();
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.Button == MouseButtons.Left)
        {
            _pressed = true;
            Invalidate();
        }
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        if (_pressed && e.Button == MouseButtons.Left && ClientRectangle.Contains(e.Location))
        {
            OnClick(EventArgs.Empty);
        }

        _pressed = false;
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var bounds = ClientRectangle;
        bounds.Inflate(-1, -1);
        const int radius = 12;

        var fill = _pressed
            ? PharmaTheme.SurfaceContainerHigh
            : _hover
                ? PharmaTheme.SurfaceAlt
                : PharmaTheme.SurfaceContainerHigh;
        RoundedDrawing.FillRounded(g, bounds, radius, fill);
        RoundedDrawing.DrawRoundedBorder(g, bounds, radius, PharmaTheme.BorderSoft, 1f);

        var iconRect = new Rectangle(bounds.Right - 30, bounds.Y, 24, bounds.Height);
        TextRenderer.DrawText(
            g,
            SegoeMdl2Icons.ChevronRight,
            PharmaTheme.IconFont(11f),
            iconRect,
            PharmaTheme.Primary,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

        var textRect = new Rectangle(bounds.X + 8, bounds.Y, bounds.Width - 36, bounds.Height);
        TextRenderer.DrawText(
            g,
            "رجوع",
            PharmaTheme.ArabicFont(10f, FontStyle.Bold),
            textRect,
            PharmaTheme.Primary,
            TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
    }
}

internal sealed class ReportDetailsOutlineButton : Control
{
    private bool _hover;
    private bool _pressed;

    public ReportDetailsOutlineButton()
    {
        Height = 44;
        MinimumSize = new Size(112, 42);
        Cursor = Cursors.Hand;
        RightToLeft = RightToLeft.Yes;
        SetStyle(
            ControlStyles.AllPaintingInWmPaint
                | ControlStyles.UserPaint
                | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.StandardClick,
            true);
    }

    public void ApplyThemeVisuals() => Invalidate();

    protected override void OnEnabledChanged(EventArgs e)
    {
        base.OnEnabledChanged(e);
        Cursor = Enabled ? Cursors.Hand : Cursors.Default;
        Invalidate();
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        if (Enabled)
        {
            _hover = true;
            Invalidate();
        }
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        _hover = false;
        _pressed = false;
        Invalidate();
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (Enabled && e.Button == MouseButtons.Left)
        {
            _pressed = true;
            Invalidate();
        }
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        if (_pressed && Enabled && e.Button == MouseButtons.Left && ClientRectangle.Contains(e.Location))
        {
            OnClick(EventArgs.Empty);
        }

        _pressed = false;
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var bounds = ClientRectangle;
        bounds.Inflate(-1, -1);
        const int radius = 12;

        var fill = !Enabled
            ? PharmaTheme.SurfaceContainerHigh
            : _pressed
                ? PharmaTheme.SurfaceAlt
                : _hover
                    ? PharmaTheme.SurfaceAlt
                    : PharmaTheme.Surface;
        var border = !Enabled ? PharmaTheme.BorderSoft : PharmaTheme.Primary;
        var text = !Enabled ? PharmaTheme.MutedText : PharmaTheme.Primary;

        RoundedDrawing.FillRounded(g, bounds, radius, fill);
        RoundedDrawing.DrawRoundedBorder(g, bounds, radius, border, 1.5f);
        TextRenderer.DrawText(
            g,
            Text,
            PharmaTheme.ArabicFont(10.5f, FontStyle.Bold),
            bounds,
            text,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.RightToLeft);
    }
}
