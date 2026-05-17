using System.ComponentModel;
using System.Drawing.Drawing2D;
using Pharmacy.WinForms.Ui;

namespace Pharmacy.WinForms.Controls;

public sealed class StatCardControl : Control
{
    private string _title = string.Empty;
    private string _value = "0";
    private string? _subtitle;
    private string _iconText = "●";

    public StatCardControl()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
        Size = new Size(200, 108);
        Font = PharmaTheme.BodyFont;
        BackColor = PharmaTheme.Background;
        Padding = new Padding(16, 14, 16, 14);
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string CardTitle
    {
        get => _title;
        set { _title = value; Invalidate(); }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string CardValue
    {
        get => _value;
        set { _value = value; Invalidate(); }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string? CardSubtitle
    {
        get => _subtitle;
        set { _subtitle = value; Invalidate(); }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string IconText
    {
        get => _iconText;
        set { _iconText = value; Invalidate(); }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

        var bounds = ClientRectangle;
        bounds.Inflate(-2, -2);
        using var path = RoundedRect(bounds, 14);
        using var shadow = new SolidBrush(Color.FromArgb(18, 0, 0, 0));
        e.Graphics.FillPath(shadow, RoundedRect(new Rectangle(bounds.X, bounds.Y + 2, bounds.Width, bounds.Height), 14));
        using var fill = new SolidBrush(PharmaTheme.CardBackground);
        e.Graphics.FillPath(fill, path);
        using var border = new Pen(PharmaTheme.BorderLight);
        e.Graphics.DrawPath(border, path);

        var iconRect = new Rectangle(bounds.Right - 52, bounds.Y + 14, 36, 36);
        using var iconBg = new SolidBrush(PharmaTheme.SoftGreenBackground);
        e.Graphics.FillEllipse(iconBg, iconRect);
        TextRenderer.DrawText(
            e.Graphics,
            _iconText,
            PharmaTheme.SectionFont,
            iconRect,
            PharmaTheme.PrimaryGreen,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

        var textLeft = bounds.X + Padding.Left;
        var textWidth = bounds.Width - Padding.Horizontal - 48;

        var titleRect = new Rectangle(textLeft, bounds.Y + 12, textWidth, 22);
        TextRenderer.DrawText(
            e.Graphics,
            _title,
            PharmaTheme.SmallFont,
            titleRect,
            PharmaTheme.MutedText,
            TextFormatFlags.Right | TextFormatFlags.EndEllipsis);

        var valueRect = new Rectangle(textLeft, bounds.Y + 34, textWidth, 34);
        TextRenderer.DrawText(
            e.Graphics,
            _value,
            PharmaTheme.StatValueFont,
            valueRect,
            PharmaTheme.TextDark,
            TextFormatFlags.Right | TextFormatFlags.EndEllipsis);

        if (!string.IsNullOrWhiteSpace(_subtitle))
        {
            var subRect = new Rectangle(textLeft, bounds.Bottom - 28, textWidth, 20);
            TextRenderer.DrawText(
                e.Graphics,
                _subtitle!,
                PharmaTheme.SmallFont,
                subRect,
                PharmaTheme.MutedText,
                TextFormatFlags.Right | TextFormatFlags.EndEllipsis);
        }
    }

    private static GraphicsPath RoundedRect(Rectangle bounds, int radius)
    {
        var path = new GraphicsPath();
        var d = radius * 2;
        path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
        path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
        path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
        path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }
}
