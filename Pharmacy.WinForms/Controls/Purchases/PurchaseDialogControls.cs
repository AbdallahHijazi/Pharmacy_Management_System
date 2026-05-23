using System.ComponentModel;
using System.Drawing.Drawing2D;
using Pharmacy.WinForms.Forms.Purchases;
using Pharmacy.WinForms.Ui;

namespace Pharmacy.WinForms.Controls.Purchases;

internal sealed class PurSectionCard : PurRoundedPanel
{
    private readonly Label _titleLabel;
    private readonly Panel _bodyPanel;

    public PurSectionCard(string title)
    {
        _titleLabel = new Label
        {
            Text = title,
            AutoSize = false,
            Height = 28,
            Dock = DockStyle.Top,
            TextAlign = ContentAlignment.MiddleRight,
            Font = PharmaTheme.SectionFont,
            ForeColor = PharmaTheme.TextDark,
            Padding = new Padding(0, 0, 0, 4)
        };

        _bodyPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.Transparent,
            Padding = new Padding(20, 4, 20, 20)
        };

        Controls.Add(_bodyPanel);
        Controls.Add(_titleLabel);
        Padding = new Padding(16, 14, 16, 0);
    }

    public Panel Body => _bodyPanel;

    public new void ApplyThemeVisuals()
    {
        FillColor = PharmaTheme.Surface;
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
        Height = 20;
        Dock = DockStyle.Top;
        TextAlign = ContentAlignment.MiddleRight;
        Font = PharmaTheme.SmallFont;
        ForeColor = PharmaTheme.OnSurfaceVariant;
        Margin = new Padding(0, 0, 0, 6);
    }

    public void ApplyThemeVisuals()
    {
        ForeColor = PharmaTheme.OnSurfaceVariant;
        Font = PharmaTheme.SmallFont;
    }
}

internal sealed class PurInputHost : Panel
{
    private bool _focused;

    public PurInputHost(Control inner)
    {
        DoubleBuffered = true;
        Height = 44;
        MinimumSize = new Size(80, 44);
        Padding = new Padding(12, 6, 12, 6);
        BackColor = Color.Transparent;

        inner.Dock = DockStyle.Fill;
        inner.Font = PharmaTheme.BodyFont;
        if (inner is TextBox tb)
        {
            tb.BorderStyle = BorderStyle.None;
            tb.BackColor = PharmaTheme.InputSurface;
            tb.ForeColor = PharmaTheme.TextDark;
            tb.GotFocus += (_, _) => { _focused = true; Invalidate(); };
            tb.LostFocus += (_, _) => { _focused = false; Invalidate(); };
        }
        else if (inner is ComboBox cb)
        {
            cb.FlatStyle = FlatStyle.Flat;
            cb.BackColor = PharmaTheme.InputSurface;
            cb.ForeColor = PharmaTheme.TextDark;
            cb.GotFocus += (_, _) => { _focused = true; Invalidate(); };
            cb.LostFocus += (_, _) => { _focused = false; Invalidate(); };
        }
        else if (inner is NumericUpDown nud)
        {
            nud.BorderStyle = BorderStyle.None;
            nud.BackColor = PharmaTheme.InputSurface;
            nud.ForeColor = PharmaTheme.TextDark;
            nud.GotFocus += (_, _) => { _focused = true; Invalidate(); };
            nud.LostFocus += (_, _) => { _focused = false; Invalidate(); };
        }
        else if (inner is DateTimePicker dtp)
        {
            dtp.Format = DateTimePickerFormat.Short;
            dtp.RightToLeftLayout = true;
            dtp.GotFocus += (_, _) => { _focused = true; Invalidate(); };
            dtp.LostFocus += (_, _) => { _focused = false; Invalidate(); };
        }

        Controls.Add(inner);
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
    }

    public void ApplyThemeVisuals()
    {
        foreach (Control c in Controls)
        {
            if (c is TextBox tb)
            {
                tb.BackColor = PharmaTheme.InputSurface;
                tb.ForeColor = PharmaTheme.TextDark;
            }
            else if (c is ComboBox cb)
            {
                cb.BackColor = PharmaTheme.InputSurface;
                cb.ForeColor = PharmaTheme.TextDark;
            }
            else if (c is NumericUpDown nud)
            {
                nud.BackColor = PharmaTheme.InputSurface;
                nud.ForeColor = PharmaTheme.TextDark;
            }
        }

        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var r = ClientRectangle;
        r.Inflate(-1, -1);
        RoundedDrawing.FillRounded(g, r, 12, PharmaTheme.InputSurface);
        RoundedDrawing.DrawRoundedBorder(
            g,
            r,
            12,
            _focused ? PharmaTheme.Primary : PharmaTheme.BorderSoft,
            _focused ? 1.75f : 1f);
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        e.Graphics.Clear(Parent?.BackColor ?? PharmaTheme.Surface);
    }
}

internal sealed class PurFieldStack : Panel
{
    public PurFieldStack(string label, Control input, int bottomMargin = 14)
    {
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        Dock = DockStyle.Top;
        Margin = new Padding(0, 0, 0, bottomMargin);
        BackColor = Color.Transparent;

        var caption = new PurFieldLabel(label);
        var host = input is PurInputHost existingHost
            ? existingHost
            : new PurInputHost(input) { Dock = DockStyle.Top };

        Controls.Add(host);
        Controls.Add(caption);
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

    public PurSummaryRow(string caption, bool emphasize = false)
    {
        Height = emphasize ? 36 : 30;
        Dock = DockStyle.Top;
        Margin = new Padding(0, 0, 0, 8);
        BackColor = Color.Transparent;

        _caption = new Label
        {
            Text = caption,
            Dock = DockStyle.Right,
            Width = 140,
            TextAlign = ContentAlignment.MiddleRight,
            Font = PharmaTheme.SmallFont,
            ForeColor = PharmaTheme.OnSurfaceVariant
        };
        _value = new Label
        {
            Text = "—",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = emphasize
                ? PharmaTheme.ArabicFont(14f, FontStyle.Bold)
                : PharmaTheme.ArabicFont(11f, FontStyle.Bold),
            ForeColor = emphasize ? PharmaTheme.Primary : PharmaTheme.TextDark
        };

        Controls.Add(_value);
        Controls.Add(_caption);
    }

    public Label ValueLabel => _value;

    public void ApplyThemeVisuals(bool emphasize)
    {
        _caption.ForeColor = PharmaTheme.OnSurfaceVariant;
        _caption.Font = PharmaTheme.SmallFont;
        _value.ForeColor = emphasize ? PharmaTheme.Primary : PharmaTheme.TextDark;
        _value.Font = emphasize
            ? PharmaTheme.ArabicFont(14f, FontStyle.Bold)
            : PharmaTheme.ArabicFont(11f, FontStyle.Bold);
    }
}

internal sealed class PurItemsHeaderRow : Control
{
    public PurItemsHeaderRow()
    {
        Height = 32;
        Dock = DockStyle.Top;
        Margin = new Padding(0, 0, 0, 8);
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        var bounds = ClientRectangle;
        using var back = new SolidBrush(PharmaTheme.PrimaryContainer);
        g.FillRectangle(back, bounds);

        var cols = CreatePurchaseInvoiceLineControl.GetColumnRects(bounds);
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
            PharmaTheme.ArabicFont(9f, FontStyle.Bold),
            Rectangle.Inflate(rect, -4, 0),
            PharmaTheme.PrimaryDark,
            TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
    }
}
