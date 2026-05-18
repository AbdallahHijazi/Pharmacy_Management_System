using System.ComponentModel;
using System.Drawing.Drawing2D;
using Pharmacy.WinForms.Models;
using Pharmacy.WinForms.Ui;

namespace Pharmacy.WinForms.Controls;

public sealed class SidebarControl : Panel
{
    private readonly Dictionary<AppNavigation, SidebarButton> _buttons = new();
    private AppNavigation _active = AppNavigation.Dashboard;

    public event EventHandler<AppNavigation>? NavigationRequested;
    public event EventHandler? LogoutRequested;

    public SidebarControl()
    {
        Width = 260;
        MinimumSize = new Size(240, 0);
        BackColor = PharmaTheme.SidebarLightBackground;
        Padding = new Padding(0);
        RightToLeft = RightToLeft.Yes;
        SetStyle(ControlStyles.ResizeRedraw, true);

        var root = new TableLayoutPanel
        {
            ColumnCount = 1,
            Dock = DockStyle.Fill,
            Margin = new Padding(0),
            Padding = new Padding(14, 18, 14, 12),
            RightToLeft = RightToLeft.Yes,
            RowCount = 3
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 112F));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F));

        var brandPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, 0, 8),
            Padding = new Padding(0, 0, 0, 0)
        };
        var badge = new LogoBadgeControl { Size = new Size(56, 56) };
        var title = new Label
        {
            AutoSize = false,
            Font = new Font("Segoe UI", 15F, FontStyle.Bold),
            ForeColor = PharmaTheme.PrimaryGreen,
            Size = new Size(220, 28),
            Text = "PharmaCare",
            TextAlign = ContentAlignment.MiddleCenter
        };
        var subtitle = new Label
        {
            AutoSize = false,
            Font = PharmaTheme.SmallFont,
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
            Margin = new Padding(0, 4, 0, 0),
            Padding = new Padding(0, 4, 0, 8),
            WrapContents = false
        };

        AddNavItem(navHost, AppNavigation.Dashboard, "لوحة التحكم", "⌂");
        AddNavItem(navHost, AppNavigation.Inventory, "المخزون", "▦");
        AddNavItem(navHost, AppNavigation.PointOfSale, "نقطة البيع", "₪");
        AddNavItem(navHost, AppNavigation.Purchases, "المشتريات", "↧");
        AddNavItem(navHost, AppNavigation.Customers, "الزبائن", "👤");
        AddNavItem(navHost, AppNavigation.Suppliers, "الموردين", "🏭");
        AddNavItem(navHost, AppNavigation.Reports, "التقارير", "📊");
        AddNavItem(navHost, AppNavigation.Users, "المستخدمين", "👥");
        AddNavItem(navHost, AppNavigation.Settings, "الإعدادات", "⚙");

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
            Margin = new Padding(0, 4, 0, 0),
            Padding = new Padding(0, 10, 0, 0)
        };
        logoutRow.Paint += (_, e) =>
        {
            using var pen = new Pen(Color.FromArgb(60, PharmaTheme.SidebarDivider));
            e.Graphics.DrawLine(pen, 0, 0, logoutRow.ClientSize.Width, 0);
        };
        var logout = CreateButton("تسجيل الخروج", "⎋", isLogout: true);
        logout.Dock = DockStyle.Top;
        logout.Height = 44;
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
        using var edge = new Pen(Color.FromArgb(26, PharmaTheme.PrimaryGreen));
        e.Graphics.DrawLine(edge, 0, 0, 0, Height);
    }

    private static void LayoutBrandStack(Panel brand, Control badge, Label title, Label subtitle)
    {
        var w = Math.Max(1, brand.ClientSize.Width);
        badge.Left = (w - badge.Width) / 2;
        badge.Top = 2;
        title.Width = Math.Min(220, w);
        subtitle.Width = title.Width;
        title.Left = (w - title.Width) / 2;
        title.Top = badge.Bottom + 6;
        subtitle.Left = (w - subtitle.Width) / 2;
        subtitle.Top = title.Bottom + 2;
    }

    public void SetActive(AppNavigation navigation)
    {
        _active = navigation;
        foreach (var pair in _buttons)
        {
            pair.Value.IsActive = pair.Key == navigation;
        }
    }

    private void AddNavItem(FlowLayoutPanel host, AppNavigation navigation, string text, string icon)
    {
        var button = CreateButton(text, icon);
        button.Width = host.ClientSize.Width - host.Padding.Horizontal;
        button.Click += (_, _) =>
        {
            SetActive(navigation);
            NavigationRequested?.Invoke(this, navigation);
        };
        _buttons[navigation] = button;
        host.Controls.Add(button);
    }

    private static SidebarButton CreateButton(string text, string icon, bool isLogout = false)
    {
        return new SidebarButton
        {
            ButtonText = text,
            IconText = icon,
            Height = 44,
            IsLogoutStyle = isLogout,
            Margin = new Padding(0, 0, 0, 6)
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
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            var pad = 2;
            var rect = new Rectangle(pad, pad, Width - pad * 2, Height - pad * 2);
            using var fill = new SolidBrush(PharmaTheme.PrimaryContainer);
            e.Graphics.FillEllipse(fill, rect);
            using var sh = new SolidBrush(Color.FromArgb(28, 0, 0, 0));
            e.Graphics.FillEllipse(sh, rect.X, rect.Y + 2, rect.Width, rect.Height);
            e.Graphics.FillEllipse(fill, rect);
            TextRenderer.DrawText(
                e.Graphics,
                "💊",
                new Font("Segoe UI", 20F),
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
        internal string IconText { get; set; } = "●";

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
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
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
            else if (_isHover || (IsLogoutStyle && _isHover))
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

            using (var path = RoundedRect(bounds, 10))
            using (var brush = new SolidBrush(back))
            {
                g.FillPath(brush, path);
            }

            var iconRect = new Rectangle(Width - 40, 8, 28, 28);
            TextRenderer.DrawText(
                g,
                IconText,
                PharmaTheme.BodyFont,
                iconRect,
                iconColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

            var textRect = new Rectangle(12, 0, Width - 52, Height);
            TextRenderer.DrawText(
                g,
                ButtonText,
                new Font("Segoe UI", 10.25F, _isActive ? FontStyle.Bold : FontStyle.Regular),
                textRect,
                textColor,
                TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }

        private static GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            var path = new GraphicsPath();
            var d = Math.Min(radius * 2, Math.Min(bounds.Width, bounds.Height));
            path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
            path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
