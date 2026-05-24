using System.ComponentModel;

using System.Drawing.Drawing2D;

using Pharmacy.WinForms.Forms.Purchases;

using Pharmacy.WinForms.Ui;



namespace Pharmacy.WinForms.Controls.Purchases;



internal static class PurItemColumnLayout

{

    public const int MinTableWidth = 1060;

    public const int RowHeight = 68;

    public const int HeaderHeight = 40;

    public const int CellGap = 10;

    public const int EdgePad = 10;



    public const int RemoveWidth = 58;

    public const int SubtotalWidth = 100;

    public const int PriceWidth = 96;

    public const int BonusWidth = 76;

    public const int QuantityWidth = 76;

    public const int ExpiryWidth = 120;

    public const int BatchWidth = 120;



    public static int ProductWidth(int tableWidth)

    {

        var fixedSum = RemoveWidth + SubtotalWidth + PriceWidth + BonusWidth + QuantityWidth

            + ExpiryWidth + BatchWidth + EdgePad * 2 + CellGap * 7;

        return Math.Max(160, tableWidth - fixedSum);

    }



    public static CreatePurchaseInvoiceLineControl.ColumnLayout GetColumnRects(Rectangle bounds, int tableWidth)

    {

        var width = Math.Max(tableWidth, MinTableWidth);

        var pad = EdgePad;

        var gap = CellGap;



        var productW = ProductWidth(width);

        var x = bounds.Right - pad - productW;

        var product = new Rectangle(x, bounds.Y, productW, bounds.Height);

        x -= BatchWidth + gap;

        var batch = new Rectangle(x, bounds.Y, BatchWidth, bounds.Height);

        x -= ExpiryWidth + gap;

        var expiry = new Rectangle(x, bounds.Y, ExpiryWidth, bounds.Height);

        x -= QuantityWidth + gap;

        var quantity = new Rectangle(x, bounds.Y, QuantityWidth, bounds.Height);

        x -= BonusWidth + gap;

        var bonus = new Rectangle(x, bounds.Y, BonusWidth, bounds.Height);

        x -= PriceWidth + gap;

        var price = new Rectangle(x, bounds.Y, PriceWidth, bounds.Height);

        x -= SubtotalWidth + gap;

        var subtotal = new Rectangle(x, bounds.Y, SubtotalWidth, bounds.Height);

        var removeH = 36;
        var removeY = bounds.Y + Math.Max(0, (bounds.Height - removeH) / 2);
        var remove = new Rectangle(bounds.X + pad, removeY, RemoveWidth, removeH);



        return new CreatePurchaseInvoiceLineControl.ColumnLayout(

            product, batch, expiry, quantity, bonus, price, subtotal, remove);

    }

}



internal sealed class PurSectionCard : PurRoundedPanel

{

    private readonly Label _titleLabel;

    private readonly Panel _bodyPanel;



    public PurSectionCard(string title) : base(PharmaTheme.PurchasesCardCornerRadius, drawShadow: false)

    {

        BorderColor = PharmaTheme.BorderSoft;

        _titleLabel = new Label

        {

            Text = title,

            AutoSize = false,

            Height = 30,

            Dock = DockStyle.Top,

            TextAlign = ContentAlignment.MiddleRight,

            Font = PharmaTheme.SectionFont,

            ForeColor = PharmaTheme.TextDark,

            Padding = new Padding(0, 0, 0, 6),

            BackColor = Color.Transparent

        };



        _bodyPanel = new Panel

        {

            Dock = DockStyle.Fill,

            BackColor = Color.Transparent,

            Padding = new Padding(22, 6, 22, 22)

        };



        Controls.Add(_bodyPanel);

        Controls.Add(_titleLabel);

        Padding = new Padding(18, 12, 18, 0);

    }



    public Panel Body => _bodyPanel;



    public new void ApplyThemeVisuals()

    {

        FillColor = PharmaTheme.Surface;

        BorderColor = PharmaTheme.BorderSoft;

        _titleLabel.ForeColor = PharmaTheme.TextDark;

        _titleLabel.Font = PharmaTheme.SectionFont;

        base.ApplyThemeVisuals();

    }

}



internal sealed class PurFieldLabel : Label

{

    public PurFieldLabel(string text)

    {

        Text = text;

        AutoSize = false;

        Height = 22;

        TextAlign = ContentAlignment.MiddleRight;

        Font = PharmaTheme.SmallFont;

        ForeColor = PharmaTheme.MutedText;

        BackColor = Color.Transparent;

    }



    public void ApplyThemeVisuals()

    {

        ForeColor = PharmaTheme.MutedText;

        Font = PharmaTheme.SmallFont;

    }

}



internal sealed class PurInputHost : Panel

{

    private bool _focused;

    private readonly Control _inner;



    public PurInputHost(Control inner)

    {

        _inner = inner;

        DoubleBuffered = true;

        Height = 44;

        MinimumSize = new Size(80, 44);

        Padding = new Padding(10, 5, 10, 5);

        BackColor = Color.Transparent;



        inner.Dock = DockStyle.Fill;

        inner.Font = PharmaTheme.BodyFont;

        inner.BackColor = PharmaTheme.SurfaceContainerHigh;



        if (inner is TextBox tb)

        {

            tb.BorderStyle = BorderStyle.None;

            tb.ForeColor = PharmaTheme.TextDark;

            tb.GotFocus += (_, _) => SetFocused(true);

            tb.LostFocus += (_, _) => SetFocused(false);

        }

        else if (inner is ComboBox cb)

        {

            cb.FlatStyle = FlatStyle.Flat;

            cb.ForeColor = PharmaTheme.TextDark;

            cb.GotFocus += (_, _) => SetFocused(true);

            cb.LostFocus += (_, _) => SetFocused(false);

        }

        else if (inner is NumericUpDown nud)

        {

            nud.BorderStyle = BorderStyle.None;

            nud.ForeColor = PharmaTheme.TextDark;

            nud.RightToLeft = RightToLeft.No;

            nud.GotFocus += (_, _) => SetFocused(true);

            nud.LostFocus += (_, _) => SetFocused(false);

        }

        else if (inner is DateTimePicker dtp)

        {

            dtp.Format = DateTimePickerFormat.Short;

            dtp.RightToLeftLayout = true;

            dtp.CalendarForeColor = PharmaTheme.TextDark;

            dtp.GotFocus += (_, _) => SetFocused(true);

            dtp.LostFocus += (_, _) => SetFocused(false);

        }

        else if (inner is Label lbl)

        {

            lbl.ForeColor = PharmaTheme.TextDark;

            lbl.TextAlign = ContentAlignment.MiddleRight;

        }



        Controls.Add(inner);

        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);

    }



    public Control Inner => _inner;



    private void SetFocused(bool focused)

    {

        _focused = focused;

        Invalidate();

    }



    public void ApplyThemeVisuals()

    {

        _inner.BackColor = PharmaTheme.SurfaceContainerHigh;

        if (_inner is TextBox tb)

        {

            tb.ForeColor = PharmaTheme.TextDark;

        }

        else if (_inner is ComboBox cb)

        {

            cb.ForeColor = PharmaTheme.TextDark;

        }

        else if (_inner is NumericUpDown nud)

        {

            nud.ForeColor = PharmaTheme.TextDark;

        }

        else if (_inner is Label lbl)

        {

            lbl.ForeColor = PharmaTheme.TextDark;

        }



        Invalidate();

    }



    protected override void OnPaint(PaintEventArgs e)

    {

        var g = e.Graphics;

        g.SmoothingMode = SmoothingMode.AntiAlias;

        var r = ClientRectangle;

        r.Inflate(-1, -1);

        RoundedDrawing.FillRounded(g, r, 10, PharmaTheme.SurfaceContainerHigh);

        RoundedDrawing.DrawRoundedBorder(

            g,

            r,

            10,

            _focused ? PharmaTheme.Primary : PharmaTheme.BorderSoft,

            _focused ? 1.75f : 1f);

    }



    protected override void OnPaintBackground(PaintEventArgs e)

    {

        var back = Parent?.BackColor ?? PharmaTheme.Surface;

        if (back.A == 0)

        {

            back = PharmaTheme.Surface;

        }



        e.Graphics.Clear(back);

    }

}



internal sealed class PurFieldStack : Panel

{

    private readonly PurFieldLabel _caption;

    private readonly PurInputHost _host;



    public PurFieldStack(string label, Control input)

    {

        BackColor = Color.Transparent;

        Height = 70;

        MinimumSize = new Size(120, 70);



        _caption = new PurFieldLabel(label);

        _host = input is PurInputHost existingHost

            ? existingHost

            : new PurInputHost(input);



        Controls.Add(_host);

        Controls.Add(_caption);

        Resize += (_, _) => LayoutStack();

        LayoutStack();

    }



    public PurInputHost Host => _host;



    public void ApplyThemeVisuals()

    {

        _caption.ApplyThemeVisuals();

        _host.ApplyThemeVisuals();

    }



    private void LayoutStack()

    {

        _caption.SetBounds(0, 0, Width, 22);

        _host.SetBounds(0, 26, Width, 44);

    }

}



internal sealed class PurCancelButton : Control

{

    public PurCancelButton()

    {

        Text = "إلغاء";

        Height = 48;

        MinimumSize = new Size(120, 48);

        Cursor = Cursors.Hand;

        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.StandardClick, true);

    }



    public void ApplyThemeVisuals() => Invalidate();



    protected override void OnPaintBackground(PaintEventArgs e) =>

        e.Graphics.Clear(Parent?.BackColor ?? PharmaTheme.Background);



    protected override void OnPaint(PaintEventArgs e)

    {

        var g = e.Graphics;

        g.SmoothingMode = SmoothingMode.AntiAlias;

        var b = ClientRectangle;

        b.Inflate(-1, -1);

        RoundedDrawing.FillRounded(g, b, 12, PharmaTheme.Surface);

        RoundedDrawing.DrawRoundedBorder(g, b, 12, PharmaTheme.BorderSoft, 1f);

        TextRenderer.DrawText(

            g,

            Text,

            PharmaTheme.ArabicFont(10.5f, FontStyle.Bold),

            b,

            PharmaTheme.TextDark,

            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.RightToLeft);

    }

}



internal sealed class PurSummaryRow : Panel

{

    private readonly Label _caption;

    private readonly Label _value;

    private readonly bool _emphasize;



    public PurSummaryRow(string caption, bool emphasize = false)

    {

        _emphasize = emphasize;

        Height = emphasize ? 38 : 32;

        Dock = DockStyle.Top;

        Margin = new Padding(0, 0, 0, 12);

        BackColor = Color.Transparent;



        _caption = new Label

        {

            Text = caption,

            Dock = DockStyle.Right,

            Width = 150,

            TextAlign = ContentAlignment.MiddleRight,

            Font = PharmaTheme.SmallFont,

            ForeColor = PharmaTheme.MutedText,

            BackColor = Color.Transparent

        };

        _value = new Label

        {

            Text = "—",

            Dock = DockStyle.Fill,

            TextAlign = ContentAlignment.MiddleLeft,

            Font = emphasize

                ? PharmaTheme.ArabicFont(15f, FontStyle.Bold)

                : PharmaTheme.ArabicFont(11.5f, FontStyle.Bold),

            ForeColor = emphasize ? PharmaTheme.Primary : PharmaTheme.TextDark,

            Padding = new Padding(8, 0, 0, 0),

            BackColor = Color.Transparent

        };



        Controls.Add(_value);

        Controls.Add(_caption);

    }



    public Label ValueLabel => _value;



    public void ApplyThemeVisuals(bool? emphasizeOverride = null)

    {

        var emphasize = emphasizeOverride ?? _emphasize;

        _caption.ForeColor = PharmaTheme.MutedText;

        _caption.Font = PharmaTheme.SmallFont;

        _value.ForeColor = emphasize ? PharmaTheme.Primary : PharmaTheme.TextDark;

        _value.Font = emphasize

            ? PharmaTheme.ArabicFont(15f, FontStyle.Bold)

            : PharmaTheme.ArabicFont(11.5f, FontStyle.Bold);

    }

}



internal sealed class PurItemsHeaderRow : Control

{

    private int _tableWidth = PurItemColumnLayout.MinTableWidth;



    public PurItemsHeaderRow()

    {

        Height = PurItemColumnLayout.HeaderHeight;

        Dock = DockStyle.Top;

        Margin = new Padding(0, 4, 0, 10);

        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);

    }



    public void SetTableWidth(int width)

    {

        _tableWidth = Math.Max(width, PurItemColumnLayout.MinTableWidth);

        Width = _tableWidth;

        Invalidate();

    }



    protected override void OnPaintBackground(PaintEventArgs e) =>

        e.Graphics.Clear(Parent?.BackColor ?? PharmaTheme.Surface);



    protected override void OnPaint(PaintEventArgs e)

    {

        var g = e.Graphics;

        g.SmoothingMode = SmoothingMode.AntiAlias;

        var bounds = ClientRectangle;

        RoundedDrawing.FillRounded(g, bounds, 10, PharmaTheme.PrimaryContainer);



        var cols = PurItemColumnLayout.GetColumnRects(bounds, _tableWidth);

        Draw(g, cols.Product, "المنتج");

        Draw(g, cols.Batch, "التشغيلة");

        Draw(g, cols.Expiry, "الصلاحية");

        Draw(g, cols.Quantity, "الكمية");

        Draw(g, cols.Bonus, "المجاني");

        Draw(g, cols.Price, "السعر");

        Draw(g, cols.Subtotal, "الإجمالي");

        Draw(g, cols.Remove, "حذف");

    }



    private static void Draw(Graphics g, Rectangle rect, string text)

    {

        TextRenderer.DrawText(

            g,

            text,

            PharmaTheme.ArabicFont(9.5f, FontStyle.Bold),

            Rectangle.Inflate(rect, -6, 0),

            PharmaTheme.PrimaryDark,

            TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

    }

}



internal sealed class PurItemsTableHost : Panel

{

    public PurItemsTableHost()

    {

        AutoScroll = true;

        BackColor = PharmaTheme.Surface;

        Dock = DockStyle.Fill;

        DoubleBuffered = true;

    }



    public int TableWidth { get; private set; } = PurItemColumnLayout.MinTableWidth;



    public void SyncTableWidth(int viewportWidth)

    {

        TableWidth = Math.Max(viewportWidth, PurItemColumnLayout.MinTableWidth);

    }



    protected override void OnPaintBackground(PaintEventArgs e) =>

        e.Graphics.Clear(PharmaTheme.Surface);

}


