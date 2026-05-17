using System.Drawing;
using System.Windows.Forms;

namespace Pharmacy.WinForms.Forms;

public partial class LoginForm
{
    private AmbientFormSurface ambientSurface = null!;
    private TableLayoutPanel mainLayout = null!;
    private Panel loginSide = null!;
    private DnaPanel dnaSide = null!;
    private TableLayoutPanel loginOuter = null!;
    private TableLayoutPanel loginStack = null!;
    private RoundedTextInput emailInput = null!;
    private RoundedTextInput passwordInput = null!;
    private CheckBox showPasswordCheckBox = null!;
    private Label errorLabel = null!;
    private RoundedButton loginButton = null!;
    private Panel loadingOverlay = null!;
    private Label loadingLabel = null!;

    private void InitializeComponent()
    {
        SuspendLayout();

        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        BackColor = Color.White;
        ClientSize = new Size(1180, 680);
        DoubleBuffered = true;
        Font = new Font("Segoe UI", 10F);
        FormBorderStyle = FormBorderStyle.Sizable;
        MinimumSize = new Size(900, 560);
        Name = "LoginForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Pharmacy Management System";

        ambientSurface = new AmbientFormSurface();
        Controls.Add(ambientSurface);

        mainLayout = new TableLayoutPanel
        {
            BackColor = Color.Transparent,
            ColumnCount = 2,
            Dock = DockStyle.Fill,
            RowCount = 1
        };
        mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        ambientSurface.Controls.Add(mainLayout);

        BuildLoginSide();
        BuildDnaSide();
        BuildLoadingOverlay();

        ResumeLayout(false);
        PerformLayout();
    }

    private void BuildLoginSide()
    {
        loginSide = new Panel
        {
            BackColor = Color.White,
            Dock = DockStyle.Fill,
            Padding = new Padding(48)
        };
        mainLayout.Controls.Add(loginSide, 0, 0);

        loginOuter = new TableLayoutPanel
        {
            BackColor = Color.Transparent,
            ColumnCount = 1,
            Dock = DockStyle.Fill,
            RowCount = 3
        };
        loginOuter.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
        loginOuter.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        loginOuter.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
        loginSide.Controls.Add(loginOuter);

        loginStack = new TableLayoutPanel
        {
            Anchor = AnchorStyles.None,
            AutoSize = true,
            BackColor = Color.Transparent,
            ColumnCount = 1,
            Margin = new Padding(0),
            MaximumSize = new Size(380, 0),
            MinimumSize = new Size(330, 0),
            RowCount = 12
        };

        loginOuter.Controls.Add(new Panel(), 0, 0);
        loginOuter.Controls.Add(loginStack, 0, 1);
        loginOuter.Controls.Add(new Panel(), 0, 2);

        AddHeader();
        AddLoginControls();
    }

    private void BuildDnaSide()
    {
        dnaSide = new DnaPanel
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0)
        };
        mainLayout.Controls.Add(dnaSide, 1, 0);
    }

    private void BuildLoadingOverlay()
    {
        loadingOverlay = new Panel
        {
            BackColor = Color.FromArgb(140, 255, 255, 255),
            Dock = DockStyle.Fill,
            Visible = false
        };

        loadingLabel = new Label
        {
            AutoSize = true,
            Font = new Font("Segoe UI", 11F, FontStyle.Bold),
            ForeColor = Color.FromArgb(9, 76, 50),
            Text = "جاري تسجيل الدخول...",
            TextAlign = ContentAlignment.MiddleCenter
        };
        loadingOverlay.Controls.Add(loadingLabel);
        loadingOverlay.Resize += (_, _) => CenterLoadingLabel();
        ambientSurface.Controls.Add(loadingOverlay);
        loadingOverlay.BringToFront();
    }

    private void CenterLoadingLabel()
    {
        loadingLabel.Left = (loadingOverlay.ClientSize.Width - loadingLabel.Width) / 2;
        loadingLabel.Top = (loadingOverlay.ClientSize.Height - loadingLabel.Height) / 2;
    }

    private void AddHeader()
    {
        var logoMark = new LogoMark
        {
            Anchor = AnchorStyles.None,
            Margin = new Padding(0, 0, 0, 14),
            Size = new Size(64, 64)
        };
        loginStack.Controls.Add(logoMark, 0, 0);

        loginStack.Controls.Add(new Label
        {
            AutoSize = false,
            Dock = DockStyle.Top,
            Font = new Font("Segoe UI", 20F, FontStyle.Bold),
            ForeColor = Color.FromArgb(9, 76, 50),
            Height = 52,
            Text = "Pharmacy Management System",
            TextAlign = ContentAlignment.MiddleCenter
        }, 0, 1);

        loginStack.Controls.Add(new Label
        {
            AutoSize = false,
            Dock = DockStyle.Top,
            Font = new Font("Segoe UI", 10F),
            ForeColor = Color.FromArgb(102, 118, 111),
            Height = 28,
            Margin = new Padding(0, 0, 0, 24),
            Text = "Sign in to manage your pharmacy.",
            TextAlign = ContentAlignment.MiddleCenter
        }, 0, 2);
    }

    private void AddLoginControls()
    {
        loginStack.Controls.Add(FieldLabel("Email"), 0, 3);
        emailInput = new RoundedTextInput
        {
            Dock = DockStyle.Top,
            Height = 48,
            Margin = new Padding(0, 8, 0, 14),
            PlaceholderText = "admin@pharmacy.com"
        };
        loginStack.Controls.Add(emailInput, 0, 4);

        loginStack.Controls.Add(FieldLabel("Password"), 0, 5);
        passwordInput = new RoundedTextInput
        {
            Dock = DockStyle.Top,
            Height = 48,
            IsPassword = true,
            Margin = new Padding(0, 8, 0, 8),
            PlaceholderText = "Enter your password"
        };
        loginStack.Controls.Add(passwordInput, 0, 6);

        showPasswordCheckBox = new CheckBox
        {
            AutoSize = true,
            Dock = DockStyle.Top,
            Font = new Font("Segoe UI", 9F),
            ForeColor = Color.FromArgb(68, 92, 80),
            Margin = new Padding(0, 0, 0, 10),
            Text = "Show password"
        };
        loginStack.Controls.Add(showPasswordCheckBox, 0, 7);

        errorLabel = new Label
        {
            AutoSize = false,
            Dock = DockStyle.Top,
            Font = new Font("Segoe UI", 9.25F),
            ForeColor = Color.FromArgb(176, 42, 42),
            Height = 44,
            Margin = new Padding(0, 0, 0, 12),
            TextAlign = ContentAlignment.TopLeft,
            Visible = false
        };
        loginStack.Controls.Add(errorLabel, 0, 8);

        loginButton = new RoundedButton
        {
            BorderRadius = 18,
            Dock = DockStyle.Top,
            Font = new Font("Segoe UI", 11F, FontStyle.Bold),
            Height = 50,
            Text = "Login",
            UseVisualStyleBackColor = false
        };
        loginStack.Controls.Add(loginButton, 0, 9);
    }

    private static Label FieldLabel(string text) => new()
    {
        AutoSize = false,
        Dock = DockStyle.Top,
        Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
        ForeColor = Color.FromArgb(34, 64, 51),
        Height = 22,
        Text = text,
        TextAlign = ContentAlignment.BottomLeft
    };
}
