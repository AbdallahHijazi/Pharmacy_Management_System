using Pharmacy.WinForms.Models;
using Pharmacy.WinForms.Ui;

namespace Pharmacy.WinForms.Forms.Purchases;

internal sealed class CreatePurchaseInvoiceLineControl : Panel
{
    private readonly ComboBox _productCombo;
    private readonly TextBox _batchBox;
    private readonly DateTimePicker _expiryPicker;
    private readonly NumericUpDown _quantityInput;
    private readonly NumericUpDown _bonusInput;
    private readonly NumericUpDown _unitPriceInput;
    private readonly Button _removeButton;
    private IReadOnlyList<PosProductView> _products = Array.Empty<PosProductView>();

    public CreatePurchaseInvoiceLineControl()
    {
        Height = 92;
        Dock = DockStyle.Top;
        Margin = new Padding(0, 0, 0, 10);
        BackColor = Color.Transparent;
        RightToLeft = RightToLeft.Yes;

        _productCombo = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = PharmaTheme.BodyFont,
            RightToLeft = RightToLeft.Yes
        };
        _batchBox = CreateField();
        _expiryPicker = new DateTimePicker
        {
            Format = DateTimePickerFormat.Short,
            Font = PharmaTheme.BodyFont,
            RightToLeft = RightToLeft.Yes,
            RightToLeftLayout = true,
            MinDate = DateTime.Today,
            Value = DateTime.Today.AddYears(1)
        };
        _quantityInput = CreateNumeric(1, 99999, 1);
        _bonusInput = CreateNumeric(0, 99999, 0);
        _unitPriceInput = CreateNumeric(0, 9999999, 0, 2);
        _removeButton = new Button
        {
            Text = "حذف",
            FlatStyle = FlatStyle.Flat,
            BackColor = PharmaTheme.ErrorContainer,
            ForeColor = PharmaTheme.Danger,
            Font = PharmaTheme.SmallFont,
            Cursor = Cursors.Hand,
            Height = 36
        };
        _removeButton.Click += (_, _) => RemoveRequested?.Invoke(this, EventArgs.Empty);
        _productCombo.SelectedIndexChanged += (_, _) => OnProductChanged();
        _quantityInput.ValueChanged += (_, _) => LineChanged?.Invoke(this, EventArgs.Empty);
        _bonusInput.ValueChanged += (_, _) => LineChanged?.Invoke(this, EventArgs.Empty);
        _unitPriceInput.ValueChanged += (_, _) => LineChanged?.Invoke(this, EventArgs.Empty);

        Controls.AddRange([
            _removeButton,
            _unitPriceInput,
            _bonusInput,
            _quantityInput,
            _expiryPicker,
            _batchBox,
            _productCombo
        ]);

        Resize += (_, _) => LayoutLine();
        LayoutLine();
    }

    public event EventHandler? RemoveRequested;

    public void BindProducts(IReadOnlyList<PosProductView> products)
    {
        _products = products;
        _productCombo.Items.Clear();
        foreach (var product in products)
        {
            _productCombo.Items.Add(new ProductComboItem(product));
        }

        if (_productCombo.Items.Count > 0)
        {
            _productCombo.SelectedIndex = 0;
        }
    }

    public bool TryBuildItem(out CreatePurchaseInvoiceItemApiRequest? item, out string? error)
    {
        item = null;
        error = null;

        if (_productCombo.SelectedItem is not ProductComboItem selected)
        {
            error = "اختر منتجًا لكل بند.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(_batchBox.Text))
        {
            error = "رقم التشغيلة مطلوب.";
            return false;
        }

        var qty = (int)_quantityInput.Value;
        if (qty <= 0)
        {
            error = "الكمية يجب أن تكون أكبر من صفر.";
            return false;
        }

        var unitPrice = _unitPriceInput.Value;
        if (unitPrice < 0)
        {
            error = "سعر الشراء لا يمكن أن يكون سالبًا.";
            return false;
        }

        var expiry = DateTime.SpecifyKind(_expiryPicker.Value.Date, DateTimeKind.Utc);
        item = new CreatePurchaseInvoiceItemApiRequest
        {
            ProductId = selected.Product.ProductId,
            BatchNumber = _batchBox.Text.Trim(),
            ExpiryDate = expiry,
            Quantity = qty,
            BonusQuantity = (int)_bonusInput.Value,
            UnitPrice = unitPrice
        };
        return true;
    }

    public decimal LineSubtotal => _quantityInput.Value * _unitPriceInput.Value;

    public event EventHandler? LineChanged;

    public void ApplyThemeVisuals()
    {
        _productCombo.BackColor = PharmaTheme.InputSurface;
        _productCombo.ForeColor = PharmaTheme.TextDark;
        _batchBox.BackColor = PharmaTheme.InputSurface;
        _batchBox.ForeColor = PharmaTheme.TextDark;
        _quantityInput.BackColor = PharmaTheme.InputSurface;
        _bonusInput.BackColor = PharmaTheme.InputSurface;
        _unitPriceInput.BackColor = PharmaTheme.InputSurface;
        _removeButton.BackColor = PharmaTheme.ErrorContainer;
        _removeButton.ForeColor = PharmaTheme.Danger;
    }

    private void OnProductChanged()
    {
        if (_productCombo.SelectedItem is ProductComboItem item && item.Product.SellingPrice > 0 && _unitPriceInput.Value == 0)
        {
            _unitPriceInput.Value = Math.Min(_unitPriceInput.Maximum, item.Product.SellingPrice);
        }

        LineChanged?.Invoke(this, EventArgs.Empty);
    }

    private void LayoutLine()
    {
        var pad = 4;
        var y = pad;
        var h = 36;
        var removeW = 72;
        var priceW = 100;
        var bonusW = 72;
        var qtyW = 72;
        var expiryW = 120;
        var batchW = 120;
        var productW = Math.Max(160, Width - removeW - priceW - bonusW - qtyW - expiryW - batchW - pad * 8);

        var x = pad;
        _productCombo.SetBounds(x, y, productW, h);
        x += productW + pad;
        _batchBox.SetBounds(x, y, batchW, h);
        x += batchW + pad;
        _expiryPicker.SetBounds(x, y, expiryW, h);
        x += expiryW + pad;
        _quantityInput.SetBounds(x, y, qtyW, h);
        x += qtyW + pad;
        _bonusInput.SetBounds(x, y, bonusW, h);
        x += bonusW + pad;
        _unitPriceInput.SetBounds(x, y, priceW, h);
        x += priceW + pad;
        _removeButton.SetBounds(Width - removeW - pad, y, removeW, h);
    }

    private void OnLineValueChanged(object? sender, EventArgs e) => LineChanged?.Invoke(this, EventArgs.Empty);

    private static TextBox CreateField() => new()
    {
        BorderStyle = BorderStyle.FixedSingle,
        Font = PharmaTheme.BodyFont,
        BackColor = PharmaTheme.InputSurface,
        ForeColor = PharmaTheme.TextDark,
        RightToLeft = RightToLeft.Yes
    };

    private static NumericUpDown CreateNumeric(decimal min, decimal max, decimal value, int decimals = 0)
    {
        var nud = new NumericUpDown
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
        nud.ValueChanged += (_, _) => { };
        return nud;
    }

    private sealed class ProductComboItem
    {
        public ProductComboItem(PosProductView product) => Product = product;
        public PosProductView Product { get; }
        public override string ToString() => Product.DisplayName;
    }
}
