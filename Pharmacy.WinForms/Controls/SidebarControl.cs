using System.ComponentModel;
using Pharmacy.WinForms.Models;
using Pharmacy.WinForms.Ui;

namespace Pharmacy.WinForms.Controls;

public sealed class SidebarControl : Panel
{
    private readonly Dictionary<AppNavigation, SidebarButton> _buttons = new();

    public event EventHandler<AppNavigation>? NavigationRequested;
    public event EventHandler? LogoutRequested;

    public SidebarControl()
    {
        Width = 268;
        MinimumSize = new Size(248, 0);
        BackColor = PharmaTheme.SidebarLightBackground;
        Padding = new Padding(0);
        RightToLeft = RightToLeft.Yes;
        SetStyle(ControlStyles.ResizeRedraw, true);

        var root = new TableLayoutPanel
        {
            ColumnCount = 1,
            Dock = DockStyle.Fill,
            Margin = new Padding(0),
            Padding = new Padding(16, 20, 16, 14),
            RightToLeft = RightToLeft.Yes,
            RowCount = 3
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 118F));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 64F));

        var brandPanel = new Panel { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 0, 10) };
        var badge = new LogoBadgeControl { Size = new Size(60, 60) };
        var title = new Label
        {
            AutoSize = false,
            Font = PharmaTheme.ArabicFont(16f, FontStyle.Bold),
            ForeColor = PharmaTheme.PrimaryGreen,
            Size = new Size(220, 28),
            Text = "PharmaCare",
            TextAlign = ContentAlignment.MiddleCenter
        };
        var subtitle = new Label
        {
            AutoSize = false,
            Font = PharmaTheme.ArabicFont(9.5f),
            ForeColor = PharmaTheme.OnSurfaceVariant,
            Size = new Size(220, 22),
            Text = "صيدلية الشفاء",
            TextAlign = ContentAlignment.MiddleCenter
        };
        brandPanel.Controls.Add(badge);
        brandPanel.Controls.Add(title);
        brandPanel.Controls.Add(subtitle);
        brandPanel.Resize += (_, _) => LayoutBrandStack(brandPanel, badge, title, subtitle);
        LayoutBrandStack(brandPanel, badge, title, subtitle);

        var navHost = new FlowLayoutPanel
        {
            AutoScroll = true,
            BackColor = PharmaTheme.SidebarLightBackground,
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            Margin = new Padding(0, 6, 0, 0),
            Padding = new Padding(0, 4, 0, 8),
            WrapContents = false
        };

        AddNavItem(navHost, AppNavigation.Dashboard, "لوحة التحكم", SegoeMdl2Icons.Dashboard);
        AddNavItem(navHost, AppNavigation.Inventory, "المخزون", SegoeMdl2Icons.Inventory);
        AddNavItem(navHost, AppNavigation.PointOfSale, "نقطة البيع", SegoeMdl2Icons.PointOfSale);
        AddNavItem(navHost, AppNavigation.Purchases, "المشتريات", SegoeMdl2Icons.Purchases);
        AddNavItem(navHost, AppNavigation.Customers, "الزبائن", SegoeMdl2Icons.Customers);
        AddNavItem(navHost, AppNavigation.Suppliers, "الموردين", SegoeMdl2Icons.Suppliers);
        AddNavItem(navHost, AppNavigation.Reports, "التقارير", SegoeMdl2Icons.Reports);
        AddNavItem(navHost, AppNavigation.Users, "المستخدمين", SegoeMdl2Icons.Users);
        AddNavItem(navHost, AppNavigation.Settings, "الإعدادات", SegoeMdl2Icons.Settings);

        navHost.Resize += (_, _) =>
        {
            foreach (Control control in navHost.Controls)
            {
                control.Width = navHost.ClientSize.Width - navHost.Padding.Horizontal;
            }
        };

        var logoutRow = new Panel
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 6, 0, 0),
            Padding = new Padding(0, 12, 0, 0)
        };
        logoutRow.Paint += (_, e) =>
        {
            using var pen = new Pen(Color.FromArgb(50, PharmaTheme.SidebarDivider));
            e.Graphics.DrawLine(pen, 0, 0, logoutRow.ClientSize.Width, 0);
        };
        var logout = CreateButton("تسجيل الخروج", SegoeMdl2Icons.SignOut, isLogout: true);
        logout.Dock = DockStyle.Top;
        logout.Height = 46;
        logout.Click += (_, _) => LogoutRequested?.Invoke(this, EventArgs.Empty);
        logoutRow.Controls.Add(logout);

        root.Controls.Add(brandPanel, 0, 0);
        root.Controls.Add(navHost, 0, 1);
        root.Controls.Add(logoutRow, 0, 2);
        Controls.Add(root);

        SetActive(AppNavigation.Dashboard);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        using var edge = new Pen(Color.FromArgb(22, PharmaTheme.PrimaryGreen));
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
        button.Width = host.ClientSize.Width - host.Padding.Horizontal;
        button.Click += (_, _) =>
        {
            SetActive(navigation);
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
            Height = 46,
            IsLogoutStyle = isLogout,
            Margin = new Padding(0, 0, 0, 8)
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
                Color.White,
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
            var bounds = ClientRectangle;
            bounds.Inflate(-1, -1);

            Color back;
            Color textColor;
            Color iconColor;
            if (_isActive)
            {
                back = PharmaTheme.PrimaryContainer;
                textColor = Color.White;
                iconColor = Color.White;
            }
            else if (_isHover)
            {
                back = PharmaTheme.SidebarNavHoverFill;
                textColor = PharmaTheme.TextDark;
                iconColor = PharmaTheme.PrimaryGreen;
            }
            else
            {
                back = PharmaTheme.SidebarLightBackground;
                textColor = PharmaTheme.TextDark;
                iconColor = PharmaTheme.PrimaryGreen;
            }

            RoundedDrawing.FillRounded(g, bounds, PharmaTheme.DashboardSidebarItemRadius, back);

            var iconRect = new Rectangle(bounds.Right - 42, bounds.Y + (bounds.Height - 28) / 2, 28, 28);
            TextRenderer.DrawText(
                g,
                IconGlyph,
                PharmaTheme.IconFont(14f),
                iconRect,
                iconColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

            var textRect = new Rectangle(bounds.X + 12, bounds.Y, bounds.Width - 54, bounds.Height);
            TextRenderer.DrawText(
                g,
                ButtonText,
                PharmaTheme.ArabicFont(10.5f, _isActive ? FontStyle.Bold : FontStyle.Regular),
                textRect,
                textColor,
                TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }
    }
}
