using System.Drawing;
using System.Windows.Forms;
using Pharmacy.WinForms.Controls;
using Pharmacy.WinForms.Ui;

namespace Pharmacy.WinForms.Forms;

public partial class LoginForm
{
    private LoginBackgroundControl loginBackground = null!;
    private LoginCardPanel loginCard = null!;
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
        BackColor = PharmaTheme.LoginGradientTop;
        ClientSize = new Size(1180, 720);
        DoubleBuffered = true;
        Font = PharmaTheme.BodyFont;
        FormBorderStyle = FormBorderStyle.Sizable;
        MinimumSize = new Size(880, 520);
        Name = "LoginForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "PharmaCare — تسجيل الدخول";

        loginBackground = new LoginBackgroundControl();
        Controls.Add(loginBackground);

        loginCard = new LoginCardPanel();

        loginStack = new TableLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            BackColor = PharmaTheme.LoginCardFill,
            ColumnCount = 1,
            Dock = DockStyle.Top,
            Margin = new Padding(0)
        };

        AddHeader();
        AddLoginControls();

        loginCard.Controls.Add(loginStack);
        loginBackground.SetHostedCard(loginCard);

        BuildLoadingOverlay();

        ResumeLayout(false);
        PerformLayout();
    }

    private void BuildLoadingOverlay()
    {
        loadingOverlay = new Panel
        {
            BackColor = PharmaTheme.LoginOverlayScrim,
            Dock = DockStyle.Fill,
            Visible = false
        };

        loadingLabel = new Label
        {
            AutoSize = true,
            Font = PharmaTheme.SectionFont,
            ForeColor = PharmaTheme.PrimaryGreen,
            Text = "جاري تسجيل الدخول...",
            TextAlign = ContentAlignment.MiddleCenter
        };
        loadingOverlay.Controls.Add(loadingLabel);
        loadingOverlay.Resize += (_, _) => CenterLoadingLabel();
        loginBackground.Controls.Add(loadingOverlay);
        loadingOverlay.BringToFront();
    }

    private void CenterLoadingLabel()
    {
        loadingLabel.Left = (loadingOverlay.ClientSize.Width - loadingLabel.Width) / 2;
        loadingLabel.Top = (loadingOverlay.ClientSize.Height - loadingLabel.Height) / 2;
    }

    private void AddHeader()
    {
        var logoRow = new TableLayoutPanel
        {
            AutoSize = true,
            BackColor = PharmaTheme.LoginCardFill,
            ColumnCount = 3,
            Dock = DockStyle.Top,
            Padding = new Padding(0, 0, 0, 8),
            RowCount = 1
        };
        logoRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        logoRow.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        logoRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

        var logoMark = new LogoMark
        {
            Anchor = AnchorStyles.None,
            Margin = new Padding(0),
            Size = new Size(64, 64)
        };

        var leftPad = new Panel
        {
            BackColor = PharmaTheme.LoginCardFill,
            Dock = DockStyle.Fill
        };
        var rightPad = new Panel
        {
            BackColor = PharmaTheme.LoginCardFill,
            Dock = DockStyle.Fill
        };

        logoRow.Controls.Add(leftPad, 0, 0);
        logoRow.Controls.Add(logoMark, 1, 0);
        logoRow.Controls.Add(rightPad, 2, 0);

        loginStack.Controls.Add(logoRow, 0, 0);

        loginStack.Controls.Add(new Label
        {
            AutoSize = false,
            Dock = DockStyle.Top,
            Font = new Font("Segoe UI", 20F, FontStyle.Bold),
            ForeColor = PharmaTheme.PrimaryGreen,
            Height = 44,
            Text = "مرحباً بعودتك",
            TextAlign = ContentAlignment.MiddleCenter
        }, 0, 1);

        loginStack.Controls.Add(new Label
        {
            AutoSize = false,
            Dock = DockStyle.Top,
            Font = PharmaTheme.BodyFont,
            ForeColor = PharmaTheme.MutedText,
            Height = 26,
            Margin = new Padding(0, 0, 0, 20),
            Text = "Welcome back — سجّل الدخول للمتابعة",
            TextAlign = ContentAlignment.MiddleCenter
        }, 0, 2);
    }

    private void AddLoginControls()
    {
        loginStack.Controls.Add(FieldLabel("البريد الإلكتروني"), 0, 3);
        emailInput = new RoundedTextInput
        {
            Dock = DockStyle.Top,
            Height = 48,
            Margin = new Padding(0, 6, 0, 12),
            PlaceholderText = "admin@pharmacy.com"
        };
        loginStack.Controls.Add(emailInput, 0, 4);

        loginStack.Controls.Add(FieldLabel("كلمة المرور"), 0, 5);
        passwordInput = new RoundedTextInput
        {
            Dock = DockStyle.Top,
            Height = 48,
            IsPassword = true,
            Margin = new Padding(0, 6, 0, 8),
            PlaceholderText = "••••••••"
        };
        loginStack.Controls.Add(passwordInput, 0, 6);

        showPasswordCheckBox = new CheckBox
        {
            AutoSize = true,
            Dock = DockStyle.Top,
            Font = PharmaTheme.SmallFont,
            ForeColor = PharmaTheme.MutedText,
            Margin = new Padding(0, 0, 0, 8),
            Text = "إظهار كلمة المرور"
        };
        loginStack.Controls.Add(showPasswordCheckBox, 0, 7);

        errorLabel = new Label
        {
            AutoSize = false,
            Dock = DockStyle.Top,
            Font = PharmaTheme.SmallFont,
            ForeColor = PharmaTheme.Danger,
            Height = 40,
            Margin = new Padding(0, 0, 0, 10),
            TextAlign = ContentAlignment.TopCenter,
            Visible = false
        };
        loginStack.Controls.Add(errorLabel, 0, 8);

        loginButton = new RoundedButton
        {
            BorderRadius = 16,
            Dock = DockStyle.Top,
            Font = new Font("Segoe UI", 11F, FontStyle.Bold),
            Height = 48,
            Text = "تسجيل الدخول",
            UseVisualStyleBackColor = false
        };
        loginStack.Controls.Add(loginButton, 0, 9);
    }

    private static Label FieldLabel(string text) => new()
    {
        AutoSize = false,
        Dock = DockStyle.Top,
        Font = PharmaTheme.SectionFont,
        ForeColor = PharmaTheme.TextDark,
        Height = 22,
        Text = text,
        TextAlign = ContentAlignment.BottomCenter
    };
}
