using System.Drawing.Drawing2D;
using System.Globalization;
using Pharmacy.WinForms.Services;
using Pharmacy.WinForms.Ui;

namespace Pharmacy.WinForms.Controls;

public sealed class TopBarControl : Panel
{
    private readonly ToolTip _toolTips = new();
    private readonly Label _userNameLabel;
    private readonly Label _roleLabel;
    private readonly TextBox _searchBox;
    private readonly Panel _searchShell;
    private readonly Label _searchIconLabel;
    private readonly FlowLayoutPanel _actionsFlow;
    private readonly Label _dateLabel;

    public event EventHandler? LogoutRequested;
    public event EventHandler<string>? SearchSubmitted;

    public TopBarControl()
    {
        Dock = DockStyle.Top;
        Height = 84;
        RightToLeft = RightToLeft.Yes;
        Padding = new Padding(24, 14, 28, 14);
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);

        _searchShell = new Panel
        {
            BackColor = PharmaTheme.SurfaceContainerHighest,
            Height = 48
        };
        _searchShell.Paint += (_, e) =>
        {
            RoundedDrawing.FillRounded(e.Graphics, _searchShell.ClientRectangle, PharmaTheme.DashboardSearchCornerRadius, PharmaTheme.SurfaceContainerHighest);
            RoundedDrawing.DrawRoundedBorder(e.Graphics, _searchShell.ClientRectangle, PharmaTheme.DashboardSearchCornerRadius, Color.FromArgb(40, PharmaTheme.OutlineVariant));
        };

        _searchIconLabel = new Label
        {
            AutoSize = false,
            BackColor = PharmaTheme.SurfaceContainerHighest,
            Font = PharmaTheme.IconFont(14f),
            ForeColor = PharmaTheme.OnSurfaceVariant,
            Size = new Size(36, 48),
            Text = SegoeMdl2Icons.Search,
            TextAlign = ContentAlignment.MiddleCenter
        };

        _searchBox = new TextBox
        {
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            BackColor = PharmaTheme.SurfaceContainerHighest,
            BorderStyle = BorderStyle.None,
            Font = PharmaTheme.BodyFont,
            ForeColor = PharmaTheme.TextDark,
            Location = new Point(8, 12),
            PlaceholderText = "ابحث عن دواء، مريض، أو فاتورة...",
            RightToLeft = RightToLeft.Yes,
            Size = new Size(280, 26)
        };
        _searchShell.Controls.Add(_searchBox);
        _searchShell.Controls.Add(_searchIconLabel);
        _searchShell.Resize += (_, _) => LayoutSearch();

        _searchBox.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Enter)
            {
                SearchSubmitted?.Invoke(this, _searchBox.Text.Trim());
            }
        };

        _actionsFlow = new FlowLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            BackColor = PharmaTheme.SoftGreenBackground,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false
        };

        _dateLabel = new Label
        {
            AutoSize = true,
            Font = PharmaTheme.SmallFont,
            ForeColor = PharmaTheme.OnSurfaceVariant,
            Margin = new Padding(8, 12, 10, 0),
            Text = FormatTodayArabic(),
            TextAlign = ContentAlignment.MiddleRight
        };

        var notificationsButton = new TopBarIconButton(SegoeMdl2Icons.Notification, "الإشعارات");
        var themeButton = new TopBarIconButton("\uE708", "المظهر");
        var logoutButton = new TopBarIconButton(SegoeMdl2Icons.SignOut, "تسجيل الخروج");
        logoutButton.Click += (_, _) => LogoutRequested?.Invoke(this, EventArgs.Empty);

        _userNameLabel = new Label
        {
            AutoSize = false,
            Font = PharmaTheme.ArabicFont(10.5f, FontStyle.Bold),
            ForeColor = PharmaTheme.TextDark,
            Margin = new Padding(6, 2, 0, 0),
            Size = new Size(160, 22),
            TextAlign = ContentAlignment.MiddleRight
        };

        _roleLabel = new Label
        {
            AutoSize = false,
            Font = PharmaTheme.SmallFont,
            ForeColor = PharmaTheme.MutedText,
            Margin = new Padding(6, 0, 0, 0),
            Size = new Size(160, 18),
            TextAlign = ContentAlignment.MiddleRight
        };

        var userStack = new FlowLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            BackColor = PharmaTheme.SoftGreenBackground,
            FlowDirection = FlowDirection.TopDown,
            Margin = new Padding(4, 0, 0, 0),
            WrapContents = false
        };
        userStack.Controls.Add(_userNameLabel);
        userStack.Controls.Add(_roleLabel);

        _actionsFlow.Controls.Add(userStack);
        _actionsFlow.Controls.Add(logoutButton);
        _actionsFlow.Controls.Add(themeButton);
        _actionsFlow.Controls.Add(notificationsButton);
        _actionsFlow.Controls.Add(_dateLabel);

        Controls.Add(_actionsFlow);
        Controls.Add(_searchShell);

        BindUser();
        Resize += (_, _) => LayoutChrome();
        LayoutChrome();
    }

    private void LayoutSearch()
    {
        _searchIconLabel.Left = _searchShell.ClientSize.Width - _searchIconLabel.Width - 4;
        _searchIconLabel.Top = 0;
        _searchBox.Width = Math.Max(100, _searchShell.ClientSize.Width - _searchIconLabel.Width - 12);
        _searchBox.Top = (_searchShell.ClientSize.Height - _searchBox.Height) / 2;
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        using var brush = new LinearGradientBrush(
            ClientRectangle,
            PharmaTheme.SoftGreenBackground,
            PharmaTheme.TopBarGradientDeep,
            LinearGradientMode.Vertical);
        e.Graphics.FillRectangle(brush, ClientRectangle);
    }

    public void BindUser()
    {
        var user = SessionManager.CurrentUser;
        _userNameLabel.Text = user?.FullName ?? "مستخدم";
        _roleLabel.Text = user?.Role ?? "—";
    }

    private void LayoutChrome()
    {
        var pad = Padding;
        var innerW = ClientSize.Width - pad.Horizontal;
        var innerH = ClientSize.Height - pad.Vertical;
        var searchW = Math.Clamp((int)(innerW * 0.38), 260, 460);
        _searchShell.SetBounds(pad.Left + innerW - searchW, pad.Top + (innerH - _searchShell.Height) / 2, searchW, _searchShell.Height);
        LayoutSearch();

        _actionsFlow.PerformLayout();
        var flowW = Math.Min(_actionsFlow.PreferredSize.Width + 8, Math.Max(140, innerW - searchW - 28));
        _actionsFlow.SetBounds(
            pad.Left,
            pad.Top + (innerH - Math.Max(_actionsFlow.PreferredSize.Height, 48)) / 2,
            flowW,
            Math.Max(_actionsFlow.PreferredSize.Height, 48));
    }

    private static string FormatTodayArabic()
    {
        try
        {
            return DateTime.Now.ToString("dddd، d MMMM yyyy", new CultureInfo("ar-SA"));
        }
        catch (Exception)
        {
            return DateTime.Now.ToString("d", CultureInfo.CurrentCulture);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _toolTips.Dispose();
        }

        base.Dispose(disposing);
    }

    private sealed class TopBarIconButton : Control
    {
        private bool _isHover;
        private readonly string _glyph;

        public TopBarIconButton(string glyph, string tooltip)
        {
            _glyph = glyph;
            Size = new Size(44, 44);
            Cursor = Cursors.Hand;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
            var tip = new ToolTip();
            tip.SetToolTip(this, tooltip);
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
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var bounds = ClientRectangle;
            bounds.Inflate(-2, -2);
            var fill = _isHover ? Color.FromArgb(36, PharmaTheme.PrimaryContainer) : Color.FromArgb(18, PharmaTheme.PrimaryContainer);
            RoundedDrawing.FillRounded(g, bounds, bounds.Height / 2, fill);
            TextRenderer.DrawText(
                g,
                _glyph,
                PharmaTheme.IconFont(14f),
                bounds,
                PharmaTheme.PrimaryGreen,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }
    }
}
