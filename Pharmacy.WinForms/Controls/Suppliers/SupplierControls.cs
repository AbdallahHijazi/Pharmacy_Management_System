using System.ComponentModel;
using System.Drawing.Drawing2D;
using Pharmacy.WinForms.Controls;
using Pharmacy.WinForms.Models;
using Pharmacy.WinForms.Ui;

namespace Pharmacy.WinForms.Controls.Suppliers;

internal static class SupColumnLayout
{
    internal readonly record struct Layout(
        Rectangle Name,
        Rectangle Contact,
        Rectangle Phone,
        Rectangle Purchases,
        Rectangle Payable,
        Rectangle Actions,
        bool Compact);

    internal static Layout Calculate(Rectangle bounds, bool compact)
    {
        const int pad = 12;
        var actionsW = Math.Max(96, (int)(bounds.Width * 0.11));
        var payableW = Math.Max(96, (int)(bounds.Width * 0.13));
        var purchasesW = compact ? 0 : Math.Max(96, (int)(bounds.Width * 0.14));
        var phoneW = Math.Max(96, (int)(bounds.Width * 0.13));
        var contactW = compact ? 0 : Math.Max(96, (int)(bounds.Width * 0.16));
        var fixedW = pad * 2 + actionsW + payableW + purchasesW + phoneW + contactW;
        var nameW = Math.Max(160, bounds.Width - fixedW);

        var x = bounds.X + pad;
        var name = new Rectangle(x, bounds.Y, nameW, bounds.Height);
        x += nameW;
        var contact = compact ? Rectangle.Empty : new Rectangle(x, bounds.Y, contactW, bounds.Height);
        if (!compact)
        {
            x += contactW;
        }

        var phone = new Rectangle(x, bounds.Y, phoneW, bounds.Height);
        x += phoneW;
        var purchases = compact ? Rectangle.Empty : new Rectangle(x, bounds.Y, purchasesW, bounds.Height);
        if (!compact)
        {
            x += purchasesW;
        }

        var payable = new Rectangle(x, bounds.Y, payableW, bounds.Height);
        x += payableW;
        var actions = new Rectangle(x, bounds.Y, actionsW, bounds.Height);

        return new Layout(name, contact, phone, purchases, payable, actions, compact);
    }
}

internal class SupRoundedPanel : Panel
{
    private readonly int _radius;

    public SupRoundedPanel(int radius = PharmaTheme.SuppliersCardCornerRadius)
    {
        _radius = radius;
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

        RoundedDrawing.FillRounded(g, bounds, _radius, FillColor);
        RoundedDrawing.DrawRoundedBorder(g, bounds, _radius, BorderColor, 1f);
    }

    protected override void OnPaintBackground(PaintEventArgs e) =>
        e.Graphics.Clear(Parent?.BackColor ?? PharmaTheme.Background);
}

internal sealed class SupSearchBox : UserControl
{
    private TextBox? _box;
    private bool _focused;

    public SupSearchBox()
    {
        SetStyle(
            ControlStyles.AllPaintingInWmPaint
                | ControlStyles.UserPaint
                | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.ResizeRedraw,
            true);
        DoubleBuffered = true;
        Height = 50;
        MinimumSize = new Size(220, 50);
        Padding = new Padding(44, 0, 14, 0);

        _box = new TextBox
        {
            BorderStyle = BorderStyle.None,
            Font = PharmaTheme.ArabicFont(11f),
            BackColor = PharmaTheme.SurfaceContainerHigh,
            ForeColor = PharmaTheme.TextDark,
            RightToLeft = RightToLeft.Yes,
            TextAlign = HorizontalAlignment.Right,
            PlaceholderText = "البحث عن مورد..."
        };
        _box.GotFocus += (_, _) => { _focused = true; Invalidate(); };
        _box.LostFocus += (_, _) => { _focused = false; Invalidate(); };
        _box.TextChanged += (_, _) => SearchTextChanged?.Invoke(this, EventArgs.Empty);
        Controls.Add(_box);
        _box.Dock = DockStyle.Fill;
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
        if (_box is not null)
        {
            _box.BackColor = PharmaTheme.SurfaceContainerHigh;
            _box.ForeColor = PharmaTheme.TextDark;
            _box.Font = PharmaTheme.ArabicFont(11f);
        }

        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var r = ClientRectangle;
        r.Inflate(-1, -1);
        RoundedDrawing.FillRounded(g, r, PharmaTheme.SuppliersSearchCornerRadius, PharmaTheme.SurfaceContainerHigh);
        RoundedDrawing.DrawRoundedBorder(
            g,
            r,
            PharmaTheme.SuppliersSearchCornerRadius,
            _focused ? PharmaTheme.Primary : PharmaTheme.BorderSoft,
            _focused ? 1.75f : 1f);

        TextRenderer.DrawText(
            g,
            SegoeMdl2Icons.Search,
            PharmaTheme.IconFont(12f),
            new Rectangle(14, 0, 28, Height),
            PharmaTheme.OnSurfaceVariant,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
    }

    protected override void OnPaintBackground(PaintEventArgs e) =>
        e.Graphics.Clear(Parent?.BackColor ?? PharmaTheme.Background);
}

internal sealed class SupStatCard : Control
{
    private string _title = string.Empty;
    private string _value = "0";
    private string _iconGlyph = SegoeMdl2Icons.Suppliers;
    private bool _dangerTone;

    public SupStatCard()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        DoubleBuffered = true;
        MinimumSize = new Size(200, 128);
        Height = 128;
        RightToLeft = RightToLeft.Yes;
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string CardTitle
    {
        get => _title;
        set { _title = value; Invalidate(); }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string CardValue
    {
        get => _value;
        set { _value = value; Invalidate(); }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string IconGlyph
    {
        get => _iconGlyph;
        set { _iconGlyph = value; Invalidate(); }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool DangerTone
    {
        get => _dangerTone;
        set { _dangerTone = value; Invalidate(); }
    }

    public void ApplyThemeVisuals() => Invalidate();

    protected override void OnPaintBackground(PaintEventArgs e) =>
        e.Graphics.Clear(Parent?.BackColor ?? PharmaTheme.Background);

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
        var bounds = ClientRectangle;
        bounds.Inflate(-1, -1);
        RoundedDrawing.FillRounded(g, bounds, PharmaTheme.SuppliersStatCornerRadius, PharmaTheme.Surface);
        RoundedDrawing.DrawRoundedBorder(g, bounds, PharmaTheme.SuppliersStatCornerRadius, PharmaTheme.BorderSoft, 1f);

        const int innerPad = 22;
        var iconSize = 44;
        var iconRect = new Rectangle(bounds.Right - innerPad - iconSize, bounds.Y + innerPad, iconSize, iconSize);
        RoundedDrawing.FillRounded(g, iconRect, 12, PharmaTheme.WithAlpha(PharmaTheme.PrimaryContainer, 120));
        TextRenderer.DrawText(
            g,
            _iconGlyph,
            PharmaTheme.IconFont(16f),
            iconRect,
            _dangerTone ? PharmaTheme.Danger : PharmaTheme.Primary,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

        var textX = bounds.X + innerPad;
        var textW = Math.Max(80, iconRect.X - textX - 12);
        var titleRect = new Rectangle(textX, bounds.Y + innerPad, textW, 40);
        TextRenderer.DrawText(
            g,
            _title,
            PharmaTheme.StatTitleFont,
            titleRect,
            PharmaTheme.OnSurfaceVariant,
            TextFormatFlags.Right | TextFormatFlags.Top | TextFormatFlags.WordBreak | TextFormatFlags.EndEllipsis);

        var valueRect = new Rectangle(textX, bounds.Y + innerPad + 44, textW, 36);
        var valueColor = _dangerTone ? PharmaTheme.Danger : PharmaTheme.TextDark;
        DrawStatValue(g, valueRect, _value, valueColor);
    }

    private static void DrawStatValue(Graphics g, Rectangle rect, string value, Color color)
    {
        if (string.IsNullOrWhiteSpace(value) || string.Equals(value, "غير متوفر", StringComparison.Ordinal))
        {
            TextRenderer.DrawText(
                g,
                "غير متوفر",
                PharmaTheme.ArabicFont(11f, FontStyle.Bold),
                rect,
                color,
                TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
            return;
        }

        var display = SupplierDisplayHelper.FormatStatDisplay(value);
        TextRenderer.DrawText(
            g,
            display,
            PharmaTheme.NumberFont(20f, FontStyle.Bold),
            rect,
            color,
            TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
    }
}

internal sealed class SupTableHeader : Control
{
    public SupTableHeader()
    {
        Height = 48;
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
    }

    public void ApplyThemeVisuals() => Invalidate();

    protected override void OnPaintBackground(PaintEventArgs e) =>
        e.Graphics.Clear(Parent?.BackColor ?? PharmaTheme.Background);

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var bounds = ClientRectangle;
        RoundedDrawing.FillRounded(g, bounds, 12, PharmaTheme.SurfaceAlt);

        var compact = bounds.Width < 980;
        var columns = SupColumnLayout.Calculate(bounds, compact);
        DrawHeader(g, columns.Name, "اسم المورد");
        if (!compact)
        {
            DrawHeader(g, columns.Contact, "الشخص المسؤول");
            DrawHeader(g, columns.Purchases, "إجمالي المشتريات");
        }

        DrawHeader(g, columns.Phone, "الهاتف");
        DrawHeader(g, columns.Payable, "المستحقات");
        DrawHeader(g, columns.Actions, "إجراءات");
    }

    private static void DrawHeader(Graphics g, Rectangle rect, string text)
    {
        if (rect.Width <= 0)
        {
            return;
        }

        TextRenderer.DrawText(
            g,
            text,
            PharmaTheme.TableHeaderFont,
            Rectangle.Inflate(rect, -8, 0),
            PharmaTheme.MutedText,
            TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
    }
}

internal sealed class SupSupplierRow : Control
{
    private readonly SupplierListItemView _supplier;
    private bool _hover;
    private readonly SupIconButton _editButton;
    private readonly SupIconButton _moreButton;

    public SupSupplierRow(SupplierListItemView supplier)
    {
        _supplier = supplier;
        Height = PharmaTheme.SuppliersRowHeight;
        Cursor = Cursors.Default;
        RightToLeft = RightToLeft.Yes;
        SetStyle(
            ControlStyles.AllPaintingInWmPaint
                | ControlStyles.UserPaint
                | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.ResizeRedraw,
            true);

        _editButton = new SupIconButton(SegoeMdl2Icons.Edit, "تعديل");
        _moreButton = new SupIconButton(SegoeMdl2Icons.MoreVertical, "المزيد");
        _editButton.Click += (_, _) => EditRequested?.Invoke(this, EventArgs.Empty);
        _moreButton.Click += (_, _) => DetailsRequested?.Invoke(this, EventArgs.Empty);
        Controls.Add(_editButton);
        Controls.Add(_moreButton);
        Resize += (_, _) => LayoutButtons();
    }

    public SupplierListItemView Supplier => _supplier;

    public event EventHandler? EditRequested;
    public event EventHandler? DetailsRequested;

    public void ApplyThemeVisuals()
    {
        _editButton.ApplyThemeVisuals();
        _moreButton.ApplyThemeVisuals();
        Invalidate();
    }

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
        var bounds = ClientRectangle;
        bounds.Inflate(-1, -1);
        var fill = _hover ? PharmaTheme.SurfaceContainerLow : PharmaTheme.Surface;
        RoundedDrawing.FillRounded(g, bounds, PharmaTheme.SuppliersRowCornerRadius, fill);
        RoundedDrawing.DrawRoundedBorder(g, bounds, PharmaTheme.SuppliersRowCornerRadius, PharmaTheme.BorderSoft, 1f);

        var compact = Width < 980;
        var columns = SupColumnLayout.Calculate(bounds, compact);
        DrawNameCell(g, columns.Name);
        if (!compact)
        {
            DrawCell(g, columns.Contact, _supplier.ContactPerson, PharmaTheme.TableCellFont, PharmaTheme.TextDark);
        }

        DrawPhoneCell(g, columns.Phone, _supplier.PhoneNumber);
        if (!compact)
        {
            DrawMoneyCell(g, columns.Purchases, _supplier.FormattedTotalPurchases, PharmaTheme.TextDark);
        }

        var payableColor = _supplier.HasUnpaidDues ? PharmaTheme.Danger : PharmaTheme.Success;
        DrawMoneyCell(g, columns.Payable, _supplier.FormattedPayableAmount, payableColor);
        LayoutButtons();
    }

    private void DrawNameCell(Graphics g, Rectangle rect)
    {
        var avatarSize = 40;
        var pad = 12;
        var avatarX = rect.Right - pad - avatarSize;
        var avatarY = rect.Y + (rect.Height - avatarSize) / 2;
        var avatarRect = new Rectangle(avatarX, avatarY, avatarSize, avatarSize);
        RoundedDrawing.FillRounded(g, avatarRect, 10, PharmaTheme.PrimaryContainer);
        TextRenderer.DrawText(
            g,
            _supplier.Initials,
            PharmaTheme.NumberFont(12f, FontStyle.Bold),
            avatarRect,
            PharmaTheme.PrimaryDark,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

        var textX = rect.X + 8;
        var textW = avatarX - textX - 10;
        TextRenderer.DrawText(
            g,
            _supplier.DisplayName,
            PharmaTheme.ArabicFont(11f, FontStyle.Bold),
            new Rectangle(textX, rect.Y + 14, textW, 22),
            PharmaTheme.TextDark,
            TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

        var subtitle = string.IsNullOrWhiteSpace(_supplier.Subtitle)
            ? _supplier.Address
            : string.IsNullOrWhiteSpace(_supplier.Address) || _supplier.Address == "—"
                ? _supplier.Subtitle
                : $"{_supplier.Address}";

        TextRenderer.DrawText(
            g,
            subtitle,
            PharmaTheme.SmallFont,
            new Rectangle(textX, rect.Y + 38, textW, 18),
            PharmaTheme.OnSurfaceVariant,
            TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
    }

    private static void DrawPhoneCell(Graphics g, Rectangle rect, string phone)
    {
        var display = string.Equals(phone, "لا يوجد رقم", StringComparison.Ordinal)
            ? phone
            : "\u200E" + phone;

        TextRenderer.DrawText(
            g,
            display,
            PharmaTheme.TableCellFont,
            Rectangle.Inflate(rect, -8, 0),
            PharmaTheme.OnSurfaceVariant,
            TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
    }

    private static void DrawCell(Graphics g, Rectangle rect, string text, Font font, Color color)
    {
        if (rect.Width <= 0)
        {
            return;
        }

        TextRenderer.DrawText(
            g,
            text,
            font,
            Rectangle.Inflate(rect, -8, 0),
            color,
            TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
    }

    private static void DrawMoneyCell(Graphics g, Rectangle rect, string text, Color color)
    {
        if (rect.Width <= 0)
        {
            return;
        }

        if (string.Equals(text, "غير متوفر", StringComparison.Ordinal)
            || string.Equals(text, "لا توجد مستحقات", StringComparison.Ordinal))
        {
            DrawCell(g, rect, text, PharmaTheme.TableCellFont, PharmaTheme.OnSurfaceVariant);
            return;
        }

        var display = SupplierDisplayHelper.FormatStatDisplay(text);
        TextRenderer.DrawText(
            g,
            display,
            PharmaTheme.TableAmountFont,
            Rectangle.Inflate(rect, -8, 0),
            color,
            TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
    }

    private void LayoutButtons()
    {
        var columns = SupColumnLayout.Calculate(ClientRectangle, Width < 980);
        if (columns.Actions.Width <= 0)
        {
            return;
        }

        var y = (Height - 32) / 2;
        _moreButton.SetBounds(columns.Actions.Right - 36, y, 32, 32);
        _editButton.SetBounds(columns.Actions.Right - 72, y, 32, 32);
    }
}

internal sealed class SupIconButton : Control
{
    private readonly string _glyph;
    private readonly string _tooltip;
    private bool _hover;

    public SupIconButton(string glyph, string tooltip)
    {
        _glyph = glyph;
        _tooltip = tooltip;
        Size = new Size(32, 32);
        Cursor = Cursors.Hand;
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.StandardClick, true);
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

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var bounds = ClientRectangle;
        bounds.Inflate(-1, -1);
        if (_hover)
        {
            RoundedDrawing.FillRounded(g, bounds, 8, PharmaTheme.SurfaceContainerHigh);
        }

        TextRenderer.DrawText(
            g,
            _glyph,
            PharmaTheme.IconFont(11f),
            bounds,
            PharmaTheme.OnSurfaceVariant,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
    }

    protected override void OnPaintBackground(PaintEventArgs e) =>
        e.Graphics.Clear(Parent?.BackColor ?? PharmaTheme.Background);
}

internal sealed class SupPaginationBar : SupRoundedPanel
{
    private readonly Label _prevButton = new();
    private readonly Label _nextButton = new();
    private readonly Label _infoLabel = new();
    private int _currentPage = 1;
    private int _totalPages = 1;
    private int _fromIndex;
    private int _toIndex;
    private int _totalCount;

    public SupPaginationBar() : base(PharmaTheme.SuppliersCardCornerRadius)
    {
        Height = 56;
        FillColor = PharmaTheme.Surface;
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

        _infoLabel.AutoSize = false;
        _infoLabel.Height = 24;
        _infoLabel.TextAlign = ContentAlignment.MiddleCenter;
        _infoLabel.Font = PharmaTheme.SmallFont;

        Controls.Add(_infoLabel);
        Controls.Add(_nextButton);
        Controls.Add(_prevButton);
        Resize += (_, _) => LayoutBar();
    }

    public event EventHandler<int>? PageChangeRequested;

    public void Update(int currentPage, int totalPages, int fromIndex, int toIndex, int totalCount)
    {
        _currentPage = Math.Max(1, currentPage);
        _totalPages = Math.Max(1, totalPages);
        _fromIndex = fromIndex;
        _toIndex = toIndex;
        _totalCount = totalCount;
        _infoLabel.Text = _totalCount <= 0
            ? "لا يوجد موردون"
            : $"عرض {_fromIndex} إلى {_toIndex} من {_totalCount} مورد";
        LayoutBar();
        Invalidate();
    }

    public new void ApplyThemeVisuals()
    {
        FillColor = PharmaTheme.Surface;
        _prevButton.ForeColor = _currentPage <= 1 ? PharmaTheme.MutedText : PharmaTheme.Primary;
        _nextButton.ForeColor = _currentPage >= _totalPages ? PharmaTheme.MutedText : PharmaTheme.Primary;
        _infoLabel.ForeColor = PharmaTheme.OnSurfaceVariant;
        base.ApplyThemeVisuals();
    }

    private void LayoutBar()
    {
        _prevButton.Location = new Point(20, 16);
        _nextButton.Location = new Point(Width - _nextButton.Width - 20, 16);
        var infoW = Math.Min(420, Math.Max(240, Width - 220));
        _infoLabel.SetBounds((Width - infoW) / 2, 16, infoW, 24);
    }
}

internal sealed class SupSupplierDetailsPanel : SupRoundedPanel
{
    private SupplierListItemView? _supplier;
    private readonly Label _closeButton = new();
    private readonly Panel _contentPanel = new();

    public SupSupplierDetailsPanel() : base(PharmaTheme.SuppliersCardCornerRadius)
    {
        FillColor = PharmaTheme.Surface;
        Visible = false;
        Width = PharmaTheme.SuppliersDetailsWidth;
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
        Resize += (_, _) => Render();
    }

    public event EventHandler? CloseRequested;

    public void Bind(SupplierListItemView? supplier)
    {
        _supplier = supplier;
        Visible = supplier is not null;
        Render();
    }

    public new void ApplyThemeVisuals()
    {
        FillColor = PharmaTheme.Surface;
        _closeButton.ForeColor = PharmaTheme.OnSurfaceVariant;
        _contentPanel.BackColor = PharmaTheme.Surface;
        base.ApplyThemeVisuals();
        Render();
    }

    private void Render()
    {
        _contentPanel.Controls.Clear();
        if (_supplier is null)
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
            BackColor = PharmaTheme.Surface,
            RightToLeft = RightToLeft.Yes,
            Width = contentW,
            Padding = new Padding(8, 36, 8, 12)
        };

        host.Controls.Add(MakeTitle(_supplier.DisplayName));
        host.Controls.Add(MakeRow("الشخص المسؤول", _supplier.ContactPerson, contentW));
        host.Controls.Add(MakeRow("الهاتف", _supplier.PhoneNumber, contentW));
        host.Controls.Add(MakeRow("العنوان", _supplier.Address, contentW));
        host.Controls.Add(MakeRow("إجمالي المشتريات", _supplier.FormattedTotalPurchases, contentW));
        host.Controls.Add(MakeRow("المستحقات", _supplier.FormattedPayableAmount, contentW));

        _contentPanel.Controls.Add(host);
    }

    private static Control MakeTitle(string text) => new Label
    {
        Text = text,
        AutoSize = true,
        Font = PharmaTheme.ArabicFont(16f, FontStyle.Bold),
        ForeColor = PharmaTheme.TextDark,
        Margin = new Padding(0, 0, 0, 12)
    };

    private static Control MakeRow(string caption, string value, int width)
    {
        var panel = new Panel { Height = 30, Width = width, Margin = new Padding(0, 0, 0, 4), BackColor = PharmaTheme.Surface };
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
            TextAlign = ContentAlignment.MiddleRight,
            AutoEllipsis = true
        };
        panel.Controls.Add(val);
        panel.Controls.Add(cap);
        return panel;
    }
}
