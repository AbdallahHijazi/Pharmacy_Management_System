using System.ComponentModel;
using System.Drawing.Drawing2D;
using Pharmacy.WinForms.Controls;
using Pharmacy.WinForms.Models;
using Pharmacy.WinForms.Ui;

namespace Pharmacy.WinForms.Controls.Customers;

internal class CusRoundedPanel : Panel
{
    private readonly int _radius;

    public CusRoundedPanel(int radius = PharmaTheme.CustomersCardCornerRadius)
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

internal sealed class CusSearchBox : UserControl
{
    private TextBox? _box;
    private bool _focused;

    public CusSearchBox()
    {
        SetStyle(
            ControlStyles.AllPaintingInWmPaint
                | ControlStyles.UserPaint
                | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.ResizeRedraw,
            true);
        DoubleBuffered = true;
        Height = 48;
        MinimumSize = new Size(220, 48);
        Padding = new Padding(40, 0, 14, 0);

        _box = new TextBox
        {
            BorderStyle = BorderStyle.None,
            Font = PharmaTheme.ArabicFont(11f),
            BackColor = PharmaTheme.SurfaceContainerHigh,
            ForeColor = PharmaTheme.TextDark,
            RightToLeft = RightToLeft.Yes,
            TextAlign = HorizontalAlignment.Right,
            PlaceholderText = "بحث عن زبون..."
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
        RoundedDrawing.FillRounded(g, r, PharmaTheme.CustomersSearchCornerRadius, PharmaTheme.SurfaceContainerHigh);
        RoundedDrawing.DrawRoundedBorder(
            g,
            r,
            PharmaTheme.CustomersSearchCornerRadius,
            _focused ? PharmaTheme.Primary : PharmaTheme.BorderSoft,
            _focused ? 1.75f : 1f);

        TextRenderer.DrawText(
            g,
            SegoeMdl2Icons.Search,
            PharmaTheme.IconFont(12f),
            new Rectangle(12, 0, 28, Height),
            PharmaTheme.OnSurfaceVariant,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

        if (_box is not null && string.IsNullOrEmpty(_box.Text) && !_box.Focused && string.IsNullOrWhiteSpace(_box.PlaceholderText))
        {
            TextRenderer.DrawText(
                g,
                "بحث عن زبون...",
                PharmaTheme.ArabicFont(11f),
                new Rectangle(Padding.Left, 0, Width - Padding.Horizontal, Height),
                PharmaTheme.MutedText,
                TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }
    }

    protected override void OnPaintBackground(PaintEventArgs e) =>
        e.Graphics.Clear(Parent?.BackColor ?? PharmaTheme.Background);
}

internal sealed class CusViewToggle : Control
{
    private CustomerViewMode _mode = CustomerViewMode.Grid;
    private Rectangle _gridRect;
    private Rectangle _listRect;

    public CusViewToggle()
    {
        Height = 44;
        Width = 92;
        Cursor = Cursors.Hand;
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.StandardClick, true);
        Resize += (_, _) => LayoutButtons();
        LayoutButtons();
    }

    public event EventHandler? ModeChanged;

    public CustomerViewMode Mode => _mode;

    public void SetMode(CustomerViewMode mode)
    {
        if (_mode == mode)
        {
            return;
        }

        _mode = mode;
        Invalidate();
        ModeChanged?.Invoke(this, EventArgs.Empty);
    }

    public void ApplyThemeVisuals() => Invalidate();

    protected override void OnClick(EventArgs e)
    {
        base.OnClick(e);
        var pt = PointToClient(Cursor.Position);
        if (_gridRect.Contains(pt))
        {
            SetMode(CustomerViewMode.Grid);
        }
        else if (_listRect.Contains(pt))
        {
            SetMode(CustomerViewMode.List);
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var r = ClientRectangle;
        r.Inflate(-1, -1);
        RoundedDrawing.FillRounded(g, r, 12, PharmaTheme.SurfaceContainerHigh);
        RoundedDrawing.DrawRoundedBorder(g, r, 12, PharmaTheme.BorderSoft, 1f);
        DrawButton(g, _gridRect, SegoeMdl2Icons.GridView, _mode == CustomerViewMode.Grid);
        DrawButton(g, _listRect, SegoeMdl2Icons.ViewList, _mode == CustomerViewMode.List);
    }

    protected override void OnPaintBackground(PaintEventArgs e) =>
        e.Graphics.Clear(Parent?.BackColor ?? PharmaTheme.Background);

    private void LayoutButtons()
    {
        _gridRect = new Rectangle(6, 4, 40, 36);
        _listRect = new Rectangle(46, 4, 40, 36);
    }

    private static void DrawButton(Graphics g, Rectangle bounds, string glyph, bool active)
    {
        if (active)
        {
            RoundedDrawing.FillRounded(g, bounds, 8, PharmaTheme.Surface);
            RoundedDrawing.DrawRoundedBorder(g, bounds, 8, PharmaTheme.BorderSoft, 1f);
        }

        TextRenderer.DrawText(
            g,
            glyph,
            PharmaTheme.IconFont(13f),
            bounds,
            active ? PharmaTheme.Primary : PharmaTheme.OnSurfaceVariant,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
    }
}

internal sealed class CusCustomerCard : Control
{
    private readonly CustomerListItemView _customer;
    private bool _hover;
    private Rectangle _menuRect;

    public CusCustomerCard(CustomerListItemView customer)
    {
        _customer = customer;
        Height = PharmaTheme.CustomersCardHeight;
        Cursor = Cursors.Hand;
        RightToLeft = RightToLeft.Yes;
        SetStyle(
            ControlStyles.AllPaintingInWmPaint
                | ControlStyles.UserPaint
                | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.StandardClick,
            true);
    }

    public CustomerListItemView Customer => _customer;

    public event EventHandler? ViewDetailsRequested;
    public event EventHandler? MenuRequested;

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
        var pt = PointToClient(Cursor.Position);
        if (_menuRect.Contains(pt))
        {
            MenuRequested?.Invoke(this, EventArgs.Empty);
            return;
        }

        ViewDetailsRequested?.Invoke(this, EventArgs.Empty);
    }

    protected override void OnPaintBackground(PaintEventArgs e) =>
        e.Graphics.Clear(Parent?.BackColor ?? PharmaTheme.Background);

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
        g.Clear(Parent?.BackColor ?? PharmaTheme.Background);

        var bounds = ClientRectangle;
        bounds.Inflate(-1, -1);
        var fill = _hover ? PharmaTheme.SurfaceContainerLow : PharmaTheme.Surface;
        RoundedDrawing.FillRounded(g, bounds, PharmaTheme.CustomersCardCornerRadius, fill);
        RoundedDrawing.DrawRoundedBorder(g, bounds, PharmaTheme.CustomersCardCornerRadius, PharmaTheme.BorderSoft, 1f);

        var accentColor = _customer.HasDebt
            ? PharmaTheme.WithAlpha(PharmaTheme.Danger, 180)
            : PharmaTheme.WithAlpha(PharmaTheme.Primary, 80);
        var accent = new Rectangle(bounds.X, bounds.Y + 14, 4, bounds.Height - 28);
        RoundedDrawing.FillRounded(g, accent, 2, accentColor);

        var pad = 18;
        _menuRect = new Rectangle(bounds.X + pad, bounds.Y + 14, 28, 28);
        TextRenderer.DrawText(
            g,
            SegoeMdl2Icons.MoreVertical,
            PharmaTheme.IconFont(12f),
            _menuRect,
            PharmaTheme.OnSurfaceVariant,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

        var avatarSize = 48;
        var avatarX = bounds.Right - pad - avatarSize;
        var avatarY = bounds.Y + 16;
        var avatarRect = new Rectangle(avatarX, avatarY, avatarSize, avatarSize);
        RoundedDrawing.FillRounded(g, avatarRect, avatarSize / 2, PharmaTheme.PrimaryContainer);
        TextRenderer.DrawText(
            g,
            _customer.Initials,
            PharmaTheme.NumberFont(14f, FontStyle.Bold),
            avatarRect,
            PharmaTheme.PrimaryDark,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

        var textX = bounds.X + pad + 8;
        var textW = avatarX - textX - 12;
        var nameRect = new Rectangle(textX, avatarY, textW, 26);
        TextRenderer.DrawText(
            g,
            _customer.DisplayName,
            PharmaTheme.ArabicFont(13f, FontStyle.Bold),
            nameRect,
            PharmaTheme.TextDark,
            TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

        var phoneRect = new Rectangle(textX, avatarY + 28, textW, 20);
        TextRenderer.DrawText(
            g,
            $"{SegoeMdl2Icons.Phone}  {_customer.PhoneNumber}",
            PharmaTheme.SmallFont,
            phoneRect,
            PharmaTheme.OnSurfaceVariant,
            TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

        var statsY = bounds.Bottom - pad - 56;
        var statGap = 12;
        var statW = (bounds.Width - pad * 2 - statGap) / 2;
        var purchasesRect = new Rectangle(bounds.Right - pad - statW, statsY, statW, 52);
        var debtRect = new Rectangle(purchasesRect.X - statGap - statW, statsY, statW, 52);
        DrawStatBox(g, purchasesRect, "إجمالي المشتريات", _customer.FormattedTotalPurchases, PharmaTheme.SurfaceAlt, PharmaTheme.TextDark);
        var debtBack = _customer.HasDebt ? PharmaTheme.WithAlpha(PharmaTheme.ErrorContainer, 120) : PharmaTheme.SurfaceAlt;
        var debtFore = _customer.HasDebt ? PharmaTheme.Danger : PharmaTheme.Success;
        DrawStatBox(g, debtRect, "حالة الديون", _customer.DebtStatusText, debtBack, debtFore);
    }

    private static void DrawStatBox(Graphics g, Rectangle rect, string caption, string value, Color back, Color fore)
    {
        RoundedDrawing.FillRounded(g, rect, 10, back);
        var capRect = new Rectangle(rect.X + 8, rect.Y + 8, rect.Width - 16, 16);
        var valRect = new Rectangle(rect.X + 8, rect.Y + 24, rect.Width - 16, 20);
        TextRenderer.DrawText(
            g,
            caption,
            PharmaTheme.SmallFont,
            capRect,
            PharmaTheme.OnSurfaceVariant,
            TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        TextRenderer.DrawText(
            g,
            value,
            PharmaTheme.ArabicFont(10.5f, FontStyle.Bold),
            valRect,
            fore,
            TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
    }
}

internal sealed class CusCustomerListRow : Control
{
    private readonly CustomerListItemView _customer;
    private bool _hover;

    public CusCustomerListRow(CustomerListItemView customer)
    {
        _customer = customer;
        Height = 64;
        Cursor = Cursors.Hand;
        RightToLeft = RightToLeft.Yes;
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.StandardClick, true);
    }

    public CustomerListItemView Customer => _customer;

    public event EventHandler? ViewDetailsRequested;

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
        ViewDetailsRequested?.Invoke(this, EventArgs.Empty);
    }

    protected override void OnPaintBackground(PaintEventArgs e) =>
        e.Graphics.Clear(Parent?.BackColor ?? PharmaTheme.Background);

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(Parent?.BackColor ?? PharmaTheme.Background);
        var bounds = ClientRectangle;
        bounds.Inflate(-1, -1);
        var fill = _hover ? PharmaTheme.SurfaceContainerLow : PharmaTheme.Surface;
        RoundedDrawing.FillRounded(g, bounds, 12, fill);
        RoundedDrawing.DrawRoundedBorder(g, bounds, 12, PharmaTheme.BorderSoft, 1f);

        var pad = 16;
        TextRenderer.DrawText(g, _customer.DisplayName, PharmaTheme.ArabicFont(11f, FontStyle.Bold),
            new Rectangle(bounds.X + pad, bounds.Y + 10, bounds.Width / 3, 22), PharmaTheme.TextDark,
            TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        TextRenderer.DrawText(g, _customer.PhoneNumber, PharmaTheme.SmallFont,
            new Rectangle(bounds.X + bounds.Width / 3, bounds.Y + 10, bounds.Width / 4, 22), PharmaTheme.OnSurfaceVariant,
            TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        TextRenderer.DrawText(g, _customer.FormattedTotalPurchases, PharmaTheme.NumberFont(10f, FontStyle.Bold),
            new Rectangle(bounds.X + bounds.Width * 2 / 3, bounds.Y + 10, bounds.Width / 5, 22), PharmaTheme.TextDark,
            TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        var debtColor = _customer.HasDebt ? PharmaTheme.Danger : PharmaTheme.Success;
        TextRenderer.DrawText(g, _customer.DebtStatusText, PharmaTheme.SmallFont,
            new Rectangle(bounds.X + pad, bounds.Y + 34, bounds.Width - pad * 2, 20), debtColor,
            TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
    }
}

internal sealed class CusCustomerDetailsPanel : CusRoundedPanel
{
    private CustomerListItemView? _customer;
    private readonly Label _closeButton = new();
    private readonly Panel _contentPanel = new();

    public CusCustomerDetailsPanel() : base(PharmaTheme.CustomersCardCornerRadius)
    {
        FillColor = PharmaTheme.Surface;
        Visible = false;
        Width = PharmaTheme.CustomersDetailsWidth;
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

    public void Bind(CustomerListItemView? customer)
    {
        _customer = customer;
        Visible = customer is not null;
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
        if (_customer is null)
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

        host.Controls.Add(MakeTitle(_customer.DisplayName));
        host.Controls.Add(MakeRow("الهاتف", _customer.PhoneNumber, contentW));
        host.Controls.Add(MakeRow("العنوان", _customer.Address, contentW));
        host.Controls.Add(MakeRow("إجمالي المشتريات", _customer.FormattedTotalPurchases, contentW));
        host.Controls.Add(MakeRow("حالة الديون", _customer.DebtStatusText, contentW));

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

internal sealed class CusPaginationBar : CusRoundedPanel
{
    private readonly Label _prevButton = new();
    private readonly Label _nextButton = new();
    private readonly Label _infoLabel = new();
    private int _currentPage = 1;
    private int _totalPages = 1;

    public CusPaginationBar()
    {
        Height = 52;
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

    public void Update(int currentPage, int totalPages)
    {
        _currentPage = Math.Max(1, currentPage);
        _totalPages = Math.Max(1, totalPages);
        _infoLabel.Text = $"صفحة {_currentPage} من {_totalPages}";
        LayoutBar();
        Invalidate();
    }

    public new void ApplyThemeVisuals()
    {
        FillColor = PharmaTheme.Surface;
        _prevButton.ForeColor = PharmaTheme.Primary;
        _nextButton.ForeColor = PharmaTheme.Primary;
        _infoLabel.ForeColor = PharmaTheme.OnSurfaceVariant;
        base.ApplyThemeVisuals();
    }

    private void LayoutBar()
    {
        _prevButton.Location = new Point(16, 14);
        _nextButton.Location = new Point(Width - _nextButton.Width - 16, 14);
        _infoLabel.SetBounds((Width - 160) / 2, 14, 160, 24);
    }
}

internal sealed class CusFieldLabel : Label
{
    public CusFieldLabel(string text)
    {
        Text = text;
        AutoSize = false;
        Height = 22;
        Dock = DockStyle.Top;
        TextAlign = ContentAlignment.MiddleRight;
        Font = PharmaTheme.SmallFont;
        ForeColor = PharmaTheme.MutedText;
        BackColor = PharmaTheme.Surface;
        Padding = new Padding(0, 0, 0, 6);
    }

    public void ApplyThemeVisuals() => ForeColor = PharmaTheme.MutedText;
}

internal sealed class CusInputHost : Panel
{
    private readonly int _radius;
    private bool _focused;

    public CusInputHost(int radius = 12)
    {
        _radius = radius;
        DoubleBuffered = true;
        BackColor = PharmaTheme.Surface;
        Height = 44;
        Padding = new Padding(12, 0, 12, 0);
        SetStyle(
            ControlStyles.AllPaintingInWmPaint
                | ControlStyles.UserPaint
                | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.ResizeRedraw,
            true);
    }

    public void ApplyThemeVisuals() => Invalidate(true);

    public void SetFocused(bool focused)
    {
        if (_focused == focused)
        {
            return;
        }

        _focused = focused;
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var bounds = ClientRectangle;
        bounds.Inflate(-1, -1);
        if (bounds.Width <= 4 || bounds.Height <= 4)
        {
            return;
        }

        RoundedDrawing.FillRounded(g, bounds, _radius, PharmaTheme.SurfaceContainerHigh);
        RoundedDrawing.DrawRoundedBorder(
            g,
            bounds,
            _radius,
            _focused ? PharmaTheme.Primary : PharmaTheme.BorderSoft,
            _focused ? 1.5f : 1f);
    }

    protected override void OnPaintBackground(PaintEventArgs e) =>
        e.Graphics.Clear(Parent?.BackColor ?? PharmaTheme.Surface);
}

internal sealed class CusFieldStack : Panel
{
    private readonly CusFieldLabel _label;
    private readonly CusInputHost _host;

    public CusFieldStack(string label, Control inner, int hostHeight = 44)
    {
        BackColor = PharmaTheme.Surface;
        _label = new CusFieldLabel(label);
        _host = new CusInputHost { Height = hostHeight, Dock = DockStyle.Top };
        inner.Dock = DockStyle.Fill;
        inner.Margin = new Padding(0);
        _host.Controls.Add(inner);
        _host.BringToFront();
        inner.BringToFront();
        Controls.Add(_host);
        Controls.Add(_label);
        Height = 22 + 6 + hostHeight;
    }

    public CusInputHost Host => _host;

    public void ApplyThemeVisuals()
    {
        _label.ApplyThemeVisuals();
        _host.ApplyThemeVisuals();
    }
}

internal sealed class CusDialogCancelButton : Control
{
    public CusDialogCancelButton()
    {
        SetStyle(
            ControlStyles.AllPaintingInWmPaint
                | ControlStyles.UserPaint
                | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.ResizeRedraw
                | ControlStyles.Selectable,
            true);
        TabStop = true;
        Cursor = Cursors.Hand;
        Size = new Size(120, 46);
        Font = PharmaTheme.ArabicFont(10f, FontStyle.Bold);
        BackColor = PharmaTheme.Surface;
        ForeColor = PharmaTheme.TextDark;
    }

    public void ApplyThemeVisuals()
    {
        BackColor = PharmaTheme.Surface;
        ForeColor = PharmaTheme.TextDark;
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var bounds = ClientRectangle;
        bounds.Inflate(-1, -1);
        RoundedDrawing.FillRounded(g, bounds, 12, PharmaTheme.Surface);
        RoundedDrawing.DrawRoundedBorder(g, bounds, 12, PharmaTheme.BorderSoft, 1f);
        TextRenderer.DrawText(
            g,
            "إلغاء",
            Font,
            bounds,
            ForeColor,
            TextFormatFlags.HorizontalCenter
                | TextFormatFlags.VerticalCenter
                | TextFormatFlags.RightToLeft);
    }

    protected override void OnPaintBackground(PaintEventArgs e) =>
        e.Graphics.Clear(Parent?.BackColor ?? PharmaTheme.Background);

    protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        Invalidate();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        Invalidate();
    }
}
