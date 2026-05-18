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
    private readonly FlowLayoutPanel _actionsFlow;
    private readonly Label _dateLabel;

    public event EventHandler? LogoutRequested;
    public event EventHandler<string>? SearchSubmitted;

    public TopBarControl()
    {
        Dock = DockStyle.Top;
        Height = 80;
        RightToLeft = RightToLeft.Yes;
        Padding = new Padding(20, 12, 24, 12);
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);

        _searchShell = new Panel
        {
            BackColor = PharmaTheme.SurfaceContainerHighest,
            Height = 44,
            Width = 320
        };
        _searchShell.Paint += (_, e) => DrawRoundedBorder(e.Graphics, _searchShell.ClientRectangle, 12, PharmaTheme.OutlineVariant);

        _searchBox = new TextBox
        {
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            BackColor = PharmaTheme.SurfaceContainerHighest,
            BorderStyle = BorderStyle.None,
            Font = PharmaTheme.BodyFont,
            ForeColor = PharmaTheme.TextDark,
            Location = new Point(14, 10),
            PlaceholderText = "ابحث عن دواء، مريض، أو فاتورة...",
            RightToLeft = RightToLeft.Yes,
            Size = new Size(292, 24)
        };
        _searchShell.Controls.Add(_searchBox);
        _searchShell.Resize += (_, _) =>
        {
            _searchBox.Width = Math.Max(80, _searchShell.ClientSize.Width - 28);
            _searchBox.Top = (_searchShell.ClientSize.Height - _searchBox.Height) / 2;
        };

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
            BackColor = Color.Transparent,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false
        };

        _dateLabel = new Label
        {
            AutoSize = true,
            Font = PharmaTheme.SmallFont,
            ForeColor = PharmaTheme.OnSurfaceVariant,
            Margin = new Padding(8, 10, 12, 0),
            Text = FormatTodayArabic(),
            TextAlign = ContentAlignment.MiddleRight
        };

        var notificationsButton = CreateIconButton("🔔", "الإشعارات");
        var logoutButton = CreateIconButton("⎋", "تسجيل الخروج");
        logoutButton.Click += (_, _) => LogoutRequested?.Invoke(this, EventArgs.Empty);

        _userNameLabel = new Label
        {
            AutoSize = false,
            Font = PharmaTheme.SectionFont,
            ForeColor = PharmaTheme.TextDark,
            Margin = new Padding(6, 2, 0, 0),
            Size = new Size(180, 22),
            TextAlign = ContentAlignment.MiddleRight
        };

        _roleLabel = new Label
        {
            AutoSize = false,
            Font = PharmaTheme.SmallFont,
            ForeColor = PharmaTheme.MutedText,
            Margin = new Padding(6, 0, 0, 0),
            Size = new Size(180, 18),
            TextAlign = ContentAlignment.MiddleRight
        };

        var userStack = new FlowLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            BackColor = Color.Transparent,
            FlowDirection = FlowDirection.TopDown,
            Margin = new Padding(4, 0, 0, 0),
            Padding = new Padding(0),
            WrapContents = false
        };
        userStack.Controls.Add(_userNameLabel);
        userStack.Controls.Add(_roleLabel);

        _actionsFlow.Controls.Add(userStack);
        _actionsFlow.Controls.Add(logoutButton);
        _actionsFlow.Controls.Add(notificationsButton);
        _actionsFlow.Controls.Add(_dateLabel);

        Controls.Add(_actionsFlow);
        Controls.Add(_searchShell);

        BindUser();
        Resize += (_, _) => LayoutChrome();
        LayoutChrome();
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
        var searchW = Math.Clamp((int)(innerW * 0.36), 220, 420);
        _searchShell.SetBounds(pad.Left + innerW - searchW, pad.Top + (innerH - _searchShell.Height) / 2, searchW, _searchShell.Height);
        _searchShell.PerformLayout();

        _actionsFlow.PerformLayout();
        var flowW = Math.Min(_actionsFlow.PreferredSize.Width + 8, Math.Max(120, innerW - searchW - 24));
        _actionsFlow.SetBounds(
            pad.Left,
            pad.Top + (innerH - Math.Max(_actionsFlow.PreferredSize.Height, 44)) / 2,
            flowW,
            Math.Max(_actionsFlow.PreferredSize.Height, 44));
    }

    private Button CreateIconButton(string icon, string tooltip)
    {
        var button = new Button
        {
            BackColor = Color.FromArgb(24, 45, 125, 90),
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 12F),
            Margin = new Padding(4, 2, 4, 2),
            Size = new Size(44, 40),
            Text = icon,
            UseVisualStyleBackColor = false,
            Cursor = Cursors.Hand
        };
        button.FlatAppearance.BorderSize = 0;
        _toolTips.SetToolTip(button, tooltip);
        return button;
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

    private static void DrawRoundedBorder(Graphics g, Rectangle bounds, int radius, Color color)
    {
        g.SmoothingMode = SmoothingMode.AntiAlias;
        bounds.Inflate(-1, -1);
        if (bounds.Width < 4 || bounds.Height < 4)
        {
            return;
        }

        using var path = RoundedRect(bounds, radius);
        using var pen = new Pen(Color.FromArgb(40, color));
        g.DrawPath(pen, path);
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

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _toolTips.Dispose();
        }

        base.Dispose(disposing);
    }
}
