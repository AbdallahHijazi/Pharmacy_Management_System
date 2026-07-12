using Pharmacy.WinForms.Controls;
using Pharmacy.WinForms.Controls.Customers;
using Pharmacy.WinForms.Models;
using Pharmacy.WinForms.Services;
using Pharmacy.WinForms.Ui;

namespace Pharmacy.WinForms.Forms.Users;

internal sealed class AddUserDialog : Form
{
    private readonly UserService _userService;
    private TextBox _nameBox = null!;
    private TextBox _emailBox = null!;
    private TextBox _phoneBox = null!;
    private TextBox _passwordBox = null!;
    private TextBox _confirmPasswordBox = null!;
    private ComboBox _roleCombo = null!;
    private CheckBox _activeCheck = null!;
    private GradientRoundedButton _saveButton = null!;
    private CusDialogCancelButton _cancelButton = null!;
    private Label _statusLabel = null!;
    private bool _isSaving;
    private IReadOnlyList<RoleListItemView> _roles = Array.Empty<RoleListItemView>();

    public AddUserDialog(UserService userService)
    {
        _userService = userService;
        SuspendLayout();
        DoubleBuffered = true;
        RightToLeft = RightToLeft.Yes;
        RightToLeftLayout = true;
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(520, 620);
        Text = "إضافة مستخدم";
        BackColor = PharmaTheme.Background;
        BuildUi();
        Shown += async (_, _) => await LoadRolesAsync();
        ThemeManager.ThemeChanged += (_, _) => ApplyThemeVisuals();
        ResumeLayout(false);
    }

    private void BuildUi()
    {
        var headerPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 88,
            Padding = new Padding(24, 20, 24, 0),
            BackColor = PharmaTheme.Background
        };
        headerPanel.Controls.Add(new Label
        {
            Text = "أدخل بيانات المستخدم الجديد",
            Dock = DockStyle.Top,
            Height = 24,
            TextAlign = ContentAlignment.MiddleRight,
            Font = PharmaTheme.SmallFont,
            ForeColor = PharmaTheme.MutedText,
            BackColor = PharmaTheme.Background
        });
        headerPanel.Controls.Add(new Label
        {
            Text = "إضافة مستخدم",
            Dock = DockStyle.Top,
            Height = 32,
            TextAlign = ContentAlignment.MiddleRight,
            Font = PharmaTheme.ArabicFont(14f, FontStyle.Bold),
            ForeColor = PharmaTheme.PrimaryDark,
            BackColor = PharmaTheme.Background
        });

        var fieldsCard = new CusRoundedPanel(14)
        {
            FillColor = PharmaTheme.Surface,
            BorderColor = PharmaTheme.BorderSoft,
            Padding = new Padding(24),
            Dock = DockStyle.Fill
        };

        _nameBox = CreateTextBox(true);
        _emailBox = CreateTextBox(false);
        _phoneBox = CreateTextBox(false);
        _passwordBox = CreateTextBox(false, password: true);
        _confirmPasswordBox = CreateTextBox(false, password: true);
        _roleCombo = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            RightToLeft = RightToLeft.Yes,
            Font = PharmaTheme.ArabicFont(10.5f),
            BackColor = PharmaTheme.SurfaceContainerHigh,
            ForeColor = PharmaTheme.TextDark,
            Height = 36
        };
        _activeCheck = new CheckBox
        {
            Text = "نشط",
            Checked = true,
            RightToLeft = RightToLeft.Yes,
            Font = PharmaTheme.BodyFont,
            ForeColor = PharmaTheme.TextDark,
            AutoSize = true
        };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            BackColor = PharmaTheme.Surface,
            AutoScroll = true
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        layout.Controls.Add(MakeField("الاسم الكامل *", _nameBox), 0, 0);
        layout.Controls.Add(MakeField("البريد الإلكتروني *", _emailBox), 0, 1);
        layout.Controls.Add(MakeField("رقم الهاتف", _phoneBox), 0, 2);
        layout.Controls.Add(MakeField("الدور *", _roleCombo), 0, 3);
        layout.Controls.Add(MakeField("كلمة المرور *", _passwordBox), 0, 4);
        layout.Controls.Add(MakeField("تأكيد كلمة المرور *", _confirmPasswordBox), 0, 5);
        layout.Controls.Add(_activeCheck, 0, 6);
        fieldsCard.Controls.Add(layout);

        _statusLabel = new Label
        {
            Dock = DockStyle.Top,
            Height = 24,
            TextAlign = ContentAlignment.MiddleRight,
            ForeColor = PharmaTheme.Danger,
            Font = PharmaTheme.SmallFont,
            Visible = false,
            Padding = new Padding(24, 0, 24, 0),
            BackColor = PharmaTheme.Background
        };

        var footer = new Panel { Dock = DockStyle.Bottom, Height = 78, Padding = new Padding(24, 12, 24, 20), BackColor = PharmaTheme.Background };
        _saveButton = new GradientRoundedButton { Text = "حفظ المستخدم", IconGlyph = SegoeMdl2Icons.Save, Width = 160, Height = 46 };
        _saveButton.Click += async (_, _) => await SaveAsync();
        _cancelButton = new CusDialogCancelButton();
        _cancelButton.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };
        footer.Controls.Add(_cancelButton);
        footer.Controls.Add(_saveButton);
        footer.Resize += (_, _) =>
        {
            _saveButton.Location = new Point(footer.Width - _saveButton.Width, 12);
            _cancelButton.Location = new Point(_saveButton.Left - _cancelButton.Width - 12, 14);
        };

        Controls.Add(fieldsCard);
        Controls.Add(_statusLabel);
        Controls.Add(footer);
        Controls.Add(headerPanel);
    }

    private async Task LoadRolesAsync()
    {
        var result = await _userService.LoadRolesAsync().ConfigureAwait(true);
        if (!result.Success || result.Roles.Count == 0)
        {
            ShowStatus(result.ErrorMessage ?? "تعذر تحميل الأدوار.");
            _saveButton.Enabled = false;
            return;
        }

        _roles = result.Roles;
        _roleCombo.DataSource = _roles.ToList();
        _roleCombo.DisplayMember = nameof(RoleListItemView.Name);
        _roleCombo.ValueMember = nameof(RoleListItemView.RoleId);
    }

    private async Task SaveAsync()
    {
        if (_isSaving)
        {
            return;
        }

        var fullName = _nameBox.Text.Trim();
        var email = _emailBox.Text.Trim();
        var phone = _phoneBox.Text.Trim();
        var password = _passwordBox.Text;
        var confirm = _confirmPasswordBox.Text;

        if (string.IsNullOrWhiteSpace(fullName))
        {
            ShowStatus("الاسم الكامل مطلوب.");
            return;
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            ShowStatus("البريد الإلكتروني مطلوب.");
            return;
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            ShowStatus("كلمة المرور مطلوبة.");
            return;
        }

        if (!string.Equals(password, confirm, StringComparison.Ordinal))
        {
            ShowStatus("تأكيد كلمة المرور غير مطابق.");
            return;
        }

        if (_roleCombo.SelectedItem is not RoleListItemView role)
        {
            ShowStatus("اختر دورًا للمستخدم.");
            return;
        }

        _isSaving = true;
        _saveButton.Enabled = false;
        _saveButton.Text = "جارٍ الحفظ...";
        try
        {
            var result = await _userService.CreateUserAsync(new CreateUserApiRequest
            {
                FullName = fullName,
                Email = email,
                Phone = phone,
                Password = password,
                RoleId = role.RoleId,
                IsActive = _activeCheck.Checked
            }).ConfigureAwait(true);

            if (!result.Success)
            {
                ShowStatus(result.ErrorMessage ?? "تعذر إضافة المستخدم.");
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }
        finally
        {
            _isSaving = false;
            _saveButton.Enabled = true;
            _saveButton.Text = "حفظ المستخدم";
        }
    }

    private void ShowStatus(string message)
    {
        _statusLabel.Text = message;
        _statusLabel.Visible = true;
    }

    private void ApplyThemeVisuals()
    {
        BackColor = PharmaTheme.Background;
        _saveButton.Invalidate();
    }

    private static TextBox CreateTextBox(bool rtl, bool password = false)
    {
        var box = new TextBox
        {
            BorderStyle = BorderStyle.FixedSingle,
            Font = PharmaTheme.ArabicFont(10.5f),
            BackColor = PharmaTheme.SurfaceContainerHigh,
            ForeColor = PharmaTheme.TextDark,
            RightToLeft = rtl ? RightToLeft.Yes : RightToLeft.No,
            Dock = DockStyle.Top,
            Height = 36
        };
        if (password)
        {
            box.UseSystemPasswordChar = true;
        }

        return box;
    }

    private static Control MakeField(string label, Control input)
    {
        var panel = new Panel { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Margin = new Padding(0, 0, 0, 14), BackColor = PharmaTheme.Surface };
        panel.Controls.Add(input);
        panel.Controls.Add(new Label
        {
            Text = label,
            Dock = DockStyle.Top,
            Height = 22,
            TextAlign = ContentAlignment.MiddleRight,
            Font = PharmaTheme.SmallFont,
            ForeColor = PharmaTheme.OnSurfaceVariant,
            BackColor = PharmaTheme.Surface
        });
        return panel;
    }
}
