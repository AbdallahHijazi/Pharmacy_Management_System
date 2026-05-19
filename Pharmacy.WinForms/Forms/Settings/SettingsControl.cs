using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.IO;
using Pharmacy.WinForms.Controls;
using Pharmacy.WinForms.Models;
using Pharmacy.WinForms.Services;
using Pharmacy.WinForms.Ui;

namespace Pharmacy.WinForms.Forms.Settings;

/// <summary>
/// Settings page content only. Layout is manual/responsive via <see cref="LayoutSettingsContent"/>.
/// Sidebar / TopBar are owned by MainForm and are not modified here.
/// </summary>
internal sealed class SettingsControl : UserControl
{
    private const int ContentPadding = 32;
    private const int CardGap = 24;
    private const int MinCardWidth = 320;
    private const int TwoColumnBreakpoint = 1100;
    private const int SingleColumnBreakpoint = 700;

    private const int HeaderHeight = 120;
    private const int StatusExtraHeight = 36;
    private const int FieldHeight = 42;
    private const int LabelHeight = 24;
    private const int SectionHeaderHeight = 40;

    private const int PharmacyCardHeight = 280;
    private const int CurrencyCardHeight = 210;
    private const int AppearanceCardHeight = 340;
    private const int AlertsCardHeight = 240;
    private const int BackupCardHeight = 280;

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
    private SettingsFormState _loadedState = new();
    private IReadOnlyDictionary<string, SystemSettingApiModel> _settingsByKey =
        new Dictionary<string, SystemSettingApiModel>();
    private bool _isLoading;
    private bool _isSaving;

    private readonly Panel _scrollPanel;
    private readonly Panel _contentCanvas;

    private readonly Label _titleLabel;
    private readonly Label _subtitleLabel;
    private readonly Label _statusLabel;
    private readonly GradientRoundedButton _saveButton;
    private readonly Button _cancelButton;

    private readonly SettingsCardPanel _cardPharmacy;
    private readonly SettingsCardPanel _cardCurrency;
    private readonly SettingsCardPanel _cardAppearance;
    private readonly SettingsCardPanel _cardAlerts;
    private readonly SettingsCardPanel _cardBackup;

    private readonly TextBox _pharmacyNameInput;
    private readonly TextBox _addressInput;
    private readonly TextBox _phoneInput;
    private readonly SettingsToggleButton _currencySypButton;
    private readonly SettingsToggleButton _currencyUsdButton;
    private readonly TextBox _exchangeRateInput;
    private readonly ThemeOptionButton[] _themeButtons;
    private readonly TrackBar _fontSizeTrack;
    private readonly Label _fontSizeHintLabel;
    private readonly TextBox _expiryDaysInput;
    private readonly TextBox _lowStockInput;
    private readonly TextBox _backupPathInput;
    private readonly ComboBox _autoBackupCombo;
    private readonly Button _browseFolderButton;
    private readonly Button _backupNowButton;

    public SettingsControl() : this(AppServices.SettingsService)
    {
    }

    public SettingsControl(SettingsService settingsService)
    {
        _settingsService = settingsService;

        Dock = DockStyle.Fill;
        BackColor = PharmaTheme.Background;
        RightToLeft = RightToLeft.Yes;
        AutoScroll = false;
        Padding = Padding.Empty;

        _scrollPanel = new Panel
        {
            AutoScroll = true,
            BackColor = PharmaTheme.Background,
            Dock = DockStyle.Fill
        };

        _contentCanvas = new Panel
        {
            BackColor = PharmaTheme.Background,
            Location = Point.Empty,
            Size = new Size(400, 1200)
        };

        _titleLabel = new Label
        {
            AutoSize = false,
            Font = PharmaTheme.ArabicFont(18f, FontStyle.Bold),
            ForeColor = PharmaTheme.PrimaryGreen,
            Height = 36,
            Text = "الإعدادات",
            TextAlign = ContentAlignment.MiddleRight,
            UseCompatibleTextRendering = true
        };

        _subtitleLabel = new Label
        {
            AutoSize = false,
            Font = PharmaTheme.SmallFont,
            ForeColor = PharmaTheme.OnSurfaceVariant,
            Height = LabelHeight,
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
            TextAlign = ContentAlignment.TopRight,
            UseCompatibleTextRendering = true,
            Visible = false
        };

        _saveButton = new GradientRoundedButton
        {
            IconGlyph = SegoeMdl2Icons.Save,
            Size = new Size(160, 44),
            Text = "حفظ التغييرات"
        };
        _saveButton.Click += async (_, _) => await SaveAsync();

        _cancelButton = new Button
        {
            BackColor = PharmaTheme.SurfaceContainer,
            FlatStyle = FlatStyle.Flat,
            Font = PharmaTheme.BodyFont,
            ForeColor = PharmaTheme.TextDark,
            Size = new Size(100, 44),
            Text = "إلغاء",
            UseCompatibleTextRendering = true
        };
        _cancelButton.FlatAppearance.BorderSize = 0;
        _cancelButton.Click += (_, _) => ApplyState(_loadedState.Clone());

        _pharmacyNameInput = CreateTextInput("صيدلية الشفاء");
        _addressInput = CreateTextInput("شارع الاستقلال, البناء 4");
        _phoneInput = CreateTextInput("011-234-5678");
        _phoneInput.TextAlign = HorizontalAlignment.Left;

        _currencySypButton = new SettingsToggleButton("SYP", selected: true);
        _currencyUsdButton = new SettingsToggleButton("USD", selected: false);
        _currencySypButton.Click += (_, _) => SetCurrency("SYP");
        _currencyUsdButton.Click += (_, _) => SetCurrency("USD");

        _exchangeRateInput = CreateTextInput("14500");
        _exchangeRateInput.TextAlign = HorizontalAlignment.Left;

        _cardPharmacy = CreateSettingsCard(SegoeMdl2Icons.Store, "معلومات الصيدلية", PharmacyCardHeight);
        _cardCurrency = CreateSettingsCard(SegoeMdl2Icons.Currency, "العملة", CurrencyCardHeight);
        _cardAppearance = CreateSettingsCard(SegoeMdl2Icons.Palette, "المظهر", AppearanceCardHeight);
        _cardAlerts = CreateSettingsCard(SegoeMdl2Icons.Warning, "التنبيهات", AlertsCardHeight);
        _cardBackup = CreateSettingsCard(SegoeMdl2Icons.Backup, "النسخ الاحتياطي", BackupCardHeight);

        _themeButtons = new ThemeOptionButton[ThemeOptions.Length];
        for (var i = 0; i < ThemeOptions.Length; i++)
        {
            var index = i;
            var option = ThemeOptions[i];
            var button = new ThemeOptionButton(option.Name, option.Color, index == 0);
            button.Click += (_, _) => SetTheme(index);
            _themeButtons[i] = button;
            _cardAppearance.Controls.Add(button);
        }

        _fontSizeTrack = new TrackBar
        {
            Height = 36,
            LargeChange = 1,
            Maximum = 3,
            Minimum = 1,
            RightToLeft = RightToLeft.No,
            TickStyle = TickStyle.None,
            Value = 2,
            Width = 200
        };
        _fontSizeTrack.ValueChanged += (_, _) => UpdateFontSizeHint();

        _fontSizeHintLabel = new Label
        {
            AutoSize = false,
            Font = PharmaTheme.SmallFont,
            ForeColor = PharmaTheme.MutedText,
            Height = LabelHeight,
            TextAlign = ContentAlignment.MiddleCenter,
            UseCompatibleTextRendering = true
        };
        UpdateFontSizeHint();

        _expiryDaysInput = CreateNumberInput("90");
        _lowStockInput = CreateNumberInput("5");

        _backupPathInput = CreateTextInput(@"D:\PharmacyBackups");
        _backupPathInput.ReadOnly = true;
        _backupPathInput.TextAlign = HorizontalAlignment.Left;

        _browseFolderButton = new Button
        {
            BackColor = PharmaTheme.SurfaceContainerHighest,
            FlatStyle = FlatStyle.Flat,
            Font = PharmaTheme.IconFont(13f),
            ForeColor = PharmaTheme.PrimaryGreen,
            Size = new Size(48, FieldHeight),
            Text = SegoeMdl2Icons.Folder,
            UseCompatibleTextRendering = true
        };
        _browseFolderButton.FlatAppearance.BorderSize = 0;
        _browseFolderButton.Click += (_, _) => BrowseBackupFolder();

        _autoBackupCombo = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = PharmaTheme.BodyFont,
            Height = FieldHeight,
            Width = 140
        };
        _autoBackupCombo.Items.AddRange(["يومياً", "أسبوعياً", "شهرياً"]);
        _autoBackupCombo.SelectedIndex = 0;

        _backupNowButton = new Button
        {
            BackColor = PharmaTheme.SurfaceContainerLow,
            FlatStyle = FlatStyle.Flat,
            Font = PharmaTheme.ArabicFont(10f, FontStyle.Bold),
            ForeColor = PharmaTheme.PrimaryGreen,
            Height = 44,
            Text = "إنشاء نسخة الآن",
            UseCompatibleTextRendering = true
        };
        _backupNowButton.FlatAppearance.BorderColor = PharmaTheme.PrimaryGreen;
        _backupNowButton.FlatAppearance.BorderSize = 1;
        _backupNowButton.Click += (_, _) =>
            UiFeedback.ShowFeatureNotAvailable(FindForm(), "ميزة النسخ الاحتياطي");

        AddFieldToCard(_cardPharmacy, "اسم الصيدلية", _pharmacyNameInput, 0);
        AddFieldToCard(_cardPharmacy, "العنوان", _addressInput, 1);
        AddFieldToCard(_cardPharmacy, "رقم الهاتف", _phoneInput, 2);

        _cardCurrency.Controls.Add(_currencySypButton);
        _cardCurrency.Controls.Add(_currencyUsdButton);
        _cardCurrency.Controls.Add(_exchangeRateInput);

        _cardAppearance.Controls.Add(_fontSizeTrack);
        _cardAppearance.Controls.Add(_fontSizeHintLabel);

        _cardAlerts.Controls.Add(_expiryDaysInput);
        _cardAlerts.Controls.Add(_lowStockInput);

        _cardBackup.Controls.Add(_backupPathInput);
        _cardBackup.Controls.Add(_browseFolderButton);
        _cardBackup.Controls.Add(_autoBackupCombo);
        _cardBackup.Controls.Add(_backupNowButton);

        _contentCanvas.Controls.Add(_cardBackup);
        _contentCanvas.Controls.Add(_cardAlerts);
        _contentCanvas.Controls.Add(_cardAppearance);
        _contentCanvas.Controls.Add(_cardCurrency);
        _contentCanvas.Controls.Add(_cardPharmacy);
        _contentCanvas.Controls.Add(_statusLabel);
        _contentCanvas.Controls.Add(_cancelButton);
        _contentCanvas.Controls.Add(_saveButton);
        _contentCanvas.Controls.Add(_subtitleLabel);
        _contentCanvas.Controls.Add(_titleLabel);

        _scrollPanel.Controls.Add(_contentCanvas);
        Controls.Add(_scrollPanel);

        Load += async (_, _) => await LoadSettingsAsync();
        _scrollPanel.Resize += (_, _) => LayoutSettingsContent();
        Resize += (_, _) => LayoutSettingsContent();
    }

    protected override void OnLayout(LayoutEventArgs levent)
    {
        base.OnLayout(levent);
        LayoutSettingsContent();
    }

    private void LayoutSettingsContent()
    {
        if (Disposing || IsDisposed || _scrollPanel.IsDisposed)
        {
            return;
        }

        var viewportW = Math.Max(360, _scrollPanel.ClientSize.Width);
        var availableW = Math.Max(MinCardWidth, viewportW - ContentPadding * 2);
        var singleColumn = availableW < SingleColumnBreakpoint
            || availableW < MinCardWidth * 2 + CardGap;

        var rightColW = singleColumn
            ? availableW
            : Math.Max(MinCardWidth, (int)(availableW * 0.38));
        if (!singleColumn && viewportW >= TwoColumnBreakpoint)
        {
            rightColW = Math.Max(MinCardWidth, (int)(availableW * 0.38));
        }
        else if (!singleColumn)
        {
            rightColW = Math.Max(MinCardWidth, (availableW - CardGap) / 2);
        }

        var leftColW = singleColumn ? availableW : availableW - CardGap - rightColW;
        if (!singleColumn && leftColW < 520)
        {
            singleColumn = true;
            rightColW = availableW;
            leftColW = availableW;
        }

        var contentRight = viewportW - ContentPadding;
        var contentLeft = ContentPadding;
        var y = ContentPadding;

        var actionsW = _saveButton.Width + 12 + _cancelButton.Width;
        var titleW = singleColumn ? availableW : Math.Max(240, availableW - actionsW - 16);
        _titleLabel.SetBounds(contentRight - titleW, y, titleW, 36);
        _subtitleLabel.SetBounds(contentRight - titleW, y + 38, titleW, LabelHeight);

        _saveButton.SetBounds(contentRight - _saveButton.Width, y + 8, _saveButton.Width, 44);
        _cancelButton.SetBounds(_saveButton.Left - 12 - _cancelButton.Width, y + 8, _cancelButton.Width, 44);

        y += HeaderHeight;

        if (_statusLabel.Visible)
        {
            _statusLabel.SetBounds(contentLeft, y, availableW, StatusExtraHeight);
            y += StatusExtraHeight;
        }

        var cardsTop = y;

        if (singleColumn)
        {
            var cardW = availableW;
            var x = contentLeft;

            PlaceCard(_cardPharmacy, x, y, cardW, PharmacyCardHeight);
            y += PharmacyCardHeight + CardGap;

            PlaceCard(_cardCurrency, x, y, cardW, CurrencyCardHeight);
            y += CurrencyCardHeight + CardGap;

            PlaceCard(_cardAppearance, x, y, cardW, AppearanceCardHeight);
            y += AppearanceCardHeight + CardGap;

            PlaceCard(_cardAlerts, x, y, cardW, AlertsCardHeight);
            y += AlertsCardHeight + CardGap;

            PlaceCard(_cardBackup, x, y, cardW, BackupCardHeight);
            y += BackupCardHeight;
        }
        else
        {
            var rightX = contentRight - rightColW;
            var leftX = contentLeft;
            var rightY = cardsTop;
            var leftY = cardsTop;

            PlaceCard(_cardPharmacy, rightX, rightY, rightColW, PharmacyCardHeight);
            rightY += PharmacyCardHeight + CardGap;

            PlaceCard(_cardCurrency, rightX, rightY, rightColW, CurrencyCardHeight);
            rightY += CurrencyCardHeight;

            PlaceCard(_cardAppearance, leftX, leftY, leftColW, AppearanceCardHeight);
            leftY += AppearanceCardHeight + CardGap;

            var splitW = (leftColW - CardGap) / 2;
            if (splitW >= MinCardWidth)
            {
                PlaceCard(_cardAlerts, leftX, leftY, splitW, AlertsCardHeight);
                PlaceCard(_cardBackup, leftX + splitW + CardGap, leftY, splitW, BackupCardHeight);
                leftY += Math.Max(AlertsCardHeight, BackupCardHeight);
            }
            else
            {
                PlaceCard(_cardAlerts, leftX, leftY, leftColW, AlertsCardHeight);
                leftY += AlertsCardHeight + CardGap;
                PlaceCard(_cardBackup, leftX, leftY, leftColW, BackupCardHeight);
                leftY += BackupCardHeight;
            }

            y = Math.Max(rightY, leftY);
        }

        var totalHeight = y + ContentPadding;
        _contentCanvas.Size = new Size(viewportW, totalHeight);
        _scrollPanel.AutoScrollMinSize = new Size(0, totalHeight);

        LayoutCardInternals();
    }

    private void PlaceCard(SettingsCardPanel card, int x, int y, int width, int height)
    {
        if (card.IsDisposed)
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
        if (_cardPharmacy.IsDisposed)
        {
            return;
        }

        var inner = _cardPharmacy.InnerBounds;
        var y = inner.Y;
        LayoutFieldRow(_cardPharmacy, "اسم الصيدلية", _pharmacyNameInput, inner, ref y);
        LayoutFieldRow(_cardPharmacy, "العنوان", _addressInput, inner, ref y);
        LayoutFieldRow(_cardPharmacy, "رقم الهاتف", _phoneInput, inner, ref y);
    }

    private static void LayoutFieldRow(
        SettingsCardPanel card,
        string label,
        TextBox field,
        Rectangle inner,
        ref int y)
    {
        var caption = card.Controls.OfType<Label>().FirstOrDefault(l => ReferenceEquals(l.Tag, label));
        if (caption is null)
        {
            return;
        }

        caption.SetBounds(inner.X, y, inner.Width, LabelHeight);
        y += LabelHeight + 4;
        field.SetBounds(inner.X, y, inner.Width, FieldHeight);
        y += FieldHeight + 14;
    }

    private void LayoutCurrencyCard()
    {
        if (_cardCurrency.IsDisposed)
        {
            return;
        }

        var inner = _cardCurrency.InnerBounds;
        var y = inner.Y;

        var capCurrency = EnsureCaption(_cardCurrency, "العملة الافتراضية");
        capCurrency.SetBounds(inner.X, y, inner.Width, LabelHeight);
        y += LabelHeight + 6;
        var toggleW = 140;
        var toggleX = inner.Right - toggleW;
        _currencyUsdButton.SetBounds(toggleX, y, 64, 36);
        _currencySypButton.SetBounds(toggleX - 68, y, 64, 36);
        y += 44;

        var capRate = EnsureCaption(_cardCurrency, "سعر الصرف (SYP لـ 1 USD)");
        capRate.SetBounds(inner.X, y, inner.Width, LabelHeight);
        y += LabelHeight + 4;
        _exchangeRateInput.SetBounds(inner.X, y, inner.Width, FieldHeight);
    }

    private void LayoutAppearanceCard()
    {
        if (_cardAppearance.IsDisposed)
        {
            return;
        }

        var inner = _cardAppearance.InnerBounds;
        var y = inner.Y;
        var half = (inner.Width - CardGap) / 2;
        var leftW = Math.Max(280, half);
        var rightW = inner.Width - CardGap - leftW;
        var rightX = inner.X + leftW + CardGap;

        var capTheme = EnsureCaption(_cardAppearance, "نسق الألوان");
        capTheme.SetBounds(inner.X, y, leftW, LabelHeight);
        y += LabelHeight + 6;
        var themeY = y;
        var cellW = (leftW - 16) / 3;
        var cellH = 82;
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

        var themeBlockH = cellH * 2 + 8 + 16;
        var fontY = inner.Y;
        var capFont = EnsureCaption(_cardAppearance, "حجم الخط");
        capFont.SetBounds(rightX, fontY, rightW, LabelHeight);
        fontY += LabelHeight + 6;
        _fontSizeTrack.SetBounds(rightX, fontY, rightW, 36);
        _fontSizeHintLabel.SetBounds(rightX, fontY + 40, rightW, LabelHeight);

        _ = themeBlockH;
    }

    private void LayoutAlertsCard()
    {
        if (_cardAlerts.IsDisposed)
        {
            return;
        }

        var inner = _cardAlerts.InnerBounds;
        LayoutAlertRow(inner, "تحذير انتهاء الصلاحية", "قبل كم يوم يتم التنبيه", _expiryDaysInput, "يوم", 0);
        LayoutAlertRow(inner, "حد النقص في المخزون", "التنبيه عند وصول الكمية إلى", _lowStockInput, "علبة", 1);
    }

    private void LayoutBackupCard()
    {
        if (_cardBackup.IsDisposed)
        {
            return;
        }

        var inner = _cardBackup.InnerBounds;
        var y = inner.Y;

        var capPath = EnsureCaption(_cardBackup, "مسار الحفظ المحلي");
        capPath.SetBounds(inner.X, y, inner.Width, LabelHeight);
        y += LabelHeight + 4;
        _browseFolderButton.SetBounds(inner.Right - 48, y, 48, FieldHeight);
        _backupPathInput.SetBounds(inner.X, y, inner.Width - 56, FieldHeight);
        y += FieldHeight + 14;

        var scheduleCaption = EnsureCaption(_cardBackup, "النسخ التلقائي", PharmaTheme.BodyFont, PharmaTheme.TextDark);
        scheduleCaption.SetBounds(inner.X, y, inner.Width - 150, LabelHeight);
        _autoBackupCombo.SetBounds(inner.Right - 140, y, 140, FieldHeight);
        y += FieldHeight + 14;

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

    private void LayoutAlertRow(Rectangle inner, string title, string subtitle, TextBox input, string unit, int index)
    {
        var rowH = 72;
        var y = inner.Y + index * (rowH + 10);
        var boxW = 52;
        var unitLabelW = 36;
        var valueX = inner.Right - boxW;
        input.SetBounds(valueX, y + 20, boxW, FieldHeight);

        var unitLabel = _cardAlerts.Controls.OfType<Label>()
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
            _cardAlerts.Controls.Add(unitLabel);
        }

        unitLabel.SetBounds(valueX - unitLabelW - 4, y + 24, unitLabelW, LabelHeight);

        var titleLabel = _cardAlerts.Controls.OfType<Label>()
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
            _cardAlerts.Controls.Add(titleLabel);
        }

        titleLabel.Text = title;
        titleLabel.SetBounds(inner.X + 8, y + 8, inner.Width - boxW - unitLabelW - 24, 22);

        var subLabel = _cardAlerts.Controls.OfType<Label>()
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
            _cardAlerts.Controls.Add(subLabel);
        }

        subLabel.Text = subtitle;
        subLabel.SetBounds(inner.X + 8, y + 30, inner.Width - boxW - unitLabelW - 24, LabelHeight);
    }

    private static SettingsCardPanel CreateSettingsCard(string iconGlyph, string title, int height)
    {
        var card = new SettingsCardPanel(iconGlyph, title)
        {
            MinimumSize = new Size(MinCardWidth, height),
            Size = new Size(MinCardWidth, height)
        };
        return card;
    }

    private static void AddFieldToCard(SettingsCardPanel card, string label, TextBox input, int index)
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
            UseCompatibleTextRendering = true,
            Top = index * 80
        };
        card.Controls.Add(caption);
        card.Controls.Add(input);
    }

    private static TextBox CreateTextInput(string value) => new()
    {
        BackColor = PharmaTheme.SurfaceContainerHighest,
        BorderStyle = BorderStyle.FixedSingle,
        Font = PharmaTheme.BodyFont,
        ForeColor = PharmaTheme.TextDark,
        Height = FieldHeight,
        Text = value
    };

    private static TextBox CreateNumberInput(string value)
    {
        var box = CreateTextInput(value);
        box.TextAlign = HorizontalAlignment.Center;
        return box;
    }

    private async Task LoadSettingsAsync()
    {
        if (_isLoading)
        {
            return;
        }

        _isLoading = true;
        SetBusy(true);
        try
        {
            var result = await _settingsService.LoadAsync();
            _settingsByKey = result.SettingsByKey;
            _loadedState = result.State.Clone();
            ApplyState(_loadedState);

            if (!string.IsNullOrWhiteSpace(result.ErrorMessage))
            {
                ShowStatus(result.ErrorMessage!, PharmaTheme.WarningStrong);
            }
            else if (result.UsedDefaults)
            {
                ShowStatus(
                    "تم تحميل القيم الافتراضية محلياً. الحفظ على الخادم يتطلب إعدادات مسجّلة في النظام.",
                    PharmaTheme.MutedText);
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
            LayoutSettingsContent();
        }
    }

    private async Task SaveAsync()
    {
        if (_isSaving || _isLoading)
        {
            return;
        }

        var current = CaptureState();
        _isSaving = true;
        SetBusy(true);
        try
        {
            var result = await _settingsService.SaveAsync(current, _loadedState, _settingsByKey);
            if (result.NoChanges)
            {
                MessageBox.Show(
                    FindForm(),
                    result.Message ?? "لا توجد تغييرات لحفظها على الخادم.",
                    "الإعدادات",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            if (result.NotSupported)
            {
                UiFeedback.ShowError(
                    FindForm(),
                    result.Message ?? "حفظ إعدادات النظام غير مدعوم بعد في الواجهة الحالية.");
                return;
            }

            if (!string.IsNullOrWhiteSpace(result.ErrorMessage))
            {
                UiFeedback.ShowError(FindForm(), result.ErrorMessage);
                return;
            }

            if (result.AnySaved)
            {
                _loadedState = current.Clone();
                UiFeedback.ShowSuccess(FindForm(), result.Message ?? "تم الحفظ بنجاح.");
                ShowStatus("تمت مزامنة الإعدادات المحفوظة مع الخادم.", PharmaTheme.Success);
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
            LayoutSettingsContent();
        }
    }

    private void SetCurrency(string code)
    {
        var normalized = string.Equals(code, "USD", StringComparison.OrdinalIgnoreCase) ? "USD" : "SYP";
        _currencySypButton.IsSelected = normalized == "SYP";
        _currencyUsdButton.IsSelected = normalized == "USD";
    }

    private void SetTheme(int index)
    {
        for (var i = 0; i < _themeButtons.Length; i++)
        {
            _themeButtons[i].IsSelected = i == index;
        }
    }

    private void UpdateFontSizeHint()
    {
        var label = _fontSizeTrack.Value switch
        {
            1 => "صغير",
            3 => "كبير",
            _ => "متوسط"
        };
        _fontSizeHintLabel.Text = $"الحجم الحالي: {label}";
    }

    private SettingsFormState CaptureState() => new()
    {
        PharmacyName = _pharmacyNameInput.Text,
        Address = _addressInput.Text,
        Phone = _phoneInput.Text,
        CurrencyCode = _currencySypButton.IsSelected ? "SYP" : "USD",
        ExchangeRate = _exchangeRateInput.Text,
        ThemeIndex = Array.FindIndex(_themeButtons, b => b.IsSelected),
        FontSizeLevel = _fontSizeTrack.Value,
        ExpiryAlertDays = _expiryDaysInput.Text,
        LowStockThreshold = _lowStockInput.Text,
        BackupPath = _backupPathInput.Text,
        AutoBackupSchedule = _autoBackupCombo.SelectedItem?.ToString() ?? "يومياً"
    };

    private void ApplyState(SettingsFormState state)
    {
        _pharmacyNameInput.Text = state.PharmacyName;
        _addressInput.Text = state.Address;
        _phoneInput.Text = state.Phone;
        SetCurrency(state.CurrencyCode);
        _exchangeRateInput.Text = state.ExchangeRate;
        SetTheme(Math.Clamp(state.ThemeIndex, 0, _themeButtons.Length - 1));
        _fontSizeTrack.Value = Math.Clamp(state.FontSizeLevel, _fontSizeTrack.Minimum, _fontSizeTrack.Maximum);
        UpdateFontSizeHint();
        _expiryDaysInput.Text = state.ExpiryAlertDays;
        _lowStockInput.Text = state.LowStockThreshold;
        _backupPathInput.Text = state.BackupPath;

        var scheduleIndex = Math.Max(0, _autoBackupCombo.Items.IndexOf(state.AutoBackupSchedule));
        _autoBackupCombo.SelectedIndex = scheduleIndex >= 0 ? scheduleIndex : 0;
        LayoutSettingsContent();
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
        LayoutSettingsContent();
    }

    private sealed class SettingsCardPanel : Panel
    {
        private readonly Label _titleLabel;
        private readonly Label _iconLabel;
        private readonly int _cornerRadius = PharmaTheme.DashboardSectionCornerRadius;
        private const int HeaderH = SectionHeaderHeight;
        private const int Pad = 22;

        public Rectangle InnerBounds
        {
            get
            {
                var w = Math.Max(0, ClientSize.Width - Pad * 2);
                return new Rectangle(Pad, Pad + HeaderH + 8, w, Math.Max(0, ClientSize.Height - Pad * 2 - HeaderH - 8));
            }
        }

        public SettingsCardPanel(string iconGlyph, string title)
        {
            BackColor = PharmaTheme.CardBackground;
            DoubleBuffered = true;
            Padding = Padding.Empty;
            RightToLeft = RightToLeft.Yes;
            SetStyle(
                ControlStyles.AllPaintingInWmPaint
                    | ControlStyles.OptimizedDoubleBuffer
                    | ControlStyles.ResizeRedraw
                    | ControlStyles.UserPaint,
                true);

            _iconLabel = new Label
            {
                BackColor = PharmaTheme.SurfaceContainer,
                Font = PharmaTheme.IconFont(15f),
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
                Height = SectionHeaderHeight,
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
            _titleLabel.SetBounds(Pad, headerTop + 4, Math.Max(80, Width - Pad * 2 - 48), SectionHeaderHeight);
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
            RoundedDrawing.FillRounded(g, bounds, _cornerRadius, PharmaTheme.CardBackground);
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
            Size = new Size(64, 36);
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
            RoundedDrawing.FillRounded(e.Graphics, bounds, 8, back);
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
            MinimumSize = new Size(88, 76);
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

            var back = _isSelected ? PharmaTheme.SurfaceContainerLow : PharmaTheme.CardBackground;
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
