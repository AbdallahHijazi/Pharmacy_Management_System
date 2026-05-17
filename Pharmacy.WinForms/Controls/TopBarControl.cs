using Pharmacy.WinForms.Services;
using Pharmacy.WinForms.Ui;

namespace Pharmacy.WinForms.Controls;

public sealed class TopBarControl : Panel
{
    private readonly Label _userNameLabel;
    private readonly Label _roleLabel;
    private readonly TextBox _searchBox;

    public event EventHandler? LogoutRequested;
    public event EventHandler<string>? SearchSubmitted;

    public TopBarControl()
    {
        Dock = DockStyle.Top;
        Height = 72;
        BackColor = PharmaTheme.CardBackground;
        Padding = new Padding(20, 14, 20, 14);

        var searchPanel = new Panel
        {
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            BackColor = PharmaTheme.SoftGreenBackground,
            Location = new Point(20, 16),
            Size = new Size(320, 40)
        };
        Controls.Add(searchPanel);

        _searchBox = new TextBox
        {
            BackColor = PharmaTheme.SoftGreenBackground,
            BorderStyle = BorderStyle.None,
            Dock = DockStyle.Fill,
            Font = PharmaTheme.BodyFont,
            ForeColor = PharmaTheme.TextDark,
            PlaceholderText = "بحث عن منتج، فاتورة، زبون...",
            RightToLeft = RightToLeft.Yes
        };
        _searchBox.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Enter)
            {
                SearchSubmitted?.Invoke(this, _searchBox.Text.Trim());
            }
        };
        searchPanel.Controls.Add(_searchBox);

        var notificationsButton = CreateIconButton("🔔", "الإشعارات");
        notificationsButton.Anchor = AnchorStyles.Top | AnchorStyles.Left;
        notificationsButton.Location = new Point(20, 16);
        Controls.Add(notificationsButton);

        var logoutButton = CreateIconButton("⎋", "تسجيل الخروج");
        logoutButton.Anchor = AnchorStyles.Top | AnchorStyles.Left;
        logoutButton.Location = new Point(72, 16);
        logoutButton.Click += (_, _) => LogoutRequested?.Invoke(this, EventArgs.Empty);
        Controls.Add(logoutButton);

        _userNameLabel = new Label
        {
            Anchor = AnchorStyles.Top | AnchorStyles.Left,
            AutoSize = false,
            Font = PharmaTheme.SectionFont,
            ForeColor = PharmaTheme.TextDark,
            Location = new Point(140, 14),
            Size = new Size(260, 24),
            TextAlign = ContentAlignment.MiddleLeft
        };
        Controls.Add(_userNameLabel);

        _roleLabel = new Label
        {
            Anchor = AnchorStyles.Top | AnchorStyles.Left,
            AutoSize = false,
            Font = PharmaTheme.SmallFont,
            ForeColor = PharmaTheme.MutedText,
            Location = new Point(140, 38),
            Size = new Size(260, 20),
            TextAlign = ContentAlignment.MiddleLeft
        };
        Controls.Add(_roleLabel);

        BindUser();
        Resize += (_, _) => LayoutTopBar();
        LayoutTopBar();
    }

    public void BindUser()
    {
        var user = SessionManager.CurrentUser;
        _userNameLabel.Text = user?.FullName ?? "مستخدم";
        _roleLabel.Text = user?.Role ?? "—";
    }

    private void LayoutTopBar()
    {
        var right = Width - 20;
        var searchPanel = _searchBox.Parent!;
        searchPanel.Width = Math.Min(360, Math.Max(220, Width / 3));
        searchPanel.Location = new Point(right - searchPanel.Width, 16);
    }

    private static Button CreateIconButton(string icon, string tooltip)
    {
        var button = new Button
        {
            BackColor = PharmaTheme.SoftGreenBackground,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 12F),
            Size = new Size(44, 40),
            Text = icon,
            UseVisualStyleBackColor = false
        };
        button.FlatAppearance.BorderColor = PharmaTheme.BorderLight;
        button.FlatAppearance.BorderSize = 1;
        var tip = new ToolTip();
        tip.SetToolTip(button, tooltip);
        return button;
    }
}
