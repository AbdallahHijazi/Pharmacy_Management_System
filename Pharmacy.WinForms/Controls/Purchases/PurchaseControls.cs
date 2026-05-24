using System.ComponentModel;
using System.Drawing.Drawing2D;
using Pharmacy.WinForms.Controls;
using Pharmacy.WinForms.Models;
using Pharmacy.WinForms.Ui;

namespace Pharmacy.WinForms.Controls.Purchases;

internal class PurRoundedPanel : Panel
{
    private readonly int _radius;
    private readonly bool _drawShadow;

    public PurRoundedPanel(int radius = PharmaTheme.PurchasesCardCornerRadius, bool drawShadow = true)
    {
        _radius = radius;
        _drawShadow = drawShadow;
        DoubleBuffered = true;
        BackColor = FillColor;
        SetStyle(
            ControlStyles.AllPaintingInWmPaint
                | ControlStyles.UserPaint
                | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.ResizeRedraw,
            true);
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color FillColor { get; set; } = PharmaTheme.Surface;

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color BorderColor { get; set; } = PharmaTheme.BorderSoft;

    public void ApplyThemeVisuals()
    {
        BackColor = FillColor;
        Invalidate(true);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(Parent?.BackColor ?? PharmaTheme.Background);

        var bounds = ClientRectangle;
        bounds.Inflate(-1, -1);
        if (bounds.Width <= 4 || bounds.Height <= 4)
        {
            return;
        }

        if (_drawShadow)
        {
            RoundedDrawing.DrawSoftShadow(g, bounds, _radius, PharmaTheme.DashboardCardShadow);
        }

        RoundedDrawing.FillRounded(g, bounds, _radius, FillColor);
        RoundedDrawing.DrawRoundedBorder(
            g,
            bounds,
            _radius,
            PharmaTheme.WithAlpha(BorderColor, 90),
            1f);
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        e.Graphics.Clear(Parent?.BackColor ?? PharmaTheme.Background);
    }
}

internal sealed class PurSearchBox : UserControl
{
    private TextBox? _box;
    private bool _focused;

    public PurSearchBox()
    {
        SetStyle(
            ControlStyles.AllPaintingInWmPaint
                | ControlStyles.UserPaint
                | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.ResizeRedraw,
            true);
        DoubleBuffered = true;
        Height = 52;
        MinimumSize = new Size(200, 52);
        Padding = new Padding(44, 0, 16, 0);

        _box = new TextBox
        {
            BorderStyle = BorderStyle.None,
            Font = PharmaTheme.ArabicFont(11f),
            BackColor = PharmaTheme.InputSurface,
            ForeColor = PharmaTheme.TextDark,
            RightToLeft = RightToLeft.Yes,
            TextAlign = HorizontalAlignment.Right
        };
        _box.GotFocus += (_, _) => { _focused = true; Invalidate(); };
        _box.LostFocus += (_, _) => { _focused = false; Invalidate(); };
        _box.TextChanged += (_, _) => SearchTextChanged?.Invoke(this, EventArgs.Empty);
        Controls.Add(_box);
    }

    public event EventHandler? SearchTextChanged;

#pragma warning disable CS8765, CS8764
    public override string? Text
    {
        get => _box?.Text ?? string.Empty;
        set
        {
            if (_box is not null)
            {
                _box.Text = value ?? string.Empty;
            }
        }
    }
#pragma warning restore CS8765, CS8764

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string PlaceholderText
    {
        get => _box?.PlaceholderText ?? string.Empty;
        set
        {
            if (_box is not null)
            {
                _box.PlaceholderText = value;
            }
        }
    }

    public void ApplyThemeVisuals()
    {
        if (_box is null)
        {
            return;
        }

        _box.BackColor = PharmaTheme.InputSurface;
        _box.ForeColor = PharmaTheme.TextDark;
        _box.Font = PharmaTheme.ArabicFont(11f);
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var r = ClientRectangle;
        r.Inflate(-1, -1);
        RoundedDrawing.FillRounded(g, r, PharmaTheme.PurchasesSearchCornerRadius, PharmaTheme.InputSurface);
        RoundedDrawing.DrawRoundedBorder(
            g,
            r,
            PharmaTheme.PurchasesSearchCornerRadius,
            _focused ? PharmaTheme.Primary : PharmaTheme.BorderSoft,
            _focused ? 1.75f : 1f);

        var iconRect = new Rectangle(r.X + 10, r.Y + (r.Height - 28) / 2, 28, 28);
        TextRenderer.DrawText(
            g,
            SegoeMdl2Icons.Search,
            PharmaTheme.IconFont(14f),
            iconRect,
            PharmaTheme.OnSurfaceVariant,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
    }

    protected override void OnLayout(LayoutEventArgs levent)
    {
        base.OnLayout(levent);
        if (_box is null || _box.IsDisposed)
        {
            return;
        }

        var innerH = Math.Max(24, ClientSize.Height - 8);
        _box.SetBounds(
            Padding.Left,
            (ClientSize.Height - innerH) / 2,
            Math.Max(40, ClientSize.Width - Padding.Horizontal),
            innerH);
    }
}

internal sealed class PurOutlineButton : Control
{
    public PurOutlineButton()
    {
        Height = 52;
        MinimumSize = new Size(120, 52);
        Cursor = Cursors.Hand;
        SetStyle(
            ControlStyles.AllPaintingInWmPaint
                | ControlStyles.UserPaint
                | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.StandardClick,
            true);
    }

    public void ApplyThemeVisuals() => Invalidate();

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var b = ClientRectangle;
        b.Inflate(-1, -1);
        RoundedDrawing.FillRounded(g, b, 12, PharmaTheme.Surface);
        RoundedDrawing.DrawRoundedBorder(g, b, 12, PharmaTheme.BorderSoft);
        TextRenderer.DrawText(
            g,
            Text,
            PharmaTheme.ArabicFont(10f, FontStyle.Bold),
            b,
            PharmaTheme.TextDark,
            TextFormatFlags.HorizontalCenter
                | TextFormatFlags.VerticalCenter
                | TextFormatFlags.RightToLeft
                | TextFormatFlags.EndEllipsis);
    }
}

internal sealed class PurIconFilterButton : Control
{
    public PurIconFilterButton()
    {
        Size = new Size(52, 52);
        MinimumSize = new Size(52, 52);
        Cursor = Cursors.Hand;
        SetStyle(
            ControlStyles.AllPaintingInWmPaint
                | ControlStyles.UserPaint
                | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.StandardClick,
            true);
    }

    public void ApplyThemeVisuals() => Invalidate();

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var b = ClientRectangle;
        b.Inflate(-1, -1);
        RoundedDrawing.FillRounded(g, b, 12, PharmaTheme.SurfaceContainerHigh);
        RoundedDrawing.DrawRoundedBorder(g, b, 12, PharmaTheme.BorderSoft);
        TextRenderer.DrawText(
            g,
            SegoeMdl2Icons.Filter,
            PharmaTheme.IconFont(14f),
            b,
            PharmaTheme.Primary,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
    }
}

internal sealed class PurPurchaseInvoiceCard : Control
{
    private readonly PurchaseInvoiceListItemView _invoice;
    private bool _hover;
    private Rectangle _viewDetailsRect;
    private Rectangle _printRect;

    public PurPurchaseInvoiceCard(PurchaseInvoiceListItemView invoice)
    {
        _invoice = invoice;
        Height = PharmaTheme.PurchasesInvoiceCardHeight;
        Dock = DockStyle.Top;
        Margin = new Padding(0, 0, 0, 16);
        Cursor = Cursors.Hand;
        RightToLeft = RightToLeft.Yes;
        SetStyle(
            ControlStyles.AllPaintingInWmPaint
                | ControlStyles.UserPaint
                | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.StandardClick,
            true);
    }

    public PurchaseInvoiceListItemView Invoice => _invoice;

    public event EventHandler? ViewDetailsRequested;
    public event EventHandler? PrintRequested;

    public void ApplyThemeVisuals() => Invalidate();

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        Invalidate();
    }

    protected override void OnPaintBackground(PaintEventArgs e) =>
        e.Graphics.Clear(Parent?.BackColor ?? PharmaTheme.Background);

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

    protected override void OnClick(EventArgs e)
    {
        base.OnClick(e);
        var pt = PointToClient(Cursor.Position);
        if (_printRect.Contains(pt))
        {
            PrintRequested?.Invoke(this, EventArgs.Empty);
            return;
        }

        ViewDetailsRequested?.Invoke(this, EventArgs.Empty);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
        g.Clear(Parent?.BackColor ?? PharmaTheme.Background);

        var bounds = ClientRectangle;
        bounds.Inflate(-1, -1);
        var fill = _hover ? PharmaTheme.SurfaceContainerLow : PharmaTheme.Surface;
        RoundedDrawing.FillRounded(g, bounds, PharmaTheme.PurchasesCardCornerRadius, fill);
        RoundedDrawing.DrawRoundedBorder(g, bounds, PharmaTheme.PurchasesCardCornerRadius, PharmaTheme.BorderSoft, 1f);

        var accent = new Rectangle(bounds.Right - 5, bounds.Y + 12, 4, bounds.Height - 24);
        RoundedDrawing.FillRounded(g, accent, 2, PharmaTheme.Primary);

        var layout = BuildLayout(bounds);
        _viewDetailsRect = layout.ViewDetailsRect;
        _printRect = layout.PrintRect;

        DrawIconBlock(g, layout.IconRect);
        DrawInvoiceBlock(g, layout.InvoiceRect);
        DrawSupplierBlock(g, layout.SupplierRect);
        DrawTotalBlock(g, layout.TotalRect);
        DrawPaidBlock(g, layout.PaidRect);
        DrawStatusBadge(g, layout.StatusRect);
        DrawActions(g, layout.ViewDetailsRect, layout.PrintRect);
    }

    private void DrawIconBlock(Graphics g, Rectangle rect)
    {
        RoundedDrawing.FillRounded(g, rect, 12, PharmaTheme.PrimaryContainer);
        TextRenderer.DrawText(
            g,
            SegoeMdl2Icons.Receipt,
            PharmaTheme.IconFont(18f),
            rect,
            PharmaTheme.Primary,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
    }

    private void DrawInvoiceBlock(Graphics g, Rectangle rect)
    {
        DrawCaption(g, rect, "رقم الفاتورة", 0, 18);
        DrawValue(g, rect, _invoice.InvoiceNumber, 20, PharmaTheme.ArabicFont(12f, FontStyle.Bold), PharmaTheme.TextDark);
        DrawValue(g, rect, _invoice.FormattedDate, 44, PharmaTheme.SmallFont, PharmaTheme.OnSurfaceVariant);
    }

    private void DrawSupplierBlock(Graphics g, Rectangle rect)
    {
        DrawCaption(g, rect, "المورد", 0, 18);
        DrawValue(g, rect, ResolveSupplierDisplayName(_invoice.SupplierName), 20, PharmaTheme.ArabicFont(11f, FontStyle.Bold), PharmaTheme.TextDark);
        if (_invoice.ItemsCount.HasValue)
        {
            DrawValue(g, rect, $"عدد الأصناف: {_invoice.ItemsCount.Value:N0}", 44, PharmaTheme.SmallFont, PharmaTheme.OnSurfaceVariant);
        }
    }

    private void DrawTotalBlock(Graphics g, Rectangle rect)
    {
        DrawCaption(g, rect, "الإجمالي", 0, 18);
        DrawValue(
            g,
            rect,
            PosFormatting.FormatMoneyCompact(_invoice.GrandTotal),
            22,
            PharmaTheme.NumberFont(12f, FontStyle.Bold),
            PharmaTheme.TextDark);
    }

    private void DrawPaidBlock(Graphics g, Rectangle rect)
    {
        DrawCaption(g, rect, "المدفوع / المتبقي", 0, 18);
        var paid = PosFormatting.FormatMoneyCompact(_invoice.PaidAmount);
        var remaining = PosFormatting.FormatMoneyCompact(_invoice.RemainingAmount);
        var remainingColor = _invoice.RemainingAmount > 0
            ? (_invoice.StatusKind == PurchaseInvoiceStatusKind.PartiallyPaid
                ? PharmaTheme.WarningStrong
                : PharmaTheme.Danger)
            : PharmaTheme.TextDark;
        var text = $"{paid} / {remaining}";
        DrawValue(g, rect, text, 22, PharmaTheme.NumberFont(10.5f, FontStyle.Bold), remainingColor);
    }

    private void DrawStatusBadge(Graphics g, Rectangle rect)
    {
        var (back, fore) = _invoice.StatusKind switch
        {
            PurchaseInvoiceStatusKind.Paid => (PharmaTheme.SuccessSurface, PharmaTheme.Success),
            PurchaseInvoiceStatusKind.PartiallyPaid => (PharmaTheme.WarningSurface, PharmaTheme.WarningStrong),
            PurchaseInvoiceStatusKind.Unpaid => (PharmaTheme.ErrorContainer, PharmaTheme.Danger),
            PurchaseInvoiceStatusKind.Cancelled => (PharmaTheme.SurfaceContainerHigh, PharmaTheme.OnSurfaceVariant),
            _ => (PharmaTheme.SurfaceContainerLow, PharmaTheme.OnSurfaceVariant)
        };

        var label = _invoice.DisplayStatus;
        var size = TextRenderer.MeasureText(label, PharmaTheme.ArabicFont(9f, FontStyle.Bold));
        var badgeW = Math.Min(rect.Width, size.Width + 20);
        var badgeH = 26;
        var badgeRect = new Rectangle(rect.X + (rect.Width - badgeW) / 2, rect.Y + 8, badgeW, badgeH);
        RoundedDrawing.FillRounded(g, badgeRect, badgeH / 2, back);
        TextRenderer.DrawText(
            g,
            label,
            PharmaTheme.ArabicFont(9f, FontStyle.Bold),
            badgeRect,
            fore,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
    }

    private static void DrawActions(Graphics g, Rectangle viewRect, Rectangle printRect)
    {
        TextRenderer.DrawText(
            g,
            "عرض التفاصيل",
            PharmaTheme.ArabicFont(10f, FontStyle.Bold),
            viewRect,
            PharmaTheme.Primary,
            TextFormatFlags.Right | TextFormatFlags.VerticalCenter);

        TextRenderer.DrawText(
            g,
            "طباعة",
            PharmaTheme.SmallFont,
            printRect,
            PharmaTheme.MutedText,
            TextFormatFlags.Right | TextFormatFlags.VerticalCenter);
    }

    private static void DrawCaption(Graphics g, Rectangle area, string text, int y, int height)
    {
        var rect = new Rectangle(area.X, area.Y + y, area.Width, height);
        TextRenderer.DrawText(
            g,
            text,
            PharmaTheme.SmallFont,
            rect,
            PharmaTheme.OnSurfaceVariant,
            TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
    }

    private static void DrawValue(Graphics g, Rectangle area, string text, int y, Font font, Color color)
    {
        var rect = new Rectangle(area.X, area.Y + y, area.Width, 22);
        TextRenderer.DrawText(
            g,
            text,
            font,
            rect,
            color,
            TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
    }

    private static string ResolveSupplierDisplayName(string? rawName) =>
        PurchaseDisplayHelper.ResolveSupplierDisplayName(rawName);

    private CardLayout BuildLayout(Rectangle bounds)
    {
        const int pad = 16;
        const int gap = 10;
        var actionsW = 96;
        var statusW = 92;
        var paidW = 128;
        var totalW = 108;
        var iconSize = 48;

        var available = bounds.Width - pad * 2 - iconSize - actionsW - gap * 6;
        var invoiceW = Math.Max(96, (int)(available * 0.22));
        var supplierW = Math.Max(96, (int)(available * 0.28));
        var used = invoiceW + supplierW + totalW + paidW + statusW;
        if (used > available && available > 0)
        {
            var scale = available / (double)used;
            invoiceW = Math.Max(88, (int)(invoiceW * scale));
            supplierW = Math.Max(88, (int)(supplierW * scale));
            totalW = Math.Max(80, (int)(totalW * scale));
            paidW = Math.Max(88, (int)(paidW * scale));
            statusW = Math.Max(72, (int)(statusW * scale));
        }

        var x = bounds.Right - pad - iconSize;
        var iconRect = new Rectangle(x, bounds.Y + (bounds.Height - iconSize) / 2, iconSize, iconSize);
        x -= invoiceW + gap;
        var invoiceRect = new Rectangle(x, bounds.Y + 12, invoiceW, bounds.Height - 24);
        x -= supplierW + gap;
        var supplierRect = new Rectangle(x, bounds.Y + 12, supplierW, bounds.Height - 24);
        x -= totalW + gap;
        var totalRect = new Rectangle(x, bounds.Y + 12, totalW, bounds.Height - 24);
        x -= paidW + gap;
        var paidRect = new Rectangle(x, bounds.Y + 12, paidW, bounds.Height - 24);
        x -= statusW + gap;
        var statusRect = new Rectangle(x, bounds.Y + 12, statusW, bounds.Height - 24);

        var actionsX = bounds.X + pad;
        var viewDetailsRect = new Rectangle(actionsX, bounds.Y + 28, actionsW, 28);
        var printRect = new Rectangle(actionsX, bounds.Y + 58, actionsW, 24);

        return new CardLayout(iconRect, invoiceRect, supplierRect, totalRect, paidRect, statusRect, viewDetailsRect, printRect);
    }

    private readonly record struct CardLayout(
        Rectangle IconRect,
        Rectangle InvoiceRect,
        Rectangle SupplierRect,
        Rectangle TotalRect,
        Rectangle PaidRect,
        Rectangle StatusRect,
        Rectangle ViewDetailsRect,
        Rectangle PrintRect);
}

internal sealed class PurPaginationBar : PurRoundedPanel
{
    private readonly Label _prevButton = new();
    private readonly Label _nextButton = new();
    private readonly FlowLayoutPanel _pagesPanel = new();
    private int _currentPage = 1;
    private int _totalPages = 1;

    public PurPaginationBar() : base(PharmaTheme.PurchasesCardCornerRadius, drawShadow: true)
    {
        Height = 56;
        FillColor = PharmaTheme.Surface;
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);

        _prevButton.Text = "السابق";
        _prevButton.AutoSize = true;
        _prevButton.Cursor = Cursors.Hand;
        _prevButton.Font = PharmaTheme.SmallFont;
        _prevButton.Click += (_, _) => PageChangeRequested?.Invoke(this, Math.Max(1, _currentPage - 1));

        _nextButton.Text = "التالي";
        _nextButton.AutoSize = true;
        _nextButton.Cursor = Cursors.Hand;
        _nextButton.Font = PharmaTheme.SmallFont;
        _nextButton.Click += (_, _) => PageChangeRequested?.Invoke(this, Math.Min(_totalPages, _currentPage + 1));

        _pagesPanel.FlowDirection = FlowDirection.RightToLeft;
        _pagesPanel.WrapContents = false;
        _pagesPanel.AutoSize = true;
        _pagesPanel.BackColor = Color.Transparent;

        Controls.Add(_pagesPanel);
        Controls.Add(_nextButton);
        Controls.Add(_prevButton);
        Resize += (_, _) => LayoutBar();
    }

    public event EventHandler<int>? PageChangeRequested;

    public void Update(int currentPage, int totalPages)
    {
        _currentPage = Math.Max(1, currentPage);
        _totalPages = Math.Max(1, totalPages);
        RebuildPageButtons();
        LayoutBar();
        Invalidate();
    }

    public new void ApplyThemeVisuals()
    {
        FillColor = PharmaTheme.Surface;
        _prevButton.ForeColor = PharmaTheme.Primary;
        _nextButton.ForeColor = PharmaTheme.Primary;
        _prevButton.Font = PharmaTheme.SmallFont;
        _nextButton.Font = PharmaTheme.SmallFont;
        base.ApplyThemeVisuals();
    }

    private void RebuildPageButtons()
    {
        _pagesPanel.Controls.Clear();
        if (_totalPages <= 1)
        {
            return;
        }

        var start = Math.Max(1, _currentPage - 2);
        var end = Math.Min(_totalPages, start + 4);
        start = Math.Max(1, end - 4);

        for (var page = start; page <= end; page++)
        {
            var p = page;
            var btn = new PurPageChip(p, p == _currentPage);
            btn.Click += (_, _) => PageChangeRequested?.Invoke(this, p);
            _pagesPanel.Controls.Add(btn);
        }
    }

    private void LayoutBar()
    {
        var centerY = (Height - 32) / 2;
        _prevButton.Location = new Point(16, centerY);
        _pagesPanel.Location = new Point((Width - _pagesPanel.PreferredSize.Width) / 2, centerY - 2);
        _nextButton.Location = new Point(Width - _nextButton.PreferredWidth - 16, centerY);
    }
}

internal sealed class PurPageChip : Control
{
    private readonly bool _selected;

    public PurPageChip(int page, bool selected)
    {
        _selected = selected;
        Text = page.ToString();
        Size = new Size(32, 32);
        Cursor = Cursors.Hand;
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.StandardClick, true);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var b = ClientRectangle;
        b.Inflate(-1, -1);
        var back = _selected ? PharmaTheme.Primary : Color.Transparent;
        var text = _selected ? PharmaTheme.OnPrimary : PharmaTheme.OnSurfaceVariant;
        if (_selected)
        {
            RoundedDrawing.FillRounded(g, b, 10, back);
        }

        TextRenderer.DrawText(
            g,
            Text,
            PharmaTheme.NumberFont(10f, FontStyle.Bold),
            b,
            text,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
    }
}

internal sealed class PurInvoiceDetailsPanel : PurRoundedPanel
{
    private PurchaseInvoiceDetailsView? _details;
    private readonly Label _closeButton = new();
    private readonly Panel _contentPanel = new();

    public PurInvoiceDetailsPanel() : base(PharmaTheme.PurchasesCardCornerRadius)
    {
        FillColor = PharmaTheme.Surface;
        Visible = false;
        Width = PharmaTheme.PurchasesDetailsWidth;
        RightToLeft = RightToLeft.Yes;

        _closeButton.Text = SegoeMdl2Icons.Close;
        _closeButton.Font = PharmaTheme.IconFont(11f);
        _closeButton.AutoSize = true;
        _closeButton.Cursor = Cursors.Hand;
        _closeButton.Click += (_, _) => CloseRequested?.Invoke(this, EventArgs.Empty);

        _contentPanel.AutoScroll = true;
        _contentPanel.BackColor = PharmaTheme.Surface;
        _contentPanel.Dock = DockStyle.Fill;

        Controls.Add(_contentPanel);
        Controls.Add(_closeButton);
        Resize += (_, _) =>
        {
            if (_details is not null && Visible)
            {
                Render();
            }
        };
    }

    public event EventHandler? CloseRequested;

    public void Bind(PurchaseInvoiceDetailsView? details)
    {
        _details = details;
        Visible = details is not null;
        Render();
    }

    public new void ApplyThemeVisuals()
    {
        FillColor = PharmaTheme.Surface;
        _closeButton.ForeColor = PharmaTheme.OnSurfaceVariant;
        _closeButton.Font = PharmaTheme.IconFont(11f);
        base.ApplyThemeVisuals();
        Render();
    }

    private void Render()
    {
        _contentPanel.Controls.Clear();
        if (_details is null)
        {
            return;
        }

        var contentW = Math.Max(280, ClientSize.Width - 24);
        var host = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            BackColor = Color.Transparent,
            RightToLeft = RightToLeft.Yes,
            Width = contentW,
            Padding = new Padding(8, 36, 8, 12)
        };

        host.Controls.Add(MakeTitle(_details.Summary.InvoiceNumber));
        host.Controls.Add(MakeRow("المورد", _details.Summary.SupplierName, contentW));
        host.Controls.Add(MakeRow("التاريخ", _details.Summary.FormattedDate, contentW));
        host.Controls.Add(MakeRow("الحالة", _details.Summary.DisplayStatus, contentW));
        host.Controls.Add(MakeRow("الإجمالي", PosFormatting.FormatMoneyCompact(_details.Summary.GrandTotal), contentW));
        host.Controls.Add(MakeRow("المدفوع", PosFormatting.FormatMoneyCompact(_details.Summary.PaidAmount), contentW));
        host.Controls.Add(MakeRow("المتبقي", PosFormatting.FormatMoneyCompact(_details.Summary.RemainingAmount), contentW));

        if (_details.Lines.Count > 0)
        {
            host.Controls.Add(MakeSectionTitle("البنود"));
            foreach (var line in _details.Lines)
            {
                host.Controls.Add(MakeLineCard(line, contentW));
            }
        }
        else
        {
            host.Controls.Add(MakeMuted("لا توجد بنود متاحة"));
        }

        _contentPanel.Controls.Add(host);
    }

    private static Control MakeTitle(string text) =>
        new Label
        {
            Text = text,
            AutoSize = true,
            Font = PharmaTheme.ArabicFont(16f, FontStyle.Bold),
            ForeColor = PharmaTheme.TextDark,
            Margin = new Padding(0, 0, 0, 12)
        };

    private static Control MakeSectionTitle(string text) =>
        new Label
        {
            Text = text,
            AutoSize = true,
            Font = PharmaTheme.ArabicFont(11f, FontStyle.Bold),
            ForeColor = PharmaTheme.TextDark,
            Margin = new Padding(0, 12, 0, 6)
        };

    private static Control MakeRow(string caption, string value, int width)
    {
        var panel = new Panel { Height = 30, Width = width, Margin = new Padding(0, 0, 0, 4), BackColor = Color.Transparent };
        var cap = new Label
        {
            Text = caption,
            Dock = DockStyle.Right,
            Width = 112,
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

    private static Control MakeLineCard(PurchaseInvoiceLineView line, int width)
    {
        var card = new PurRoundedPanel(10, drawShadow: false)
        {
            FillColor = PharmaTheme.SurfaceAlt,
            BorderColor = PharmaTheme.BorderSoft,
            Width = width,
            Height = 52,
            Margin = new Padding(0, 0, 0, 6)
        };
        var text = new Label
        {
            Text =
                $"{line.ProductName}{Environment.NewLine}" +
                $"الكمية: {line.Quantity}  •  {PosFormatting.FormatMoneyCompact(line.UnitPrice)}  •  {PosFormatting.FormatMoneyCompact(line.LineTotal)}",
            Dock = DockStyle.Fill,
            ForeColor = PharmaTheme.TextDark,
            Font = PharmaTheme.SmallFont,
            TextAlign = ContentAlignment.MiddleRight,
            Padding = new Padding(10, 6, 10, 6)
        };
        card.Controls.Add(text);
        return card;
    }

    private static Control MakeMuted(string text) =>
        new Label
        {
            Text = text,
            AutoSize = true,
            ForeColor = PharmaTheme.OnSurfaceVariant,
            Font = PharmaTheme.BodyFont,
            Margin = new Padding(0, 8, 0, 0)
        };
}
