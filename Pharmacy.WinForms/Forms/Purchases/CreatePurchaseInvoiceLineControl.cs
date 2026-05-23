using Pharmacy.WinForms.Controls.Purchases;
using Pharmacy.WinForms.Models;
using Pharmacy.WinForms.Ui;

namespace Pharmacy.WinForms.Forms.Purchases;

internal sealed class CreatePurchaseInvoiceLineControl : Panel
{
    private readonly PurInputHost _productHost;
    private readonly PurInputHost _batchHost;
    private readonly PurInputHost _expiryHost;
    private readonly PurInputHost _quantityHost;
    private readonly PurInputHost _bonusHost;
    private readonly PurInputHost _priceHost;
    private readonly ComboBox _productCombo;
    private readonly TextBox _batchBox;
    private readonly DateTimePicker _expiryPicker;
    private readonly NumericUpDown _quantityInput;
    private readonly NumericUpDown _bonusInput;
    private readonly NumericUpDown _unitPriceInput;
    private readonly Label _subtotalLabel;
    private readonly PurRemoveLineButton _removeButton;
    private int _tableWidth = PurItemColumnLayout.MinTableWidth;

    public CreatePurchaseInvoiceLineControl()
    {
        Height = PurItemColumnLayout.RowHeight;
        Dock = DockStyle.Top;
        Margin = new Padding(0, 0, 0, 10);
        Padding = new Padding(0, 4, 0, 4);
        BackColor = Color.Transparent;
        RightToLeft = RightToLeft.Yes;

        _productCombo = CreateCombo();
        _batchBox = CreateInnerTextBox();
        _expiryPicker = new DateTimePicker
        {
            Format = DateTimePickerFormat.Short,
            Font = PharmaTheme.BodyFont,
            RightToLeft = RightToLeft.Yes,
            RightToLeftLayout = true,
            MinDate = DateTime.Today,
            Value = DateTime.Today.AddYears(1),
            BackColor = PharmaTheme.SurfaceContainerHigh
        };
        _quantityInput = CreateNumeric(1, 99999, 1);
        _bonusInput = CreateNumeric(0, 99999, 0);
        _unitPriceInput = CreateNumeric(0, 9999999, 0, 2);

        _productHost = new PurInputHost(_productCombo);
        _batchHost = new PurInputHost(_batchBox);
        _expiryHost = new PurInputHost(_expiryPicker);
        _quantityHost = new PurInputHost(_quantityInput);
        _bonusHost = new PurInputHost(_bonusInput);
        _priceHost = new PurInputHost(_unitPriceInput);

        _subtotalLabel = new Label
        {
            Text = "0.00",
            TextAlign = ContentAlignment.MiddleCenter,
            Font = PharmaTheme.NumberFont(10.5f, FontStyle.Bold),
            ForeColor = PharmaTheme.Primary,
            BackColor = Color.Transparent
        };
        _removeButton = new PurRemoveLineButton();
        _removeButton.Click += (_, _) => RemoveRequested?.Invoke(this, EventArgs.Empty);

        _productCombo.SelectedIndexChanged += (_, _) => OnProductChanged();
        _quantityInput.ValueChanged += (_, _) => UpdateSubtotal();
        _unitPriceInput.ValueChanged += (_, _) => UpdateSubtotal();

        Controls.AddRange([
            _removeButton,
            _subtotalLabel,
            _priceHost,
            _bonusHost,
            _quantityHost,
            _expiryHost,
            _batchHost,
            _productHost
        ]);

        Resize += (_, _) => LayoutLine();
        LayoutLine();
        UpdateSubtotal();
    }

    public event EventHandler? RemoveRequested;
    public event EventHandler? LineChanged;

    public decimal LineSubtotal => _quantityInput.Value * _unitPriceInput.Value;

    public void SetTableWidth(int width)
    {
        _tableWidth = Math.Max(width, PurItemColumnLayout.MinTableWidth);
        Width = _tableWidth;
        LayoutLine();
    }

    public void BindProducts(IReadOnlyList<PosProductView> products)
    {
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

        if (_unitPriceInput.Value < 0)
        {
            error = "سعر الشراء لا يمكن أن يكون سالبًا.";
            return false;
        }

        item = new CreatePurchaseInvoiceItemApiRequest
        {
            ProductId = selected.Product.ProductId,
            BatchNumber = _batchBox.Text.Trim(),
            ExpiryDate = DateTime.SpecifyKind(_expiryPicker.Value.Date, DateTimeKind.Utc),
            Quantity = qty,
            BonusQuantity = (int)_bonusInput.Value,
            UnitPrice = _unitPriceInput.Value
        };
        return true;
    }

    public void ApplyThemeVisuals()
    {
        _subtotalLabel.ForeColor = PharmaTheme.Primary;
        _removeButton.ApplyThemeVisuals();
        _productHost.ApplyThemeVisuals();
        _batchHost.ApplyThemeVisuals();
        _expiryHost.ApplyThemeVisuals();
        _quantityHost.ApplyThemeVisuals();
        _bonusHost.ApplyThemeVisuals();
        _priceHost.ApplyThemeVisuals();
    }

    internal static ColumnLayout GetColumnRects(Rectangle bounds) =>
        PurItemColumnLayout.GetColumnRects(bounds);

    private void OnProductChanged()
    {
        if (_productCombo.SelectedItem is ProductComboItem item && item.Product.SellingPrice > 0 && _unitPriceInput.Value == 0)
        {
            _unitPriceInput.Value = Math.Min(_unitPriceInput.Maximum, item.Product.SellingPrice);
        }

        UpdateSubtotal();
        LineChanged?.Invoke(this, EventArgs.Empty);
    }

    private void UpdateSubtotal()
    {
        _subtotalLabel.Text = PosFormatting.FormatMoneyCompact(LineSubtotal);
        LineChanged?.Invoke(this, EventArgs.Empty);
    }

    private void LayoutLine()
    {
        var inner = ClientRectangle;
        inner.Y += Padding.Top;
        inner.Height -= Padding.Vertical;
        var cols = PurItemColumnLayout.GetColumnRects(inner, _tableWidth);

        _productHost.Bounds = cols.Product;
        _batchHost.Bounds = cols.Batch;
        _expiryHost.Bounds = cols.Expiry;
        _quantityHost.Bounds = cols.Quantity;
        _bonusHost.Bounds = cols.Bonus;
        _priceHost.Bounds = cols.Price;
        _subtotalLabel.Bounds = cols.Subtotal;
        _removeButton.Bounds = cols.Remove;
    }

    private static ComboBox CreateCombo() => new()
    {
        DropDownStyle = ComboBoxStyle.DropDownList,
        Font = PharmaTheme.BodyFont,
        RightToLeft = RightToLeft.Yes,
        IntegralHeight = false
    };

    private static TextBox CreateInnerTextBox() => new()
    {
        BorderStyle = BorderStyle.None,
        Font = PharmaTheme.BodyFont,
        RightToLeft = RightToLeft.Yes
    };

    private static NumericUpDown CreateNumeric(decimal min, decimal max, decimal value, int decimals = 0) => new()
    {
        Minimum = min,
        Maximum = max,
        Value = value,
        DecimalPlaces = decimals,
        Font = PharmaTheme.BodyFont,
        ThousandsSeparator = true,
        BorderStyle = BorderStyle.None
    };

    private sealed class ProductComboItem
    {
        public ProductComboItem(PosProductView product) => Product = product;
        public PosProductView Product { get; }
        public override string ToString() => Product.DisplayName;
    }

    internal readonly record struct ColumnLayout(
        Rectangle Product,
        Rectangle Batch,
        Rectangle Expiry,
        Rectangle Quantity,
        Rectangle Bonus,
        Rectangle Price,
        Rectangle Subtotal,
        Rectangle Remove);
}

internal sealed class PurRemoveLineButton : Control
{
    public PurRemoveLineButton()
    {
        Text = "حذف";
        Size = new Size(56, 36);
        Cursor = Cursors.Hand;
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.StandardClick, true);
    }

    public void ApplyThemeVisuals() => Invalidate();

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        var b = ClientRectangle;
        b.Inflate(-1, -1);
        RoundedDrawing.FillRounded(g, b, 10, PharmaTheme.ErrorContainer);
        TextRenderer.DrawText(
            g,
            Text,
            PharmaTheme.ArabicFont(8.5f, FontStyle.Bold),
            b,
            PharmaTheme.Danger,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
    }
}
