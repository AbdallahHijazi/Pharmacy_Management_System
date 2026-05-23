using System.ComponentModel;
using System.Drawing.Drawing2D;
using Pharmacy.WinForms.Models;
using Pharmacy.WinForms.Ui;

namespace Pharmacy.WinForms.Controls.Pos;

internal class PosRoundedPanel : Panel
{
    private readonly int _radius;
    private readonly bool _drawShadow;

    public PosRoundedPanel(int radius = PharmaTheme.PosCardCornerRadius, bool drawShadow = true)
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

internal sealed class PosCategoryChip : Control
{
    private bool _selected;

    public PosCategoryChip(string caption)
    {
        Text = caption;
        Height = 40;
        MinimumSize = new Size(72, 40);
        Cursor = Cursors.Hand;
        SetStyle(
            ControlStyles.AllPaintingInWmPaint
                | ControlStyles.UserPaint
                | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.StandardClick,
            true);
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool IsSelected
    {
        get => _selected;
        set
        {
            _selected = value;
            Invalidate();
        }
    }

    public void ApplyThemeVisuals() => Invalidate();

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var b = ClientRectangle;
        b.Inflate(-1, -1);
        var back = _selected ? PharmaTheme.Primary : PharmaTheme.SurfaceContainerHigh;
        var text = _selected ? PharmaTheme.OnPrimary : PharmaTheme.OnSurfaceVariant;
        var radius = Math.Min(b.Height / 2, 20);
        RoundedDrawing.FillRounded(g, b, radius, back);
        if (!_selected)
        {
            RoundedDrawing.DrawRoundedBorder(g, b, radius, PharmaTheme.BorderSoft);
        }

        TextRenderer.DrawText(
            g,
            Text,
            PharmaTheme.SmallFont,
            b,
            text,
            TextFormatFlags.HorizontalCenter
                | TextFormatFlags.VerticalCenter
                | TextFormatFlags.RightToLeft);
    }
}

internal sealed class PosPaymentButton : Control
{
    private bool _selected;

    public PosPaymentButton(string caption)
    {
        Text = caption;
        Height = 44;
        MinimumSize = new Size(72, 44);
        Cursor = Cursors.Hand;
        SetStyle(
            ControlStyles.AllPaintingInWmPaint
                | ControlStyles.UserPaint
                | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.StandardClick,
            true);
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool IsSelected
    {
        get => _selected;
        set
        {
            _selected = value;
            Invalidate();
        }
    }

    public void ApplyThemeVisuals() => Invalidate();

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var b = ClientRectangle;
        b.Inflate(-1, -1);
        var back = _selected ? PharmaTheme.PrimaryContainer : PharmaTheme.Surface;
        var text = _selected ? PharmaTheme.Primary : PharmaTheme.TextDark;
        RoundedDrawing.FillRounded(g, b, 12, back);
        RoundedDrawing.DrawRoundedBorder(
            g,
            b,
            12,
            _selected ? PharmaTheme.Primary : PharmaTheme.BorderSoft,
            _selected ? 2f : 1f);
        TextRenderer.DrawText(
            g,
            Text,
            PharmaTheme.ArabicFont(10f, FontStyle.Bold),
            b,
            text,
            TextFormatFlags.HorizontalCenter
                | TextFormatFlags.VerticalCenter
                | TextFormatFlags.RightToLeft);
    }
}

internal sealed class PosSearchBox : UserControl
{
    private readonly TextBox _box;
    private bool _focused;

    public PosSearchBox()
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
        RightToLeft = RightToLeft.No;

        _box = new TextBox
        {
            BorderStyle = BorderStyle.None,
            Dock = DockStyle.Fill,
            Font = PharmaTheme.ArabicFont(11f),
            BackColor = PharmaTheme.InputSurface,
            ForeColor = PharmaTheme.TextDark,
            RightToLeft = RightToLeft.Yes,
            TextAlign = HorizontalAlignment.Right
        };
        _box.GotFocus += (_, _) => { _focused = true; Invalidate(); };
        _box.LostFocus += (_, _) => { _focused = false; Invalidate(); };
        _box.TextChanged += (_, _) => SearchTextChanged?.Invoke(this, EventArgs.Empty);
        _box.KeyDown += (_, e) => SearchKeyDown?.Invoke(this, e);
        Controls.Add(_box);
    }

    public event EventHandler? SearchTextChanged;
    public event KeyEventHandler? SearchKeyDown;

#pragma warning disable CS8765, CS8764
    public override string? Text
    {
        get => _box.Text;
        set => _box.Text = value ?? string.Empty;
    }
#pragma warning restore CS8765, CS8764

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string PlaceholderText
    {
        get => _box.PlaceholderText;
        set => _box.PlaceholderText = value;
    }

    public void ApplyThemeVisuals()
    {
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
        RoundedDrawing.FillRounded(g, r, PharmaTheme.PosSearchCornerRadius, PharmaTheme.InputSurface);
        RoundedDrawing.DrawRoundedBorder(
            g,
            r,
            PharmaTheme.PosSearchCornerRadius,
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
        if (_box.IsDisposed)
        {
            return;
        }

        var innerH = Math.Max(24, ClientSize.Height - 8);
        _box.SetBounds(Padding.Left, (ClientSize.Height - innerH) / 2, Math.Max(40, ClientSize.Width - Padding.Horizontal), innerH);
    }
}

internal sealed class PosProductCard : Control
{
    private readonly PosProductView _product;
    private bool _hover;

    public PosProductCard(PosProductView product)
    {
        _product = product;
        Height = PharmaTheme.PosProductCardHeight;
        MinimumSize = new Size(180, PharmaTheme.PosProductCardHeight);
        Cursor = Cursors.Hand;
        RightToLeft = RightToLeft.Yes;
        SetStyle(
            ControlStyles.AllPaintingInWmPaint
                | ControlStyles.UserPaint
                | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.StandardClick,
            true);
    }

    public PosProductView Product => _product;

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

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var bounds = ClientRectangle;
        bounds.Inflate(-2, -2);

        var fill = _hover ? PharmaTheme.SurfaceContainerHigh : PharmaTheme.Surface;
        RoundedDrawing.DrawSoftShadow(g, bounds, PharmaTheme.PosCardCornerRadius, PharmaTheme.DashboardCardShadow);
        RoundedDrawing.FillRounded(g, bounds, PharmaTheme.PosCardCornerRadius, fill);
        if (_product.IsLowStock)
        {
            RoundedDrawing.DrawRoundedBorder(g, bounds, PharmaTheme.PosCardCornerRadius, PharmaTheme.ErrorContainer, 1.5f);
        }
        else
        {
            RoundedDrawing.DrawRoundedBorder(g, bounds, PharmaTheme.PosCardCornerRadius, PharmaTheme.BorderSoft);
        }

        if (_product.ShowRxBadge)
        {
            var badge = new Rectangle(bounds.X + 10, bounds.Y + 10, 34, 20);
            RoundedDrawing.FillRounded(g, badge, 6, PharmaTheme.PrimaryContainer);
            TextRenderer.DrawText(
                g,
                "RX",
                PharmaTheme.NumberFont(8f, FontStyle.Bold),
                badge,
                PharmaTheme.Primary,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }

        var textW = bounds.Width - 24;
        var nameRect = new Rectangle(bounds.X + 12, bounds.Y + 16, textW, 28);
        TextRenderer.DrawText(
            g,
            _product.Name,
            PharmaTheme.ArabicFont(11.5f, FontStyle.Bold),
            nameRect,
            PharmaTheme.TextDark,
            TextFormatFlags.Right
                | TextFormatFlags.EndEllipsis
                | TextFormatFlags.VerticalCenter
                | TextFormatFlags.RightToLeft);

        var sciRect = new Rectangle(bounds.X + 12, nameRect.Bottom + 2, textW, 22);
        TextRenderer.DrawText(
            g,
            string.IsNullOrWhiteSpace(_product.ScientificName) ? " " : _product.ScientificName,
            PharmaTheme.SmallFont,
            sciRect,
            PharmaTheme.OnSurfaceVariant,
            TextFormatFlags.Right
                | TextFormatFlags.EndEllipsis
                | TextFormatFlags.VerticalCenter
                | TextFormatFlags.RightToLeft);

        var priceText = PosFormatting.FormatMoneyCompact(_product.SellingPrice);
        var priceRect = new Rectangle(bounds.X + 12, bounds.Bottom - 44, textW / 2 + 40, 28);
        TextRenderer.DrawText(
            g,
            priceText,
            PharmaTheme.NumberFont(14f, FontStyle.Bold),
            priceRect,
            PharmaTheme.Primary,
            TextFormatFlags.Right | TextFormatFlags.VerticalCenter);

        var stockLabel = _product.IsOutOfStock ? "نفد" : $"Stock: {_product.SellableQuantity}";
        var stockBack = _product.IsLowStock || _product.IsOutOfStock
            ? PharmaTheme.ErrorContainer
            : PharmaTheme.SurfaceContainer;
        var stockFore = _product.IsLowStock || _product.IsOutOfStock
            ? PharmaTheme.Danger
            : PharmaTheme.OnSurfaceVariant;
        var stockSize = TextRenderer.MeasureText(g, stockLabel, PharmaTheme.SmallFont, Size.Empty, TextFormatFlags.NoPadding);
        var stockRect = new Rectangle(bounds.Right - stockSize.Width - 20, bounds.Bottom - 40, stockSize.Width + 12, 24);
        RoundedDrawing.FillRounded(g, stockRect, 8, stockBack);
        TextRenderer.DrawText(
            g,
            stockLabel,
            PharmaTheme.SmallFont,
            stockRect,
            stockFore,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
    }
}

internal sealed class PosCartItemControl : Control
{
    private readonly PosCartLine _line;
    public event EventHandler? IncreaseRequested;
    public event EventHandler? DecreaseRequested;
    public event EventHandler? RemoveRequested;

    public PosCartItemControl(PosCartLine line)
    {
        _line = line;
        Height = 72;
        MinimumSize = new Size(260, 72);
        RightToLeft = RightToLeft.Yes;
        SetStyle(
            ControlStyles.AllPaintingInWmPaint
                | ControlStyles.UserPaint
                | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.StandardClick,
            true);
    }

    public void ApplyThemeVisuals() => Invalidate();

    protected override void OnMouseClick(MouseEventArgs e)
    {
        base.OnMouseClick(e);
        var bounds = ClientRectangle;
        var qtyRect = new Rectangle(bounds.X + bounds.Width / 2 - 70, bounds.Y + 22, 140, 34);
        var removeRect = new Rectangle(bounds.X + 6, bounds.Y + 6, 22, 22);
        var plusRect = new Rectangle(qtyRect.Right - 30, qtyRect.Y + 4, 26, 26);
        var minusRect = new Rectangle(qtyRect.X + 4, qtyRect.Y + 4, 26, 26);

        if (removeRect.Contains(e.Location))
        {
            RemoveRequested?.Invoke(this, EventArgs.Empty);
            return;
        }

        if (plusRect.Contains(e.Location))
        {
            IncreaseRequested?.Invoke(this, EventArgs.Empty);
            return;
        }

        if (minusRect.Contains(e.Location))
        {
            DecreaseRequested?.Invoke(this, EventArgs.Empty);
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var bounds = ClientRectangle;
        bounds.Inflate(-2, -2);
        RoundedDrawing.FillRounded(g, bounds, 12, PharmaTheme.Surface);
        RoundedDrawing.DrawRoundedBorder(g, bounds, 12, PharmaTheme.BorderSoft);

        var removeRect = new Rectangle(bounds.X + 6, bounds.Y + 6, 22, 22);
        RoundedDrawing.FillRounded(g, removeRect, 11, PharmaTheme.Danger);
        TextRenderer.DrawText(
            g,
            SegoeMdl2Icons.Close,
            PharmaTheme.IconFont(9f),
            removeRect,
            PharmaTheme.OnPrimary,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

        var nameRect = new Rectangle(bounds.X + 36, bounds.Y + 10, bounds.Width / 2, 24);
        TextRenderer.DrawText(
            g,
            _line.Product.Name,
            PharmaTheme.ArabicFont(10f, FontStyle.Bold),
            nameRect,
            PharmaTheme.TextDark,
            TextFormatFlags.Right
                | TextFormatFlags.EndEllipsis
                | TextFormatFlags.VerticalCenter
                | TextFormatFlags.RightToLeft);

        var unitRect = new Rectangle(bounds.X + 36, nameRect.Bottom, bounds.Width / 2, 20);
        TextRenderer.DrawText(
            g,
            PosFormatting.FormatMoneyCompact(_line.Product.SellingPrice),
            PharmaTheme.SmallFont,
            unitRect,
            PharmaTheme.OnSurfaceVariant,
            TextFormatFlags.Right | TextFormatFlags.VerticalCenter);

        var qtyRect = new Rectangle(bounds.X + bounds.Width / 2 - 70, bounds.Y + 22, 140, 34);
        RoundedDrawing.FillRounded(g, qtyRect, 17, PharmaTheme.SurfaceContainer);
        var plusRect = new Rectangle(qtyRect.Right - 30, qtyRect.Y + 4, 26, 26);
        var minusRect = new Rectangle(qtyRect.X + 4, qtyRect.Y + 4, 26, 26);
        DrawCircleButton(g, plusRect, SegoeMdl2Icons.Add);
        DrawCircleButton(g, minusRect, SegoeMdl2Icons.Remove);
        var qtyTextRect = new Rectangle(minusRect.Right, qtyRect.Y, plusRect.X - minusRect.Right, qtyRect.Height);
        TextRenderer.DrawText(
            g,
            _line.Quantity.ToString(),
            PharmaTheme.NumberFont(10f, FontStyle.Bold),
            qtyTextRect,
            PharmaTheme.TextDark,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

        var totalRect = new Rectangle(bounds.Right - 90, bounds.Y + 20, 80, 32);
        TextRenderer.DrawText(
            g,
            PosFormatting.FormatMoneyCompact(_line.LineTotal),
            PharmaTheme.NumberFont(10.5f, FontStyle.Bold),
            totalRect,
            PharmaTheme.Primary,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
    }

    private static void DrawCircleButton(Graphics g, Rectangle rect, string glyph)
    {
        RoundedDrawing.FillRounded(g, rect, rect.Width / 2, PharmaTheme.Surface);
        RoundedDrawing.DrawRoundedBorder(g, rect, rect.Width / 2, PharmaTheme.BorderSoft);
        TextRenderer.DrawText(
            g,
            glyph,
            PharmaTheme.IconFont(10f),
            rect,
            PharmaTheme.Primary,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
    }
}
