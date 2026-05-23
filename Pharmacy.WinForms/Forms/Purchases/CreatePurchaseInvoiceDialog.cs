using Pharmacy.WinForms.Controls;
using Pharmacy.WinForms.Controls.Purchases;
using Pharmacy.WinForms.Models;
using Pharmacy.WinForms.Services;
using Pharmacy.WinForms.Ui;

namespace Pharmacy.WinForms.Forms.Purchases;

internal sealed class CreatePurchaseInvoiceDialog : Form
{
    private readonly PurchaseService _purchaseService;
    private readonly List<CreatePurchaseInvoiceLineControl> _lines = new();

    private IReadOnlyList<SupplierOptionView> _suppliers = Array.Empty<SupplierOptionView>();
    private IReadOnlyList<PosProductView> _products = Array.Empty<PosProductView>();
    private bool _isSaving;

    private Panel _rootPanel = null!;
    private Label _titleLabel = null!;
    private Label _closeButton = null!;
    private PurRoundedPanel _infoCard = null!;
    private ComboBox _supplierCombo = null!;
    private TextBox _invoiceNumberBox = null!;
    private Label _invoiceDateLabel = null!;
    private ComboBox _paymentMethodCombo = null!;
    private NumericUpDown _taxRateInput = null!;
    private NumericUpDown _paidAmountInput = null!;
    private PurRoundedPanel _itemsCard = null!;
    private Panel _linesScrollPanel = null!;
    private Panel _linesHost = null!;
    private GradientRoundedButton _addLineButton = null!;
    private PurRoundedPanel _totalsCard = null!;
    private Label _subtotalValueLabel = null!;
    private Label _taxValueLabel = null!;
    private Label _grandTotalValueLabel = null!;
    private Label _remainingValueLabel = null!;
    private Label _itemsCountLabel = null!;
    private Label _statusLabel = null!;
    private GradientRoundedButton _saveButton = null!;
    private Button _cancelButton = null!;

    public CreatePurchaseInvoiceDialog() : this(AppServices.PurchaseService)
    {
    }

    public CreatePurchaseInvoiceDialog(PurchaseService purchaseService)
    {
        _purchaseService = purchaseService;

        SuspendLayout();
        SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
        DoubleBuffered = true;
        RightToLeft = RightToLeft.Yes;
        RightToLeftLayout = true;
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.Sizable;
        MinimumSize = new Size(980, 720);
        Size = new Size(1100, 780);
        Text = "إضافة فاتورة شراء";
        BackColor = PharmaTheme.Background;
        Font = PharmaTheme.BodyFont;

        BuildUi();
        WireEvents();

        ThemeManager.ThemeChanged += (_, _) => ApplyThemeVisuals();
        FontScaleManager.Changed += (_, _) => ApplyThemeVisuals();

        Shown += async (_, _) => await LoadLookupsAsync();
        ResumeLayout(false);
    }

    private void BuildUi()
    {
        _rootPanel = new Panel { Dock = DockStyle.Fill, BackColor = PharmaTheme.Background, Padding = new Padding(24) };

        _titleLabel = new Label
        {
            Text = "إضافة فاتورة شراء",
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleRight,
            Font = PharmaTheme.DashboardHeadlineFont,
            ForeColor = PharmaTheme.PrimaryDark,
            Dock = DockStyle.Fill
        };
        _closeButton = new Label
        {
            Text = SegoeMdl2Icons.Close,
            Font = PharmaTheme.IconFont(12f),
            AutoSize = true,
            Cursor = Cursors.Hand,
            ForeColor = PharmaTheme.OnSurfaceVariant
        };

        _infoCard = new PurRoundedPanel(PharmaTheme.PurchasesCardCornerRadius) { FillColor = PharmaTheme.Surface, Height = 200 };
        _supplierCombo = CreateCombo();
        _invoiceNumberBox = CreateTextField();
        _invoiceDateLabel = new Label
        {
            Text = $"تاريخ الفاتورة: {DateTime.Today:yyyy-MM-dd}",
            AutoSize = false,
            Height = 32,
            TextAlign = ContentAlignment.MiddleRight,
            Font = PharmaTheme.BodyFont,
            ForeColor = PharmaTheme.OnSurfaceVariant
        };
        _paymentMethodCombo = CreateCombo();
        _paymentMethodCombo.Items.AddRange(
        [
            new PaymentMethodItem("Cash", "نقدي"),
            new PaymentMethodItem("Credit", "آجل"),
            new PaymentMethodItem("BankTransfer", "تحويل بنكي"),
            new PaymentMethodItem("ShamCash", "شام كاش"),
            new PaymentMethodItem("Mixed", "مختلط")
        ]);
        _paymentMethodCombo.DisplayMember = nameof(PaymentMethodItem.Display);
        _paymentMethodCombo.ValueMember = nameof(PaymentMethodItem.Value);
        if (_paymentMethodCombo.Items.Count > 0)
        {
            _paymentMethodCombo.SelectedIndex = 0;
        }

        _taxRateInput = CreateNumeric(0, 100, 0, 2);
        _paidAmountInput = CreateNumeric(0, 99999999, 0, 2);

        _infoCard.Controls.AddRange([
            CreateCaption("المورد"), _supplierCombo,
            CreateCaption("رقم الفاتورة"), _invoiceNumberBox,
            CreateCaption("طريقة الدفع"), _paymentMethodCombo,
            CreateCaption("نسبة الضريبة %"), _taxRateInput,
            CreateCaption("المبلغ المدفوع"), _paidAmountInput,
            _invoiceDateLabel
        ]);

        _itemsCard = new PurRoundedPanel(PharmaTheme.PurchasesCardCornerRadius) { FillColor = PharmaTheme.Surface };
        var itemsHeader = new Label
        {
            Text = "الأصناف",
            Height = 32,
            Dock = DockStyle.Top,
            TextAlign = ContentAlignment.MiddleRight,
            Font = PharmaTheme.SectionFont,
            ForeColor = PharmaTheme.TextDark
        };
        _linesScrollPanel = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.Transparent };
        _linesHost = new Panel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Dock = DockStyle.Top,
            BackColor = Color.Transparent,
            Width = 10
        };
        _linesScrollPanel.Controls.Add(_linesHost);
        _addLineButton = new GradientRoundedButton
        {
            Text = "إضافة صنف",
            IconGlyph = SegoeMdl2Icons.Add,
            Height = 44,
            Dock = DockStyle.Bottom
        };
        _itemsCard.Controls.Add(_linesScrollPanel);
        _itemsCard.Controls.Add(_addLineButton);
        _itemsCard.Controls.Add(itemsHeader);

        _totalsCard = new PurRoundedPanel(PharmaTheme.PurchasesCardCornerRadius) { FillColor = PharmaTheme.SurfaceAlt, Height = 150 };
        _subtotalValueLabel = CreateValueLabel();
        _taxValueLabel = CreateValueLabel();
        _grandTotalValueLabel = CreateValueLabel();
        _remainingValueLabel = CreateValueLabel();
        _itemsCountLabel = CreateValueLabel();
        AddTotalRow("عدد الأصناف", _itemsCountLabel, 0);
        AddTotalRow("المجموع الفرعي", _subtotalValueLabel, 34);
        AddTotalRow("الضريبة", _taxValueLabel, 68);
        AddTotalRow("الإجمالي", _grandTotalValueLabel, 102);
        AddTotalRow("المتبقي", _remainingValueLabel, 136);

        _statusLabel = new Label
        {
            Dock = DockStyle.Bottom,
            Height = 28,
            TextAlign = ContentAlignment.MiddleRight,
            ForeColor = PharmaTheme.Danger,
            Font = PharmaTheme.SmallFont,
            Visible = false
        };

        var footer = new Panel { Dock = DockStyle.Bottom, Height = 64, BackColor = Color.Transparent };
        _saveButton = new GradientRoundedButton
        {
            Text = "حفظ الفاتورة",
            IconGlyph = SegoeMdl2Icons.Save,
            Width = 180,
            Height = 48
        };
        _cancelButton = new Button
        {
            Text = "إلغاء",
            Width = 120,
            Height = 48,
            FlatStyle = FlatStyle.Flat,
            BackColor = PharmaTheme.Surface,
            ForeColor = PharmaTheme.TextDark,
            Font = PharmaTheme.ArabicFont(10.5f, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        footer.Controls.Add(_cancelButton);
        footer.Controls.Add(_saveButton);
        footer.Resize += (_, _) => LayoutFooter(footer);

        var body = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
        _infoCard.Dock = DockStyle.Top;
        _totalsCard.Dock = DockStyle.Top;
        _itemsCard.Dock = DockStyle.Fill;
        body.Controls.Add(_itemsCard);
        body.Controls.Add(_totalsCard);
        body.Controls.Add(_infoCard);

        var header = new Panel { Dock = DockStyle.Top, Height = 48, BackColor = Color.Transparent };
        header.Controls.Add(_closeButton);
        header.Controls.Add(_titleLabel);
        header.Resize += (_, _) =>
        {
            _closeButton.Location = new Point(8, 10);
            _titleLabel.SetBounds(48, 0, header.Width - 56, header.Height);
        };

        _rootPanel.Controls.Add(_statusLabel);
        _rootPanel.Controls.Add(footer);
        _rootPanel.Controls.Add(body);
        _rootPanel.Controls.Add(header);
        Controls.Add(_rootPanel);

        _invoiceNumberBox.Text = GenerateInvoiceNumber();
        AddLine();
        LayoutInfoCard();
        LayoutFooter(footer);
        ApplyThemeVisuals();
    }

    private void WireEvents()
    {
        _closeButton.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };
        _cancelButton.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };
        _addLineButton.Click += (_, _) => AddLine();
        _saveButton.Click += async (_, _) => await SaveAsync();
        _taxRateInput.ValueChanged += (_, _) => UpdateTotals();
        _paidAmountInput.ValueChanged += (_, _) => UpdateTotals();
    }

    private async Task LoadLookupsAsync()
    {
        SetStatus("جاري تحميل البيانات...", false);
        _saveButton.Enabled = false;
        _addLineButton.Enabled = false;

        try
        {
            var suppliersTask = _purchaseService.LoadSupplierChoicesAsync();
            var productsTask = _purchaseService.LoadProductsAsync();
            await Task.WhenAll(suppliersTask, productsTask).ConfigureAwait(true);

            _suppliers = await suppliersTask.ConfigureAwait(true);
            _products = (await productsTask.ConfigureAwait(true)).Products;

            if (_suppliers.Count == 0)
            {
                SetStatus("تعذر تحميل الموردين. تحقق من الاتصال بالخادم.", true);
            }
            else
            {
                _supplierCombo.DataSource = _suppliers.ToList();
                _supplierCombo.DisplayMember = nameof(SupplierOptionView.Name);
                _supplierCombo.ValueMember = nameof(SupplierOptionView.SupplierId);
            }

            if (_products.Count == 0)
            {
                SetStatus("تعذر تحميل المنتجات. تحقق من الاتصال بالخادم.", true);
            }
            else
            {
                foreach (var line in _lines)
                {
                    line.BindProducts(_products);
                }

                SetStatus(string.Empty, false);
            }
        }
        catch (Exception ex)
        {
            SetStatus($"تعذر تحميل البيانات: {ex.Message}", true);
        }
        finally
        {
            _saveButton.Enabled = _suppliers.Count > 0 && _products.Count > 0;
            _addLineButton.Enabled = _products.Count > 0;
        }
    }

    private void AddLine()
    {
        var line = new CreatePurchaseInvoiceLineControl();
        line.BindProducts(_products);
        line.RemoveRequested += (_, _) =>
        {
            if (_lines.Count <= 1)
            {
                MessageBox.Show(this, "يجب أن تحتوي الفاتورة على بند واحد على الأقل.", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _lines.Remove(line);
            _linesHost.Controls.Remove(line);
            line.Dispose();
            UpdateTotals();
        };
        line.LineChanged += (_, _) => UpdateTotals();
        line.ApplyThemeVisuals();
        _lines.Add(line);
        _linesHost.Controls.Add(line);
        _linesHost.Controls.SetChildIndex(line, 0);
        UpdateTotals();
    }

    private async Task SaveAsync()
    {
        if (_isSaving)
        {
            return;
        }

        if (!TryValidate(out var request, out var error))
        {
            SetStatus(error, true);
            MessageBox.Show(this, error, "تعذر الحفظ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        _isSaving = true;
        _saveButton.Enabled = false;
        _cancelButton.Enabled = false;
        SetStatus("جارٍ حفظ الفاتورة...", false);

        try
        {
            var result = await _purchaseService.CreatePurchaseInvoiceAsync(request).ConfigureAwait(true);
            if (!result.Success)
            {
                var message = result.ErrorMessage ?? "تعذر حفظ فاتورة الشراء.";
                SetStatus(message, true);
                MessageBox.Show(
                    this,
                    result.IsConnectionError
                        ? $"{message}{Environment.NewLine}تحقق من أن API يعمل على {ApiConfiguration.BaseUrl}"
                        : message,
                    "فشل الحفظ",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            MessageBox.Show(
                this,
                $"تم حفظ فاتورة الشراء بنجاح.{Environment.NewLine}رقم الفاتورة: {result.Invoice?.InvoiceNumber ?? request.InvoiceNumber}",
                "نجاح",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            DialogResult = DialogResult.OK;
            Close();
        }
        finally
        {
            _isSaving = false;
            _saveButton.Enabled = true;
            _cancelButton.Enabled = true;
        }
    }

    private bool TryValidate(out CreatePurchaseInvoiceApiRequest request, out string error)
    {
        request = new CreatePurchaseInvoiceApiRequest();
        error = string.Empty;

        if (_supplierCombo.SelectedItem is not SupplierOptionView supplier
            || supplier.SupplierId is not Guid supplierId
            || supplierId == Guid.Empty)
        {
            error = "يجب اختيار مورد.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(_invoiceNumberBox.Text))
        {
            error = "رقم الفاتورة مطلوب.";
            return false;
        }

        if (_paymentMethodCombo.SelectedItem is not PaymentMethodItem payment)
        {
            error = "طريقة الدفع مطلوبة.";
            return false;
        }

        if (_lines.Count == 0)
        {
            error = "يجب إضافة بند واحد على الأقل.";
            return false;
        }

        var items = new List<CreatePurchaseInvoiceItemApiRequest>();
        var batchKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var line in _lines)
        {
            if (!line.TryBuildItem(out var item, out var lineError) || item is null)
            {
                error = lineError ?? "بند غير صالح.";
                return false;
            }

            var key = $"{item.ProductId:N}|{item.BatchNumber.Trim().ToLowerInvariant()}";
            if (!batchKeys.Add(key))
            {
                error = "لا يمكن تكرار نفس التشغيلة لنفس المنتج.";
                return false;
            }

            items.Add(item);
        }

        var subtotal = items.Sum(i => i.Quantity * i.UnitPrice);
        var taxRate = _taxRateInput.Value;
        var taxAmount = subtotal * (taxRate / 100m);
        var grandTotal = subtotal + taxAmount;
        var paid = _paidAmountInput.Value;

        if (paid < 0)
        {
            error = "المبلغ المدفوع لا يمكن أن يكون سالبًا.";
            return false;
        }

        if (paid > grandTotal)
        {
            error = "المبلغ المدفوع لا يمكن أن يتجاوز إجمالي الفاتورة.";
            return false;
        }

        request = new CreatePurchaseInvoiceApiRequest
        {
            InvoiceNumber = _invoiceNumberBox.Text.Trim(),
            SupplierId = supplierId,
            TaxRate = taxRate,
            PaidAmount = paid,
            PaymentMethod = payment.Value,
            Items = items
        };
        return true;
    }

    private void UpdateTotals()
    {
        var subtotal = _lines.Sum(l => l.LineSubtotal);
        var taxRate = _taxRateInput.Value;
        var taxAmount = subtotal * (taxRate / 100m);
        var grandTotal = subtotal + taxAmount;
        var paid = _paidAmountInput.Value;
        var remaining = Math.Max(0, grandTotal - paid);

        _itemsCountLabel.Text = _lines.Count.ToString("N0");
        _subtotalValueLabel.Text = PosFormatting.FormatMoneyCompact(subtotal);
        _taxValueLabel.Text = PosFormatting.FormatMoneyCompact(taxAmount);
        _grandTotalValueLabel.Text = PosFormatting.FormatMoneyCompact(grandTotal);
        _remainingValueLabel.Text = PosFormatting.FormatMoneyCompact(remaining);
        _remainingValueLabel.ForeColor = remaining > 0 ? PharmaTheme.Danger : PharmaTheme.TextDark;
    }

    private void SetStatus(string message, bool isError)
    {
        _statusLabel.Text = message;
        _statusLabel.Visible = !string.IsNullOrWhiteSpace(message);
        _statusLabel.ForeColor = isError ? PharmaTheme.Danger : PharmaTheme.OnSurfaceVariant;
    }

    private void ApplyThemeVisuals()
    {
        BackColor = PharmaTheme.Background;
        _rootPanel.BackColor = PharmaTheme.Background;
        _titleLabel.ForeColor = PharmaTheme.PrimaryDark;
        _titleLabel.Font = PharmaTheme.DashboardHeadlineFont;
        _infoCard.FillColor = PharmaTheme.Surface;
        _infoCard.ApplyThemeVisuals();
        _itemsCard.FillColor = PharmaTheme.Surface;
        _itemsCard.ApplyThemeVisuals();
        _totalsCard.FillColor = PharmaTheme.SurfaceAlt;
        _totalsCard.ApplyThemeVisuals();
        _saveButton.ForeColor = PharmaTheme.OnPrimary;
        _saveButton.Invalidate();
        foreach (var line in _lines)
        {
            line.ApplyThemeVisuals();
        }
        Invalidate(true);
    }

    private void LayoutInfoCard()
    {
        const int pad = 16;
        const int labelW = 120;
        const int rowH = 36;
        const int gap = 10;
        var y = pad;
        var contentW = Math.Max(200, _infoCard.ClientSize.Width - pad * 2 - labelW - 8);

        LayoutField(_infoCard, "المورد", _supplierCombo, ref y, pad, labelW, contentW, rowH, gap);
        LayoutField(_infoCard, "رقم الفاتورة", _invoiceNumberBox, ref y, pad, labelW, contentW, rowH, gap);
        _invoiceDateLabel.SetBounds(pad + labelW + 8, y, contentW, rowH);
        y += rowH + gap;
        LayoutField(_infoCard, "طريقة الدفع", _paymentMethodCombo, ref y, pad, labelW, contentW, rowH, gap);
        LayoutField(_infoCard, "نسبة الضريبة %", _taxRateInput, ref y, pad, labelW, contentW / 2, rowH, gap);
        LayoutField(_infoCard, "المبلغ المدفوع", _paidAmountInput, ref y, pad, labelW, contentW / 2, rowH, gap);
        _infoCard.Height = y + pad;
    }

    private static void LayoutField(
        Control parent,
        string caption,
        Control field,
        ref int y,
        int pad,
        int labelW,
        int fieldW,
        int rowH,
        int gap)
    {
        var cap = parent.Controls.OfType<Label>().FirstOrDefault(l => l.Text == caption);
        if (cap is not null)
        {
            cap.SetBounds(pad, y + 8, labelW, 20);
        }

        field.SetBounds(pad + labelW + 8, y, fieldW, rowH);
        y += rowH + gap;
    }

    private void LayoutFooter(Panel footer)
    {
        _saveButton.Location = new Point(footer.Width - _saveButton.Width - 8, 8);
        _cancelButton.Location = new Point(_saveButton.Left - _cancelButton.Width - 12, 8);
    }

    private static string GenerateInvoiceNumber() =>
        $"PI-{DateTime.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(1000, 9999)}";

    private static ComboBox CreateCombo() => new()
    {
        DropDownStyle = ComboBoxStyle.DropDownList,
        Font = PharmaTheme.BodyFont,
        BackColor = PharmaTheme.InputSurface,
        ForeColor = PharmaTheme.TextDark,
        RightToLeft = RightToLeft.Yes
    };

    private static TextBox CreateTextField() => new()
    {
        BorderStyle = BorderStyle.FixedSingle,
        Font = PharmaTheme.BodyFont,
        BackColor = PharmaTheme.InputSurface,
        ForeColor = PharmaTheme.TextDark,
        RightToLeft = RightToLeft.Yes
    };

    private static NumericUpDown CreateNumeric(decimal min, decimal max, decimal value, int decimals) => new()
    {
        Minimum = min,
        Maximum = max,
        Value = value,
        DecimalPlaces = decimals,
        Font = PharmaTheme.BodyFont,
        BackColor = PharmaTheme.InputSurface,
        ForeColor = PharmaTheme.TextDark,
        ThousandsSeparator = true,
        RightToLeft = RightToLeft.Yes
    };

    private static Label CreateCaption(string text) => new()
    {
        Text = text,
        AutoSize = false,
        Height = 20,
        TextAlign = ContentAlignment.MiddleRight,
        Font = PharmaTheme.SmallFont,
        ForeColor = PharmaTheme.OnSurfaceVariant
    };

    private static Label CreateValueLabel() => new()
    {
        AutoSize = false,
        Height = 24,
        TextAlign = ContentAlignment.MiddleRight,
        Font = PharmaTheme.ArabicFont(11f, FontStyle.Bold),
        ForeColor = PharmaTheme.TextDark,
        Text = "—"
    };

    private void AddTotalRow(string caption, Label value, int top)
    {
        var cap = new Label
        {
            Text = caption,
            AutoSize = false,
            Bounds = new Rectangle(16, top, 140, 24),
            TextAlign = ContentAlignment.MiddleRight,
            Font = PharmaTheme.SmallFont,
            ForeColor = PharmaTheme.OnSurfaceVariant
        };
        value.Bounds = new Rectangle(170, top, 300, 24);
        _totalsCard.Controls.Add(cap);
        _totalsCard.Controls.Add(value);
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        if (_infoCard is not null)
        {
            LayoutInfoCard();
        }
    }

    private sealed class PaymentMethodItem(string value, string display)
    {
        public string Value { get; } = value;
        public string Display { get; } = display;
    }
}
