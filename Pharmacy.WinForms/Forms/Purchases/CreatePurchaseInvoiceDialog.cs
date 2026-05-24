using Pharmacy.WinForms.Controls;
using Pharmacy.WinForms.Controls.Purchases;
using Pharmacy.WinForms.Models;
using Pharmacy.WinForms.Services;
using Pharmacy.WinForms.Ui;

namespace Pharmacy.WinForms.Forms.Purchases;

internal sealed class CreatePurchaseInvoiceDialog : Form
{
    private const int SummaryPanelWidth = 320;
    private const int ColumnGap = 16;
    private const int InfoCardMaxHeight = 220;
    private const int SummaryContentHeight = 292;
    private const int StackedLayoutBreakpoint = 980;

    private readonly PurchaseService _purchaseService;
    private readonly List<CreatePurchaseInvoiceLineControl> _lines = new();

    private IReadOnlyList<SupplierOptionView> _suppliers = Array.Empty<SupplierOptionView>();
    private IReadOnlyList<PosProductView> _products = Array.Empty<PosProductView>();
    private bool _isSaving;

    private Panel _rootPanel = null!;
    private Panel _headerPanel = null!;
    private Label _titleLabel = null!;
    private Label _subtitleLabel = null!;
    private Label _closeButton = null!;
    private Panel _bodyPanel = null!;
    private Panel _contentHost = null!;
    private Panel _mainColumn = null!;
    private Panel _summaryColumn = null!;
    private PurSectionCard _infoCard = null!;
    private Panel _infoFieldsPanel = null!;
    private PurInputHost _supplierHost = null!;
    private PurInputHost _invoiceNumberHost = null!;
    private PurInputHost _paymentHost = null!;
    private PurInputHost _taxHost = null!;
    private PurInputHost _paidHost = null!;
    private ComboBox _supplierCombo = null!;
    private TextBox _invoiceNumberBox = null!;
    private ComboBox _paymentMethodCombo = null!;
    private NumericUpDown _taxRateInput = null!;
    private NumericUpDown _paidAmountInput = null!;
    private Label _invoiceDateValue = null!;
    private PurSectionCard _itemsCard = null!;
    private PurItemsTableHost _itemsTableHost = null!;
    private Panel _itemsTableInner = null!;
    private PurItemsHeaderRow _itemsHeader = null!;
    private Panel _linesHost = null!;
    private GradientRoundedButton _addLineButton = null!;
    private PurSectionCard _summaryCard = null!;
    private PurSummaryRow _itemsCountRow = null!;
    private PurSummaryRow _subtotalRow = null!;
    private PurSummaryRow _taxRow = null!;
    private PurSummaryRow _grandTotalRow = null!;
    private PurSummaryRow _paidRow = null!;
    private PurSummaryRow _remainingRow = null!;
    private Panel _footerPanel = null!;
    private Label _statusLabel = null!;
    private GradientRoundedButton _saveButton = null!;
    private PurCancelButton _cancelButton = null!;

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
        MinimumSize = new Size(1024, 680);
        Size = new Size(1180, 800);
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
        _rootPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = PharmaTheme.Background,
            Padding = new Padding(20, 16, 20, 12)
        };

        _headerPanel = new Panel { Dock = DockStyle.Top, Height = 64, BackColor = Color.Transparent };
        _titleLabel = new Label
        {
            Text = "إضافة فاتورة شراء",
            AutoSize = false,
            Height = 28,
            TextAlign = ContentAlignment.MiddleRight,
            Font = PharmaTheme.ArabicFont(15f, FontStyle.Bold),
            ForeColor = PharmaTheme.PrimaryDark,
            Dock = DockStyle.Top
        };
        _subtitleLabel = new Label
        {
            Text = "أدخل بيانات المورد والأصناف لإنشاء فاتورة شراء جديدة",
            AutoSize = false,
            Height = 22,
            TextAlign = ContentAlignment.MiddleRight,
            Font = PharmaTheme.SmallFont,
            ForeColor = PharmaTheme.OnSurfaceVariant,
            Dock = DockStyle.Top
        };
        _closeButton = new Label
        {
            Text = SegoeMdl2Icons.Close,
            Font = PharmaTheme.IconFont(11f),
            AutoSize = true,
            Cursor = Cursors.Hand,
            ForeColor = PharmaTheme.OnSurfaceVariant,
            Size = new Size(32, 32),
            TextAlign = ContentAlignment.MiddleCenter
        };
        _headerPanel.Controls.Add(_closeButton);
        _headerPanel.Controls.Add(_subtitleLabel);
        _headerPanel.Controls.Add(_titleLabel);
        _headerPanel.Resize += (_, _) => _closeButton.Location = new Point(0, 4);

        _footerPanel = new Panel { Dock = DockStyle.Bottom, Height = 84, BackColor = Color.Transparent, Padding = new Padding(16, 8, 16, 12) };
        _statusLabel = new Label
        {
            Dock = DockStyle.Top,
            Height = 22,
            TextAlign = ContentAlignment.MiddleRight,
            ForeColor = PharmaTheme.Danger,
            Font = PharmaTheme.SmallFont,
            Visible = false
        };
        var footerButtons = new Panel { Dock = DockStyle.Bottom, Height = 52, BackColor = Color.Transparent };
        _saveButton = new GradientRoundedButton
        {
            Text = "حفظ الفاتورة",
            IconGlyph = SegoeMdl2Icons.Save,
            Width = 200,
            Height = 50
        };
        _cancelButton = new PurCancelButton { Width = 128, Height = 50 };
        footerButtons.Controls.Add(_cancelButton);
        footerButtons.Controls.Add(_saveButton);
        footerButtons.Resize += (_, _) => LayoutFooterButtons(footerButtons);
        _footerPanel.Controls.Add(footerButtons);
        _footerPanel.Controls.Add(_statusLabel);

        _bodyPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = PharmaTheme.Background
        };
        _contentHost = new Panel
        {
            BackColor = PharmaTheme.Background
        };
        _mainColumn = new Panel { BackColor = PharmaTheme.Background };
        _summaryColumn = new Panel { BackColor = PharmaTheme.Background };

        BuildInfoCard();
        BuildItemsCard();
        BuildSummaryCard();

        _mainColumn.Controls.Add(_itemsCard);
        _mainColumn.Controls.Add(_infoCard);
        _summaryColumn.Controls.Add(_summaryCard);
        _contentHost.Controls.Add(_summaryColumn);
        _contentHost.Controls.Add(_mainColumn);
        _bodyPanel.Controls.Add(_contentHost);
        _bodyPanel.Resize += (_, _) => LayoutContent();

        _rootPanel.Controls.Add(_bodyPanel);
        _rootPanel.Controls.Add(_footerPanel);
        _rootPanel.Controls.Add(_headerPanel);
        Controls.Add(_rootPanel);

        _invoiceNumberBox.Text = GenerateInvoiceNumber();
        AddLine();
        LayoutFooterButtons(footerButtons);
        LayoutContent();
        ApplyThemeVisuals();
    }

    private void BuildInfoCard()
    {
        _infoCard = new PurSectionCard("معلومات الفاتورة");
        _infoFieldsPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };

        _supplierCombo = CreateCombo();
        _invoiceNumberBox = CreateInnerTextBox();
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

        _taxRateInput = CreateNumeric(0, 100, 0, 2, ltr: true);
        _paidAmountInput = CreateNumeric(0, 99999999, 0, 2, ltr: true);

        _supplierHost = new PurInputHost(_supplierCombo);
        _invoiceNumberHost = new PurInputHost(_invoiceNumberBox);
        _paymentHost = new PurInputHost(_paymentMethodCombo);
        _taxHost = new PurInputHost(_taxRateInput);
        _paidHost = new PurInputHost(_paidAmountInput);

        _invoiceDateValue = new Label
        {
            Text = DateTime.Today.ToString("yyyy-MM-dd"),
            TextAlign = ContentAlignment.MiddleRight,
            Font = PharmaTheme.BodyFont,
            ForeColor = PharmaTheme.TextDark,
            Dock = DockStyle.Fill
        };
        var dateHost = new PurInputHost(_invoiceDateValue) { Enabled = false };

        _infoFieldsPanel.Controls.AddRange([
            CreateFieldStack("المورد", _supplierHost),
            CreateFieldStack("رقم الفاتورة", _invoiceNumberHost),
            CreateFieldStack("طريقة الدفع", _paymentHost),
            CreateFieldStack("نسبة الضريبة %", _taxHost),
            CreateFieldStack("المبلغ المدفوع", _paidHost),
            CreateFieldStack("تاريخ اليوم", dateHost)
        ]);

        _infoCard.Body.Controls.Add(_infoFieldsPanel);
        _infoCard.Dock = DockStyle.Top;
        _infoCard.Margin = new Padding(0, 0, 0, 14);
    }

    private void BuildItemsCard()
    {
        _itemsCard = new PurSectionCard("الأصناف");
        _itemsCard.Dock = DockStyle.Fill;

        var itemsToolbar = new Panel { Dock = DockStyle.Top, Height = 52, BackColor = Color.Transparent, Padding = new Padding(0, 0, 0, 10) };
        _addLineButton = new GradientRoundedButton
        {
            Text = "+ إضافة صنف",
            IconGlyph = SegoeMdl2Icons.Add,
            Width = 168,
            Height = 40,
            Anchor = AnchorStyles.Top | AnchorStyles.Left
        };
        itemsToolbar.Controls.Add(_addLineButton);
        itemsToolbar.Resize += (_, _) => _addLineButton.Location = new Point(0, 6);

        _itemsTableHost = new PurItemsTableHost();
        _itemsTableInner = new Panel
        {
            BackColor = PharmaTheme.Surface
        };
        _itemsHeader = new PurItemsHeaderRow();
        _linesHost = new Panel
        {
            BackColor = PharmaTheme.Surface,
            Padding = new Padding(0, 0, 0, 8)
        };
        _itemsTableInner.Controls.Add(_linesHost);
        _itemsTableInner.Controls.Add(_itemsHeader);
        _itemsTableHost.Controls.Add(_itemsTableInner);
        _itemsTableHost.Resize += (_, _) => SyncItemsTableLayout();

        _itemsCard.Body.Controls.Add(_itemsTableHost);
        _itemsCard.Body.Controls.Add(itemsToolbar);
    }

    private void BuildSummaryCard()
    {
        _summaryCard = new PurSectionCard("ملخص الفاتورة");
        _summaryCard.Dock = DockStyle.None;
        _summaryCard.FillColor = PharmaTheme.SurfaceAlt;

        _itemsCountRow = new PurSummaryRow("عدد الأصناف");
        _subtotalRow = new PurSummaryRow("المجموع الفرعي");
        _taxRow = new PurSummaryRow("الضريبة");
        _grandTotalRow = new PurSummaryRow("الإجمالي", emphasize: true);
        _paidRow = new PurSummaryRow("المدفوع");
        _remainingRow = new PurSummaryRow("المتبقي");

        var rowsPanel = new Panel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            BackColor = Color.Transparent,
            Padding = new Padding(4, 12, 4, 8),
            Width = SummaryPanelWidth - 48
        };
        rowsPanel.Controls.Add(_remainingRow);
        rowsPanel.Controls.Add(_paidRow);
        rowsPanel.Controls.Add(_grandTotalRow);
        rowsPanel.Controls.Add(_taxRow);
        rowsPanel.Controls.Add(_subtotalRow);
        rowsPanel.Controls.Add(_itemsCountRow);

        _summaryCard.Body.Padding = new Padding(24, 8, 24, 24);
        _summaryCard.Body.Controls.Add(rowsPanel);
        _summaryCard.FillColor = PharmaTheme.SurfaceAlt;
    }

    private void WireEvents()
    {
        _closeButton.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };
        _cancelButton.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };
        _addLineButton.Click += (_, _) => AddLine();
        _saveButton.Click += async (_, _) => await SaveAsync();
        _taxRateInput.ValueChanged += (_, _) => UpdateTotals();
        _paidAmountInput.ValueChanged += (_, _) => UpdateTotals();
        Resize += (_, _) => LayoutContent();
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
                _supplierCombo.DisplayMember = nameof(SupplierOptionView.DisplayName);
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
            SyncItemsTableLayout();
            UpdateTotals();
        };
        line.LineChanged += (_, _) => UpdateTotals();
        line.ApplyThemeVisuals();
        line.SetTableWidth(_itemsTableHost.TableWidth);
        _lines.Add(line);
        _linesHost.Controls.Add(line);
        _linesHost.Controls.SetChildIndex(line, 0);
        SyncItemsTableLayout();
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
        var previousSaveText = _saveButton.Text;
        _saveButton.Enabled = false;
        _saveButton.Text = "جارٍ الحفظ...";
        _cancelButton.Enabled = false;
        _addLineButton.Enabled = false;
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
            _saveButton.Text = previousSaveText;
            _saveButton.Enabled = _suppliers.Count > 0 && _products.Count > 0;
            _cancelButton.Enabled = true;
            _addLineButton.Enabled = _products.Count > 0;
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

        _itemsCountRow.ValueLabel.Text = _lines.Count.ToString("N0");
        _subtotalRow.ValueLabel.Text = PosFormatting.FormatMoneyCompact(subtotal);
        _taxRow.ValueLabel.Text = PosFormatting.FormatMoneyCompact(taxAmount);
        _grandTotalRow.ValueLabel.Text = PosFormatting.FormatMoneyCompact(grandTotal);
        _paidRow.ValueLabel.Text = PosFormatting.FormatMoneyCompact(paid);
        _remainingRow.ValueLabel.Text = PosFormatting.FormatMoneyCompact(remaining);
        _remainingRow.ValueLabel.ForeColor = remaining > 0 ? PharmaTheme.Danger : PharmaTheme.TextDark;
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
        _bodyPanel.BackColor = PharmaTheme.Background;
        _contentHost.BackColor = PharmaTheme.Background;
        _mainColumn.BackColor = PharmaTheme.Background;
        _summaryColumn.BackColor = PharmaTheme.Background;
        _itemsTableInner.BackColor = PharmaTheme.Surface;
        _linesHost.BackColor = PharmaTheme.Surface;
        _titleLabel.ForeColor = PharmaTheme.PrimaryDark;
        _titleLabel.Font = PharmaTheme.ArabicFont(15f, FontStyle.Bold);
        _subtitleLabel.ForeColor = PharmaTheme.OnSurfaceVariant;
        _closeButton.ForeColor = PharmaTheme.OnSurfaceVariant;

        _infoCard.ApplyThemeVisuals();
        _itemsCard.ApplyThemeVisuals();
        _summaryCard.FillColor = PharmaTheme.SurfaceAlt;
        _summaryCard.ApplyThemeVisuals();

        foreach (var stack in _infoFieldsPanel.Controls.OfType<PurFieldStack>())
        {
            stack.ApplyThemeVisuals();
        }

        _itemsCountRow.ApplyThemeVisuals(false);
        _subtotalRow.ApplyThemeVisuals(false);
        _taxRow.ApplyThemeVisuals(false);
        _grandTotalRow.ApplyThemeVisuals(true);
        _paidRow.ApplyThemeVisuals(false);
        _remainingRow.ApplyThemeVisuals(false);

        _saveButton.ForeColor = PharmaTheme.OnPrimary;
        _saveButton.Invalidate();
        _cancelButton.ApplyThemeVisuals();

        foreach (var line in _lines)
        {
            line.ApplyThemeVisuals();
        }

        Invalidate(true);
    }

    private void LayoutContent()
    {
        var viewport = _bodyPanel.ClientSize;
        if (viewport.Width <= 0 || viewport.Height <= 0)
        {
            return;
        }

        var stacked = viewport.Width < StackedLayoutBreakpoint;
        _bodyPanel.AutoScroll = stacked;

        if (stacked)
        {
            var mainH = Math.Max(420, viewport.Height - SummaryContentHeight - ColumnGap);
            _mainColumn.SetBounds(0, 0, viewport.Width, mainH);
            _summaryColumn.SetBounds(0, mainH + ColumnGap, viewport.Width, SummaryContentHeight);
            _contentHost.SetBounds(0, 0, viewport.Width, mainH + ColumnGap + SummaryContentHeight);
        }
        else
        {
            _contentHost.SetBounds(0, 0, viewport.Width, viewport.Height);
            var mainW = Math.Max(480, viewport.Width - SummaryPanelWidth - ColumnGap);
            _mainColumn.SetBounds(0, 0, mainW, viewport.Height);
            _summaryColumn.SetBounds(mainW + ColumnGap, 0, SummaryPanelWidth, viewport.Height);
        }

        LayoutInfoFields(_mainColumn.Width);
        LayoutMainColumn();
        LayoutSummaryCard();
        SyncItemsTableLayout();
    }

    private void LayoutSummaryCard()
    {
        var cardW = _summaryColumn.Width;
        _summaryCard.SetBounds(0, 0, cardW, SummaryContentHeight);
    }

    private void SyncItemsTableLayout()
    {
        if (_itemsTableHost is null || _itemsHeader is null)
        {
            return;
        }

        var viewportW = Math.Max(1, _itemsTableHost.ClientSize.Width);
        _itemsTableHost.SyncTableWidth(viewportW);
        var tableW = _itemsTableHost.TableWidth;

        var linesH = 0;
        foreach (Control control in _linesHost.Controls)
        {
            linesH += control.Height + control.Margin.Bottom;
        }
        linesH += _linesHost.Padding.Vertical;

        var headerBlock = PurItemColumnLayout.HeaderHeight + _itemsHeader.Margin.Bottom + 10;
        var innerH = Math.Max(headerBlock + linesH, headerBlock + PurItemColumnLayout.RowHeight);

        _itemsTableInner.SetBounds(0, 0, tableW, innerH);
        _itemsHeader.SetTableWidth(tableW);
        _itemsHeader.SetBounds(0, 0, tableW, PurItemColumnLayout.HeaderHeight);
        _linesHost.SetBounds(0, _itemsHeader.Bottom + _itemsHeader.Margin.Bottom, tableW, linesH);

        foreach (var line in _lines)
        {
            line.SetTableWidth(tableW);
        }
    }

    private void LayoutMainColumn()
    {
        var h = _mainColumn.ClientSize.Height;
        var infoH = Math.Min(InfoCardMaxHeight, Math.Max(196, _infoFieldsPanel.Height + 56));

        _infoCard.SetBounds(0, 0, _mainColumn.Width, infoH);
        _itemsCard.SetBounds(0, infoH + 14, _mainColumn.Width, Math.Max(160, h - infoH - 14));
    }

    private void LayoutInfoFields(int availableWidth)
    {
        const int fieldH = 70;
        const int gapX = 18;
        const int gapY = 14;
        const int cols = 2;
        var colW = Math.Max(160, (availableWidth - gapX) / cols);

        var stacks = _infoFieldsPanel.Controls.OfType<PurFieldStack>().ToList();
        for (var i = 0; i < stacks.Count; i++)
        {
            var col = i % cols;
            var row = i / cols;
            var x = col * (colW + gapX);
            var y = row * (fieldH + gapY);
            stacks[i].SetBounds(x, y, colW, fieldH);
        }

        var rows = (int)Math.Ceiling(stacks.Count / (double)cols);
        _infoFieldsPanel.Height = rows * (fieldH + gapY) - gapY;
        _infoCard.Height = Math.Min(InfoCardMaxHeight, _infoFieldsPanel.Height + 64);
    }

    private void LayoutFooterButtons(Panel footer)
    {
        _saveButton.Location = new Point(footer.Width - _saveButton.Width, 2);
        _cancelButton.Location = new Point(_saveButton.Left - _cancelButton.Width - 12, 2);
    }

    private static PurFieldStack CreateFieldStack(string label, PurInputHost host) =>
        new(label, host) { Tag = label, Dock = DockStyle.None };

    private static string GenerateInvoiceNumber() =>
        $"PI-{DateTime.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(1000, 9999)}";

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

    private static NumericUpDown CreateNumeric(decimal min, decimal max, decimal value, int decimals, bool ltr = false) => new()
    {
        Minimum = min,
        Maximum = max,
        Value = value,
        DecimalPlaces = decimals,
        Font = PharmaTheme.BodyFont,
        ThousandsSeparator = true,
        RightToLeft = ltr ? RightToLeft.No : RightToLeft.Yes,
        BorderStyle = BorderStyle.None
    };

    private sealed class PaymentMethodItem(string value, string display)
    {
        public string Value { get; } = value;
        public string Display { get; } = display;
    }
}
