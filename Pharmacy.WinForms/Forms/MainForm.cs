using System.Drawing;
using System.Windows.Forms;
using Pharmacy.WinForms.Services;

namespace Pharmacy.WinForms.Forms;

public sealed class MainForm : Form
{
    private readonly AuthService _authService;

    public MainForm(AuthService authService)
    {
        _authService = authService;

        Text = "Pharmacy Management System";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(720, 480);
        Size = new Size(960, 600);
        BackColor = Color.FromArgb(248, 251, 249);
        Font = new Font("Segoe UI", 10F);

        var user = SessionManager.CurrentUser;
        var welcome = user is null
            ? "Welcome"
            : $"Welcome, {user.FullName}";

        var header = new Label
        {
            AutoSize = false,
            Dock = DockStyle.Top,
            Font = new Font("Segoe UI", 22F, FontStyle.Bold),
            ForeColor = Color.FromArgb(9, 76, 50),
            Height = 56,
            Padding = new Padding(32, 24, 32, 0),
            Text = welcome
        };

        var details = new Label
        {
            AutoSize = false,
            Dock = DockStyle.Top,
            Font = new Font("Segoe UI", 10.5F),
            ForeColor = Color.FromArgb(80, 100, 90),
            Height = 72,
            Padding = new Padding(32, 8, 32, 0),
            Text = user is null
                ? "Dashboard placeholder — modules will be added here."
                : $"Email: {user.Email}\r\nRole: {user.Role}\r\nBranch: {user.BranchId}"
        };

        var logoutButton = new Button
        {
            AutoSize = true,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = Color.FromArgb(2, 104, 67),
            Margin = new Padding(32, 24, 32, 32),
            Padding = new Padding(20, 10, 20, 10),
            Text = "Logout",
            UseVisualStyleBackColor = false
        };
        logoutButton.FlatAppearance.BorderSize = 0;
        logoutButton.Click += (_, _) => Close();
        FormClosed += (_, _) => _authService.Logout();

        var footer = new FlowLayoutPanel
        {
            AutoSize = true,
            Dock = DockStyle.Bottom,
            FlowDirection = FlowDirection.LeftToRight,
            Padding = new Padding(0, 0, 0, 16)
        };
        footer.Controls.Add(logoutButton);

        Controls.Add(footer);
        Controls.Add(details);
        Controls.Add(header);
    }
}
