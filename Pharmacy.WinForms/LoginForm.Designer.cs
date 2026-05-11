using System.Drawing;
using System.Windows.Forms;

namespace Pharmacy.WinForms
{
    public partial class LoginForm
    {
        private AmbientFormSurface ambientSurface = null!;
        private TableLayoutPanel mainLayout = null!;
        private Panel loginSide = null!;
        private DnaPanel dnaSide = null!;
        private TableLayoutPanel loginOuter = null!;
        private TableLayoutPanel loginStack = null!;
        private RoundedTextInput usernameInput = null!;
        private RoundedTextInput passwordInput = null!;
        private RoundedButton loginButton = null!;

        private void InitializeComponent()
        {
            SuspendLayout();

            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.White;
            ClientSize = new Size(1180, 680);
            DoubleBuffered = true;
            Font = new Font("Segoe UI", 10F);
            FormBorderStyle = FormBorderStyle.None;
            MinimumSize = new Size(900, 560);
            Name = "LoginForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "PharmaCare Login";

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
                MaximumSize = new Size(360, 0),
                MinimumSize = new Size(330, 0),
                RowCount = 10,
                RightToLeft = RightToLeft.No
            };

            loginOuter.Controls.Add(new Panel(), 0, 0);
            loginOuter.Controls.Add(loginStack, 0, 1);
            loginOuter.Controls.Add(new Panel(), 0, 2);

            AddLogo();
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

        private void AddLogo()
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
                Font = new Font("Segoe UI", 24F, FontStyle.Bold),
                ForeColor = Color.FromArgb(9, 76, 50),
                Height = 42,
                Margin = new Padding(0),
                Text = "PharmaCare",
                TextAlign = ContentAlignment.MiddleCenter
            }, 0, 1);

            loginStack.Controls.Add(new Label
            {
                AutoSize = false,
                Dock = DockStyle.Top,
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.FromArgb(102, 118, 111),
                Height = 30,
                Margin = new Padding(0, 0, 0, 30),
                Text = "Welcome back. Sign in to continue.",
                TextAlign = ContentAlignment.MiddleCenter
            }, 0, 2);
        }

        private void AddLoginControls()
        {
            loginStack.Controls.Add(FieldLabel("Email address"), 0, 3);
            usernameInput = new RoundedTextInput
            {
                Dock = DockStyle.Top,
                Height = 48,
                Margin = new Padding(0, 8, 0, 18),
                PlaceholderText = "username@pharmacare.com"
            };
            loginStack.Controls.Add(usernameInput, 0, 4);

            loginStack.Controls.Add(FieldLabel("Password"), 0, 5);
            passwordInput = new RoundedTextInput
            {
                Dock = DockStyle.Top,
                Height = 48,
                IsPassword = true,
                Margin = new Padding(0, 8, 0, 12),
                PlaceholderText = "Enter your password"
            };
            loginStack.Controls.Add(passwordInput, 0, 6);

            var forgot = new LinkLabel
            {
                ActiveLinkColor = Color.FromArgb(2, 104, 67),
                AutoSize = false,
                Dock = DockStyle.Top,
                Font = new Font("Segoe UI", 9F),
                Height = 26,
                LinkColor = Color.FromArgb(2, 104, 67),
                Margin = new Padding(0, 0, 0, 18),
                Text = "Forgot password?",
                TextAlign = ContentAlignment.MiddleRight,
                VisitedLinkColor = Color.FromArgb(2, 104, 67)
            };
            loginStack.Controls.Add(forgot, 0, 7);

            loginButton = new RoundedButton
            {
                BorderRadius = 18,
                Dock = DockStyle.Top,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Height = 50,
                Margin = new Padding(0),
                Text = "Sign In",
                UseVisualStyleBackColor = false
            };
            loginStack.Controls.Add(loginButton, 0, 8);
        }

        private static Label FieldLabel(string text)
        {
            return new Label
            {
                AutoSize = false,
                Dock = DockStyle.Top,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(34, 64, 51),
                Height = 22,
                Margin = new Padding(0),
                Text = text,
                TextAlign = ContentAlignment.BottomLeft
            };
        }
    }

    internal sealed class LogoMark : Control
    {
        public LogoMark()
        {
            DoubleBuffered = true;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            using var green = new SolidBrush(Color.FromArgb(2, 104, 67));
            using var lightGreen = new SolidBrush(Color.FromArgb(34, 158, 101));
            using var whitePen = new Pen(Color.White, 2.2F);

            e.Graphics.FillRectangle(green, 26, 6, 12, 52);
            e.Graphics.FillRectangle(green, 6, 26, 52, 12);

            using var leaf = new System.Drawing.Drawing2D.GraphicsPath();
            leaf.AddBezier(31, 55, 60, 50, 60, 16, 32, 24);
            leaf.AddBezier(32, 24, 40, 34, 39, 46, 31, 55);
            e.Graphics.FillPath(lightGreen, leaf);
            e.Graphics.DrawBezier(whitePen, 34, 52, 41, 41, 48, 31, 56, 19);
        }
    }
}
