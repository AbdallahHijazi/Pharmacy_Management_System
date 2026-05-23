using System.ComponentModel;
using Pharmacy.WinForms.Models;
using Pharmacy.WinForms.Ui;

namespace Pharmacy.WinForms.Controls;

/// <summary>Shared application sidebar — hosted only by <see cref="Forms.MainForm"/>.</summary>
public sealed class SidebarControl : Panel
{
    private readonly Dictionary<AppNavigation, SidebarButton> _buttons = new();
    private readonly LogoBadgeControl _logoBadge;
    private readonly TableLayoutPanel _root;
    private FlowLayoutPanel _navHost = null!;
    private Label _brandTitle = null!;
    private Label _brandSubtitle = null!;

    public event EventHandler<AppNavigation>? NavigationRequested;
    public event EventHandler? LogoutRequested;

    public SidebarControl()
    {
        Width = AppShellLayout.SidebarColumnWidth + 8;
        MinimumSize = new Size(AppShellLayout.SidebarColumnWidth, 0);
        BackColor = PharmaTheme.SidebarLightBackground;
        Padding = new Padding(0);
        RightToLeft = RightToLeft.Yes;
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);

        _root = new TableLayoutPanel
        {
            ColumnCount = 1,
            Dock = DockStyle.Fill,
            Margin = new Padding(0),
            Padding = new Padding(16, 20, 16, 14),
            RightToLeft = RightToLeft.Yes,
            RowCount = 3
        };
        _root.RowStyles.Add(new RowStyle(SizeType.Absolute, 118F));
        _root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        _root.RowStyles.Add(new RowStyle(SizeType.Absolute, 64F));

        var brandPanel = new Panel
        {
            BackColor = PharmaTheme.SidebarLightBackground,
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, 0, 10)
        };
        _logoBadge = new LogoBadgeControl { Size = new Size(60, 60) };
        _brandTitle = new Label
        {
            AutoSize = false,
            BackColor = PharmaTheme.SidebarLightBackground,
            Font = PharmaTheme.SidebarBrandFont,
            ForeColor = PharmaTheme.Primary,
            Size = new Size(220, 32),
            Text = "PharmaCare",
            TextAlign = ContentAlignment.MiddleCenter,
            UseCompatibleTextRendering = true
        };
        _brandSubtitle = new Label
        {
            AutoSize = false,
            BackColor = PharmaTheme.SidebarLightBackground,
            Font = PharmaTheme.SidebarSubtitleFont,
            ForeColor = PharmaTheme.OnSurfaceVariant,
            Size = new Size(220, 24),
            Text = UiBranding.PharmacyDisplayName,
            TextAlign = ContentAlignment.MiddleCenter,
            UseCompatibleTextRendering = true
        };
        brandPanel.Controls.Add(_logoBadge);
        brandPanel.Controls.Add(_brandTitle);
        brandPanel.Controls.Add(_brandSubtitle);
        brandPanel.Resize += (_, _) => LayoutBrandStack(brandPanel, _logoBadge, _brandTitle, _brandSubtitle);
        LayoutBrandStack(brandPanel, _logoBadge, _brandTitle, _brandSubtitle);

        UiBranding.PharmacyDisplayNameChanged += OnPharmacyDisplayNameChanged;

        _navHost = new FlowLayoutPanel
        {
            AutoScroll = true,
            BackColor = PharmaTheme.SidebarLightBackground,
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            Margin = new Padding(0, 6, 0, 0),
            Padding = new Padding(0, 4, 0, 8),
            WrapContents = false
        };

        AddNavItem(_navHost, AppNavigation.Dashboard, "لوحة التحكم", SegoeMdl2Icons.Dashboard);
        AddNavItem(_navHost, AppNavigation.Inventory, "المخزون", SegoeMdl2Icons.Inventory);
        AddNavItem(_navHost, AppNavigation.PointOfSale, "نقطة البيع", SegoeMdl2Icons.PointOfSale);
        AddNavItem(_navHost, AppNavigation.Purchases, "المشتريات", SegoeMdl2Icons.Purchases);
        AddNavItem(_navHost, AppNavigation.Customers, "الزبائن", SegoeMdl2Icons.Customers);
        AddNavItem(_navHost, AppNavigation.Suppliers, "الموردين", SegoeMdl2Icons.Suppliers);
        AddNavItem(_navHost, AppNavigation.Reports, "التقارير", SegoeMdl2Icons.Reports);
        AddNavItem(_navHost, AppNavigation.Users, "المستخدمين", SegoeMdl2Icons.Users);
        AddNavItem(_navHost, AppNavigation.Settings, "الإعدادات", SegoeMdl2Icons.Settings);

        _navHost.Resize += (_, _) =>
        {
            foreach (Control control in _navHost.Controls)
            {
                control.Width = _navHost.ClientSize.Width - _navHost.Padding.Horizontal;
            }
        };

        var logoutRow = new Panel
        {
            BackColor = PharmaTheme.SidebarLightBackground,
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 6, 0, 0),
            Padding = new Padding(0, 12, 0, 0)
        };
        logoutRow.Paint += (_, e) =>
        {
            using var pen = new Pen(PharmaTheme.SidebarDividerLine);
            e.Graphics.DrawLine(pen, 0, 0, logoutRow.ClientSize.Width, 0);
        };
        var logout = CreateButton("تسجيل الخروج", SegoeMdl2Icons.SignOut, isLogout: true);
        logout.Dock = DockStyle.Top;
        logout.Height = 48;
        logout.Click += (_, _) => LogoutRequested?.Invoke(this, EventArgs.Empty);
        logoutRow.Controls.Add(logout);

        _root.Controls.Add(brandPanel, 0, 0);
        _root.Controls.Add(_navHost, 0, 1);
        _root.Controls.Add(logoutRow, 0, 2);
        Controls.Add(_root);

        SetActive(AppNavigation.Dashboard);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            UiBranding.PharmacyDisplayNameChanged -= OnPharmacyDisplayNameChanged;
        }

        base.Dispose(disposing);
    }

    public void RefreshChrome()
    {
        BackColor = PharmaTheme.SidebarLightBackground;
        _root.BackColor = PharmaTheme.SidebarLightBackground;
        _navHost.BackColor = PharmaTheme.SidebarLightBackground;

        _brandTitle.Font = PharmaTheme.SidebarBrandFont;
        _brandTitle.ForeColor = PharmaTheme.Primary;
        _brandTitle.BackColor = PharmaTheme.SidebarLightBackground;

        _brandSubtitle.Font = PharmaTheme.SidebarSubtitleFont;
        _brandSubtitle.ForeColor = PharmaTheme.OnSurfaceVariant;
        _brandSubtitle.BackColor = PharmaTheme.SidebarLightBackground;

        foreach (var button in _buttons.Values)
        {
            button.BackColor = PharmaTheme.SidebarLightBackground;
            button.Invalidate();
        }

        _logoBadge.Invalidate();
        Invalidate(true);
    }

    private void OnPharmacyDisplayNameChanged(object? sender, EventArgs e)
    {
        _brandSubtitle.Text = UiBranding.PharmacyDisplayName;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        using var edge = new Pen(PharmaTheme.SidebarEdgeLine);
        e.Graphics.DrawLine(edge, 0, 0, 0, Height);
    }

    private static void LayoutBrandStack(Panel brand, Control badge, Label title, Label subtitle)
    {
        var w = Math.Max(1, brand.ClientSize.Width);
        badge.Left = (w - badge.Width) / 2;
        badge.Top = 0;
        title.Width = Math.Min(220, w);
        subtitle.Width = title.Width;
        title.Left = (w - title.Width) / 2;
        title.Top = badge.Bottom + 8;
        subtitle.Left = (w - subtitle.Width) / 2;
        subtitle.Top = title.Bottom + 2;
    }

    public void SetActive(AppNavigation navigation)
    {
        foreach (var pair in _buttons)
        {
            pair.Value.IsActive = pair.Key == navigation;
        }
    }

    private void AddNavItem(FlowLayoutPanel host, AppNavigation navigation, string text, string iconGlyph)
    {
        var button = CreateButton(text, iconGlyph);
        button.Width = Math.Max(180, host.ClientSize.Width - host.Padding.Horizontal);
        button.Click += (_, _) =>
        {
            NavigationRequested?.Invoke(this, navigation);
        };
        _buttons[navigation] = button;
        host.Controls.Add(button);
    }

    private static SidebarButton CreateButton(string text, string iconGlyph, bool isLogout = false)
    {
        return new SidebarButton
        {
            ButtonText = text,
            IconGlyph = iconGlyph,
            Height = 48,
            IsLogoutStyle = isLogout,
            Margin = new Padding(0, 0, 0, 10)
        };
    }

    private sealed class LogoBadgeControl : Control
    {
        public LogoBadgeControl()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            var rect = new Rectangle(2, 2, Width - 4, Height - 4);
            RoundedDrawing.DrawSoftShadow(e.Graphics, rect, rect.Width / 2, PharmaTheme.DashboardCardShadow, 2);
            using var fill = new SolidBrush(PharmaTheme.PrimaryContainer);
            e.Graphics.FillEllipse(fill, rect);
            TextRenderer.DrawText(
                e.Graphics,
                SegoeMdl2Icons.Pharmacy,
                PharmaTheme.IconFont(22f),
                rect,
                PharmaTheme.Primary,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }
    }

    private sealed class SidebarButton : Control
    {
        private bool _isActive;
        private bool _isHover;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        internal string ButtonText { get; set; } = string.Empty;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        internal string IconGlyph { get; set; } = SegoeMdl2Icons.Dashboard;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        internal bool IsLogoutStyle { get; set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        internal bool IsActive
        {
            get => _isActive;
            set
            {
                _isActive = value;
                Invalidate();
            }
        }

        public SidebarButton()
        {
            BackColor = PharmaTheme.SidebarLightBackground;
            Cursor = Cursors.Hand;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            _isHover = true;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _isHover = false;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            var bounds = ClientRectangle;
            bounds.Inflate(-1, -1);

            Color back;
            Color textColor;
            Color iconColor;
            if (_isActive)
            {
                back = PharmaTheme.SidebarNavActiveFill;
                textColor = PharmaTheme.SidebarNavActiveText;
                iconColor = PharmaTheme.SidebarNavActiveIcon;
            }
            else if (_isHover)
            {
                back = PharmaTheme.SidebarNavHoverFill;
                textColor = PharmaTheme.TextDark;
                iconColor = PharmaTheme.Primary;
            }
            else if (IsLogoutStyle)
            {
                back = PharmaTheme.SidebarLightBackground;
                textColor = PharmaTheme.Danger;
                iconColor = PharmaTheme.Danger;
            }
            else
            {
                back = PharmaTheme.SidebarLightBackground;
                textColor = PharmaTheme.TextDark;
                iconColor = PharmaTheme.Primary;
            }

            RoundedDrawing.FillRounded(g, bounds, PharmaTheme.DashboardSidebarItemRadius, back);

            var iconRect = new Rectangle(bounds.Right - 46, bounds.Y + (bounds.Height - 30) / 2, 30, 30);
            TextRenderer.DrawText(
                g,
                IconGlyph,
                PharmaTheme.IconFont(15f),
                iconRect,
                iconColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

            var navFont = IsLogoutStyle ? PharmaTheme.SidebarLogoutFont : PharmaTheme.SidebarNavFont;
            var textRect = TextLayoutHelper.DeflateVertical(
                new Rectangle(bounds.X + 14, bounds.Y, bounds.Width - 60, bounds.Height),
                3);
            TextRenderer.DrawText(
                g,
                ButtonText,
                navFont,
                textRect,
                textColor,
                TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
        }
    }
}
