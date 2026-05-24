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
    private readonly Label _closeButton = new();
    private readonly Label _titleLabel = new();
    private readonly Label _periodLabel = new();
    private readonly Panel _contentPanel = new();
    private readonly Label _statusLabel = new();
    private readonly Button _refreshButton = new();
    private readonly Button _exportButton = new();
    private ReportCardViewModel? _card;
    private ReportLoadResult? _lastLoadedResult;

    public ReportDetailsPanel()
    {
        DoubleBuffered = true;
        Visible = false;
        Width = PharmaTheme.ReportsDetailsWidth;
        BackColor = PharmaTheme.Surface;
        RightToLeft = RightToLeft.Yes;
        Padding = new Padding(20, 16, 20, 16);

        _closeButton.Text = SegoeMdl2Icons.Close;
        _closeButton.Font = PharmaTheme.IconFont(11f);
        _closeButton.AutoSize = true;
        _closeButton.Cursor = Cursors.Hand;
        _closeButton.Click += (_, _) => CloseRequested?.Invoke(this, EventArgs.Empty);

        _titleLabel.Font = PharmaTheme.ArabicFont(15f, FontStyle.Bold);
        _titleLabel.ForeColor = PharmaTheme.TextDark;
        _titleLabel.AutoSize = false;
        _titleLabel.Height = 32;
        _titleLabel.TextAlign = ContentAlignment.MiddleRight;
        _titleLabel.Dock = DockStyle.Top;

        _periodLabel.Font = PharmaTheme.SmallFont;
        _periodLabel.ForeColor = PharmaTheme.OnSurfaceVariant;
        _periodLabel.AutoSize = false;
        _periodLabel.Height = 22;
        _periodLabel.TextAlign = ContentAlignment.MiddleRight;
        _periodLabel.Dock = DockStyle.Top;

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

        _refreshButton.Text = "تحديث";
        _refreshButton.AutoSize = true;
        _refreshButton.FlatStyle = FlatStyle.Flat;
        _refreshButton.BackColor = PharmaTheme.Primary;
        _refreshButton.ForeColor = PharmaTheme.OnPrimary;
        _refreshButton.Font = PharmaTheme.ArabicFont(10f, FontStyle.Bold);
        _refreshButton.Cursor = Cursors.Hand;
        _refreshButton.Click += (_, _) => RefreshRequested?.Invoke(this, EventArgs.Empty);

        _exportButton.Text = "تصدير";
        _exportButton.AutoSize = true;
        _exportButton.FlatStyle = FlatStyle.Flat;
        _exportButton.BackColor = PharmaTheme.SurfaceContainerHigh;
        _exportButton.ForeColor = PharmaTheme.TextDark;
        _exportButton.Font = PharmaTheme.ArabicFont(10f, FontStyle.Bold);
        _exportButton.Cursor = Cursors.Hand;
        _exportButton.Click += (_, _) => ExportRequested?.Invoke(this, EventArgs.Empty);

        var footer = new Panel { Height = 44, Dock = DockStyle.Bottom, BackColor = PharmaTheme.Surface };
        footer.Controls.Add(_exportButton);
        footer.Controls.Add(_refreshButton);
        footer.Resize += (_, _) =>
        {
            _refreshButton.Location = new Point(footer.Width - _refreshButton.Width - 8, 8);
            _exportButton.Location = new Point(_refreshButton.Left - _exportButton.Width - 8, 8);
        };

        Controls.Add(_contentPanel);
        Controls.Add(_statusLabel);
        Controls.Add(_periodLabel);
        Controls.Add(_titleLabel);
        Controls.Add(footer);
        Controls.Add(_closeButton);
        Resize += (_, _) => _closeButton.Location = new Point(12, 12);
    }

    public event EventHandler? CloseRequested;
    public event EventHandler? RefreshRequested;
    public event EventHandler? ExportRequested;

    public ReportKind? ActiveKind => _card?.Kind;

    public ReportLoadResult? LastLoadedResult => _lastLoadedResult;

    public void SetExportBusy(bool busy)
    {
        _exportButton.Enabled = !busy;
        _exportButton.Text = busy ? "جارٍ التصدير..." : "تصدير";
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
        _titleLabel.ForeColor = PharmaTheme.TextDark;
        _periodLabel.ForeColor = PharmaTheme.OnSurfaceVariant;
        _closeButton.ForeColor = PharmaTheme.OnSurfaceVariant;
        _contentPanel.BackColor = PharmaTheme.Surface;
        Invalidate(true);
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
