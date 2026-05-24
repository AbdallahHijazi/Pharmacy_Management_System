using Pharmacy.WinForms.Controls;
using Pharmacy.WinForms.Controls.Customers;
using Pharmacy.WinForms.Models;
using Pharmacy.WinForms.Services;
using Pharmacy.WinForms.Ui;

namespace Pharmacy.WinForms.Forms.Customers;

internal sealed class AddCustomerDialog : Form
{
    private readonly CustomerService _customerService;
    private TextBox _nameBox = null!;
    private TextBox _phoneBox = null!;
    private TextBox _addressBox = null!;
    private GradientRoundedButton _saveButton = null!;
    private CusDialogCancelButton _cancelButton = null!;
    private Label _statusLabel = null!;
    private Label _titleLabel = null!;
    private Label _subtitleLabel = null!;
    private CusRoundedPanel _fieldsCard = null!;
    private CusFieldStack _nameStack = null!;
    private CusFieldStack _phoneStack = null!;
    private CusFieldStack _addressStack = null!;
    private Panel _footerPanel = null!;
    private bool _isSaving;

    public AddCustomerDialog() : this(AppServices.CustomerService)
    {
    }

    public AddCustomerDialog(CustomerService customerService)
    {
        _customerService = customerService;
        SuspendLayout();
        DoubleBuffered = true;
        RightToLeft = RightToLeft.Yes;
        RightToLeftLayout = true;
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(480, 460);
        Text = "إضافة زبون";
        BackColor = PharmaTheme.Background;
        BuildUi();
        ThemeManager.ThemeChanged += OnThemeChanged;
        FormClosed += (_, _) => ThemeManager.ThemeChanged -= OnThemeChanged;
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

        _titleLabel = new Label
        {
            Text = "إضافة زبون جديد",
            Dock = DockStyle.Top,
            Height = 32,
            TextAlign = ContentAlignment.MiddleRight,
            Font = PharmaTheme.ArabicFont(14f, FontStyle.Bold),
            ForeColor = PharmaTheme.PrimaryDark,
            BackColor = PharmaTheme.Background
        };

        _subtitleLabel = new Label
        {
            Text = "أدخل بيانات الزبون لإضافته إلى السجلات",
            Dock = DockStyle.Top,
            Height = 24,
            TextAlign = ContentAlignment.MiddleRight,
            Font = PharmaTheme.SmallFont,
            ForeColor = PharmaTheme.MutedText,
            BackColor = PharmaTheme.Background
        };

        var closeButton = new Button
        {
            Text = SegoeMdl2Icons.Close,
            Font = new Font("Segoe MDL2 Assets", 11f),
            FlatStyle = FlatStyle.Flat,
            Size = new Size(32, 32),
            Location = new Point(8, 12),
            TabStop = false,
            Cursor = Cursors.Hand,
            BackColor = PharmaTheme.Background,
            ForeColor = PharmaTheme.MutedText
        };
        closeButton.FlatAppearance.BorderSize = 0;
        closeButton.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };

        headerPanel.Controls.Add(closeButton);
        headerPanel.Controls.Add(_subtitleLabel);
        headerPanel.Controls.Add(_titleLabel);

        _fieldsCard = new CusRoundedPanel(14)
        {
            FillColor = PharmaTheme.Surface,
            BorderColor = PharmaTheme.BorderSoft,
            Padding = new Padding(24)
        };

        _nameBox = CreateTextBox(rtl: true);
        _phoneBox = CreateTextBox(rtl: false);
        _addressBox = CreateTextBox(rtl: true, multiline: true);

        _nameStack = new CusFieldStack("الاسم *", _nameBox, 44) { Margin = new Padding(0, 0, 0, 16) };
        _phoneStack = new CusFieldStack("رقم الهاتف", _phoneBox, 44) { Margin = new Padding(0, 0, 0, 16) };
        _addressStack = new CusFieldStack("العنوان", _addressBox, 84, multilineHost: true);

        WireFieldFocus(_nameStack, _nameBox);
        WireFieldFocus(_phoneStack, _phoneBox);
        WireFieldFocus(_addressStack, _addressBox);
        _addressBox.TextChanged += (_, _) => UpdateAddressScrollBars();
        _addressStack.Resize += (_, _) => UpdateAddressScrollBars();

        var fieldsLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            BackColor = PharmaTheme.Surface,
            AutoSize = false
        };
        fieldsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        fieldsLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        fieldsLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        fieldsLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        fieldsLayout.Controls.Add(_nameStack, 0, 0);
        fieldsLayout.Controls.Add(_phoneStack, 0, 1);
        fieldsLayout.Controls.Add(_addressStack, 0, 2);
        _fieldsCard.Controls.Add(fieldsLayout);

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

        _footerPanel = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 78,
            Padding = new Padding(24, 12, 24, 20),
            BackColor = PharmaTheme.Background
        };

        _saveButton = new GradientRoundedButton
        {
            Text = "حفظ الزبون",
            IconGlyph = SegoeMdl2Icons.Save,
            Width = 150,
            Height = 46,
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        _saveButton.Click += async (_, _) => await SaveAsync();

        _cancelButton = new CusDialogCancelButton
        {
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        _cancelButton.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };

        _footerPanel.Resize += (_, _) => LayoutFooter();
        _footerPanel.Controls.Add(_cancelButton);
        _footerPanel.Controls.Add(_saveButton);

        var contentPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(24, 0, 24, 0),
            BackColor = PharmaTheme.Background
        };
        _fieldsCard.Dock = DockStyle.Fill;
        contentPanel.Controls.Add(_fieldsCard);

        Controls.Add(contentPanel);
        Controls.Add(_footerPanel);
        Controls.Add(_statusLabel);
        Controls.Add(headerPanel);

        LayoutFooter();
        ApplyThemeVisuals();
        Shown += (_, _) => UpdateAddressScrollBars();
    }

    private void LayoutFooter()
    {
        const int gap = 12;
        var right = _footerPanel.ClientSize.Width - _footerPanel.Padding.Right;
        _saveButton.Location = new Point(right - _saveButton.Width, _footerPanel.Padding.Top);
        _cancelButton.Location = new Point(_saveButton.Left - gap - _cancelButton.Width, _footerPanel.Padding.Top);
    }

    private void UpdateAddressScrollBars()
    {
        if (_addressBox.ClientSize.Width <= 0 || _addressBox.ClientSize.Height <= 0)
        {
            return;
        }

        var sample = string.IsNullOrWhiteSpace(_addressBox.Text) ? " " : _addressBox.Text;
        var textHeight = TextRenderer.MeasureText(
            sample,
            _addressBox.Font,
            new Size(_addressBox.ClientSize.Width, int.MaxValue),
            TextFormatFlags.WordBreak | TextFormatFlags.RightToLeft).Height;
        _addressBox.ScrollBars = textHeight > _addressBox.ClientSize.Height
            ? ScrollBars.Vertical
            : ScrollBars.None;
    }

    private static void WireFieldFocus(CusFieldStack stack, TextBox box)
    {
        box.GotFocus += (_, _) => stack.Host.SetFocused(true);
        box.LostFocus += (_, _) => stack.Host.SetFocused(false);
        box.Enter += (_, _) => box.BringToFront();
    }

    private async Task SaveAsync()
    {
        if (_isSaving)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(_nameBox.Text))
        {
            SetStatus("الاسم مطلوب.", true);
            _nameBox.Focus();
            return;
        }

        _isSaving = true;
        var previousSaveText = _saveButton.Text;
        _saveButton.Enabled = false;
        _saveButton.Text = "جارٍ الحفظ...";
        _cancelButton.Enabled = false;
        SetStatus("جارٍ الحفظ...", false);

        try
        {
            var request = new CreateCustomerApiRequest
            {
                FullName = _nameBox.Text.Trim(),
                Phone = _phoneBox.Text.Trim(),
                Address = _addressBox.Text.Trim()
            };

            var result = await _customerService.CreateCustomerAsync(request).ConfigureAwait(true);
            if (!result.Success)
            {
                SetStatus(result.ErrorMessage ?? "تعذر إضافة الزبون.", true);
                MessageBox.Show(
                    this,
                    result.IsConnectionError
                        ? $"{result.ErrorMessage}{Environment.NewLine}تحقق من أن API يعمل على {ApiConfiguration.BaseUrl}"
                        : result.ErrorMessage ?? "تعذر إضافة الزبون.",
                    "فشل الحفظ",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }
        finally
        {
            _isSaving = false;
            _saveButton.Enabled = true;
            _saveButton.Text = previousSaveText;
            _cancelButton.Enabled = true;
        }
    }

    private void SetStatus(string message, bool isError)
    {
        _statusLabel.Text = message;
        _statusLabel.Visible = !string.IsNullOrWhiteSpace(message);
        _statusLabel.ForeColor = isError ? PharmaTheme.Danger : PharmaTheme.OnSurfaceVariant;
    }

    private void OnThemeChanged(object? sender, EventArgs e) => ApplyThemeVisuals();

    private void ApplyThemeVisuals()
    {
        BackColor = PharmaTheme.Background;
        _titleLabel.ForeColor = PharmaTheme.PrimaryDark;
        _subtitleLabel.ForeColor = PharmaTheme.MutedText;
        _fieldsCard.FillColor = PharmaTheme.Surface;
        _fieldsCard.BorderColor = PharmaTheme.BorderSoft;
        _fieldsCard.ApplyThemeVisuals();
        _nameStack.ApplyThemeVisuals();
        _phoneStack.ApplyThemeVisuals();
        _addressStack.ApplyThemeVisuals();
        ApplyTextBoxTheme(_nameBox);
        ApplyTextBoxTheme(_phoneBox);
        ApplyTextBoxTheme(_addressBox);
        _cancelButton.ApplyThemeVisuals();
    }

    private static void ApplyTextBoxTheme(TextBox box)
    {
        box.BackColor = PharmaTheme.SurfaceContainerHigh;
        box.ForeColor = PharmaTheme.TextDark;
    }

    private static TextBox CreateTextBox(bool rtl, bool multiline = false) => new()
    {
        BorderStyle = BorderStyle.None,
        Font = PharmaTheme.BodyFont,
        BackColor = PharmaTheme.SurfaceContainerHigh,
        ForeColor = PharmaTheme.TextDark,
        RightToLeft = rtl ? RightToLeft.Yes : RightToLeft.No,
        TextAlign = rtl ? HorizontalAlignment.Right : HorizontalAlignment.Left,
        Multiline = multiline,
        ScrollBars = ScrollBars.None,
        WordWrap = multiline
    };
}
