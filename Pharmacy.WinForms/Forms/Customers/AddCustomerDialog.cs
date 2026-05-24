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
    private CusRoundedPanel _cancelButton = null!;
    private Label _statusLabel = null!;
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
        ClientSize = new Size(460, 420);
        Text = "إضافة زبون";
        BackColor = PharmaTheme.Background;
        BuildUi();
        ResumeLayout(false);
    }

    private void BuildUi()
    {
        var title = new Label
        {
            Text = "إضافة زبون جديد",
            AutoSize = false,
            Bounds = new Rectangle(24, 20, 412, 32),
            TextAlign = ContentAlignment.MiddleRight,
            Font = PharmaTheme.ArabicFont(14f, FontStyle.Bold),
            ForeColor = PharmaTheme.PrimaryDark
        };

        _nameBox = CreateField();
        _phoneBox = CreateField();
        _addressBox = CreateField(multiline: true);
        _addressBox.Height = 72;

        var nameStack = CreateFieldStack("الاسم *", _nameBox, 24, 68);
        var phoneStack = CreateFieldStack("رقم الهاتف", _phoneBox, 24, 148);
        var addressStack = CreateFieldStack("العنوان", _addressBox, 24, 228);

        _statusLabel = new Label
        {
            Bounds = new Rectangle(24, 310, 412, 22),
            TextAlign = ContentAlignment.MiddleRight,
            ForeColor = PharmaTheme.Danger,
            Font = PharmaTheme.SmallFont,
            Visible = false
        };

        _saveButton = new GradientRoundedButton
        {
            Text = "حفظ الزبون",
            IconGlyph = SegoeMdl2Icons.Save,
            Width = 150,
            Height = 46,
            Location = new Point(286, 348)
        };
        _saveButton.Click += async (_, _) => await SaveAsync();

        _cancelButton = new CusRoundedPanel(12)
        {
            Width = 110,
            Height = 46,
            Location = new Point(168, 348),
            Cursor = Cursors.Hand
        };
        _cancelButton.Paint += (_, e) =>
        {
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            var b = _cancelButton.ClientRectangle;
            b.Inflate(-1, -1);
            RoundedDrawing.FillRounded(g, b, 12, PharmaTheme.Surface);
            RoundedDrawing.DrawRoundedBorder(g, b, 12, PharmaTheme.BorderSoft, 1f);
            TextRenderer.DrawText(g, "إلغاء", PharmaTheme.ArabicFont(10f, FontStyle.Bold), b, PharmaTheme.TextDark,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        };
        _cancelButton.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };

        Controls.Add(_cancelButton);
        Controls.Add(_saveButton);
        Controls.Add(_statusLabel);
        Controls.Add(addressStack);
        Controls.Add(phoneStack);
        Controls.Add(nameStack);
        Controls.Add(title);
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
            return;
        }

        _isSaving = true;
        _saveButton.Enabled = false;
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
        }
    }

    private void SetStatus(string message, bool isError)
    {
        _statusLabel.Text = message;
        _statusLabel.Visible = !string.IsNullOrWhiteSpace(message);
        _statusLabel.ForeColor = isError ? PharmaTheme.Danger : PharmaTheme.OnSurfaceVariant;
    }

    private static TextBox CreateField(bool multiline = false) => new()
    {
        BorderStyle = BorderStyle.None,
        Font = PharmaTheme.BodyFont,
        BackColor = PharmaTheme.SurfaceContainerHigh,
        ForeColor = PharmaTheme.TextDark,
        RightToLeft = RightToLeft.Yes,
        Multiline = multiline
    };

    private static Panel CreateFieldStack(string label, Control field, int x, int y)
    {
        var panel = new Panel { Location = new Point(x, y), Size = new Size(412, multilineHeight(field) ? 108 : 68), BackColor = PharmaTheme.Background };
        var caption = new Label
        {
            Text = label,
            Bounds = new Rectangle(0, 0, 412, 20),
            TextAlign = ContentAlignment.MiddleRight,
            Font = PharmaTheme.SmallFont,
            ForeColor = PharmaTheme.MutedText
        };
        var host = new CusRoundedPanel(10)
        {
            Bounds = new Rectangle(0, 24, 412, field.Height),
            FillColor = PharmaTheme.SurfaceContainerHigh,
            Padding = new Padding(12, 8, 12, 8)
        };
        field.Dock = DockStyle.Fill;
        host.Controls.Add(field);
        panel.Controls.Add(host);
        panel.Controls.Add(caption);
        return panel;

        static bool multilineHeight(Control c) => c is TextBox { Multiline: true };
    }
}
