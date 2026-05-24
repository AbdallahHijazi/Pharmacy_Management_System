using Pharmacy.WinForms.Controls;
using Pharmacy.WinForms.Controls.Customers;
using Pharmacy.WinForms.Models;
using Pharmacy.WinForms.Services;
using Pharmacy.WinForms.Ui;

namespace Pharmacy.WinForms.Forms.Suppliers;

internal sealed class AddSupplierDialog : Form
{
    private readonly SupplierService _supplierService;
    private TextBox _nameBox = null!;
    private TextBox _contactBox = null!;
    private TextBox _phoneBox = null!;
    private TextBox _addressBox = null!;
    private GradientRoundedButton _saveButton = null!;
    private CusDialogCancelButton _cancelButton = null!;
    private Label _statusLabel = null!;
    private bool _isSaving;

    public AddSupplierDialog() : this(AppServices.SupplierService)
    {
    }

    public AddSupplierDialog(SupplierService supplierService)
    {
        _supplierService = supplierService;
        SuspendLayout();
        DoubleBuffered = true;
        RightToLeft = RightToLeft.Yes;
        RightToLeftLayout = true;
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(500, 520);
        Text = "إضافة مورد";
        BackColor = PharmaTheme.Background;
        BuildUi();
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

        var title = new Label
        {
            Text = "إضافة مورد جديد",
            Dock = DockStyle.Top,
            Height = 32,
            TextAlign = ContentAlignment.MiddleRight,
            Font = PharmaTheme.ArabicFont(14f, FontStyle.Bold),
            ForeColor = PharmaTheme.PrimaryDark,
            BackColor = PharmaTheme.Background
        };
        var subtitle = new Label
        {
            Text = "أدخل بيانات المورد لإضافته إلى السجلات",
            Dock = DockStyle.Top,
            Height = 24,
            TextAlign = ContentAlignment.MiddleRight,
            Font = PharmaTheme.SmallFont,
            ForeColor = PharmaTheme.MutedText,
            BackColor = PharmaTheme.Background
        };
        headerPanel.Controls.Add(subtitle);
        headerPanel.Controls.Add(title);

        var fieldsCard = new CusRoundedPanel(14)
        {
            FillColor = PharmaTheme.Surface,
            BorderColor = PharmaTheme.BorderSoft,
            Padding = new Padding(24),
            Dock = DockStyle.Fill
        };

        _nameBox = CreateTextBox(rtl: true);
        _contactBox = CreateTextBox(rtl: true);
        _phoneBox = CreateTextBox(rtl: false);
        _addressBox = CreateTextBox(rtl: true, multiline: true);

        var nameStack = new CusFieldStack("اسم المورد *", _nameBox, 44) { Margin = new Padding(0, 0, 0, 16) };
        var contactStack = new CusFieldStack("الشخص المسؤول", _contactBox, 44) { Margin = new Padding(0, 0, 0, 16) };
        var phoneStack = new CusFieldStack("رقم الهاتف", _phoneBox, 44) { Margin = new Padding(0, 0, 0, 16) };
        var addressStack = new CusFieldStack("العنوان", _addressBox, 84, multilineHost: true);

        WireFieldFocus(nameStack, _nameBox);
        WireFieldFocus(contactStack, _contactBox);
        WireFieldFocus(phoneStack, _phoneBox);
        WireFieldFocus(addressStack, _addressBox);

        var fieldsLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            BackColor = PharmaTheme.Surface
        };
        fieldsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        fieldsLayout.Controls.Add(nameStack, 0, 0);
        fieldsLayout.Controls.Add(contactStack, 0, 1);
        fieldsLayout.Controls.Add(phoneStack, 0, 2);
        fieldsLayout.Controls.Add(addressStack, 0, 3);
        fieldsCard.Controls.Add(fieldsLayout);

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

        var footerPanel = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 78,
            Padding = new Padding(24, 12, 24, 20),
            BackColor = PharmaTheme.Background
        };

        _saveButton = new GradientRoundedButton
        {
            Text = "حفظ المورد",
            IconGlyph = SegoeMdl2Icons.Save,
            Width = 150,
            Height = 46,
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        _saveButton.Click += async (_, _) => await SaveAsync();

        _cancelButton = new CusDialogCancelButton { Anchor = AnchorStyles.Top | AnchorStyles.Right };
        _cancelButton.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };

        footerPanel.Resize += (_, _) => LayoutFooter(footerPanel);
        footerPanel.Controls.Add(_cancelButton);
        footerPanel.Controls.Add(_saveButton);

        var contentPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(24, 0, 24, 0),
            BackColor = PharmaTheme.Background
        };
        contentPanel.Controls.Add(fieldsCard);

        Controls.Add(contentPanel);
        Controls.Add(footerPanel);
        Controls.Add(_statusLabel);
        Controls.Add(headerPanel);

        LayoutFooter(footerPanel);
        ApplyThemeVisuals();
    }

    private void LayoutFooter(Panel footerPanel)
    {
        const int gap = 12;
        var right = footerPanel.ClientSize.Width - footerPanel.Padding.Right;
        _saveButton.Location = new Point(right - _saveButton.Width, footerPanel.Padding.Top);
        _cancelButton.Location = new Point(_saveButton.Left - gap - _cancelButton.Width, footerPanel.Padding.Top);
    }

    private async Task SaveAsync()
    {
        if (_isSaving)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(_nameBox.Text))
        {
            SetStatus("اسم المورد مطلوب.", true);
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
            var request = new CreateSupplierApiRequest
            {
                Name = _nameBox.Text.Trim(),
                ContactPerson = _contactBox.Text.Trim(),
                Phone = _phoneBox.Text.Trim(),
                Address = _addressBox.Text.Trim()
            };

            var result = await _supplierService.CreateSupplierAsync(request).ConfigureAwait(true);
            if (!result.Success)
            {
                SetStatus(result.ErrorMessage ?? "تعذر إضافة المورد.", true);
                MessageBox.Show(
                    this,
                    result.IsConnectionError
                        ? $"{result.ErrorMessage}{Environment.NewLine}تحقق من أن API يعمل على {ApiConfiguration.BaseUrl}"
                        : result.ErrorMessage ?? "تعذر إضافة المورد.",
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

    private void ApplyThemeVisuals()
    {
        foreach (Control control in Controls)
        {
            ApplyTextBoxThemeRecursive(control);
        }
    }

    private static void ApplyTextBoxThemeRecursive(Control control)
    {
        if (control is TextBox box)
        {
            box.BackColor = PharmaTheme.SurfaceContainerHigh;
            box.ForeColor = PharmaTheme.TextDark;
        }

        foreach (Control child in control.Controls)
        {
            ApplyTextBoxThemeRecursive(child);
        }
    }

    private static void WireFieldFocus(CusFieldStack stack, TextBox box)
    {
        box.GotFocus += (_, _) => stack.Host.SetFocused(true);
        box.LostFocus += (_, _) => stack.Host.SetFocused(false);
        box.Enter += (_, _) => box.BringToFront();
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
