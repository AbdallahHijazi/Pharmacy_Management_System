using Pharmacy.WinForms.Controls;
using Pharmacy.WinForms.Controls.Pos;
using Pharmacy.WinForms.Models;
using Pharmacy.WinForms.Services;
using Pharmacy.WinForms.Ui;

namespace Pharmacy.WinForms.Forms.PointOfSale;

internal sealed class PointOfSaleControl : UserControl
{
    private const int WorkspacePadding = 24;
    private const int ColumnGap = 24;
    private const int MinCartWidth = 420;
    private const int WideBreakpoint = 1100;
    private const int StackBreakpoint = 850;
    private const int GridGap = 16;
    private const int SearchCardMinHeight = 168;

    private readonly PointOfSaleService _posService;
    private readonly System.Windows.Forms.Timer _searchDebounce = new() { Interval = 300 };

    private readonly List<PosProductView> _allProducts = new();
    private readonly List<PosCartLine> _cart = new();
    private readonly string[] _categoryFilters = ["الكل", "مسكنات", "مضادات حيوية", "فيتامينات", "أدوية مزمنة"];

    private List<PosProductView> _filteredProducts = new();
    private string _activeCategory = "الكل";
    private string _searchText = string.Empty;
    private PosPaymentUiMode _paymentMode = PosPaymentUiMode.Cash;
    private decimal _discountPercent;
    private bool _stackedLayout;
    private CancellationTokenSource? _loadCts;
    private CancellationTokenSource? _searchCts;
    private bool _isSubmitting;

    private Panel _rootPanel = null!;
    private Panel _catalogPanel = null!;
    private Panel _cartPanel = null!;
    private PosRoundedPanel _searchFilterCard = null!;
    private PosSearchBox _searchBox = null!;
    private FlowLayoutPanel _categoryChipsPanel = null!;
    private readonly List<PosCategoryChip> _categoryChips = new();
    private Panel _productsScrollPanel = null!;
    private Panel _productGridPanel = null!;
    private Label _catalogStatusLabel = null!;
    private PosRoundedPanel _cartCard = null!;
    private Panel _customerHeader = null!;
    private Label _customerAvatar = null!;
    private Label _customerHint = null!;
    private Label _addCustomerBtn = null!;
    private TextBox _customerNameBox = null!;
    private Panel _cartItemsScrollPanel = null!;
    private Panel _cartItemsHost = null!;
    private Label _emptyCartLabel = null!;
    private Panel _totalsFooter = null!;
    private Label _subtotalValueLabel = null!;
    private NumericUpDown _discountInput = null!;
    private Label _totalValueLabel = null!;
    private PosPaymentButton _cashButton = null!;
    private PosPaymentButton _creditButton = null!;
    private PosPaymentButton _cardButton = null!;
    private GradientRoundedButton _checkoutButton = null!;
    private Label _messageLabel = null!;

    public PointOfSaleControl() : this(AppServices.PointOfSaleService)
    {
    }

    public PointOfSaleControl(PointOfSaleService posService)
    {
        _posService = posService;
        _searchDebounce.Tick += (_, _) => _ = ApplySearchFilterAsync();

        SuspendLayout();
        SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        DoubleBuffered = true;
        Dock = DockStyle.Fill;
        BackColor = PharmaTheme.Background;
        RightToLeft = RightToLeft.Yes;
        Padding = Padding.Empty;

        BuildUi();
        WireEvents();

        ThemeManager.ThemeChanged += HandleThemeChanged;
        FontScaleManager.Changed += HandleThemeChanged;

        ResumeLayout(false);
        Load += async (_, _) => await LoadProductsAsync();
        PerformLayout();
    }

    private void BuildUi()
    {
        _rootPanel = new Panel
        {
            BackColor = PharmaTheme.Background,
            Dock = DockStyle.Fill,
            Padding = Padding.Empty
        };

        _catalogPanel = new Panel { BackColor = PharmaTheme.Background };
        _cartPanel = new Panel { BackColor = PharmaTheme.Background };

        _searchFilterCard = new PosRoundedPanel(PharmaTheme.PosCardCornerRadius)
        {
            FillColor = PharmaTheme.SurfaceAlt,
            BorderColor = PharmaTheme.BorderSoft
        };

        _searchBox = new PosSearchBox
        {
            Dock = DockStyle.Top,
            PlaceholderText = "ابحث عن دواء بالاسم التجاري أو العلمي أو الباركود..."
        };

        _categoryChipsPanel = new FlowLayoutPanel
        {
            AutoSize = false,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = true,
            Padding = new Padding(0, 12, 0, 0),
            RightToLeft = RightToLeft.Yes
        };

        foreach (var filter in _categoryFilters)
        {
            var chip = new PosCategoryChip(filter) { Margin = new Padding(6, 0, 0, 8) };
            chip.Click += (_, _) => SetCategoryFilter(filter);
            _categoryChips.Add(chip);
            _categoryChipsPanel.Controls.Add(chip);
        }

        _searchFilterCard.Controls.Add(_categoryChipsPanel);
        _searchFilterCard.Controls.Add(_searchBox);

        _catalogStatusLabel = new Label
        {
            AutoSize = false,
            Font = PharmaTheme.BodyFont,
            ForeColor = PharmaTheme.MutedText,
            Height = 28,
            TextAlign = ContentAlignment.MiddleRight,
            UseCompatibleTextRendering = true,
            Visible = false
        };

        _productsScrollPanel = new Panel
        {
            AutoScroll = true,
            BackColor = PharmaTheme.Background
        };
        _productGridPanel = new Panel { BackColor = PharmaTheme.Background };
        _productsScrollPanel.Controls.Add(_productGridPanel);

        _catalogPanel.Controls.Add(_productsScrollPanel);
        _catalogPanel.Controls.Add(_catalogStatusLabel);
        _catalogPanel.Controls.Add(_searchFilterCard);

        _cartCard = new PosRoundedPanel(PharmaTheme.PosCartCornerRadius)
        {
            FillColor = PharmaTheme.SurfaceAlt,
            BorderColor = PharmaTheme.BorderSoft
        };

        _customerHeader = new Panel { BackColor = PharmaTheme.SurfaceContainerHigh };
        _customerAvatar = new Label
        {
            BackColor = PharmaTheme.PrimaryContainer,
            Font = PharmaTheme.IconFont(16f),
            ForeColor = PharmaTheme.Primary,
            Size = new Size(44, 44),
            Text = SegoeMdl2Icons.Person,
            TextAlign = ContentAlignment.MiddleCenter
        };
        _customerAvatar.Paint += (_, e) =>
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            var r = new Rectangle(2, 2, _customerAvatar.Width - 4, _customerAvatar.Height - 4);
            RoundedDrawing.FillRounded(e.Graphics, r, r.Width / 2, PharmaTheme.PrimaryContainer);
            TextRenderer.DrawText(
                e.Graphics,
                SegoeMdl2Icons.Person,
                PharmaTheme.IconFont(16f),
                _customerAvatar.ClientRectangle,
                PharmaTheme.Primary,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        };

        _customerNameBox = new TextBox
        {
            BorderStyle = BorderStyle.None,
            Font = PharmaTheme.ArabicFont(11f, FontStyle.Bold),
            BackColor = PharmaTheme.SurfaceContainerHigh,
            ForeColor = PharmaTheme.TextDark,
            PlaceholderText = "اسم الزبون (اختياري)",
            RightToLeft = RightToLeft.Yes
        };
        _customerHint = new Label
        {
            AutoSize = false,
            Font = PharmaTheme.SmallFont,
            ForeColor = PharmaTheme.OnSurfaceVariant,
            Height = 20,
            Text = "مبيعات عامة",
            TextAlign = ContentAlignment.TopRight,
            UseCompatibleTextRendering = true
        };
        _addCustomerBtn = new Label
        {
            Cursor = Cursors.Hand,
            Font = PharmaTheme.IconFont(14f),
            ForeColor = PharmaTheme.Primary,
            Size = new Size(36, 36),
            Text = SegoeMdl2Icons.PersonAdd,
            TextAlign = ContentAlignment.MiddleCenter
        };
        _addCustomerBtn.Click += (_, _) =>
            ShowMessage("اختيار الزبائن من القائمة سيتوفر لاحقًا.", PharmaTheme.OnSurfaceVariant);

        _customerHeader.Controls.Add(_addCustomerBtn);
        _customerHeader.Controls.Add(_customerHint);
        _customerHeader.Controls.Add(_customerNameBox);
        _customerHeader.Controls.Add(_customerAvatar);
        _customerHeader.Resize += (_, _) => LayoutCustomerHeader();

        _cartItemsScrollPanel = new Panel { AutoScroll = true, BackColor = PharmaTheme.SurfaceAlt };
        _cartItemsHost = new Panel { BackColor = PharmaTheme.SurfaceAlt };
        _cartItemsScrollPanel.Controls.Add(_cartItemsHost);

        _emptyCartLabel = new Label
        {
            AutoSize = false,
            Font = PharmaTheme.BodyFont,
            ForeColor = PharmaTheme.MutedText,
            Text = "السلة فارغة\r\nاختر دواء من القائمة لإضافته إلى الفاتورة",
            TextAlign = ContentAlignment.MiddleCenter,
            UseCompatibleTextRendering = true
        };

        _totalsFooter = new Panel { BackColor = PharmaTheme.SurfaceContainerHigh };
        _subtotalValueLabel = CreateTotalsValueLabel();
        var subtotalCaption = CreateTotalsCaptionLabel("المجموع الفرعي");
        var discountCaption = CreateTotalsCaptionLabel("الخصم");

        _discountInput = new NumericUpDown
        {
            DecimalPlaces = 0,
            Maximum = 100,
            Minimum = 0,
            Value = 0,
            Width = 64,
            Font = PharmaTheme.NumberFont(9f),
            BackColor = PharmaTheme.Surface,
            ForeColor = PharmaTheme.TextDark,
            BorderStyle = BorderStyle.None,
            TextAlign = HorizontalAlignment.Center
        };
        var percentLabel = new Label
        {
            AutoSize = true,
            Font = PharmaTheme.SmallFont,
            ForeColor = PharmaTheme.OnSurfaceVariant,
            Text = "%",
            UseCompatibleTextRendering = true
        };

        _totalValueLabel = new Label
        {
            AutoSize = false,
            Font = PharmaTheme.NumberFont(22f, FontStyle.Bold),
            ForeColor = PharmaTheme.Primary,
            Height = 40,
            TextAlign = ContentAlignment.MiddleLeft,
            UseCompatibleTextRendering = true
        };
        var totalCaption = CreateTotalsCaptionLabel("الإجمالي");

        _cashButton = new PosPaymentButton("نقد") { IsSelected = true };
        _creditButton = new PosPaymentButton("دين");
        _cardButton = new PosPaymentButton("بطاقة");
        _cashButton.Click += (_, _) => SetPaymentMode(PosPaymentUiMode.Cash);
        _creditButton.Click += (_, _) => SetPaymentMode(PosPaymentUiMode.Credit);
        _cardButton.Click += (_, _) => SetPaymentMode(PosPaymentUiMode.Card);

        _checkoutButton = new GradientRoundedButton
        {
            Height = 64,
            IconGlyph = SegoeMdl2Icons.Print,
            Text = "طباعة الفاتورة"
        };

        _messageLabel = new Label
        {
            AutoSize = false,
            Font = PharmaTheme.SmallFont,
            ForeColor = PharmaTheme.Danger,
            Height = 24,
            TextAlign = ContentAlignment.MiddleRight,
            UseCompatibleTextRendering = true,
            Visible = false
        };

        _totalsFooter.Controls.Add(_checkoutButton);
        _totalsFooter.Controls.Add(_cardButton);
        _totalsFooter.Controls.Add(_creditButton);
        _totalsFooter.Controls.Add(_cashButton);
        _totalsFooter.Controls.Add(_totalValueLabel);
        _totalsFooter.Controls.Add(totalCaption);
        _totalsFooter.Controls.Add(percentLabel);
        _totalsFooter.Controls.Add(_discountInput);
        _totalsFooter.Controls.Add(discountCaption);
        _totalsFooter.Controls.Add(_subtotalValueLabel);
        _totalsFooter.Controls.Add(subtotalCaption);
        _totalsFooter.Resize += (_, _) => LayoutTotalsFooter(subtotalCaption, discountCaption, totalCaption, percentLabel);
        _totalsFooter.Paint += TotalsFooterPaintSep;

        _cartCard.Controls.Add(_messageLabel);
        _cartCard.Controls.Add(_totalsFooter);
        _cartCard.Controls.Add(_cartItemsScrollPanel);
        _cartCard.Controls.Add(_emptyCartLabel);
        _cartCard.Controls.Add(_customerHeader);
        _cartPanel.Controls.Add(_cartCard);

        _rootPanel.Controls.Add(_catalogPanel);
        _rootPanel.Controls.Add(_cartPanel);
        Controls.Add(_rootPanel);

        SetCategoryFilter("الكل");
        UpdateTotals();
        RebuildCartItems();
    }

    private static Label CreateTotalsCaptionLabel(string text) => new()
    {
        AutoSize = false,
        Font = PharmaTheme.SmallFont,
        ForeColor = PharmaTheme.OnSurfaceVariant,
        Height = 22,
        Text = text,
        TextAlign = ContentAlignment.MiddleRight,
        UseCompatibleTextRendering = true
    };

    private static Label CreateTotalsValueLabel() => new()
    {
        AutoSize = false,
        Font = PharmaTheme.NumberFont(10f),
        ForeColor = PharmaTheme.OnSurfaceVariant,
        Height = 22,
        TextAlign = ContentAlignment.MiddleLeft,
        UseCompatibleTextRendering = true
    };

    private void WireEvents()
    {
        _searchBox.SearchTextChanged += (_, _) =>
        {
            _searchText = _searchBox.Text.Trim();
            _searchDebounce.Stop();
            _searchDebounce.Start();
        };
        _searchBox.SearchKeyDown += async (_, e) =>
        {
            if (e.KeyCode != Keys.Enter)
            {
                return;
            }

            e.Handled = true;
            e.SuppressKeyPress = true;
            await TryQuickAddFromSearchAsync();
        };
        _discountInput.ValueChanged += (_, _) =>
        {
            _discountPercent = Math.Clamp(_discountInput.Value, 0, 100);
            UpdateTotals();
        };
        _checkoutButton.Click += async (_, _) => await CheckoutAsync();
        Resize += (_, _) => LayoutPosWorkspace();
    }

    protected override void OnLayout(LayoutEventArgs levent)
    {
        base.OnLayout(levent);
        LayoutPosWorkspace();
    }

    private void LayoutPosWorkspace()
    {
        if (_rootPanel is null || _catalogPanel is null || _cartPanel is null)
        {
            return;
        }

        var innerW = Math.Max(320, ClientSize.Width - WorkspacePadding * 2);
        var innerH = Math.Max(400, ClientSize.Height - WorkspacePadding * 2);
        _stackedLayout = innerW < StackBreakpoint;

        int cartW;
        int catalogW;
        if (_stackedLayout)
        {
            cartW = innerW;
            catalogW = innerW;
        }
        else
        {
            cartW = Math.Max(MinCartWidth, (int)(innerW * 0.40));
            if (innerW < WideBreakpoint)
            {
                cartW = Math.Max(400, cartW);
            }

            catalogW = innerW - cartW - ColumnGap;
        }

        if (_stackedLayout)
        {
            var catalogH = Math.Max(320, (int)(innerH * 0.55));
            var cartH = innerH - catalogH - ColumnGap;
            _catalogPanel.SetBounds(WorkspacePadding, WorkspacePadding, catalogW, catalogH);
            _cartPanel.SetBounds(WorkspacePadding, WorkspacePadding + catalogH + ColumnGap, cartW, cartH);
        }
        else
        {
            var catalogX = WorkspacePadding + cartW + ColumnGap;
            _cartPanel.SetBounds(WorkspacePadding, WorkspacePadding, cartW, innerH);
            _catalogPanel.SetBounds(catalogX, WorkspacePadding, catalogW, innerH);
        }

        LayoutCatalogInternals();
        LayoutCartInternals();
        RebuildProductGrid();
    }

    private void LayoutCatalogInternals()
    {
        var w = Math.Max(200, _catalogPanel.ClientSize.Width);
        var h = Math.Max(200, _catalogPanel.ClientSize.Height);
        var searchH = Math.Max(SearchCardMinHeight, _searchFilterCard.PreferredSize.Height + 24);
        if (_searchFilterCard.Controls.Count > 0)
        {
            searchH = Math.Max(SearchCardMinHeight, 56 + 12 + _categoryChipsPanel.PreferredSize.Height + 20);
        }

        _searchFilterCard.SetBounds(0, 0, w, searchH);
        _searchBox.Width = w - 32;
        _searchBox.Location = new Point(16, 16);
        _categoryChipsPanel.SetBounds(16, _searchBox.Bottom + 8, w - 32, Math.Max(48, _categoryChipsPanel.PreferredSize.Height));

        var statusH = _catalogStatusLabel.Visible ? 30 : 0;
        _catalogStatusLabel.SetBounds(0, searchH + 8, w, statusH);
        _productsScrollPanel.SetBounds(0, searchH + 8 + statusH, w, Math.Max(120, h - searchH - 8 - statusH));
    }

    private void LayoutCartInternals()
    {
        var w = Math.Max(280, _cartPanel.ClientSize.Width);
        var h = Math.Max(400, _cartPanel.ClientSize.Height);
        _cartCard.SetBounds(0, 0, w, h);

        const int headerH = 96;
        const int footerH = 250;
        _customerHeader.SetBounds(0, 0, w, headerH);
        LayoutCustomerHeader();
        _totalsFooter.SetBounds(0, h - footerH, w, footerH);
        _messageLabel.SetBounds(16, h - footerH - 28, w - 32, 24);

        var itemsTop = headerH;
        var itemsH = Math.Max(80, h - headerH - footerH);
        _cartItemsScrollPanel.SetBounds(0, itemsTop, w, itemsH);
        _emptyCartLabel.SetBounds(16, itemsTop + 40, w - 32, 80);
    }

    private void LayoutCustomerHeader()
    {
        var w = _customerHeader.ClientSize.Width;
        _customerAvatar.SetBounds(w - 60, 24, 44, 44);
        _addCustomerBtn.SetBounds(12, 28, 36, 36);
        var textW = Math.Max(120, w - 120);
        _customerNameBox.SetBounds(56, 26, textW, 28);
        _customerHint.SetBounds(56, 54, textW, 20);
    }

    private void LayoutTotalsFooter(Label subtotalCaption, Label discountCaption, Label totalCaption, Label percentLabel)
    {
        var w = _totalsFooter.ClientSize.Width;
        var pad = 16;
        var y = pad;
        subtotalCaption.SetBounds(pad, y, w / 2, 22);
        _subtotalValueLabel.SetBounds(w / 2, y, w / 2 - pad, 22);
        y += 28;
        discountCaption.SetBounds(pad, y, 80, 22);
        _discountInput.SetBounds(w - pad - 90, y - 2, 64, 26);
        percentLabel.Location = new Point(w - pad - 24, y + 2);
        y += 36;
        y += 10;
        totalCaption.SetBounds(pad, y, 100, 28);
        _totalValueLabel.SetBounds(w / 2 - 40, y - 4, w / 2 + 30, 40);
        y += 48;
        var btnW = (w - pad * 2 - 16) / 3;
        _cashButton.SetBounds(pad, y, btnW, 44);
        _creditButton.SetBounds(pad + btnW + 8, y, btnW, 44);
        _cardButton.SetBounds(pad + (btnW + 8) * 2, y, btnW, 44);
        y += 52;
        _checkoutButton.SetBounds(pad, y, w - pad * 2, 64);
    }

    private void TotalsFooterPaintSep(object? sender, PaintEventArgs e)
    {
        var w = _totalsFooter.ClientSize.Width;
        var pad = 16;
        var y = pad + 28 + 36 + 10;
        using var pen = new Pen(PharmaTheme.WithAlpha(PharmaTheme.Border, 80));
        e.Graphics.DrawLine(pen, pad, y, w - pad, y);
    }

    private async Task LoadProductsAsync()
    {
        _loadCts?.Cancel();
        _loadCts = new CancellationTokenSource();
        var token = _loadCts.Token;

        SetCatalogStatus("جارٍ تحميل المنتجات...", visible: true);
        var result = await _posService.LoadProductsAsync(token).ConfigureAwait(true);
        if (IsDisposed || token.IsCancellationRequested)
        {
            return;
        }

        if (!result.Success)
        {
            _allProducts.Clear();
            _filteredProducts.Clear();
            SetCatalogStatus(
                result.IsConnectionError
                    ? "تعذر تحميل المنتجات، تحقق من اتصال API"
                    : result.ErrorMessage ?? "تعذر تحميل المنتجات.",
                visible: true,
                isError: true);
            RebuildProductGrid();
            return;
        }

        _allProducts.Clear();
        _allProducts.AddRange(result.Products);
        SetCatalogStatus(string.Empty, visible: false);
        await ApplySearchFilterAsync().ConfigureAwait(true);
    }

    private async Task ApplySearchFilterAsync()
    {
        _searchCts?.Cancel();
        _searchCts = new CancellationTokenSource();
        var token = _searchCts.Token;

        List<PosProductView> source;
        if (string.IsNullOrWhiteSpace(_searchText))
        {
            source = _allProducts.ToList();
        }
        else
        {
            var remote = await _posService.SearchProductsAsync(_searchText, token).ConfigureAwait(true);
            if (IsDisposed || token.IsCancellationRequested)
            {
                return;
            }

            if (!remote.Success)
            {
                source = FilterProductsLocal(_allProducts, _searchText);
            }
            else
            {
                source = remote.Products.Count > 0
                    ? remote.Products.ToList()
                    : FilterProductsLocal(_allProducts, _searchText);
            }
        }

        _filteredProducts = ApplyCategoryFilter(source);
        if (IsDisposed)
        {
            return;
        }

        RebuildProductGrid();
    }

    private List<PosProductView> ApplyCategoryFilter(IEnumerable<PosProductView> source)
    {
        if (_activeCategory == "الكل")
        {
            return source.ToList();
        }

        return source.Where(p => MatchesCategory(p, _activeCategory)).ToList();
    }

    private static bool MatchesCategory(PosProductView product, string filter) =>
        filter switch
        {
            "مسكنات" => ContainsAny(product, "مسكن", "pain", "panadol", "ibuprofen"),
            "مضادات حيوية" => ContainsAny(product, "مضاد", "antibiotic", "amox", "cipro"),
            "فيتامينات" => ContainsAny(product, "فيتام", "vitamin", "centrum"),
            "أدوية مزمنة" => ContainsAny(product, "مزمن", "ضغط", "سكر", "قلب", "chronic"),
            _ => true
        };

    private static bool ContainsAny(PosProductView product, params string[] tokens)
    {
        var hay = $"{product.Name} {product.ScientificName} {product.CategoryName}".ToLowerInvariant();
        return tokens.Any(t => hay.Contains(t, StringComparison.OrdinalIgnoreCase));
    }

    private static List<PosProductView> FilterProductsLocal(IEnumerable<PosProductView> source, string query)
    {
        var q = query.Trim().ToLowerInvariant();
        return source
            .Where(p =>
                p.Name.Contains(q, StringComparison.OrdinalIgnoreCase)
                || p.ScientificName.Contains(q, StringComparison.OrdinalIgnoreCase)
                || p.Barcode.Contains(q, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    private void SetCategoryFilter(string filter)
    {
        _activeCategory = filter;
        foreach (var chip in _categoryChips)
        {
            chip.IsSelected = chip.Text == filter;
        }

        _filteredProducts = ApplyCategoryFilter(
            string.IsNullOrWhiteSpace(_searchText) ? _allProducts : FilterProductsLocal(_allProducts, _searchText));
        RebuildProductGrid();
    }

    private void RebuildProductGrid()
    {
        if (_productGridPanel is null || _productsScrollPanel is null)
        {
            return;
        }

        _productGridPanel.SuspendLayout();
        _productGridPanel.Controls.Clear();

        var gridW = Math.Max(180, _productsScrollPanel.ClientSize.Width - 8);
        var columns = gridW >= 720 ? 3 : gridW >= 460 ? 2 : 1;
        var cardW = Math.Max(180, (gridW - GridGap * (columns - 1)) / columns);
        var cardH = PharmaTheme.PosProductCardHeight;

        for (var i = 0; i < _filteredProducts.Count; i++)
        {
            var product = _filteredProducts[i];
            var col = i % columns;
            var row = i / columns;
            var card = new PosProductCard(product)
            {
                Size = new Size(cardW, cardH),
                Location = new Point(col * (cardW + GridGap), row * (cardH + GridGap))
            };
            card.Click += (_, _) => TryAddProductToCart(product);
            _productGridPanel.Controls.Add(card);
        }

        var rows = _filteredProducts.Count == 0 ? 0 : ((_filteredProducts.Count - 1) / columns) + 1;
        var gridH = rows == 0 ? 80 : rows * (cardH + GridGap);
        _productGridPanel.Size = new Size(gridW, gridH);
        _productGridPanel.ResumeLayout(true);
    }

    private void TryAddProductToCart(PosProductView product)
    {
        if (product.SellingPrice <= 0)
        {
            ShowMessage("لا يمكن إضافة منتج بدون سعر.", PharmaTheme.Danger);
            return;
        }

        if (product.IsOutOfStock)
        {
            ShowMessage("لا يوجد مخزون كافٍ لهذا المنتج.", PharmaTheme.Danger);
            return;
        }

        var line = _cart.FirstOrDefault(c => c.Product.ProductId == product.ProductId);
        if (line is null)
        {
            _cart.Add(new PosCartLine { Product = product, Quantity = 1 });
        }
        else if (line.Quantity >= product.SellableQuantity)
        {
            ShowMessage("لا يمكن تجاوز الكمية المتاحة في المخزون.", PharmaTheme.Danger);
            return;
        }
        else
        {
            line.Quantity++;
        }

        ShowMessage(string.Empty, PharmaTheme.MutedText, visible: false);
        RebuildCartItems();
        UpdateTotals();
    }

    private async Task TryQuickAddFromSearchAsync()
    {
        await ApplySearchFilterAsync().ConfigureAwait(true);
        if (_filteredProducts.Count == 1)
        {
            TryAddProductToCart(_filteredProducts[0]);
            _searchBox.Text = string.Empty;
            _searchText = string.Empty;
            await ApplySearchFilterAsync().ConfigureAwait(true);
            return;
        }

        var exact = _filteredProducts.FirstOrDefault(p =>
            string.Equals(p.Barcode, _searchText, StringComparison.OrdinalIgnoreCase));
        if (exact is not null)
        {
            TryAddProductToCart(exact);
            _searchBox.Text = string.Empty;
            _searchText = string.Empty;
            await ApplySearchFilterAsync().ConfigureAwait(true);
            return;
        }

        if (_filteredProducts.Count == 0 && !string.IsNullOrWhiteSpace(_searchText))
        {
            ShowMessage("لم يتم العثور على المنتج.", PharmaTheme.Danger);
        }
    }

    private void RebuildCartItems()
    {
        _cartItemsHost.SuspendLayout();
        _cartItemsHost.Controls.Clear();

        var y = 8;
        foreach (var line in _cart)
        {
            var item = new PosCartItemControl(line)
            {
                Width = Math.Max(260, _cartItemsScrollPanel.ClientSize.Width - 24),
                Location = new Point(8, y)
            };
            item.IncreaseRequested += (_, _) => ChangeCartQuantity(line, 1);
            item.DecreaseRequested += (_, _) => ChangeCartQuantity(line, -1);
            item.RemoveRequested += (_, _) =>
            {
                _cart.Remove(line);
                RebuildCartItems();
                UpdateTotals();
            };
            _cartItemsHost.Controls.Add(item);
            y += item.Height + 8;
        }

        _cartItemsHost.Size = new Size(Math.Max(260, _cartItemsScrollPanel.ClientSize.Width), Math.Max(80, y));
        _emptyCartLabel.Visible = _cart.Count == 0;
        _cartItemsScrollPanel.Visible = _cart.Count > 0;
        _cartItemsHost.ResumeLayout(true);
    }

    private void ChangeCartQuantity(PosCartLine line, int delta)
    {
        var next = line.Quantity + delta;
        if (next <= 0)
        {
            _cart.Remove(line);
        }
        else if (next > line.Product.SellableQuantity)
        {
            ShowMessage("لا يمكن تجاوز الكمية المتاحة في المخزون.", PharmaTheme.Danger);
            return;
        }
        else
        {
            line.Quantity = next;
        }

        RebuildCartItems();
        UpdateTotals();
    }

    private void UpdateTotals()
    {
        var subtotal = _cart.Sum(l => l.LineTotal);
        var discount = Math.Clamp(_discountPercent, 0, 100);
        var total = subtotal - subtotal * (discount / 100m);
        if (total < 0)
        {
            total = 0;
        }

        _subtotalValueLabel.Text = PosFormatting.FormatMoney(subtotal);
        _totalValueLabel.Text = PosFormatting.FormatMoney(total);
    }

    private void SetPaymentMode(PosPaymentUiMode mode)
    {
        _paymentMode = mode;
        _cashButton.IsSelected = mode == PosPaymentUiMode.Cash;
        _creditButton.IsSelected = mode == PosPaymentUiMode.Credit;
        _cardButton.IsSelected = mode == PosPaymentUiMode.Card;
    }

    private async Task CheckoutAsync()
    {
        if (_isSubmitting)
        {
            return;
        }

        if (_cart.Count == 0)
        {
            ShowMessage("السلة فارغة. أضف منتجات قبل طباعة الفاتورة.", PharmaTheme.Danger);
            return;
        }

        if (_discountPercent < 0 || _discountPercent > 100)
        {
            ShowMessage("نسبة الخصم يجب أن تكون بين 0 و 100.", PharmaTheme.Danger);
            return;
        }

        if (_paymentMode == PosPaymentUiMode.Credit
            && string.IsNullOrWhiteSpace(_customerNameBox.Text))
        {
            ShowMessage("يجب إدخال اسم الزبون عند البيع بالدين.", PharmaTheme.Danger);
            return;
        }

        var subtotal = _cart.Sum(l => l.LineTotal);
        var total = subtotal - subtotal * (_discountPercent / 100m);
        if (total < 0)
        {
            total = 0;
        }

        var paymentMethod = _paymentMode switch
        {
            PosPaymentUiMode.Credit => "Credit",
            PosPaymentUiMode.Card => "ShamCash",
            _ => "Cash"
        };

        var paidAmount = _paymentMode == PosPaymentUiMode.Credit ? 0m : total;

        var request = new CreateSalesInvoiceApiRequest
        {
            CustomerId = null,
            DiscountPercentage = _discountPercent,
            PaidAmount = paidAmount,
            PaymentMethod = paymentMethod,
            Items = _cart.Select(l => new CreateSalesInvoiceItemApiRequest
            {
                ProductId = l.Product.ProductId,
                Quantity = l.Quantity
            }).ToList()
        };

        _isSubmitting = true;
        _checkoutButton.Enabled = false;
        _checkoutButton.Text = "جارٍ إنشاء الفاتورة...";
        try
        {
            var result = await _posService.CreateSalesInvoiceAsync(request).ConfigureAwait(true);
            if (!result.Success)
            {
                ShowMessage(
                    result.IsConnectionError
                        ? "تعذر الاتصال بالخادم أثناء إنشاء الفاتورة."
                        : result.ErrorMessage ?? "تعذر إنشاء الفاتورة.",
                    PharmaTheme.Danger);
                return;
            }

            var invoiceNo = result.Invoice?.InvoiceNumber ?? "—";
            UiFeedback.ShowSuccess(
                FindForm(),
                $"تم إنشاء الفاتورة بنجاح.\nرقم الفاتورة: {invoiceNo}");

            _cart.Clear();
            _discountInput.Value = 0;
            _discountPercent = 0;
            SetPaymentMode(PosPaymentUiMode.Cash);
            _customerNameBox.Text = string.Empty;
            RebuildCartItems();
            UpdateTotals();
            ShowMessage(string.Empty, PharmaTheme.MutedText, visible: false);
            await LoadProductsAsync().ConfigureAwait(true);
        }
        finally
        {
            _isSubmitting = false;
            _checkoutButton.Enabled = true;
            _checkoutButton.Text = "طباعة الفاتورة";
        }
    }

    private void SetCatalogStatus(string text, bool visible, bool isError = false)
    {
        _catalogStatusLabel.Text = text;
        _catalogStatusLabel.Visible = visible;
        _catalogStatusLabel.ForeColor = isError ? PharmaTheme.Danger : PharmaTheme.MutedText;
        LayoutCatalogInternals();
    }

    private void ShowMessage(string text, Color color, bool visible = true)
    {
        _messageLabel.Text = text;
        _messageLabel.ForeColor = color;
        _messageLabel.Visible = visible && !string.IsNullOrWhiteSpace(text);
    }

    public void ApplyThemeVisuals()
    {
        BackColor = PharmaTheme.Background;
        _rootPanel.BackColor = PharmaTheme.Background;
        _catalogPanel.BackColor = PharmaTheme.Background;
        _cartPanel.BackColor = PharmaTheme.Background;
        _productsScrollPanel.BackColor = PharmaTheme.Background;
        _productGridPanel.BackColor = PharmaTheme.Background;

        _searchFilterCard.FillColor = PharmaTheme.SurfaceAlt;
        _searchFilterCard.BorderColor = PharmaTheme.BorderSoft;
        _searchFilterCard.ApplyThemeVisuals();
        _searchBox.ApplyThemeVisuals();

        foreach (var chip in _categoryChips)
        {
            chip.ApplyThemeVisuals();
        }

        _cartCard.FillColor = PharmaTheme.SurfaceAlt;
        _cartCard.BorderColor = PharmaTheme.BorderSoft;
        _cartCard.ApplyThemeVisuals();
        _customerHeader.BackColor = PharmaTheme.SurfaceContainerHigh;
        _customerNameBox.BackColor = PharmaTheme.SurfaceContainerHigh;
        _customerNameBox.ForeColor = PharmaTheme.TextDark;
        _cartItemsScrollPanel.BackColor = PharmaTheme.SurfaceAlt;
        _cartItemsHost.BackColor = PharmaTheme.SurfaceAlt;
        _totalsFooter.BackColor = PharmaTheme.SurfaceContainerHigh;
        _emptyCartLabel.ForeColor = PharmaTheme.MutedText;

        _cashButton.ApplyThemeVisuals();
        _creditButton.ApplyThemeVisuals();
        _cardButton.ApplyThemeVisuals();
        _checkoutButton.ForeColor = PharmaTheme.OnPrimary;
        _checkoutButton.Invalidate();

        foreach (Control c in _productGridPanel.Controls)
        {
            if (c is PosProductCard card)
            {
                card.ApplyThemeVisuals();
            }
        }

        foreach (Control c in _cartItemsHost.Controls)
        {
            if (c is PosCartItemControl item)
            {
                item.ApplyThemeVisuals();
            }
        }

        Invalidate(true);
    }

    private void HandleThemeChanged(object? sender, EventArgs e)
    {
        if (IsDisposed)
        {
            return;
        }

        if (InvokeRequired)
        {
            BeginInvoke(ApplyThemeVisuals);
            return;
        }

        ApplyThemeVisuals();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            ThemeManager.ThemeChanged -= HandleThemeChanged;
            FontScaleManager.Changed -= HandleThemeChanged;
            _searchDebounce.Dispose();
            _loadCts?.Cancel();
            _loadCts?.Dispose();
            _searchCts?.Cancel();
            _searchCts?.Dispose();
        }

        base.Dispose(disposing);
    }
}
