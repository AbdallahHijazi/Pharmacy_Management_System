using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.IO;
using Pharmacy.WinForms.Controls;
using Pharmacy.WinForms.Models;
using Pharmacy.WinForms.Services;
using Pharmacy.WinForms.Ui;

namespace Pharmacy.WinForms.Forms.Settings;

/// <summary>
/// Settings content for MainForm host only. Manual responsive layout; no shell changes here.
/// </summary>
internal sealed class SettingsControl : UserControl
{
    private static Font SettingsIconFont(float size) =>
        PharmaTheme.IconFont(size);

    private const int ContentPadding = 32;
    private const int CardGap = 24;
    private const int MinCardWidth = 320;

    /// <remarks>Wide: two-column (38/62).</remarks>
    private const int TwoColumnBreakpoint = 1050;

    private const int SingleColumnBreakpoint = 700;

    private const int HeaderHeight = 124;
    private const int StatusExtraHeight = 40;
    private const int FieldHeight = 44;
    private const int LabelHeight = 24;
    private const int SectionTitleHeightInCard = 40;

    private const int PharmacyCardHeight = 290;
    private const int CurrencyCardHeight = 220;
    private const int AppearanceCardHeight = 380;
    private const int AlertsCardHeight = 250;
    private const int BackupCardHeight = 290;

    private static readonly (string Name, Color Color)[] ThemeOptions =
    [
        ("Healthcare Green", Color.FromArgb(7, 100, 67)),
        ("Medical Blue", Color.FromArgb(30, 64, 175)),
        ("Clinical Purple", Color.FromArgb(107, 33, 168)),
        ("Sky Teal", Color.FromArgb(15, 118, 110)),
        ("Dark Mode", Color.FromArgb(24, 24, 27)),
        ("Neutral Gray", Color.FromArgb(82, 82, 91))
    ];

    private readonly SettingsService _settingsService;

    private bool _uiBuilt;
    private bool _layoutReady;

    private SettingsFormState _loadedState = new();
    private IReadOnlyDictionary<string, SystemSettingApiModel> _settingsByKey =
        new Dictionary<string, SystemSettingApiModel>();
    private bool _isLoading;
    private bool _isSaving;

    private Panel _scrollPanel = null!;
    private Panel _contentCanvas = null!;

    private Label _titleLabel = null!;
    private Label _subtitleLabel = null!;
    private Label _statusLabel = null!;
    private GradientRoundedButton _saveButton = null!;
    private RoundedNeutralButton _cancelButton = null!;

    private SettingsCardPanel _pharmacyInfoCard = null!;
    private SettingsCardPanel _currencyCard = null!;
    private SettingsCardPanel _appearanceCard = null!;
    private SettingsCardPanel _alertsCard = null!;
    private SettingsCardPanel _backupCard = null!;

    private RoundedFieldBox _pharmacyNameInput = null!;
    private RoundedFieldBox _addressInput = null!;
    private RoundedFieldBox _phoneInput = null!;
    private SettingsToggleButton _currencySypButton = null!;
    private SettingsToggleButton _currencyUsdButton = null!;
    private RoundedFieldBox _exchangeRateInput = null!;
    private ThemeOptionButton[] _themeButtons = null!;
    private FontSizeSegmentButton _fontSmallButton = null!;
    private FontSizeSegmentButton _fontMediumButton = null!;
    private FontSizeSegmentButton _fontLargeButton = null!;
    private Label _fontSizeHintLabel = null!;
    private RoundedFieldBox _expiryDaysInput = null!;
    private RoundedFieldBox _lowStockInput = null!;
    private RoundedFieldBox _backupPathInput = null!;
    private RoundedComboInput _autoBackupComboHost = null!;
    private RoundedIconButton _browseFolderButton = null!;
    private RoundedPrimaryOutlineButton _backupNowButton = null!;

    public SettingsControl() : this(AppServices.SettingsService)
    {
    }

    public SettingsControl(SettingsService settingsService)
    {
        _settingsService = settingsService;

        SuspendLayout();

        SetStyle(
            ControlStyles.AllPaintingInWmPaint
                | ControlStyles.UserPaint
                | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.ResizeRedraw,
            true);

        Dock = DockStyle.Fill;
        BackColor = PharmaTheme.Background;
        AutoScroll = false;
        Padding = Padding.Empty;

        // Do NOT set RightToLeft yet — it fires layout before child fields exist.

        BuildUiControls();
        WireEvents();

        ThemeManager.ThemeChanged += HandleGlobalUiRefresh;
        FontScaleManager.Changed += HandleGlobalUiRefresh;

        _uiBuilt = true;
        _layoutReady = true;

        RightToLeft = RightToLeft.Yes;

        ResumeLayout(performLayout: false);

        Load += async (_, _) => await LoadSettingsFromServiceAsync();

        PerformLayout();
    }

    private void BuildUiControls()
    {
        _scrollPanel = new Panel
        {
            AutoScroll = true,
            BackColor = PharmaTheme.Background,
            Dock = DockStyle.Fill,
            Padding = Padding.Empty
        };

        _contentCanvas = new Panel
        {
            AutoSizeMode = AutoSizeMode.GrowOnly,
            BackColor = PharmaTheme.Background,
            Location = Point.Empty,
            Margin = Padding.Empty,
            Padding = Padding.Empty,
            Size = new Size(800, 2000)
        };

        _titleLabel = new Label
        {
            AutoSize = false,
            Font = PharmaTheme.ArabicFont(18f, FontStyle.Bold),
            ForeColor = PharmaTheme.PrimaryGreen,
            Height = 44,
            RightToLeft = RightToLeft.Yes,
            Text = "الإعدادات",
            TextAlign = ContentAlignment.MiddleRight,
            UseCompatibleTextRendering = true
        };

        _subtitleLabel = new Label
        {
            AutoSize = false,
            Font = PharmaTheme.SmallFont,
            ForeColor = PharmaTheme.OnSurfaceVariant,
            Height = 28,
            RightToLeft = RightToLeft.Yes,
            Text = "تكوين النظام وتفضيلات الصيدلية.",
            TextAlign = ContentAlignment.MiddleRight,
            UseCompatibleTextRendering = true
        };

        _statusLabel = new Label
        {
            AutoSize = false,
            Font = PharmaTheme.SmallFont,
            ForeColor = PharmaTheme.MutedText,
            Height = StatusExtraHeight,
            RightToLeft = RightToLeft.Yes,
            TextAlign = ContentAlignment.TopRight,
            UseCompatibleTextRendering = true,
            Visible = false
        };

        _saveButton = new GradientRoundedButton
        {
            IconGlyph = SegoeMdl2Icons.Save,
            MinimumSize = new Size(170, 44),
            Size = new Size(170, 44),
            Text = "حفظ التغييرات"
        };

        _cancelButton = new RoundedNeutralButton
        {
            Text = "إلغاء",
            Size = new Size(110, 44),
            RightToLeft = RightToLeft.Yes
        };

        _pharmacyNameInput = CreateTextInput("صيدلية الشفاء");
        _addressInput = CreateTextInput("شارع الاستقلال, البناء 4");
        _phoneInput = CreateTextInput("011-234-5678");
        _phoneInput.TextAlign = HorizontalAlignment.Left;

        _currencySypButton = new SettingsToggleButton("SYP", selected: true);
        _currencyUsdButton = new SettingsToggleButton("USD", selected: false);

        _exchangeRateInput = CreateTextInput("14500");
        _exchangeRateInput.TextAlign = HorizontalAlignment.Left;

        _pharmacyInfoCard = CreateSettingsCard(SegoeMdl2Icons.Store, "معلومات الصيدلية", PharmacyCardHeight);
        _currencyCard = CreateSettingsCard(SegoeMdl2Icons.Currency, "العملة", CurrencyCardHeight);
        _appearanceCard = CreateSettingsCard(SegoeMdl2Icons.Palette, "المظهر", AppearanceCardHeight);
        _alertsCard = CreateSettingsCard(SegoeMdl2Icons.Warning, "التنبيهات", AlertsCardHeight);
        _backupCard = CreateSettingsCard(SegoeMdl2Icons.Backup, "النسخ الاحتياطي", BackupCardHeight);

        _themeButtons = new ThemeOptionButton[ThemeOptions.Length];
        for (var i = 0; i < ThemeOptions.Length; i++)
        {
            var index = i;
            var option = ThemeOptions[i];
            var button = new ThemeOptionButton(option.Name, option.Color, index == 0);
            _themeButtons[i] = button;
            _appearanceCard.Controls.Add(button);
        }

        _fontSmallButton = new FontSizeSegmentButton("صغير") { Width = 72, RightToLeft = RightToLeft.Yes };
        _fontMediumButton = new FontSizeSegmentButton("متوسط") { Width = 80, RightToLeft = RightToLeft.Yes };
        _fontLargeButton = new FontSizeSegmentButton("كبير") { Width = 72, RightToLeft = RightToLeft.Yes };

        _fontSizeHintLabel = new Label
        {
            AutoSize = false,
            Font = PharmaTheme.SmallFont,
            ForeColor = PharmaTheme.MutedText,
            Height = LabelHeight,
            TextAlign = ContentAlignment.MiddleCenter,
            RightToLeft = RightToLeft.Yes,
            UseCompatibleTextRendering = true
        };
        SyncFontSegmentSelection(FontScaleManager.Level);
        UpdateFontSizeHint(FontScaleManager.Level);

        _expiryDaysInput = CreateNumberInput("90");
        _lowStockInput = CreateNumberInput("5");

        _backupPathInput = CreateTextInput(@"D:\PharmacyBackups");
        _backupPathInput.ReadOnly = true;
        _backupPathInput.TextAlign = HorizontalAlignment.Left;

        _browseFolderButton = new RoundedIconButton(SegoeMdl2Icons.Folder) { RightToLeft = RightToLeft.Yes };

        _autoBackupComboHost = new RoundedComboInput();
        var combo = _autoBackupComboHost.Combo;
        combo.RightToLeft = RightToLeft.Yes;
        combo.Items.AddRange(["يومياً", "أسبوعياً", "شهرياً"]);
        combo.SelectedIndex = 0;

        _backupNowButton = new RoundedPrimaryOutlineButton
        {
            Text = "إنشاء نسخة الآن",
            RightToLeft = RightToLeft.Yes
        };

        AddFieldToCard(_pharmacyInfoCard, "اسم الصيدلية", _pharmacyNameInput);
        AddFieldToCard(_pharmacyInfoCard, "العنوان", _addressInput);
        AddFieldToCard(_pharmacyInfoCard, "رقم الهاتف", _phoneInput);

        _currencyCard.Controls.Add(_currencySypButton);
        _currencyCard.Controls.Add(_currencyUsdButton);
        _currencyCard.Controls.Add(_exchangeRateInput);

        _appearanceCard.Controls.Add(_fontSmallButton);
        _appearanceCard.Controls.Add(_fontMediumButton);
        _appearanceCard.Controls.Add(_fontLargeButton);
        _appearanceCard.Controls.Add(_fontSizeHintLabel);

        _alertsCard.Controls.Add(_expiryDaysInput);
        _alertsCard.Controls.Add(_lowStockInput);

        _backupCard.Controls.Add(_backupPathInput);
        _backupCard.Controls.Add(_browseFolderButton);
        _backupCard.Controls.Add(_autoBackupComboHost);
        _backupCard.Controls.Add(_backupNowButton);

        _contentCanvas.Controls.Add(_backupCard);
        _contentCanvas.Controls.Add(_alertsCard);
        _contentCanvas.Controls.Add(_appearanceCard);
        _contentCanvas.Controls.Add(_currencyCard);
        _contentCanvas.Controls.Add(_pharmacyInfoCard);
        _contentCanvas.Controls.Add(_statusLabel);
        _contentCanvas.Controls.Add(_cancelButton);
        _contentCanvas.Controls.Add(_saveButton);
        _contentCanvas.Controls.Add(_subtitleLabel);
        _contentCanvas.Controls.Add(_titleLabel);

        _scrollPanel.Controls.Add(_contentCanvas);
        Controls.Add(_scrollPanel);
    }

    private void WireEvents()
    {
        _saveButton.Click += async (_, _) => await SaveAsync();
        _cancelButton.Click += (_, _) => CancelToLastSaved();
        _currencySypButton.Click += (_, _) => SetCurrency("SYP");
        _currencyUsdButton.Click += (_, _) => SetCurrency("USD");
        _fontSmallButton.Click += (_, _) => SetFontLevelPreview(1);
        _fontMediumButton.Click += (_, _) => SetFontLevelPreview(2);
        _fontLargeButton.Click += (_, _) => SetFontLevelPreview(3);
        _browseFolderButton.Click += (_, _) => BrowseBackupFolder();
        _backupNowButton.Click += (_, _) =>
            UiFeedback.ShowFeatureNotAvailable(FindForm(), "ميزة النسخ الاحتياطي");

        for (var i = 0; i < _themeButtons.Length; i++)
        {
            var index = i;
            _themeButtons[i].Click += (_, _) => SetTheme(index);
        }
    }

    protected override void OnLayout(LayoutEventArgs levent)
    {
        base.OnLayout(levent);

        if (!_uiBuilt || !_layoutReady || Disposing || IsDisposed)
        {
            return;
        }

        LayoutSettingsContent();
    }

    private void LayoutSettingsContent()
    {
        if (!_uiBuilt || !_layoutReady || Disposing || IsDisposed)
        {
            return;
        }

        if (ClientSize.Width <= 0 || ClientSize.Height <= 0)
        {
            return;
        }

        if (_scrollPanel is null
            || _contentCanvas is null
            || _pharmacyInfoCard is null
            || _currencyCard is null
            || _appearanceCard is null
            || _alertsCard is null
            || _backupCard is null
            || _saveButton is null
            || _cancelButton is null
            || _titleLabel is null
            || _subtitleLabel is null
            || _statusLabel is null
            || _themeButtons is null)
        {
            System.Diagnostics.Debug.WriteLine("Settings layout skipped: controls not initialized.");
            return;
        }

        var viewportW = Math.Max(360, _scrollPanel.ClientSize.Width);
        var contentWidth = Math.Max(MinCardWidth, viewportW - ContentPadding * 2);

        var contentLeft = ContentPadding;
        var contentRight = contentLeft + contentWidth;
        var y = ContentPadding;

        var actionsW = _saveButton.Width + 12 + _cancelButton.Width;
        var titleBlockW = Math.Max(200, contentWidth - actionsW - 16);
        _titleLabel.SetBounds(contentRight - titleBlockW, y, titleBlockW, 44);
        _subtitleLabel.SetBounds(contentRight - titleBlockW, y + 46, titleBlockW, 28);

        _saveButton.SetBounds(contentRight - _saveButton.Width, y + 10, _saveButton.Width, 44);
        _cancelButton.SetBounds(_saveButton.Left - 12 - _cancelButton.Width, y + 10, _cancelButton.Width, 44);

        y += HeaderHeight;

        if (_statusLabel.Visible)
        {
            _statusLabel.SetBounds(contentLeft, y, contentWidth, StatusExtraHeight);
            y += StatusExtraHeight;
        }

        var cardsTop = y;

        if (contentWidth < SingleColumnBreakpoint || contentWidth < MinCardWidth * 2 + CardGap)
        {
            LayoutSingleColumn(contentLeft, contentWidth, contentRight, ref y, cardsTop);
        }
        else if (contentWidth < TwoColumnBreakpoint)
        {
            LayoutMidWidth(contentLeft, contentWidth, contentRight, ref y, cardsTop);
        }
        else
        {
            LayoutWideTwoColumn(contentLeft, contentWidth, contentRight, ref y, cardsTop);
        }

        var totalHeight = y + ContentPadding;
        _contentCanvas.SetBounds(0, 0, viewportW, totalHeight);
        _scrollPanel.AutoScrollMinSize = new Size(0, totalHeight + ContentPadding);

        LayoutCardInternals();
    }

    private void LayoutSingleColumn(int contentLeft, int contentWidth, int contentRight, ref int y, int cardsTop)
    {
        y = cardsTop;
        var x = contentLeft;
        var w = contentWidth;

        PlaceCard(_pharmacyInfoCard, x, y, w, PharmacyCardHeight);
        y += PharmacyCardHeight + CardGap;
        PlaceCard(_currencyCard, x, y, w, CurrencyCardHeight);
        y += CurrencyCardHeight + CardGap;
        PlaceCard(_appearanceCard, x, y, w, AppearanceCardHeight);
        y += AppearanceCardHeight + CardGap;
        PlaceCard(_alertsCard, x, y, w, AlertsCardHeight);
        y += AlertsCardHeight + CardGap;
        PlaceCard(_backupCard, x, y, w, BackupCardHeight);
        y += BackupCardHeight;
    }

    private void LayoutMidWidth(int contentLeft, int contentWidth, int contentRight, ref int y, int cardsTop)
    {
        y = cardsTop;
        var half = (contentWidth - CardGap) / 2;
        var canSplitTop = half >= MinCardWidth;

        if (canSplitTop)
        {
            var rightX = contentRight - half;
            PlaceCard(_pharmacyInfoCard, rightX, y, half, PharmacyCardHeight);
            PlaceCard(_currencyCard, contentLeft, y, half, CurrencyCardHeight);
            y += Math.Max(PharmacyCardHeight, CurrencyCardHeight) + CardGap;
        }
        else
        {
            PlaceCard(_pharmacyInfoCard, contentLeft, y, contentWidth, PharmacyCardHeight);
            y += PharmacyCardHeight + CardGap;
            PlaceCard(_currencyCard, contentLeft, y, contentWidth, CurrencyCardHeight);
            y += CurrencyCardHeight + CardGap;
        }

        PlaceCard(_appearanceCard, contentLeft, y, contentWidth, AppearanceCardHeight);
        y += AppearanceCardHeight + CardGap;

        var splitW = (contentWidth - CardGap) / 2;
        if (splitW >= MinCardWidth)
        {
            PlaceCard(_alertsCard, contentLeft, y, splitW, AlertsCardHeight);
            PlaceCard(_backupCard, contentLeft + splitW + CardGap, y, splitW, BackupCardHeight);
            y += Math.Max(AlertsCardHeight, BackupCardHeight);
        }
        else
        {
            PlaceCard(_alertsCard, contentLeft, y, contentWidth, AlertsCardHeight);
            y += AlertsCardHeight + CardGap;
            PlaceCard(_backupCard, contentLeft, y, contentWidth, BackupCardHeight);
            y += BackupCardHeight;
        }
    }

    private void LayoutWideTwoColumn(int contentLeft, int contentWidth, int contentRight, ref int y, int cardsTop)
    {
        var rightColW = Math.Max(MinCardWidth, (int)(contentWidth * 0.38));
        var leftColW = contentWidth - CardGap - rightColW;
        if (leftColW < MinCardWidth)
        {
            LayoutSingleColumn(contentLeft, contentWidth, contentRight, ref y, cardsTop);
            return;
        }

        y = cardsTop;
        var rightX = contentRight - rightColW;
        var leftX = contentLeft;

        var rightY = cardsTop;
        var leftY = cardsTop;

        PlaceCard(_pharmacyInfoCard, rightX, rightY, rightColW, PharmacyCardHeight);
        rightY += PharmacyCardHeight + CardGap;
        PlaceCard(_currencyCard, rightX, rightY, rightColW, CurrencyCardHeight);
        rightY += CurrencyCardHeight;

        PlaceCard(_appearanceCard, leftX, leftY, leftColW, AppearanceCardHeight);
        leftY += AppearanceCardHeight + CardGap;

        var splitW = (leftColW - CardGap) / 2;
        if (splitW >= MinCardWidth)
        {
            PlaceCard(_alertsCard, leftX, leftY, splitW, AlertsCardHeight);
            PlaceCard(_backupCard, leftX + splitW + CardGap, leftY, splitW, BackupCardHeight);
            leftY += Math.Max(AlertsCardHeight, BackupCardHeight);
        }
        else
        {
            PlaceCard(_alertsCard, leftX, leftY, leftColW, AlertsCardHeight);
            leftY += AlertsCardHeight + CardGap;
            PlaceCard(_backupCard, leftX, leftY, leftColW, BackupCardHeight);
            leftY += BackupCardHeight;
        }

        y = Math.Max(rightY, leftY);
    }

    private static void PlaceCard(SettingsCardPanel card, int x, int y, int width, int height)
    {
        if (card is null || card.IsDisposed)
        {
            return;
        }

        card.SetBounds(x, y, Math.Max(MinCardWidth, width), height);
    }

    private void LayoutCardInternals()
    {
        if (Disposing || IsDisposed)
        {
            return;
        }

        LayoutPharmacyCard();
        LayoutCurrencyCard();
        LayoutAppearanceCard();
        LayoutAlertsCard();
        LayoutBackupCard();
    }

    private void LayoutPharmacyCard()
    {
        if (_pharmacyInfoCard is null || _pharmacyInfoCard.IsDisposed)
        {
            return;
        }

        var inner = _pharmacyInfoCard.InnerBounds;
        var y = inner.Y;
        LayoutFieldRow(_pharmacyInfoCard, "اسم الصيدلية", _pharmacyNameInput, inner, ref y);
        LayoutFieldRow(_pharmacyInfoCard, "العنوان", _addressInput, inner, ref y);
        LayoutFieldRow(_pharmacyInfoCard, "رقم الهاتف", _phoneInput, inner, ref y);
    }

    private static void LayoutFieldRow(
        SettingsCardPanel card,
        string label,
        Control field,
        Rectangle inner,
        ref int y)
    {
        if (field is null || field.IsDisposed)
        {
            return;
        }

        var caption = card.Controls.OfType<Label>().FirstOrDefault(l => ReferenceEquals(l.Tag, label));
        if (caption is null)
        {
            return;
        }

        caption.SetBounds(inner.X, y, inner.Width, LabelHeight);
        y += LabelHeight + 6;
        field.SetBounds(inner.X, y, inner.Width, FieldHeight);
        y += FieldHeight + 14;
    }

    private void LayoutCurrencyCard()
    {
        if (_currencyCard is null || _currencyCard.IsDisposed)
        {
            return;
        }

        var inner = _currencyCard.InnerBounds;
        var y = inner.Y;

        var capCurrency = EnsureCaption(_currencyCard, "العملة الافتراضية");
        capCurrency.SetBounds(inner.X, y, inner.Width, LabelHeight);
        y += LabelHeight + 8;

        var toggleW = 140;
        var toggleX = inner.Right - toggleW;
        _currencyUsdButton.SetBounds(toggleX, y, 64, FieldHeight - 6);
        _currencySypButton.SetBounds(toggleX - 70, y, 64, FieldHeight - 6);
        y += FieldHeight + 6;

        var capRate = EnsureCaption(_currencyCard, "سعر الصرف (SYP لـ 1 USD)");
        capRate.SetBounds(inner.X, y, inner.Width, LabelHeight);
        y += LabelHeight + 6;
        _exchangeRateInput.SetBounds(inner.X, y, inner.Width, FieldHeight);
    }

    private void LayoutAppearanceCard()
    {
        if (_appearanceCard is null || _appearanceCard.IsDisposed || _themeButtons is null)
        {
            return;
        }

        var inner = _appearanceCard.InnerBounds;
        var y = inner.Y;
        var half = (inner.Width - CardGap) / 2;
        var leftW = Math.Max(280, half);
        var rightW = inner.Width - CardGap - leftW;
        var rightX = inner.X + leftW + CardGap;

        var capTheme = EnsureCaption(_appearanceCard, "نسق الألوان");
        capTheme.SetBounds(inner.X, y, leftW, LabelHeight);
        y += LabelHeight + 8;
        var themeY = y;
        var cellW = Math.Max(72, (leftW - 24) / 3);
        var cellH = 86;
        for (var i = 0; i < _themeButtons.Length; i++)
        {
            var col = i % 3;
            var row = i / 3;
            _themeButtons[i].SetBounds(
                inner.X + col * (cellW + 8),
                themeY + row * (cellH + 8),
                cellW,
                cellH);
        }

        var fontY = inner.Y;
        var capFont = EnsureCaption(_appearanceCard, "حجم الخط");
        capFont.SetBounds(rightX, fontY, rightW, LabelHeight);
        fontY += LabelHeight + 8;

        var segW = Math.Max(64, (rightW - 18) / 3);
        var rowRight = rightX + rightW;
        _fontLargeButton.SetBounds(rowRight - segW, fontY, segW, 40);
        _fontMediumButton.SetBounds(rowRight - segW * 2 - 8, fontY, segW, 40);
        _fontSmallButton.SetBounds(rowRight - segW * 3 - 16, fontY, segW, 40);
        _fontSizeHintLabel.SetBounds(rightX, fontY + 46, rightW, LabelHeight);
    }

    private void LayoutAlertsCard()
    {
        if (_alertsCard is null || _alertsCard.IsDisposed)
        {
            return;
        }

        var inner = _alertsCard.InnerBounds;
        LayoutAlertRow(inner, "تحذير انتهاء الصلاحية", "قبل كم يوم يتم التنبيه", _expiryDaysInput, "يوم", 0);
        LayoutAlertRow(inner, "حد النقص في المخزون", "التنبيه عند وصول الكمية إلى", _lowStockInput, "علبة", 1);
    }

    private void LayoutBackupCard()
    {
        if (_backupCard is null || _backupCard.IsDisposed)
        {
            return;
        }

        var inner = _backupCard.InnerBounds;
        var y = inner.Y;

        var capPath = EnsureCaption(_backupCard, "مسار الحفظ المحلي");
        capPath.SetBounds(inner.X, y, inner.Width, LabelHeight);
        y += LabelHeight + 6;
        _browseFolderButton.SetBounds(inner.Right - 48, y, 48, FieldHeight);
        _backupPathInput.SetBounds(inner.X, y, inner.Width - 56, FieldHeight);
        y += FieldHeight + 16;

        var scheduleCaption = EnsureCaption(_backupCard, "النسخ التلقائي", PharmaTheme.BodyFont, PharmaTheme.TextDark);
        scheduleCaption.SetBounds(inner.X, y, inner.Width - 150, LabelHeight);
        _autoBackupComboHost.SetBounds(inner.Right - 156, y, 156, FieldHeight);
        y += FieldHeight + 16;

        _backupNowButton.SetBounds(inner.X, y, inner.Width, 44);
    }

    private static Label EnsureCaption(
        SettingsCardPanel card,
        string text,
        Font? font = null,
        Color? foreColor = null)
    {
        var caption = card.Controls.OfType<Label>()
            .FirstOrDefault(l => string.Equals(l.Tag as string, $"cap:{text}", StringComparison.Ordinal));
        if (caption is not null)
        {
            return caption;
        }

        caption = new Label
        {
            AutoSize = false,
            Font = font ?? PharmaTheme.SmallFont,
            ForeColor = foreColor ?? PharmaTheme.OnSurfaceVariant,
            Tag = $"cap:{text}",
            Text = text,
            TextAlign = ContentAlignment.MiddleRight,
            UseCompatibleTextRendering = true
        };
        card.Controls.Add(caption);
        return caption;
    }

    private void LayoutAlertRow(Rectangle inner, string title, string subtitle, RoundedFieldBox input, string unit, int index)
    {
        if (_alertsCard is null || input is null || input.IsDisposed)
        {
            return;
        }

        var rowH = 78;
        var y = inner.Y + index * (rowH + 10);
        var boxW = 56;
        var unitLabelW = 40;
        var valueX = inner.Right - boxW;
        input.SetBounds(valueX, y + 22, boxW, FieldHeight);

        var unitLabel = _alertsCard.Controls.OfType<Label>()
            .FirstOrDefault(l => l.Tag as string == $"unit-{index}");
        if (unitLabel is null)
        {
            unitLabel = new Label
            {
                AutoSize = false,
                Font = PharmaTheme.SmallFont,
                ForeColor = PharmaTheme.OnSurfaceVariant,
                Tag = $"unit-{index}",
                Text = unit,
                TextAlign = ContentAlignment.MiddleRight,
                UseCompatibleTextRendering = true
            };
            _alertsCard.Controls.Add(unitLabel);
        }

        unitLabel.SetBounds(valueX - unitLabelW - 4, y + 26, unitLabelW, LabelHeight);

        var titleLabel = _alertsCard.Controls.OfType<Label>()
            .FirstOrDefault(l => l.Tag as string == $"title-{index}");
        if (titleLabel is null)
        {
            titleLabel = new Label
            {
                AutoSize = false,
                Font = PharmaTheme.ArabicFont(10f, FontStyle.Bold),
                ForeColor = PharmaTheme.TextDark,
                Tag = $"title-{index}",
                UseCompatibleTextRendering = true
            };
            _alertsCard.Controls.Add(titleLabel);
        }

        titleLabel.Text = title;
        titleLabel.SetBounds(inner.X + 8, y + 8, inner.Width - boxW - unitLabelW - 28, 24);

        var subLabel = _alertsCard.Controls.OfType<Label>()
            .FirstOrDefault(l => l.Tag as string == $"sub-{index}");
        if (subLabel is null)
        {
            subLabel = new Label
            {
                AutoSize = false,
                Font = PharmaTheme.SmallFont,
                ForeColor = PharmaTheme.OnSurfaceVariant,
                Tag = $"sub-{index}",
                UseCompatibleTextRendering = true
            };
            _alertsCard.Controls.Add(subLabel);
        }

        subLabel.Text = subtitle;
        subLabel.SetBounds(inner.X + 8, y + 32, inner.Width - boxW - unitLabelW - 28, LabelHeight);
    }

    private static SettingsCardPanel CreateSettingsCard(string iconGlyph, string title, int height)
    {
        return new SettingsCardPanel(iconGlyph, title)
        {
            MinimumSize = new Size(MinCardWidth, height),
            Size = new Size(MinCardWidth, height)
        };
    }

    private static void AddFieldToCard(SettingsCardPanel card, string label, RoundedFieldBox input)
    {
        var caption = new Label
        {
            AutoSize = false,
            Font = PharmaTheme.SmallFont,
            ForeColor = PharmaTheme.OnSurfaceVariant,
            Height = LabelHeight,
            Tag = label,
            Text = label,
            TextAlign = ContentAlignment.MiddleRight,
            UseCompatibleTextRendering = true
        };
        card.Controls.Add(caption);
        card.Controls.Add(input);
    }

    private static RoundedFieldBox CreateTextInput(string value) => new()
    {
        Height = FieldHeight,
        MinimumSize = new Size(0, FieldHeight),
        Text = value,
        RightToLeft = RightToLeft.Yes
    };

    private static RoundedFieldBox CreateNumberInput(string value)
    {
        var box = CreateTextInput(value);
        box.TextAlign = HorizontalAlignment.Center;
        return box;
    }

    private async Task LoadSettingsFromServiceAsync()
    {
        if (_isLoading)
        {
            return;
        }

        _isLoading = true;
        SetBusy(true);
        try
        {
            var disk = LocalAppSettingsStore.LoadOrDefault();
            var result = await _settingsService.LoadAsync();
            _settingsByKey = result.SettingsByKey;

            var merged = MergeLocalWithApi(disk, result);
            _loadedState = merged.Clone();
            ApplyState(merged);
            UiBranding.SetPharmacyDisplayName(merged.PharmacyName);

            if (!string.IsNullOrWhiteSpace(result.ErrorMessage))
            {
                ShowStatus(result.ErrorMessage!, PharmaTheme.WarningStrong);
            }
            else
            {
                _statusLabel.Visible = false;
            }
        }
        finally
        {
            SetBusy(false);
            _isLoading = false;
            if (_layoutReady)
            {
                PerformLayout();
            }
        }
    }

    private async Task SaveAsync()
    {
        if (_isSaving || _isLoading)
        {
            return;
        }

        var current = CaptureState();
        if (!TryValidateForSave(current, out var validationMessage))
        {
            MessageBox.Show(
                FindForm(),
                validationMessage,
                "الإعدادات",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        var baselineForApi = _loadedState.Clone();
        _isSaving = true;
        SetBusy(true);
        try
        {
            if (!LocalAppSettingsStore.TrySave(current, out var localWriteError))
            {
                UiFeedback.ShowError(FindForm(), $"تعذر حفظ الإعدادات: {localWriteError}");
                return;
            }

            ThemeManager.ApplyThemeIndex(current.ThemeIndex);
            FontScaleManager.SetLevel(current.FontSizeLevel);
            UiBranding.SetPharmacyDisplayName(current.PharmacyName);

            var apiResult = await _settingsService.SaveAsync(current, baselineForApi, _settingsByKey);
            _loadedState = current.Clone();

            UiFeedback.ShowSuccess(FindForm(), "تم حفظ الإعدادات بنجاح");
            ShowStatus("تم حفظ الإعدادات على هذا الجهاز وتمت المحاولة على الخادم.", PharmaTheme.Success);

            var apiMessyFailure = !string.IsNullOrWhiteSpace(apiResult.ErrorMessage)
                && !apiResult.AnySaved
                && !apiResult.NoChanges
                && !apiResult.NotSupported;
            if (apiMessyFailure)
            {
                UiFeedback.ShowError(
                    FindForm(),
                    $"تم الحفظ محلياً، لكن الخادم أبلغ بتعطل: {apiResult.ErrorMessage}");
            }
        }
        finally
        {
            SetBusy(false);
            _isSaving = false;
        }
    }

    private void BrowseBackupFolder()
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "اختر مجلد النسخ الاحتياطي",
            SelectedPath = Directory.Exists(_backupPathInput.Text) ? _backupPathInput.Text : string.Empty,
            ShowNewFolderButton = true
        };

        if (dialog.ShowDialog(FindForm()) == DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.SelectedPath))
        {
            _backupPathInput.Text = dialog.SelectedPath;
            if (_layoutReady)
            {
                PerformLayout();
            }
        }
    }

    private void HandleGlobalUiRefresh(object? sender, EventArgs e)
    {
        if (IsDisposed || Disposing)
        {
            return;
        }

        if (InvokeRequired)
        {
            BeginInvoke(new Action(() => HandleGlobalUiRefresh(sender, e)));
            return;
        }

        ApplyThemeAndFontVisuals();
    }

    private void CancelToLastSaved()
    {
        var disk = LocalAppSettingsStore.LoadOrDefault();
        ApplyState(disk);
        UiBranding.SetPharmacyDisplayName(disk.PharmacyName);
    }

    private void SetFontLevelPreview(int level)
    {
        var clamped = Math.Clamp(level, 1, 3);
        FontScaleManager.SetLevel(clamped);
        SyncFontSegmentSelection(clamped);
        UpdateFontSizeHint(clamped);
    }

    private void SyncFontSegmentSelection(int level)
    {
        _fontSmallButton.IsSelected = level == 1;
        _fontMediumButton.IsSelected = level == 2;
        _fontLargeButton.IsSelected = level == 3;
    }

    private void UpdateFontSizeHint(int level)
    {
        if (_fontSizeHintLabel is null)
        {
            return;
        }

        var label = level switch
        {
            1 => "صغير",
            3 => "كبير",
            _ => "متوسط"
        };
        _fontSizeHintLabel.Text = $"الحجم الحالي: {label}";
    }

    private static SettingsFormState MergeLocalWithApi(SettingsFormState disk, SettingsLoadResult api)
    {
        var merged = disk.Clone();

        bool TryOverlay(string key, Action<string> apply)
        {
            if (!api.SettingsByKey.TryGetValue(key, out var row) || string.IsNullOrWhiteSpace(row.Value))
            {
                return false;
            }

            apply(row.Value.Trim());
            return true;
        }

        TryOverlay(SettingsKeys.PharmacyName, v => merged.PharmacyName = v);
        TryOverlay(SettingsKeys.PharmacyAddress, v => merged.Address = v);
        TryOverlay(SettingsKeys.PharmacyPhone, v => merged.Phone = v);
        TryOverlay(SettingsKeys.DefaultCurrency, v => merged.CurrencyCode = v);
        TryOverlay(SettingsKeys.ExchangeRate, v => merged.ExchangeRate = v);
        TryOverlay(SettingsKeys.ExpiryAlertDays, v => merged.ExpiryAlertDays = v);
        TryOverlay(SettingsKeys.LowStockThreshold, v => merged.LowStockThreshold = v);

        return merged;
    }

    private static bool TryValidateForSave(SettingsFormState s, out string message)
    {
        if (string.IsNullOrWhiteSpace(s.PharmacyName))
        {
            message = "يرجى إدخال اسم الصيدلية.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(s.Phone) || s.Phone.Trim().Length < 3)
        {
            message = "يرجى إدخال رقم هاتف صالح.";
            return false;
        }

        if (!decimal.TryParse(
                s.ExchangeRate,
                System.Globalization.NumberStyles.Number,
                System.Globalization.CultureInfo.InvariantCulture,
                out var rate) ||
            rate <= 0)
        {
            message = "يرجى إدخال سعر صرف موجب.";
            return false;
        }

        if (!int.TryParse(s.ExpiryAlertDays, out var exp) || exp <= 0)
        {
            message = "يرجى إدخال عدد أيام انتهاء الصلاحية بقيمة موجبة.";
            return false;
        }

        if (!int.TryParse(s.LowStockThreshold, out var low) || low < 0)
        {
            message = "يرجى إدخال حد النقص رقماً صفراً أو أكبر.";
            return false;
        }

        message = string.Empty;
        return true;
    }

    public void ApplyThemeAndFontVisuals()
    {
        if (!_uiBuilt)
        {
            return;
        }

        BackColor = PharmaTheme.Background;
        _scrollPanel.BackColor = PharmaTheme.Background;
        _contentCanvas.BackColor = PharmaTheme.Background;

        _titleLabel.Font = PharmaTheme.ArabicFont(18f, FontStyle.Bold);
        _titleLabel.ForeColor = PharmaTheme.PrimaryGreen;
        _subtitleLabel.Font = PharmaTheme.SmallFont;
        _subtitleLabel.ForeColor = PharmaTheme.OnSurfaceVariant;
        _statusLabel.Font = PharmaTheme.SmallFont;

        _saveButton.Invalidate();
        _cancelButton.RefreshThemeVisuals();

        _pharmacyNameInput.ApplyThemeVisuals();
        _addressInput.ApplyThemeVisuals();
        _phoneInput.ApplyThemeVisuals();
        _exchangeRateInput.ApplyThemeVisuals();
        _expiryDaysInput.ApplyThemeVisuals();
        _lowStockInput.ApplyThemeVisuals();
        _backupPathInput.ApplyThemeVisuals();

        _autoBackupComboHost.SyncTheme();
        _browseFolderButton.RefreshThemeVisuals();
        _backupNowButton.RefreshThemeVisuals();

        _currencySypButton.Invalidate();
        _currencyUsdButton.Invalidate();

        foreach (var b in _themeButtons)
        {
            b.Invalidate();
        }

        _fontSmallButton.Invalidate();
        _fontMediumButton.Invalidate();
        _fontLargeButton.Invalidate();
        _fontSizeHintLabel.Font = PharmaTheme.SmallFont;
        _fontSizeHintLabel.ForeColor = PharmaTheme.MutedText;

        foreach (Control c in _pharmacyInfoCard.Controls)
        {
            if (c is Label la && la.Tag is string)
            {
                la.Font = PharmaTheme.SmallFont;
                la.ForeColor = PharmaTheme.OnSurfaceVariant;
            }
        }

        Invalidate(true);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            ThemeManager.ThemeChanged -= HandleGlobalUiRefresh;
            FontScaleManager.Changed -= HandleGlobalUiRefresh;
        }

        base.Dispose(disposing);
    }

    private void SetCurrency(string code)
    {
        var normalized = string.Equals(code, "USD", StringComparison.OrdinalIgnoreCase) ? "USD" : "SYP";
        _currencySypButton.IsSelected = normalized == "SYP";
        _currencyUsdButton.IsSelected = normalized == "USD";
    }

    private void SetTheme(int index)
    {
        var clamped = Math.Clamp(index, 0, _themeButtons.Length - 1);
        for (var i = 0; i < _themeButtons.Length; i++)
        {
            _themeButtons[i].IsSelected = i == clamped;
        }

        ThemeManager.ApplyThemeIndex(clamped);
    }

    private SettingsFormState CaptureState()
    {
        var fontLevel = _fontLargeButton.IsSelected
            ? 3
            : _fontSmallButton.IsSelected
                ? 1
                : 2;
        var themeIndex = Array.FindIndex(_themeButtons, b => b.IsSelected);
        if (themeIndex < 0)
        {
            themeIndex = 0;
        }

        var combo = _autoBackupComboHost.Combo;
        return new SettingsFormState
        {
            PharmacyName = _pharmacyNameInput.Text ?? string.Empty,
            Address = _addressInput.Text ?? string.Empty,
            Phone = _phoneInput.Text ?? string.Empty,
            CurrencyCode = _currencySypButton.IsSelected ? "SYP" : "USD",
            ExchangeRate = _exchangeRateInput.Text ?? string.Empty,
            ThemeIndex = themeIndex,
            FontSizeLevel = fontLevel,
            ExpiryAlertDays = _expiryDaysInput.Text ?? string.Empty,
            LowStockThreshold = _lowStockInput.Text ?? string.Empty,
            BackupPath = _backupPathInput.Text ?? string.Empty,
            AutoBackupSchedule = combo.SelectedItem?.ToString() ?? "يومياً"
        };
    }

    private void ApplyState(SettingsFormState state)
    {
        _pharmacyNameInput.Text = state.PharmacyName;
        _addressInput.Text = state.Address;
        _phoneInput.Text = state.Phone;
        SetCurrency(state.CurrencyCode);
        _exchangeRateInput.Text = state.ExchangeRate;

        SetTheme(Math.Clamp(state.ThemeIndex, 0, _themeButtons.Length - 1));

        var lvl = Math.Clamp(state.FontSizeLevel, 1, 3);
        SyncFontSegmentSelection(lvl);
        UpdateFontSizeHint(lvl);
        if (lvl != FontScaleManager.Level)
        {
            FontScaleManager.SetLevel(lvl);
        }

        _expiryDaysInput.Text = state.ExpiryAlertDays;
        _lowStockInput.Text = state.LowStockThreshold;
        _backupPathInput.Text = state.BackupPath;

        var combo = _autoBackupComboHost.Combo;
        var scheduleIndex = combo.Items.IndexOf(state.AutoBackupSchedule);
        combo.SelectedIndex = scheduleIndex >= 0 ? scheduleIndex : 0;
        if (_layoutReady)
        {
            PerformLayout();
        }
    }

    private void SetBusy(bool busy)
    {
        Enabled = !busy;
        UseWaitCursor = busy;
    }

    private void ShowStatus(string text, Color color)
    {
        _statusLabel.Text = text;
        _statusLabel.ForeColor = color;
        _statusLabel.Visible = true;
        if (_layoutReady)
        {
            PerformLayout();
        }
    }

    private sealed class SettingsCardPanel : Panel
    {
        private readonly Label _titleLabel;
        private readonly Label _iconLabel;
        private readonly int _cornerRadius = 20;
        private const int Pad = 22;

        public Rectangle InnerBounds
        {
            get
            {
                var w = Math.Max(0, ClientSize.Width - Pad * 2);
                return new Rectangle(
                    Pad,
                    Pad + SectionTitleHeightInCard + 10,
                    w,
                    Math.Max(0, ClientSize.Height - Pad * 2 - SectionTitleHeightInCard - 10));
            }
        }

        public SettingsCardPanel(string iconGlyph, string title)
        {
            BackColor = PharmaTheme.Surface;
            DoubleBuffered = true;
            Padding = Padding.Empty;
            SetStyle(
                ControlStyles.AllPaintingInWmPaint
                    | ControlStyles.OptimizedDoubleBuffer
                    | ControlStyles.ResizeRedraw
                    | ControlStyles.UserPaint,
                true);

            _iconLabel = new Label
            {
                BackColor = PharmaTheme.SurfaceContainer,
                Font = SettingsIconFont(15f),
                ForeColor = PharmaTheme.PrimaryGreen,
                Size = new Size(40, 40),
                Text = iconGlyph,
                TextAlign = ContentAlignment.MiddleCenter,
                UseCompatibleTextRendering = true
            };

            _titleLabel = new Label
            {
                AutoSize = false,
                Font = PharmaTheme.SectionFont,
                ForeColor = PharmaTheme.TextDark,
                Height = SectionTitleHeightInCard,
                Text = title,
                TextAlign = ContentAlignment.MiddleRight,
                UseCompatibleTextRendering = true
            };

            Controls.Add(_titleLabel);
            Controls.Add(_iconLabel);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (Disposing || IsDisposed)
            {
                return;
            }

            var headerTop = Pad;
            _iconLabel.SetBounds(Width - Pad - 40, headerTop, 40, 40);
            _titleLabel.SetBounds(Pad, headerTop + 2, Math.Max(80, Width - Pad * 2 - 48), SectionTitleHeightInCard);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            using var brush = new SolidBrush(PharmaTheme.Background);
            e.Graphics.FillRectangle(brush, ClientRectangle);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var bounds = ClientRectangle;
            bounds.Inflate(-2, -2);
            if (bounds.Width <= 4 || bounds.Height <= 4)
            {
                return;
            }

            RoundedDrawing.DrawSoftShadow(g, bounds, _cornerRadius, PharmaTheme.DashboardCardShadow);
            RoundedDrawing.FillRounded(g, bounds, _cornerRadius, PharmaTheme.Surface);
            RoundedDrawing.DrawRoundedBorder(g, bounds, _cornerRadius, PharmaTheme.BorderSoft);
        }
    }

    private sealed class SettingsToggleButton : Control
    {
        private bool _isSelected;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                Invalidate();
            }
        }

        public SettingsToggleButton(string text, bool selected)
        {
            Text = text;
            Size = new Size(68, 38);
            _isSelected = selected;
            Cursor = Cursors.Hand;
            Font = PharmaTheme.BodyFont;
            SetStyle(
                ControlStyles.AllPaintingInWmPaint
                    | ControlStyles.OptimizedDoubleBuffer
                    | ControlStyles.UserPaint
                    | ControlStyles.StandardClick,
                true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var bounds = ClientRectangle;
            bounds.Inflate(-1, -1);
            var back = _isSelected ? PharmaTheme.PrimaryGreen : PharmaTheme.SurfaceContainerHighest;
            var textColor = _isSelected ? Color.White : PharmaTheme.OnSurfaceVariant;
            RoundedDrawing.FillRounded(e.Graphics, bounds, 12, back);
            TextRenderer.DrawText(
                e.Graphics,
                Text,
                Font,
                bounds,
                textColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }
    }

    private sealed class ThemeOptionButton : Control
    {
        private readonly string _caption;
        private readonly Color _swatchColor;
        private bool _isSelected;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                Invalidate();
            }
        }

        public ThemeOptionButton(string caption, Color swatchColor, bool selected)
        {
            _caption = caption;
            _swatchColor = swatchColor;
            _isSelected = selected;
            MinimumSize = new Size(88, 80);
            Cursor = Cursors.Hand;
            SetStyle(
                ControlStyles.AllPaintingInWmPaint
                    | ControlStyles.OptimizedDoubleBuffer
                    | ControlStyles.UserPaint
                    | ControlStyles.StandardClick,
                true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var bounds = ClientRectangle;
            bounds.Inflate(-2, -2);

            var back = _isSelected ? PharmaTheme.SurfaceContainerLow : PharmaTheme.Surface;
            RoundedDrawing.FillRounded(g, bounds, 10, back);
            if (_isSelected)
            {
                RoundedDrawing.DrawRoundedBorder(g, bounds, 10, PharmaTheme.PrimaryGreen, 2f);
            }

            var circle = new Rectangle(bounds.X + (bounds.Width - 32) / 2, bounds.Y + 8, 32, 32);
            using (var brush = new SolidBrush(_swatchColor))
            {
                g.FillEllipse(brush, circle);
            }

            TextRenderer.DrawText(
                g,
                _caption,
                PharmaTheme.SmallFont,
                new Rectangle(bounds.X + 2, circle.Bottom + 4, bounds.Width - 4, bounds.Height - circle.Bottom - 6),
                _isSelected ? PharmaTheme.TextDark : PharmaTheme.OnSurfaceVariant,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.Top | TextFormatFlags.WordBreak);
        }
    }
}
