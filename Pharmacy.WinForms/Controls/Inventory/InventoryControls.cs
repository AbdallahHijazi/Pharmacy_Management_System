using System.ComponentModel;
using System.Drawing.Drawing2D;
using Pharmacy.WinForms.Controls;
using Pharmacy.WinForms.Models;
using Pharmacy.WinForms.Ui;

namespace Pharmacy.WinForms.Controls.Inventory;

internal class InvRoundedPanel : Panel
{
    private readonly int _radius;
    private readonly bool _drawShadow;

    public InvRoundedPanel(int radius = PharmaTheme.InventoryCardCornerRadius, bool drawShadow = true)
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

internal sealed class InvSearchBox : UserControl
{
    private TextBox? _box;
    private bool _focused;

    public InvSearchBox()
    {
        SetStyle(
            ControlStyles.AllPaintingInWmPaint
                | ControlStyles.UserPaint
                | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.ResizeRedraw,
            true);
        DoubleBuffered = true;
        RightToLeft = RightToLeft.No;
        Height = 48;
        MinimumSize = new Size(200, 48);
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
        RoundedDrawing.FillRounded(g, r, PharmaTheme.InventorySearchCornerRadius, PharmaTheme.InputSurface);
        RoundedDrawing.DrawRoundedBorder(
            g,
            r,
            PharmaTheme.InventorySearchCornerRadius,
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

internal sealed class InvFilterChip : Control
{
    private bool _selected;

    public InvFilterChip(string caption)
    {
        Text = caption;
        Height = 36;
        MinimumSize = new Size(64, 36);
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
        var radius = Math.Min(b.Height / 2, 18);
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

internal sealed class InvFilterToggleButton : Control
{
    public InvFilterToggleButton()
    {
        Text = "تصفية";
        Height = 40;
        Width = 96;
        MinimumSize = new Size(88, 40);
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

        var iconRect = new Rectangle(b.X + 8, b.Y + (b.Height - 24) / 2, 24, 24);
        TextRenderer.DrawText(
            g,
            SegoeMdl2Icons.Filter,
            PharmaTheme.IconFont(12f),
            iconRect,
            PharmaTheme.Primary,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

        var textRect = new Rectangle(iconRect.Right + 2, b.Y, b.Width - iconRect.Width - 12, b.Height);
        TextRenderer.DrawText(
            g,
            Text,
            PharmaTheme.SmallFont,
            textRect,
            PharmaTheme.TextDark,
            TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.RightToLeft);
    }
}

internal sealed class InvProductTableHeader : Control
{
    public InvProductTableHeader()
    {
        Height = 44;
        Dock = DockStyle.Top;
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
    }

    public void ApplyThemeVisuals() => Invalidate();

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var bounds = ClientRectangle;
        using var back = new SolidBrush(PharmaTheme.PrimaryContainer);
        g.FillRectangle(back, bounds);

        var columns = GetColumns(bounds);
        DrawHeader(g, columns.Barcode, "الباركود");
        DrawHeader(g, columns.Name, "اسم الدواء");
        DrawHeader(g, columns.Category, "الفئة");
        DrawHeader(g, columns.Quantity, "الكمية");
        DrawHeader(g, columns.Status, "الحالة");
    }

    private static void DrawHeader(Graphics g, Rectangle rect, string text)
    {
        TextRenderer.DrawText(
            g,
            text,
            PharmaTheme.TableHeaderFont,
            Rectangle.Inflate(rect, -8, 0),
            PharmaTheme.PrimaryDark,
            TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
    }

    internal static TableColumnLayout GetColumns(Rectangle bounds)
    {
        const int pad = 12;
        var statusW = Math.Max(96, (int)(bounds.Width * 0.12));
        var qtyW = Math.Max(72, (int)(bounds.Width * 0.08));
        var categoryW = Math.Max(100, (int)(bounds.Width * 0.16));
        var barcodeW = Math.Max(120, (int)(bounds.Width * 0.18));
        var nameW = Math.Max(120, bounds.Width - pad * 2 - statusW - qtyW - categoryW - barcodeW);

        var x = bounds.X + pad;
        var barcode = new Rectangle(x, bounds.Y, barcodeW, bounds.Height);
        x += barcodeW;
        var name = new Rectangle(x, bounds.Y, nameW, bounds.Height);
        x += nameW;
        var category = new Rectangle(x, bounds.Y, categoryW, bounds.Height);
        x += categoryW;
        var quantity = new Rectangle(x, bounds.Y, qtyW, bounds.Height);
        x += qtyW;
        var status = new Rectangle(x, bounds.Y, statusW, bounds.Height);

        return new TableColumnLayout(barcode, name, category, quantity, status);
    }

    internal readonly record struct TableColumnLayout(
        Rectangle Barcode,
        Rectangle Name,
        Rectangle Category,
        Rectangle Quantity,
        Rectangle Status);
}

internal sealed class InvProductTableRow : Control
{
    private readonly InventoryProductView _product;
    private bool _hover;
    private bool _selected;

    public InvProductTableRow(InventoryProductView product)
    {
        _product = product;
        Height = PharmaTheme.InventoryRowHeight;
        Dock = DockStyle.Top;
        Cursor = Cursors.Hand;
        RightToLeft = RightToLeft.Yes;
        SetStyle(
            ControlStyles.AllPaintingInWmPaint
                | ControlStyles.UserPaint
                | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.StandardClick,
            true);
    }

    public InventoryProductView Product => _product;

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool IsRowSelected
    {
        get => _selected;
        set
        {
            _selected = value;
            Invalidate();
        }
    }

    public event EventHandler? RowClicked;

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

    protected override void OnClick(EventArgs e)
    {
        base.OnClick(e);
        RowClicked?.Invoke(this, EventArgs.Empty);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var bounds = ClientRectangle;

        var back = _selected
            ? PharmaTheme.PrimaryLight
            : _hover
                ? PharmaTheme.SurfaceContainerLow
                : PharmaTheme.Surface;
        using var brush = new SolidBrush(back);
        g.FillRectangle(brush, bounds);

        if (_product.Status is InventoryProductStatus.LowStock or InventoryProductStatus.OutOfStock or InventoryProductStatus.Expired)
        {
            using var accent = new SolidBrush(
                _product.Status == InventoryProductStatus.LowStock ? PharmaTheme.Warning : PharmaTheme.Danger);
            g.FillRectangle(accent, bounds.X, bounds.Y, 4, bounds.Height);
        }

        var columns = InvProductTableHeader.GetColumns(bounds);
        DrawCell(g, columns.Barcode, DisplayBarcode(), PharmaTheme.TableCellFont, PharmaTheme.OnSurfaceVariant);
        DrawCell(g, columns.Name, _product.DisplayName, PharmaTheme.TableCellFont, PharmaTheme.TextDark, true);
        DrawCell(g, columns.Category, string.IsNullOrWhiteSpace(_product.CategoryName) ? "—" : _product.CategoryName, PharmaTheme.TableCellFont, PharmaTheme.OnSurfaceVariant);
        DrawQuantity(g, columns.Quantity);
        DrawStatusBadge(g, columns.Status);

        using var line = new Pen(PharmaTheme.BorderSoft);
        g.DrawLine(line, bounds.X + 8, bounds.Bottom - 1, bounds.Right - 8, bounds.Bottom - 1);
    }

    private string DisplayBarcode() =>
        string.IsNullOrWhiteSpace(_product.Barcode) ? "—" : _product.Barcode;

    private void DrawQuantity(Graphics g, Rectangle rect)
    {
        var qtyText = _product.SellableQuantity.ToString("N0");
        var color = _product.Status switch
        {
            InventoryProductStatus.LowStock => PharmaTheme.WarningStrong,
            InventoryProductStatus.OutOfStock or InventoryProductStatus.Expired => PharmaTheme.Danger,
            _ => PharmaTheme.TextDark
        };
        DrawCell(g, rect, qtyText, PharmaTheme.TableAmountFont, color);
    }

    private void DrawStatusBadge(Graphics g, Rectangle rect)
    {
        var (back, fore) = _product.Status switch
        {
            InventoryProductStatus.LowStock => (PharmaTheme.WarningSurface, PharmaTheme.WarningStrong),
            InventoryProductStatus.OutOfStock => (PharmaTheme.ErrorContainer, PharmaTheme.Danger),
            InventoryProductStatus.Expired => (PharmaTheme.ErrorContainer, PharmaTheme.Danger),
            InventoryProductStatus.ExpiringSoon => (PharmaTheme.WarningSurface, PharmaTheme.WarningStrong),
            _ => (PharmaTheme.SuccessSurface, PharmaTheme.Success)
        };

        var label = _product.StatusLabel;
        var size = TextRenderer.MeasureText(label, PharmaTheme.ArabicFont(9f, FontStyle.Bold));
        var badgeW = Math.Min(rect.Width - 8, size.Width + 16);
        var badgeH = 24;
        var badgeRect = new Rectangle(rect.Right - badgeW - 4, rect.Y + (rect.Height - badgeH) / 2, badgeW, badgeH);
        RoundedDrawing.FillRounded(g, badgeRect, badgeH / 2, back);
        TextRenderer.DrawText(
            g,
            label,
            PharmaTheme.ArabicFont(9f, FontStyle.Bold),
            badgeRect,
            fore,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
    }

    private static void DrawCell(
        Graphics g,
        Rectangle rect,
        string text,
        Font font,
        Color color,
        bool boldName = false)
    {
        var f = boldName ? PharmaTheme.ArabicFont(10f, FontStyle.Bold) : font;
        TextRenderer.DrawText(
            g,
            text,
            f,
            Rectangle.Inflate(rect, -8, 0),
            color,
            TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
    }
}

internal sealed class InvDetailsTabButton : Control
{
    private bool _selected;

    public InvDetailsTabButton(string caption)
    {
        Text = caption;
        Height = 36;
        MinimumSize = new Size(72, 36);
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
        b.Inflate(-2, -2);
        var back = _selected ? PharmaTheme.PrimaryContainer : Color.Transparent;
        var text = _selected ? PharmaTheme.Primary : PharmaTheme.OnSurfaceVariant;
        if (_selected)
        {
            RoundedDrawing.FillRounded(g, b, 10, back);
        }

        TextRenderer.DrawText(
            g,
            Text,
            PharmaTheme.ArabicFont(10f, _selected ? FontStyle.Bold : FontStyle.Regular),
            b,
            text,
            TextFormatFlags.HorizontalCenter
                | TextFormatFlags.VerticalCenter
                | TextFormatFlags.RightToLeft);

        if (_selected)
        {
            var underline = new Rectangle(b.X + 8, b.Bottom - 3, b.Width - 16, 2);
            RoundedDrawing.FillRounded(g, underline, 1, PharmaTheme.Primary);
        }
    }
}

internal sealed class InvProductDetailsPanel : InvRoundedPanel
{
    private InventoryProductDetailsView? _details;
    private int _activeTab;

    private readonly Label _closeButton = new();
    private readonly Label _titleLabel = new();
    private readonly Label _badgeLabel = new();
    private readonly Label _barcodeLabel = new();
    private readonly FlowLayoutPanel _tabsPanel = new();
    private readonly Panel _contentPanel = new();
    private readonly Panel _actionsPanel = new();
    private readonly GradientRoundedButton _purchaseButton = new();
    private readonly InvSecondaryButton _editButton = new();
    private readonly List<InvDetailsTabButton> _tabButtons = new();

    public InvProductDetailsPanel() : base(PharmaTheme.InventoryCardCornerRadius)
    {
        FillColor = PharmaTheme.Surface;
        DoubleBuffered = true;
        RightToLeft = RightToLeft.Yes;
        Visible = false;
        Width = PharmaTheme.InventoryDetailsWidth;

        _closeButton.Text = SegoeMdl2Icons.Close;
        _closeButton.Font = PharmaTheme.IconFont(11f);
        _closeButton.AutoSize = true;
        _closeButton.Cursor = Cursors.Hand;
        _closeButton.Click += (_, _) => CloseRequested?.Invoke(this, EventArgs.Empty);

        _titleLabel.Font = PharmaTheme.ArabicFont(16f, FontStyle.Bold);
        _titleLabel.ForeColor = PharmaTheme.TextDark;
        _titleLabel.AutoSize = false;
        _titleLabel.TextAlign = ContentAlignment.MiddleRight;

        _badgeLabel.Font = PharmaTheme.ArabicFont(9f, FontStyle.Bold);
        _badgeLabel.AutoSize = true;

        _barcodeLabel.Font = PharmaTheme.SmallFont;
        _barcodeLabel.ForeColor = PharmaTheme.OnSurfaceVariant;
        _barcodeLabel.AutoSize = false;
        _barcodeLabel.TextAlign = ContentAlignment.MiddleRight;

        _tabsPanel.FlowDirection = FlowDirection.RightToLeft;
        _tabsPanel.WrapContents = false;
        _tabsPanel.AutoSize = false;
        _tabsPanel.BackColor = Color.Transparent;

        foreach (var tab in new[] { "المعلومات", "الحركة", "البدائل" })
        {
            var btn = new InvDetailsTabButton(tab);
            btn.Click += (_, _) => SetActiveTab(_tabButtons.IndexOf(btn));
            _tabButtons.Add(btn);
            _tabsPanel.Controls.Add(btn);
        }

        _contentPanel.BackColor = Color.Transparent;
        _actionsPanel.BackColor = Color.Transparent;

        _purchaseButton.Text = "طلب شراء جديد";
        _purchaseButton.Height = 48;
        _purchaseButton.Dock = DockStyle.Top;
        _purchaseButton.Click += (_, _) => PurchaseOrderRequested?.Invoke(this, EventArgs.Empty);

        _editButton.Text = "تعديل البيانات";
        _editButton.Height = 48;
        _editButton.Dock = DockStyle.Top;
        _editButton.Click += (_, _) => EditRequested?.Invoke(this, EventArgs.Empty);

        _actionsPanel.Controls.Add(_purchaseButton);
        _actionsPanel.Controls.Add(_editButton);

        Controls.Add(_actionsPanel);
        Controls.Add(_contentPanel);
        Controls.Add(_tabsPanel);
        Controls.Add(_barcodeLabel);
        Controls.Add(_badgeLabel);
        Controls.Add(_titleLabel);
        Controls.Add(_closeButton);

        SetActiveTab(0);
        Resize += (_, _) => LayoutDetailsPanel();
    }

    public event EventHandler? CloseRequested;
    public event EventHandler? PurchaseOrderRequested;
    public event EventHandler? EditRequested;

    public void Bind(InventoryProductDetailsView? details)
    {
        _details = details;
        if (details is null)
        {
            Visible = false;
            return;
        }

        Visible = true;
        _titleLabel.Text = details.Product.DisplayName;
        _barcodeLabel.Text = string.IsNullOrWhiteSpace(details.Product.Barcode)
            ? "Barcode: —"
            : $"Barcode: {details.Product.Barcode}";

        ApplyBadge(details.Product.StatusLabel, details.Product.Status);
        SetActiveTab(0);
        LayoutDetailsPanel();
        Invalidate(true);
    }

    public new void ApplyThemeVisuals()
    {
        FillColor = PharmaTheme.Surface;
        _titleLabel.ForeColor = PharmaTheme.TextDark;
        _titleLabel.Font = PharmaTheme.ArabicFont(16f, FontStyle.Bold);
        _barcodeLabel.ForeColor = PharmaTheme.OnSurfaceVariant;
        _barcodeLabel.Font = PharmaTheme.SmallFont;
        _closeButton.ForeColor = PharmaTheme.OnSurfaceVariant;
        _closeButton.Font = PharmaTheme.IconFont(11f);
        _purchaseButton.ForeColor = PharmaTheme.OnPrimary;
        _purchaseButton.Invalidate();
        _editButton.ApplyThemeVisuals();
        foreach (var tab in _tabButtons)
        {
            tab.ApplyThemeVisuals();
        }

        base.ApplyThemeVisuals();
        RenderContent();
    }

    private void ApplyBadge(string label, InventoryProductStatus status)
    {
        var (back, fore) = status switch
        {
            InventoryProductStatus.LowStock => (PharmaTheme.WarningSurface, PharmaTheme.WarningStrong),
            InventoryProductStatus.OutOfStock or InventoryProductStatus.Expired => (PharmaTheme.ErrorContainer, PharmaTheme.Danger),
            InventoryProductStatus.ExpiringSoon => (PharmaTheme.WarningSurface, PharmaTheme.WarningStrong),
            _ => (PharmaTheme.SuccessSurface, PharmaTheme.Success)
        };

        _badgeLabel.Text = $"  {label}  ";
        _badgeLabel.BackColor = back;
        _badgeLabel.ForeColor = fore;
        _badgeLabel.Padding = new Padding(8, 4, 8, 4);
    }

    private void SetActiveTab(int index)
    {
        _activeTab = Math.Clamp(index, 0, _tabButtons.Count - 1);
        for (var i = 0; i < _tabButtons.Count; i++)
        {
            _tabButtons[i].IsSelected = i == _activeTab;
        }

        RenderContent();
    }

    private void RenderContent()
    {
        _contentPanel.Controls.Clear();
        if (_details is null)
        {
            return;
        }

        var scroll = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            BackColor = Color.Transparent
        };

        var host = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            BackColor = Color.Transparent,
            RightToLeft = RightToLeft.Yes,
            Width = Math.Max(280, _contentPanel.ClientSize.Width - 4),
            Padding = new Padding(0, 4, 0, 8)
        };

        switch (_activeTab)
        {
            case 0:
                BuildInfoTab(host, _details);
                break;
            case 1:
                BuildMovementTab(host, _details);
                break;
            default:
                BuildAlternativesTab(host);
                break;
        }

        scroll.Controls.Add(host);
        _contentPanel.Controls.Add(scroll);
    }

    private void BuildInfoTab(FlowLayoutPanel host, InventoryProductDetailsView details)
    {
        host.Controls.Add(CreateSummaryCard(details));
        host.Controls.Add(CreateSectionTitle("التفاصيل الأساسية"));
        host.Controls.Add(CreateDetailRow("الاسم العلمي", NullOrValue(details.Product.ScientificName)));
        host.Controls.Add(CreateDetailRow("الفئة العلاجية", NullOrValue(details.Product.CategoryName)));
        host.Controls.Add(CreateDetailRow("المورد الأساسي", details.SupplierName));
        host.Controls.Add(CreateDetailRow("سعر الشراء", FormatPrice(details.Product.PurchasePrice)));
        host.Controls.Add(CreateDetailRow("الرف / الموقع", details.ShelfLocation));

        if (!string.IsNullOrWhiteSpace(details.ExpiryWarningText))
        {
            host.Controls.Add(CreateWarningBox(details.ExpiryWarningText));
        }
    }

    private void BuildMovementTab(FlowLayoutPanel host, InventoryProductDetailsView details)
    {
        if (details.Transactions.Count == 0)
        {
            host.Controls.Add(CreateEmptyState("لا توجد حركة متاحة"));
            return;
        }

        foreach (var tx in details.Transactions)
        {
            host.Controls.Add(CreateMovementRow(tx));
        }
    }

    private static void BuildAlternativesTab(FlowLayoutPanel host)
    {
        host.Controls.Add(CreateEmptyState("لا توجد بدائل مسجلة"));
    }

    private Control CreateSummaryCard(InventoryProductDetailsView details)
    {
        var card = new InvRoundedPanel(14, drawShadow: false)
        {
            FillColor = PharmaTheme.SurfaceAlt,
            BorderColor = PharmaTheme.BorderSoft,
            Margin = new Padding(0, 0, 0, 12),
            Width = Math.Max(280, _contentPanel.ClientSize.Width - 8),
            Height = 120
        };

        var grid = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 2,
            BackColor = Color.Transparent,
            RightToLeft = RightToLeft.Yes,
            Padding = new Padding(12)
        };
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        grid.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        grid.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

        grid.Controls.Add(CreateMetric("الكمية الحالية", details.Product.SellableQuantity.ToString("N0")), 0, 0);
        grid.Controls.Add(CreateMetric("سعر البيع", FormatPrice(details.Product.SellingPrice)), 1, 0);
        grid.Controls.Add(CreateMetric("سعر الشراء", FormatPrice(details.Product.PurchasePrice)), 0, 1);
        grid.Controls.Add(CreateMetric("الرف / الموقع", details.ShelfLocation), 1, 1);

        card.Controls.Add(grid);
        return card;
    }

    private static Control CreateMetric(string caption, string value)
    {
        var panel = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent, Padding = new Padding(4) };
        var cap = new Label
        {
            Text = caption,
            Dock = DockStyle.Top,
            Height = 18,
            ForeColor = PharmaTheme.OnSurfaceVariant,
            Font = PharmaTheme.SmallFont,
            TextAlign = ContentAlignment.MiddleRight
        };
        var val = new Label
        {
            Text = value,
            Dock = DockStyle.Fill,
            ForeColor = PharmaTheme.TextDark,
            Font = PharmaTheme.ArabicFont(11f, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleRight
        };
        panel.Controls.Add(val);
        panel.Controls.Add(cap);
        return panel;
    }

    private static Control CreateSectionTitle(string text) =>
        new Label
        {
            Text = text,
            AutoSize = true,
            Margin = new Padding(0, 8, 0, 6),
            ForeColor = PharmaTheme.TextDark,
            Font = PharmaTheme.ArabicFont(11f, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleRight
        };

    private static Control CreateDetailRow(string caption, string value)
    {
        var row = new Panel
        {
            Height = 34,
            Width = 320,
            Margin = new Padding(0, 0, 0, 2),
            BackColor = Color.Transparent
        };
        var cap = new Label
        {
            Text = caption,
            Dock = DockStyle.Right,
            Width = 120,
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
            TextAlign = ContentAlignment.MiddleRight
        };
        row.Controls.Add(val);
        row.Controls.Add(cap);
        return row;
    }

    private static Control CreateWarningBox(string text)
    {
        var box = new InvRoundedPanel(12, drawShadow: false)
        {
            FillColor = PharmaTheme.WarningSurface,
            BorderColor = PharmaTheme.Warning,
            Width = 320,
            Height = 72,
            Margin = new Padding(0, 12, 0, 0)
        };
        var label = new Label
        {
            Text = $"تنبيه صلاحية قريبة{Environment.NewLine}{text}",
            Dock = DockStyle.Fill,
            ForeColor = PharmaTheme.WarningStrong,
            Font = PharmaTheme.SmallFont,
            TextAlign = ContentAlignment.MiddleRight,
            Padding = new Padding(12)
        };
        box.Controls.Add(label);
        return box;
    }

    private static Control CreateMovementRow(InventoryTransactionView tx)
    {
        var row = new InvRoundedPanel(10, drawShadow: false)
        {
            FillColor = PharmaTheme.SurfaceAlt,
            Width = 320,
            Height = 56,
            Margin = new Padding(0, 0, 0, 6)
        };
        var text = new Label
        {
            Text = $"{tx.CreatedAt:yyyy-MM-dd}  •  {tx.Type}  •  {tx.Quantity:+0;-0;0}{Environment.NewLine}{tx.Reference}",
            Dock = DockStyle.Fill,
            ForeColor = PharmaTheme.TextDark,
            Font = PharmaTheme.SmallFont,
            TextAlign = ContentAlignment.MiddleRight,
            Padding = new Padding(10, 6, 10, 6)
        };
        row.Controls.Add(text);
        return row;
    }

    private static Control CreateEmptyState(string text) =>
        new Label
        {
            Text = text,
            AutoSize = true,
            ForeColor = PharmaTheme.OnSurfaceVariant,
            Font = PharmaTheme.BodyFont,
            Margin = new Padding(0, 16, 0, 0),
            TextAlign = ContentAlignment.MiddleRight
        };

    private static string NullOrValue(string? value) =>
        string.IsNullOrWhiteSpace(value) ? "غير متوفر" : value.Trim();

    private static string FormatPrice(decimal? value) =>
        value.HasValue ? PosFormatting.FormatMoneyCompact(value.Value) : "غير متوفر";

    private static string FormatPrice(decimal value) => PosFormatting.FormatMoneyCompact(value);

    private void LayoutDetailsPanel()
    {
        const int pad = 16;
        var w = ClientSize.Width;
        var h = ClientSize.Height;

        _closeButton.Location = new Point(pad, pad);
        _titleLabel.SetBounds(pad, pad + 4, w - pad * 2 - 28, 28);
        _badgeLabel.Location = new Point(w - pad - _badgeLabel.PreferredWidth, _titleLabel.Bottom + 6);
        _barcodeLabel.SetBounds(pad, _badgeLabel.Bottom + 4, w - pad * 2, 20);

        _tabsPanel.SetBounds(pad, _barcodeLabel.Bottom + 12, w - pad * 2, 40);
        _actionsPanel.SetBounds(pad, h - pad - 112, w - pad * 2, 112);
        _contentPanel.SetBounds(pad, _tabsPanel.Bottom + 8, w - pad * 2, _actionsPanel.Top - _tabsPanel.Bottom - 16);
    }
}

internal sealed class InvSecondaryButton : Control
{
    public InvSecondaryButton()
    {
        Height = 48;
        MinimumSize = new Size(120, 44);
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
        RoundedDrawing.FillRounded(g, b, PharmaTheme.InventoryButtonCornerRadius, PharmaTheme.Surface);
        RoundedDrawing.DrawRoundedBorder(g, b, PharmaTheme.InventoryButtonCornerRadius, PharmaTheme.Primary, 1.5f);
        TextRenderer.DrawText(
            g,
            Text,
            PharmaTheme.ArabicFont(10.5f, FontStyle.Bold),
            b,
            PharmaTheme.Primary,
            TextFormatFlags.HorizontalCenter
                | TextFormatFlags.VerticalCenter
                | TextFormatFlags.RightToLeft);
    }
}

internal sealed class InvPaginationBar : Control
{
    private readonly Label _summaryLabel = new();
    private readonly Label _prevButton = new();
    private readonly Label _nextButton = new();

    public InvPaginationBar()
    {
        Height = 44;
        Dock = DockStyle.Bottom;
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);

        _summaryLabel.AutoSize = false;
        _summaryLabel.TextAlign = ContentAlignment.MiddleRight;
        _summaryLabel.Font = PharmaTheme.SmallFont;
        _summaryLabel.ForeColor = PharmaTheme.OnSurfaceVariant;

        foreach (var btn in new[] { _prevButton, _nextButton })
        {
            btn.AutoSize = false;
            btn.Size = new Size(36, 32);
            btn.TextAlign = ContentAlignment.MiddleCenter;
            btn.Font = PharmaTheme.IconFont(12f);
            btn.Cursor = Cursors.Hand;
            btn.Click += (_, _) =>
            {
                if (ReferenceEquals(btn, _prevButton))
                {
                    PreviousRequested?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    NextRequested?.Invoke(this, EventArgs.Empty);
                }
            };
        }

        _prevButton.Text = SegoeMdl2Icons.ChevronRight;
        _nextButton.Text = SegoeMdl2Icons.ChevronLeft;
        Controls.Add(_summaryLabel);
        Controls.Add(_prevButton);
        Controls.Add(_nextButton);
        Resize += (_, _) => LayoutBar();
    }

    public event EventHandler? PreviousRequested;
    public event EventHandler? NextRequested;

    public void Update(int from, int to, int total, bool canPrev, bool canNext)
    {
        _summaryLabel.Text = total <= 0
            ? "لا توجد منتجات"
            : $"عرض {from:N0} إلى {to:N0} من أصل {total:N0}";
        _prevButton.Enabled = canPrev;
        _nextButton.Enabled = canNext;
        _prevButton.ForeColor = canPrev ? PharmaTheme.Primary : PharmaTheme.OnSurfaceVariant;
        _nextButton.ForeColor = canNext ? PharmaTheme.Primary : PharmaTheme.OnSurfaceVariant;
        Invalidate();
    }

    public void ApplyThemeVisuals()
    {
        _summaryLabel.ForeColor = PharmaTheme.OnSurfaceVariant;
        _summaryLabel.Font = PharmaTheme.SmallFont;
        Invalidate();
    }

    private void LayoutBar()
    {
        var pad = 12;
        _nextButton.Location = new Point(pad, (Height - _nextButton.Height) / 2);
        _prevButton.Location = new Point(_nextButton.Right + 6, _nextButton.Top);
        _summaryLabel.SetBounds(_prevButton.Right + 12, 0, Width - _prevButton.Right - pad - 12, Height);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.Clear(Parent?.BackColor ?? PharmaTheme.Surface);
    }
}
