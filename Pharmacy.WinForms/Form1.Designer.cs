using System.Drawing.Drawing2D;

namespace Pharmacy.WinForms
{
    public partial class LoginForm : Form
    {
        public LoginForm()
        {
            BuildUi();
        }

        private void BuildUi()
        {
            this.Text = "PharmaCare - Login";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new Size(1200, 720);
            this.MinimumSize = new Size(1000, 650);
            this.BackColor = Color.FromArgb(240, 255, 246);
            this.Font = new Font("Cairo", 10);
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout =true;

            var mainCard = new RoundedPanel
            {
                Size = new Size(1080, 620),
                BackColor = Color.White,
                BorderRadius = 28,
                Anchor = AnchorStyles.None
            };

            mainCard.Location = new Point(
                (this.ClientSize.Width - mainCard.Width) / 2,
                (this.ClientSize.Height - mainCard.Height) / 2
            );

            this.Controls.Add(mainCard);

            this.Resize += (_, _) =>
            {
                mainCard.Location = new Point(
                    (this.ClientSize.Width - mainCard.Width) / 2,
                    (this.ClientSize.Height - mainCard.Height) / 2
                );
            };

            var leftPanel = new RoundedPanel
            {
                Width = 540,
                Dock = DockStyle.Left,
                BackColor = Color.FromArgb(7, 100, 67),
                BorderRadius = 28
            };

            var rightPanel = new Panel
            {
                Width = 540,
                Dock = DockStyle.Right,
                BackColor = Color.White
            };

            mainCard.Controls.Add(rightPanel);
            mainCard.Controls.Add(leftPanel);

            BuildLeft(leftPanel);
            BuildRight(rightPanel);
        }

        private void BuildLeft(Panel leftPanel)
        {
            var logoIcon = new RoundedPanel
            {
                Size = new Size(58, 58),
                Location = new Point(420, 55),
                BackColor = Color.FromArgb(40, 255, 255, 255),
                BorderRadius = 18
            };

            var icon = new Label
            {
                Text = "✚",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };

            logoIcon.Controls.Add(icon);
            leftPanel.Controls.Add(logoIcon);

            var title = new Label
            {
                Text = "PharmaCare",
                ForeColor = Color.White,
                Font = new Font("Inter", 24, FontStyle.Bold),
                Location = new Point(190, 60),
                Size = new Size(220, 40),
                TextAlign = ContentAlignment.MiddleRight
            };
            leftPanel.Controls.Add(title);

            var headline = new Label
            {
                Text = "مستقبل الإدارة\nالصيدلانية يبدأ هنا.",
                ForeColor = Color.White,
                Font = new Font("Cairo", 28, FontStyle.Bold),
                Location = new Point(70, 210),
                Size = new Size(410, 130),
                TextAlign = ContentAlignment.MiddleRight
            };
            leftPanel.Controls.Add(headline);

            var desc = new Label
            {
                Text = "نظام متكامل يجمع بين الدقة الطبية وسهولة الاستخدام الرقمية لتوفير أفضل رعاية لمرضاكم.",
                ForeColor = Color.FromArgb(215, 255, 255, 255),
                Font = new Font("Cairo", 13, FontStyle.Regular),
                Location = new Point(75, 355),
                Size = new Size(400, 80),
                TextAlign = ContentAlignment.TopRight
            };
            leftPanel.Controls.Add(desc);

            var securityBox = new RoundedPanel
            {
                Size = new Size(430, 80),
                Location = new Point(55, 500),
                BackColor = Color.FromArgb(30, 255, 255, 255),
                BorderRadius = 20
            };

            var secIcon = new Label
            {
                Text = "🛡",
                ForeColor = Color.White,
                Font = new Font("Segoe UI Emoji", 24),
                Location = new Point(360, 20),
                Size = new Size(45, 40),
                TextAlign = ContentAlignment.MiddleCenter
            };

            var secText = new Label
            {
                Text = "الدقة والأمان هي أولويتنا القصوى في PharmaCare.",
                ForeColor = Color.White,
                Font = new Font("Cairo", 10, FontStyle.Regular),
                Location = new Point(30, 25),
                Size = new Size(320, 30),
                TextAlign = ContentAlignment.MiddleRight
            };

            securityBox.Controls.Add(secIcon);
            securityBox.Controls.Add(secText);
            leftPanel.Controls.Add(securityBox);
        }

        private void BuildRight(Panel rightPanel)
        {
            var logo = new Label
            {
                Text = "PharmaCare",
                ForeColor = Color.FromArgb(7, 100, 67),
                Font = new Font("Inter", 24, FontStyle.Bold),
                Location = new Point(180, 80),
                Size = new Size(230, 40),
                TextAlign = ContentAlignment.MiddleRight
            };
            rightPanel.Controls.Add(logo);

            var pharmacyName = new Label
            {
                Text = "صيدلية الشفاء",
                ForeColor = Color.FromArgb(80, 80, 80),
                Font = new Font("Cairo", 11),
                Location = new Point(250, 118),
                Size = new Size(160, 25),
                TextAlign = ContentAlignment.MiddleRight
            };
            rightPanel.Controls.Add(pharmacyName);

            var iconBox = new RoundedPanel
            {
                Size = new Size(56, 56),
                Location = new Point(115, 78),
                BackColor = Color.FromArgb(7, 100, 67),
                BorderRadius = 15
            };

            var icon = new Label
            {
                Text = "✚",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 22, FontStyle.Bold),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };

            iconBox.Controls.Add(icon);
            rightPanel.Controls.Add(iconBox);

            var header = new Label
            {
                Text = "تسجيل الدخول للنظام",
                ForeColor = Color.FromArgb(11, 31, 23),
                Font = new Font("Cairo", 24, FontStyle.Bold),
                Location = new Point(80, 180),
                Size = new Size(360, 45),
                TextAlign = ContentAlignment.MiddleRight
            };
            rightPanel.Controls.Add(header);

            var sub = new Label
            {
                Text = "يرجى إدخال بيانات الاعتماد الخاصة بك للمتابعة.",
                ForeColor = Color.FromArgb(110, 120, 115),
                Font = new Font("Cairo", 11),
                Location = new Point(80, 225),
                Size = new Size(360, 30),
                TextAlign = ContentAlignment.MiddleRight
            };
            rightPanel.Controls.Add(sub);

            var userLabel = MakeLabel("اسم المستخدم أو البريد الإلكتروني", 280);
            rightPanel.Controls.Add(userLabel);

            var username = new RoundedTextBox
            {
                Location = new Point(80, 310),
                Size = new Size(360, 52),
                PlaceholderText = "username@pharmacare.com"
            };
            rightPanel.Controls.Add(username);

            var passLabel = MakeLabel("كلمة المرور", 380);
            rightPanel.Controls.Add(passLabel);

            var password = new RoundedTextBox
            {
                Location = new Point(80, 410),
                Size = new Size(360, 52),
                PlaceholderText = "••••••••",
                IsPassword = true
            };
            rightPanel.Controls.Add(password);

            var remember = new CheckBox
            {
                Text = "تذكر بياناتي",
                ForeColor = Color.FromArgb(100, 110, 105),
                Font = new Font("Cairo", 9),
                Location = new Point(315, 475),
                Size = new Size(125, 30),
                BackColor = Color.White
            };
            rightPanel.Controls.Add(remember);

            var forgot = new LinkLabel
            {
                Text = "نسيت كلمة المرور؟",
                LinkColor = Color.FromArgb(7, 100, 67),
                ActiveLinkColor = Color.FromArgb(45, 125, 90),
                Font = new Font("Cairo", 9, FontStyle.Bold),
                Location = new Point(80, 480),
                Size = new Size(150, 25),
                TextAlign = ContentAlignment.MiddleLeft
            };
            rightPanel.Controls.Add(forgot);

            var loginBtn = new RoundedButton
            {
                Text = "دخول إلى لوحة التحكم",
                Location = new Point(80, 525),
                Size = new Size(360, 56),
                BackColor = Color.FromArgb(7, 100, 67),
                ForeColor = Color.White,
                Font = new Font("Cairo", 13, FontStyle.Bold),
                BorderRadius = 14
            };
            rightPanel.Controls.Add(loginBtn);

            var support = new Label
            {
                Text = "تواجه مشكلة في الدخول؟ تواصل مع الدعم الفني",
                ForeColor = Color.FromArgb(120, 130, 125),
                Font = new Font("Cairo", 9),
                Location = new Point(80, 610),
                Size = new Size(360, 30),
                TextAlign = ContentAlignment.MiddleCenter
            };
            rightPanel.Controls.Add(support);
        }

        private Label MakeLabel(string text, int y)
        {
            return new Label
            {
                Text = text,
                ForeColor = Color.FromArgb(7, 100, 67),
                Font = new Font("Cairo", 10, FontStyle.Bold),
                Location = new Point(80, y),
                Size = new Size(360, 25),
                TextAlign = ContentAlignment.MiddleRight
            };
        }
    }

    public class RoundedPanel : Panel
    {
        public int BorderRadius { get; set; } = 20;

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using var path = GetRoundPath(ClientRectangle, BorderRadius);
            Region = new Region(path);
        }

        private GraphicsPath GetRoundPath(Rectangle rect, int radius)
        {
            int d = radius * 2;
            var path = new GraphicsPath();
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }

    public class RoundedButton : Button
    {
        public int BorderRadius { get; set; } = 12;

        protected override void OnPaint(PaintEventArgs pevent)
        {
            base.OnPaint(pevent);
            using var path = GetRoundPath(ClientRectangle, BorderRadius);
            Region = new Region(path);
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
        }

        private GraphicsPath GetRoundPath(Rectangle rect, int radius)
        {
            int d = radius * 2;
            var path = new GraphicsPath();
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }

    public class RoundedTextBox : UserControl
    {
        private readonly TextBox _textBox = new TextBox();

        public string PlaceholderText
        {
            get => _textBox.PlaceholderText;
            set => _textBox.PlaceholderText = value;
        }

        public bool IsPassword
        {
            get => _textBox.UseSystemPasswordChar;
            set => _textBox.UseSystemPasswordChar = value;
        }

        public string TextValue
        {
            get => _textBox.Text;
            set => _textBox.Text = value;
        }

        public RoundedTextBox()
        {
            BackColor = Color.FromArgb(240, 255, 246);
            Padding = new Padding(14, 12, 14, 8);

            _textBox.BorderStyle = BorderStyle.None;
            _textBox.BackColor = BackColor;
            _textBox.ForeColor = Color.FromArgb(11, 31, 23);
            _textBox.Font = new Font("Cairo", 11);
            _textBox.Dock = DockStyle.Fill;
            _textBox.TextAlign = HorizontalAlignment.Right;

            Controls.Add(_textBox);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using var path = GetRoundPath(ClientRectangle, 14);
            Region = new Region(path);
        }

        private GraphicsPath GetRoundPath(Rectangle rect, int radius)
        {
            int d = radius * 2;
            var path = new GraphicsPath();
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
