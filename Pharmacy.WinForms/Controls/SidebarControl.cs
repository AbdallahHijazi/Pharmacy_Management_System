using System.ComponentModel;
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
        Dock = DockStyle.Right;
        Width = 248;
        BackColor = PharmaTheme.SidebarBackground;
        Padding = new Padding(12, 20, 12, 16);
        AutoScroll = true;

        var brand = new Label
        {
            AutoSize = false,
            Dock = DockStyle.Top,
            Font = new Font("Segoe UI", 15F, FontStyle.Bold),
            ForeColor = Color.White,
            Height = 52,
            Padding = new Padding(8, 0, 8, 8),
            Text = "PharmaCare",
            TextAlign = ContentAlignment.MiddleRight
        };
        Controls.Add(brand);

        var subtitle = new Label
        {
            AutoSize = false,
            Dock = DockStyle.Top,
            Font = PharmaTheme.SmallFont,
            ForeColor = Color.FromArgb(190, 220, 205),
            Height = 28,
            Padding = new Padding(8, 0, 8, 16),
            Text = "نظام إدارة الصيدلية",
            TextAlign = ContentAlignment.MiddleRight
        };
        Controls.Add(subtitle);

        var navHost = new FlowLayoutPanel
        {
            AutoScroll = true,
            BackColor = PharmaTheme.SidebarBackground,
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            Padding = new Padding(0, 8, 0, 8),
            WrapContents = false
        };
        Controls.Add(navHost);
        navHost.BringToFront();

        AddNavItem(navHost, AppNavigation.Dashboard, "لوحة التحكم", "⌂");
        AddNavItem(navHost, AppNavigation.Inventory, "المخزون", "▦");
        AddNavItem(navHost, AppNavigation.PointOfSale, "نقطة البيع", "₪");
        AddNavItem(navHost, AppNavigation.Purchases, "المشتريات", "↧");
        AddNavItem(navHost, AppNavigation.Customers, "الزبائن", "👤");
        AddNavItem(navHost, AppNavigation.Suppliers, "الموردين", "🏭");
        AddNavItem(navHost, AppNavigation.Reports, "التقارير", "📊");
        AddNavItem(navHost, AppNavigation.Users, "المستخدمين", "👥");
        AddNavItem(navHost, AppNavigation.Settings, "الإعدادات", "⚙");

        var logout = CreateButton("تسجيل الخروج", "⎋", isLogout: true);
        logout.Width = navHost.Width - navHost.Padding.Horizontal;
        logout.Click += (_, _) => LogoutRequested?.Invoke(this, EventArgs.Empty);
        navHost.Controls.Add(logout);
        navHost.Resize += (_, _) =>
        {
            foreach (Control control in navHost.Controls)
            {
                control.Width = navHost.ClientSize.Width - navHost.Padding.Horizontal;
            }
        };

        SetActive(AppNavigation.Dashboard);
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
            BackColor = PharmaTheme.SidebarBackground;
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
            if (_isActive || _isHover || IsLogoutStyle)
            {
                var back = _isActive
                    ? PharmaTheme.SidebarActive
                    : _isHover
                        ? PharmaTheme.SidebarHover
                        : Color.FromArgb(48, 18, 18);
                using var brush = new SolidBrush(back);
                e.Graphics.FillRectangle(brush, ClientRectangle);
            }
            else
            {
                using var brush = new SolidBrush(BackColor);
                e.Graphics.FillRectangle(brush, ClientRectangle);
            }

            var iconRect = new Rectangle(Width - 40, 8, 28, 28);
            TextRenderer.DrawText(
                e.Graphics,
                IconText,
                PharmaTheme.BodyFont,
                iconRect,
                Color.White,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

            var textRect = new Rectangle(12, 0, Width - 52, Height);
            TextRenderer.DrawText(
                e.Graphics,
                ButtonText,
                new Font("Segoe UI", 10.25F, _isActive ? FontStyle.Bold : FontStyle.Regular),
                textRect,
                Color.White,
                TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }
    }
}
