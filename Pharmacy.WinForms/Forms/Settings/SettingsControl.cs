using System.ComponentModel;
using System.IO;
using Pharmacy.WinForms.Controls;
using Pharmacy.WinForms.Models;
using Pharmacy.WinForms.Services;
using Pharmacy.WinForms.Ui;

namespace Pharmacy.WinForms.Forms.Settings;

internal sealed class SettingsControl : UserControl
{
    private const int FieldHeight = 42;
    private const int LabelLineHeight = 24;
    private const int SectionTitleHeight = 32;
    private const int CardGap = 20;
    private const int ColumnGap = 16;

    private static readonly (string Name, Color Color)[] ThemeOptions =
    [
        ("أخضر صحي", Color.FromArgb(7, 100, 67)),
        ("أزرق طبي", Color.FromArgb(30, 64, 175)),
        ("بنفسجي", Color.FromArgb(107, 33, 168)),
        ("فيروزي", Color.FromArgb(15, 118, 110)),
        ("داكن", Color.FromArgb(24, 24, 27)),
        ("رمادي", Color.FromArgb(82, 82, 91))
    ];

    private readonly SettingsService _settingsService;
    private SettingsFormState _loadedState = new();
    private IReadOnlyDictionary<string, SystemSettingApiModel> _settingsByKey =
        new Dictionary<string, SystemSettingApiModel>();
    private bool _isLoading;
    private bool _isSaving;

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
    private readonly Label _statusLabel;
    private readonly Panel _scrollHost;
    private readonly TableLayoutPanel _contentPanel;
    private readonly FlowLayoutPanel _leftStack;
    private readonly FlowLayoutPanel _rightStack;

    public SettingsControl() : this(AppServices.SettingsService)
    {
    }

    public SettingsControl(SettingsService settingsService)
    {
        _settingsService = settingsService;

        Dock = DockStyle.Fill;
        BackColor = PharmaTheme.SoftGreenBackground;
        RightToLeft = RightToLeft.Yes;
        AutoScroll = false;
        Padding = Padding.Empty;

        _scrollHost = new Panel
        {
            AutoScroll = true,
            BackColor = PharmaTheme.SoftGreenBackground,
            Dock = DockStyle.Fill,
            Padding = Padding.Empty
        };

        _contentPanel = new TableLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 1,
            Dock = DockStyle.Top,
            Margin = Padding.Empty,
            Padding = new Padding(24, 16, 24, 32),
            RightToLeft = RightToLeft.Yes
        };
        _contentPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

        var header = BuildHeader(out var saveButton, out var cancelButton);
        _contentPanel.Controls.Add(header, 0, 0);
        _contentPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        _statusLabel = new Label
        {
            AutoSize = true,
            Font = PharmaTheme.SmallFont,
            ForeColor = PharmaTheme.MutedText,
            Margin = new Padding(0, 0, 0, CardGap),
            MaximumSize = new Size(1200, 0),
            Padding = new Padding(0, 4, 0, 0),
            TextAlign = ContentAlignment.TopRight,
            UseCompatibleTextRendering = true,
            Visible = false
        };
        _contentPanel.Controls.Add(_statusLabel, 0, 1);
        _contentPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var grid = new TableLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 2,
            Dock = DockStyle.Top,
            Margin = Padding.Empty,
            Padding = Padding.Empty,
            RightToLeft = RightToLeft.Yes
        };
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34F));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 66F));
        grid.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        _leftStack = CreateColumnStack();
        _rightStack = CreateColumnStack();

        _pharmacyNameInput = CreateFieldInput("صيدلية الشفاء");
        _addressInput = CreateFieldInput("شارع الاستقلال, البناء 4");
        _phoneInput = CreateFieldInput("011-234-5678");
        _phoneInput.TextAlign = HorizontalAlignment.Left;

        var infoCard = BuildSectionCard(
            SegoeMdl2Icons.Store,
            "معلومات الصيدلية",
            CreateLabeledField("اسم الصيدلية", _pharmacyNameInput),
            CreateLabeledField("العنوان", _addressInput),
            CreateLabeledField("رقم الهاتف", _phoneInput));

        _currencySypButton = new SettingsToggleButton("SYP", selected: true);
        _currencyUsdButton = new SettingsToggleButton("USD", selected: false);
        _currencySypButton.Click += (_, _) => SetCurrency("SYP");
        _currencyUsdButton.Click += (_, _) => SetCurrency("USD");

        var currencyToggleHost = new Panel
        {
            AutoSize = true,
            BackColor = PharmaTheme.SurfaceContainerHighest,
            MinimumSize = new Size(140, 40),
            Padding = new Padding(4)
        };
        var currencyRow = new FlowLayoutPanel
        {
            AutoSize = true,
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            Margin = new Padding(0),
            Padding = new Padding(0),
            WrapContents = false
        };
        currencyRow.Controls.Add(_currencyUsdButton);
        currencyRow.Controls.Add(_currencySypButton);
        currencyToggleHost.Controls.Add(currencyRow);

        _exchangeRateInput = CreateFieldInput("14500");
        _exchangeRateInput.TextAlign = HorizontalAlignment.Left;

        var currencyCard = BuildSectionCard(
            SegoeMdl2Icons.Currency,
            "العملة",
            CreateInlineRow("العملة الافتراضية", currencyToggleHost),
            CreateLabeledField("سعر الصرف (SYP لـ 1 USD)", _exchangeRateInput));

        AddCardToStack(_leftStack, infoCard);
        AddCardToStack(_leftStack, currencyCard);

        _themeButtons = new ThemeOptionButton[ThemeOptions.Length];
        var themeGrid = new TableLayoutPanel
        {
            AutoSize = true,
            ColumnCount = 3,
            Dock = DockStyle.Top,
            Margin = new Padding(0),
            Padding = new Padding(0),
            RightToLeft = RightToLeft.Yes,
            RowCount = 2
        };
        for (var col = 0; col < 3; col++)
        {
            themeGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
        }

        themeGrid.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        themeGrid.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        for (var i = 0; i < ThemeOptions.Length; i++)
        {
            var index = i;
            var option = ThemeOptions[i];
            var button = new ThemeOptionButton(option.Name, option.Color, index == 0)
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Margin = new Padding(4),
                MinimumSize = new Size(88, 80),
                Width = 100
            };
            button.Click += (_, _) => SetTheme(index);
            _themeButtons[i] = button;
            themeGrid.Controls.Add(button, i % 3, i / 3);
        }

        _fontSizeTrack = new TrackBar
        {
            AutoSize = false,
            Dock = DockStyle.Fill,
            Height = 36,
            LargeChange = 1,
            Margin = new Padding(0, 8, 0, 0),
            Maximum = 3,
            Minimum = 1,
            RightToLeft = RightToLeft.No,
            TickStyle = TickStyle.None,
            Value = 2
        };
        _fontSizeTrack.ValueChanged += (_, _) => UpdateFontSizeHint();

        _fontSizeHintLabel = new Label
        {
            AutoSize = true,
            Font = PharmaTheme.SmallFont,
            ForeColor = PharmaTheme.MutedText,
            Margin = new Padding(0, 6, 0, 0),
            TextAlign = ContentAlignment.MiddleCenter
        };
        UpdateFontSizeHint();

        var appearanceBody = new TableLayoutPanel
        {
            AutoSize = true,
            ColumnCount = 2,
            Dock = DockStyle.Top,
            Margin = new Padding(0),
            Padding = new Padding(0),
            RightToLeft = RightToLeft.Yes
        };
        appearanceBody.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58F));
        appearanceBody.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42F));
        appearanceBody.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        appearanceBody.Controls.Add(CreateSubBlock("نسق الألوان", themeGrid), 0, 0);
        appearanceBody.Controls.Add(CreateSubBlock("حجم الخط", BuildFontSizeRow()), 1, 0);

        var appearanceCard = BuildSectionCard(
            SegoeMdl2Icons.Palette,
            "المظهر",
            appearanceBody);

        _expiryDaysInput = CreateCompactNumberInput("90");
        _lowStockInput = CreateCompactNumberInput("5");

        var alertsCard = BuildSectionCard(
            SegoeMdl2Icons.Warning,
            "التنبيهات",
            CreateAlertRow("تحذير انتهاء الصلاحية", "قبل كم يوم يتم التنبيه", _expiryDaysInput, "يوم"),
            CreateAlertRow("حد النقص في المخزون", "التنبيه عند وصول الكمية إلى", _lowStockInput, "علبة"));

        _backupPathInput = CreateFieldInput(@"D:\PharmacyBackups");
        _backupPathInput.ReadOnly = true;
        _backupPathInput.TextAlign = HorizontalAlignment.Left;

        var browseFolderButton = new Button
        {
            AutoSize = false,
            BackColor = PharmaTheme.SurfaceContainerHighest,
            FlatStyle = FlatStyle.Flat,
            Font = PharmaTheme.IconFont(12f),
            ForeColor = PharmaTheme.PrimaryGreen,
            Size = new Size(44, 36),
            Text = SegoeMdl2Icons.Folder,
            UseCompatibleTextRendering = true
        };
        browseFolderButton.FlatAppearance.BorderSize = 0;
        browseFolderButton.Click += (_, _) => BrowseBackupFolder();

        _backupPathInput.Height = FieldHeight;
        _backupPathInput.MinimumSize = new Size(120, FieldHeight);

        var backupPathRow = new TableLayoutPanel
        {
            AutoSize = true,
            ColumnCount = 2,
            Dock = DockStyle.Top,
            Margin = new Padding(0, 4, 0, 0),
            RightToLeft = RightToLeft.Yes
        };
        backupPathRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        backupPathRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 48F));
        backupPathRow.RowStyles.Add(new RowStyle(SizeType.Absolute, FieldHeight));
        backupPathRow.Controls.Add(_backupPathInput, 0, 0);
        backupPathRow.Controls.Add(browseFolderButton, 1, 0);
        browseFolderButton.Height = FieldHeight;
        browseFolderButton.Dock = DockStyle.Fill;

        _autoBackupCombo = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = PharmaTheme.BodyFont,
            Height = FieldHeight,
            MinimumSize = new Size(120, FieldHeight),
            Width = 140
        };
        _autoBackupCombo.Items.AddRange(["يومياً", "أسبوعياً", "شهرياً"]);
        _autoBackupCombo.SelectedIndex = 0;

        var backupNowButton = new Button
        {
            AutoSize = false,
            BackColor = PharmaTheme.SurfaceContainerLow,
            Dock = DockStyle.Top,
            FlatStyle = FlatStyle.Flat,
            Font = PharmaTheme.ArabicFont(10f, FontStyle.Bold),
            ForeColor = PharmaTheme.PrimaryGreen,
            Height = 42,
            Margin = new Padding(0, 10, 0, 0),
            MinimumSize = new Size(200, 42),
            Text = "إنشاء نسخة الآن",
            TextAlign = ContentAlignment.MiddleCenter,
            UseCompatibleTextRendering = true
        };
        backupNowButton.FlatAppearance.BorderColor = PharmaTheme.PrimaryGreen;
        backupNowButton.FlatAppearance.BorderSize = 1;
        backupNowButton.Click += (_, _) =>
            UiFeedback.ShowFeatureNotAvailable(FindForm(), "ميزة النسخ الاحتياطي");

        var backupCard = BuildSectionCard(
            SegoeMdl2Icons.Backup,
            "النسخ الاحتياطي",
            CreateLabeledField("مسار الحفظ المحلي", backupPathRow),
            CreateInlineRow("النسخ التلقائي", _autoBackupCombo),
            backupNowButton);

        var rightBottom = new TableLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 2,
            Dock = DockStyle.Top,
            Margin = Padding.Empty,
            Padding = Padding.Empty,
            RightToLeft = RightToLeft.Yes
        };
        rightBottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        rightBottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        rightBottom.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        alertsCard.Margin = new Padding(0, 0, ColumnGap / 2, 0);
        backupCard.Margin = new Padding(ColumnGap / 2, 0, 0, 0);
        rightBottom.Controls.Add(alertsCard, 0, 0);
        rightBottom.Controls.Add(backupCard, 1, 0);

        AddCardToStack(_rightStack, appearanceCard);
        _rightStack.Controls.Add(rightBottom);

        _leftStack.Margin = new Padding(0, 0, ColumnGap, 0);
        grid.Controls.Add(_leftStack, 0, 0);
        grid.Controls.Add(_rightStack, 1, 0);
        _contentPanel.Controls.Add(grid, 0, 2);
        _contentPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        _scrollHost.Controls.Add(_contentPanel);
        Controls.Add(_scrollHost);

        saveButton.Click += async (_, _) => await SaveAsync();
        cancelButton.Click += (_, _) => ApplyState(_loadedState.Clone());

        Load += async (_, _) => await LoadSettingsAsync();
        _scrollHost.Resize += (_, _) => LayoutContentWidth();
        SizeChanged += (_, _) => LayoutContentWidth();
        LayoutContentWidth();
    }

    private static FlowLayoutPanel CreateColumnStack() => new()
    {
        AutoSize = true,
        AutoSizeMode = AutoSizeMode.GrowAndShrink,
        FlowDirection = FlowDirection.TopDown,
        Margin = Padding.Empty,
        Padding = Padding.Empty,
        WrapContents = false
    };

    private static void AddCardToStack(FlowLayoutPanel stack, Control card)
    {
        card.AutoSize = true;
        if (card is SettingsCardPanel settingsCard)
        {
            settingsCard.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        }

        card.Dock = DockStyle.Top;
        card.Margin = new Padding(0, 0, 0, CardGap);
        stack.Controls.Add(card);
    }

    private void LayoutContentWidth()
    {
        var available = Math.Max(360, _scrollHost.ClientSize.Width);
        _contentPanel.Width = available;
        _contentPanel.MinimumSize = new Size(available, 0);

        var innerWidth = available - _contentPanel.Padding.Horizontal;
        var leftWidth = Math.Max(280, (int)((innerWidth - ColumnGap) * 0.34));
        var rightWidth = Math.Max(360, innerWidth - ColumnGap - leftWidth);
        _leftStack.Width = leftWidth;
        _rightStack.Width = rightWidth;
        _statusLabel.MaximumSize = new Size(Math.Max(280, innerWidth), 0);
        ApplyStackChildWidths(_leftStack);
        ApplyStackChildWidths(_rightStack);
    }

    private static void ApplyStackChildWidths(FlowLayoutPanel stack)
    {
        var childWidth = Math.Max(200, stack.ClientSize.Width - stack.Padding.Horizontal);
        foreach (Control child in stack.Controls)
        {
            child.Width = childWidth;
        }
    }

    private Control BuildHeader(out GradientRoundedButton saveButton, out Button cancelButton)
    {
        var header = new TableLayoutPanel
        {
            AutoSize = true,
            ColumnCount = 2,
            Dock = DockStyle.Top,
            Margin = new Padding(0, 0, 0, CardGap),
            RightToLeft = RightToLeft.Yes
        };
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 300F));

        var titleStack = new FlowLayoutPanel
        {
            AutoSize = true,
            FlowDirection = FlowDirection.TopDown,
            Margin = new Padding(0),
            WrapContents = false
        };
        titleStack.Controls.Add(new Label
        {
            AutoSize = true,
            Font = PharmaTheme.ArabicFont(16f, FontStyle.Bold),
            ForeColor = PharmaTheme.PrimaryGreen,
            Text = "الإعدادات",
            UseCompatibleTextRendering = true
        });
        titleStack.Controls.Add(new Label
        {
            AutoSize = true,
            Font = PharmaTheme.SmallFont,
            ForeColor = PharmaTheme.OnSurfaceVariant,
            Margin = new Padding(0, 4, 0, 0),
            Text = "تكوين النظام وتفضيلات الصيدلية.",
            UseCompatibleTextRendering = true
        });

        var actions = new FlowLayoutPanel
        {
            Anchor = AnchorStyles.Left | AnchorStyles.Top,
            AutoSize = true,
            FlowDirection = FlowDirection.RightToLeft,
            Margin = new Padding(0, 6, 0, 0),
            WrapContents = false
        };

        saveButton = new GradientRoundedButton
        {
            IconGlyph = SegoeMdl2Icons.Save,
            MinimumSize = new Size(150, 42),
            Text = "حفظ التغييرات",
            Width = 160
        };

        cancelButton = new Button
        {
            AutoSize = false,
            BackColor = PharmaTheme.SurfaceContainer,
            FlatStyle = FlatStyle.Flat,
            Font = PharmaTheme.BodyFont,
            ForeColor = PharmaTheme.TextDark,
            Height = 42,
            Margin = new Padding(0, 0, 8, 0),
            Text = "إلغاء",
            UseCompatibleTextRendering = true,
            Width = 96
        };
        cancelButton.FlatAppearance.BorderSize = 0;

        actions.Controls.Add(saveButton);
        actions.Controls.Add(cancelButton);

        header.Controls.Add(titleStack, 0, 0);
        header.Controls.Add(actions, 1, 0);
        return header;
    }

    private static SettingsCardPanel BuildSectionCard(string iconGlyph, string title, params Control[] rows)
    {
        var card = new SettingsCardPanel();

        var root = new TableLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 1,
            Dock = DockStyle.Top,
            Margin = Padding.Empty,
            RightToLeft = RightToLeft.Yes,
            RowCount = 1 + rows.Length
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.Controls.Add(BuildSectionHeader(iconGlyph, title), 0, 0);

        for (var i = 0; i < rows.Length; i++)
        {
            var row = rows[i];
            row.Dock = DockStyle.Top;
            row.Margin = new Padding(0, 0, 0, i == rows.Length - 1 ? 0 : 12);
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.Controls.Add(row, 0, i + 1);
        }

        card.Controls.Add(root);
        return card;
    }

    private static Control BuildSectionHeader(string iconGlyph, string title)
    {
        var header = new TableLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 2,
            Dock = DockStyle.Top,
            Margin = new Padding(0, 0, 0, 16),
            MinimumSize = new Size(0, SectionTitleHeight),
            RightToLeft = RightToLeft.Yes
        };
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 40F));
        header.RowStyles.Add(new RowStyle(SizeType.Absolute, SectionTitleHeight));

        var iconBadge = new Panel
        {
            BackColor = PharmaTheme.SurfaceContainer,
            Dock = DockStyle.Fill,
            Margin = new Padding(0),
            MinimumSize = new Size(40, 40),
            Size = new Size(40, 40)
        };
        iconBadge.Controls.Add(new Label
        {
            Dock = DockStyle.Fill,
            Font = PharmaTheme.IconFont(15f),
            ForeColor = PharmaTheme.PrimaryGreen,
            Text = iconGlyph,
            TextAlign = ContentAlignment.MiddleCenter,
            UseCompatibleTextRendering = true
        });

        header.Controls.Add(new Label
        {
            Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top,
            AutoSize = false,
            Dock = DockStyle.Fill,
            Font = PharmaTheme.SectionFont,
            ForeColor = PharmaTheme.TextDark,
            Margin = new Padding(0, 8, 10, 0),
            Text = title,
            TextAlign = ContentAlignment.MiddleRight,
            UseCompatibleTextRendering = true
        }, 0, 0);
        header.Controls.Add(iconBadge, 1, 0);
        return header;
    }

    private static Control CreateLabeledField(string label, Control input)
    {
        var panel = new TableLayoutPanel
        {
            AutoSize = true,
            ColumnCount = 1,
            Dock = DockStyle.Top,
            Margin = new Padding(0, 0, 0, 10),
            RightToLeft = RightToLeft.Yes
        };
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, FieldHeight));

        var caption = new Label
        {
            AutoSize = true,
            Dock = DockStyle.Fill,
            Font = PharmaTheme.SmallFont,
            ForeColor = PharmaTheme.OnSurfaceVariant,
            Margin = new Padding(0, 0, 0, 6),
            MinimumSize = new Size(0, LabelLineHeight),
            Text = label,
            TextAlign = ContentAlignment.MiddleRight,
            UseCompatibleTextRendering = true
        };

        input.Dock = DockStyle.Fill;
        input.Margin = new Padding(0);
        if (input is TextBox textBox)
        {
            textBox.Height = FieldHeight;
            textBox.MinimumSize = new Size(0, FieldHeight);
        }

        panel.Controls.Add(caption, 0, 0);
        panel.Controls.Add(input, 0, 1);
        return panel;
    }

    private static Control CreateSubBlock(string label, Control content)
    {
        var panel = new TableLayoutPanel
        {
            AutoSize = true,
            ColumnCount = 1,
            Dock = DockStyle.Top,
            Margin = new Padding(0, 0, 0, 8),
            RightToLeft = RightToLeft.Yes
        };
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        panel.Controls.Add(new Label
        {
            AutoSize = true,
            Font = PharmaTheme.ArabicFont(10f, FontStyle.Bold),
            ForeColor = PharmaTheme.TextDark,
            Margin = new Padding(0, 0, 0, 6),
            Text = label,
            UseCompatibleTextRendering = true
        }, 0, 0);
        content.Dock = DockStyle.Top;
        panel.Controls.Add(content, 0, 1);
        return panel;
    }

    private static Control CreateInlineRow(string label, Control right)
    {
        var row = new TableLayoutPanel
        {
            AutoSize = true,
            ColumnCount = 2,
            Dock = DockStyle.Top,
            Margin = new Padding(0, 0, 0, 10),
            RightToLeft = RightToLeft.Yes
        };
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 52F));
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 48F));
        row.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        row.Controls.Add(new Label
        {
            Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top,
            AutoSize = false,
            Dock = DockStyle.Fill,
            Font = PharmaTheme.BodyFont,
            ForeColor = PharmaTheme.TextDark,
            Margin = new Padding(0, 10, 8, 0),
            Text = label,
            TextAlign = ContentAlignment.MiddleRight,
            UseCompatibleTextRendering = true
        }, 0, 0);
        right.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
        right.Dock = DockStyle.Fill;
        row.Controls.Add(right, 1, 0);
        return row;
    }

    private static Control CreateAlertRow(string title, string subtitle, Control input, string unit)
    {
        var row = new TableLayoutPanel
        {
            AutoSize = true,
            BackColor = PharmaTheme.SurfaceContainerLow,
            ColumnCount = 2,
            Dock = DockStyle.Top,
            Margin = new Padding(0, 0, 0, 8),
            MinimumSize = new Size(0, 58),
            Padding = new Padding(12, 10, 12, 10),
            RightToLeft = RightToLeft.Yes
        };
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        row.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        var textStack = new FlowLayoutPanel
        {
            AutoSize = true,
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false
        };
        textStack.Controls.Add(new Label
        {
            AutoSize = true,
            Font = PharmaTheme.ArabicFont(10f, FontStyle.Bold),
            ForeColor = PharmaTheme.TextDark,
            Text = title,
            UseCompatibleTextRendering = true
        });
        textStack.Controls.Add(new Label
        {
            AutoSize = true,
            Font = PharmaTheme.SmallFont,
            ForeColor = PharmaTheme.OnSurfaceVariant,
            Margin = new Padding(0, 2, 0, 0),
            Text = subtitle,
            UseCompatibleTextRendering = true
        });

        var valueHost = new FlowLayoutPanel
        {
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false
        };
        valueHost.Controls.Add(new Label
        {
            AutoSize = true,
            Font = PharmaTheme.SmallFont,
            ForeColor = PharmaTheme.OnSurfaceVariant,
            Margin = new Padding(4, 6, 0, 0),
            Text = unit,
            UseCompatibleTextRendering = true
        });
        valueHost.Controls.Add(input);

        row.Controls.Add(textStack, 0, 0);
        row.Controls.Add(valueHost, 1, 0);
        return row;
    }

    private Control BuildFontSizeRow()
    {
        var row = new TableLayoutPanel
        {
            AutoSize = true,
            ColumnCount = 1,
            Dock = DockStyle.Top,
            Margin = new Padding(0),
            RightToLeft = RightToLeft.Yes
        };
        row.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        row.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var trackRow = new TableLayoutPanel
        {
            AutoSize = true,
            ColumnCount = 3,
            Dock = DockStyle.Top,
            Margin = new Padding(0),
            RightToLeft = RightToLeft.No
        };
        trackRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 28F));
        trackRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        trackRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 28F));
        trackRow.Controls.Add(new Label
        {
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Font = PharmaTheme.SmallFont,
            ForeColor = PharmaTheme.OnSurfaceVariant,
            Text = "A",
            TextAlign = ContentAlignment.MiddleCenter,
            UseCompatibleTextRendering = true
        }, 0, 0);
        trackRow.Controls.Add(_fontSizeTrack, 1, 0);
        trackRow.Controls.Add(new Label
        {
            AutoSize = true,
            Anchor = AnchorStyles.Right,
            Font = PharmaTheme.ArabicFont(14f),
            ForeColor = PharmaTheme.OnSurfaceVariant,
            Text = "A",
            TextAlign = ContentAlignment.MiddleCenter,
            UseCompatibleTextRendering = true
        }, 2, 0);

        _fontSizeHintLabel.Dock = DockStyle.Fill;
        _fontSizeHintLabel.TextAlign = ContentAlignment.MiddleCenter;
        row.Controls.Add(trackRow, 0, 0);
        row.Controls.Add(_fontSizeHintLabel, 0, 1);
        return row;
    }

    private static TextBox CreateFieldInput(string value) => new()
    {
        BackColor = PharmaTheme.SurfaceContainerHighest,
        BorderStyle = BorderStyle.FixedSingle,
        Font = PharmaTheme.BodyFont,
        ForeColor = PharmaTheme.TextDark,
        Height = FieldHeight,
        MinimumSize = new Size(0, FieldHeight),
        Text = value
    };

    private static TextBox CreateCompactNumberInput(string value)
    {
        var box = CreateFieldInput(value);
        box.MinimumSize = new Size(52, FieldHeight);
        box.Width = 52;
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
                ShowStatus("تم تحميل القيم الافتراضية محلياً. الحفظ على الخادم يتطلب إعدادات مسجّلة في النظام.", PharmaTheme.MutedText);
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
                UiFeedback.ShowError(FindForm(), result.Message ?? "حفظ إعدادات النظام غير مدعوم بعد في الواجهة الحالية.");
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
        LayoutContentWidth();
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
            Size = new Size(64, 32);
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
            Size = new Size(100, 80);
            Margin = new Padding(4);
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
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
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
                new Rectangle(bounds.X, circle.Bottom + 4, bounds.Width, bounds.Height - circle.Bottom - 6),
                _isSelected ? PharmaTheme.TextDark : PharmaTheme.OnSurfaceVariant,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.Top | TextFormatFlags.WordBreak);
        }
    }

    private sealed class SettingsCardPanel : Panel
    {
        private readonly int _cornerRadius = PharmaTheme.DashboardSectionCornerRadius;

        public SettingsCardPanel()
        {
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            BackColor = PharmaTheme.CardBackground;
            DoubleBuffered = true;
            Padding = new Padding(22, 20, 22, 22);
            SetStyle(
                ControlStyles.AllPaintingInWmPaint
                    | ControlStyles.OptimizedDoubleBuffer
                    | ControlStyles.ResizeRedraw
                    | ControlStyles.UserPaint,
                true);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            using var brush = new SolidBrush(PharmaTheme.SoftGreenBackground);
            e.Graphics.FillRectangle(brush, ClientRectangle);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            var bounds = ClientRectangle;
            bounds.Inflate(-2, -2);
            if (bounds.Width <= 2 || bounds.Height <= 2)
            {
                return;
            }

            RoundedDrawing.DrawSoftShadow(g, bounds, _cornerRadius, PharmaTheme.DashboardCardShadow);
            RoundedDrawing.FillRounded(g, bounds, _cornerRadius, PharmaTheme.CardBackground);
            RoundedDrawing.DrawRoundedBorder(g, bounds, _cornerRadius, PharmaTheme.BorderSoft);
        }
    }
}
