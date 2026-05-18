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
        RightToLeft = RightToLeft.Yes;
        RightToLeftLayout = true;
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
            Margin = new Padding(0),
            RightToLeft = RightToLeft.Yes
        };
        loginStack.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        for (var row = 0; row < 10; row++)
        {
            loginStack.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        }

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
            RightToLeft = RightToLeft.Yes,
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
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, 0, 4),
            RightToLeft = RightToLeft.Yes
        };
        logoRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        logoRow.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        logoRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

        var logoMark = new LogoMark
        {
            Anchor = AnchorStyles.None,
            Margin = new Padding(0)
        };

        logoRow.Controls.Add(new Panel { BackColor = PharmaTheme.LoginCardFill, Dock = DockStyle.Fill }, 0, 0);
        logoRow.Controls.Add(logoMark, 1, 0);
        logoRow.Controls.Add(new Panel { BackColor = PharmaTheme.LoginCardFill, Dock = DockStyle.Fill }, 2, 0);

        loginStack.Controls.Add(logoRow, 0, 0);

        loginStack.Controls.Add(CreateRtlLabel(
            "أهلاً بعودتك",
            PharmaTheme.LoginTitleFont,
            PharmaTheme.PrimaryGreen,
            ContentAlignment.MiddleCenter,
            bottomMargin: 4), 0, 1);

        loginStack.Controls.Add(CreateRtlLabel(
            "Welcome back",
            PharmaTheme.LoginSubtitleFont,
            PharmaTheme.MutedText,
            ContentAlignment.MiddleCenter,
            bottomMargin: 20,
            useRtlReading: false), 0, 2);
    }

    private void AddLoginControls()
    {
        loginStack.Controls.Add(FieldLabel("البريد الإلكتروني"), 0, 3);
        emailInput = new RoundedTextInput
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 4, 0, 12),
            MinimumSize = new Size(0, 48),
            PlaceholderText = "admin@pharmacy.com"
        };
        loginStack.Controls.Add(emailInput, 0, 4);

        loginStack.Controls.Add(FieldLabel("كلمة المرور"), 0, 5);
        passwordInput = new RoundedTextInput
        {
            Dock = DockStyle.Fill,
            IsPassword = true,
            Margin = new Padding(0, 4, 0, 8),
            MinimumSize = new Size(0, 48),
            PlaceholderText = "••••••••"
        };
        loginStack.Controls.Add(passwordInput, 0, 6);

        showPasswordCheckBox = new CheckBox
        {
            AutoSize = true,
            Dock = DockStyle.Fill,
            Font = PharmaTheme.SmallFont,
            ForeColor = PharmaTheme.MutedText,
            Margin = new Padding(0, 0, 0, 10),
            RightToLeft = RightToLeft.Yes,
            Text = "إظهار كلمة المرور",
            UseCompatibleTextRendering = true
        };
        loginStack.Controls.Add(showPasswordCheckBox, 0, 7);

        errorLabel = new Label
        {
            AutoSize = true,
            Dock = DockStyle.Fill,
            Font = PharmaTheme.SmallFont,
            ForeColor = PharmaTheme.Danger,
            Margin = new Padding(0, 0, 0, 10),
            MaximumSize = new Size(PharmaTheme.LoginCardMaxWidth, 0),
            RightToLeft = RightToLeft.Yes,
            TextAlign = ContentAlignment.TopRight,
            UseCompatibleTextRendering = true,
            Visible = false
        };
        loginStack.Controls.Add(errorLabel, 0, 8);

        loginButton = new RoundedButton
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 4, 0, 0),
            Text = "تسجيل الدخول"
        };
        loginStack.Controls.Add(loginButton, 0, 9);
    }

    private static Label CreateRtlLabel(
        string text,
        Font font,
        Color foreColor,
        ContentAlignment align,
        int bottomMargin = 0,
        bool useRtlReading = true)
    {
        return new Label
        {
            AutoSize = true,
            Dock = DockStyle.Fill,
            Font = font,
            ForeColor = foreColor,
            Margin = new Padding(0, 0, 0, bottomMargin),
            RightToLeft = useRtlReading ? RightToLeft.Yes : RightToLeft.No,
            Text = text,
            TextAlign = align,
            UseCompatibleTextRendering = true
        };
    }

    private static Label FieldLabel(string text) => new()
    {
        AutoSize = true,
        Dock = DockStyle.Fill,
        Font = PharmaTheme.LoginFieldLabelFont,
        ForeColor = PharmaTheme.TextDark,
        Margin = new Padding(0, 0, 0, 2),
        RightToLeft = RightToLeft.Yes,
        Text = text,
        TextAlign = ContentAlignment.MiddleRight,
        UseCompatibleTextRendering = true
    };
}
