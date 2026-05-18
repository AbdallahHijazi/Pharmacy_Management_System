using System.ComponentModel;
using System.Drawing.Drawing2D;
using Pharmacy.WinForms.Ui;

namespace Pharmacy.WinForms.Controls;

public enum StatCardVisualTone
{
    Normal,
    Warning,
    Danger
}

public sealed class StatCardControl : Control
{
    private string _title = string.Empty;
    private string _value = "0";
    private string? _subtitle;
    private string _iconText = "●";
    private StatCardVisualTone _tone = StatCardVisualTone.Normal;

    public StatCardControl()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
        Size = new Size(200, 112);
        Font = PharmaTheme.BodyFont;
        BackColor = PharmaTheme.Background;
        Padding = new Padding(16, 14, 16, 14);
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string CardTitle
    {
        get => _title;
        set
        {
            _title = value;
            Invalidate();
        }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string CardValue
    {
        get => _value;
        set
        {
            _value = value;
            Invalidate();
        }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string? CardSubtitle
    {
        get => _subtitle;
        set
        {
            _subtitle = value;
            Invalidate();
        }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string IconText
    {
        get => _iconText;
        set
        {
            _iconText = value;
            Invalidate();
        }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public StatCardVisualTone VisualTone
    {
        get => _tone;
        set
        {
            _tone = value;
            Invalidate();
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

        var radius = PharmaTheme.DashboardStatCornerRadius;
        var bounds = ClientRectangle;
        bounds.Inflate(-2, -2);
        using var path = RoundedRect(bounds, radius);

        using (var shadow = new SolidBrush(PharmaTheme.DashboardCardShadow))
        {
            e.Graphics.FillPath(shadow, RoundedRect(new Rectangle(bounds.X, bounds.Y + 2, bounds.Width, bounds.Height), radius));
        }

        using (var fill = new SolidBrush(PharmaTheme.SurfaceContainerLowest))
        {
            e.Graphics.FillPath(fill, path);
        }

        if (_tone != StatCardVisualTone.Normal)
        {
            var ring = _tone == StatCardVisualTone.Danger
                ? Color.FromArgb(55, PharmaTheme.Danger)
                : Color.FromArgb(50, PharmaTheme.Warning);
            using var ringPen = new Pen(ring, 1.5f);
            e.Graphics.DrawPath(ringPen, path);
        }
        else
        {
            using var border = new Pen(PharmaTheme.OutlineVariant);
            e.Graphics.DrawPath(border, path);
        }

        var (iconBack, iconFore) = _tone switch
        {
            StatCardVisualTone.Danger => (PharmaTheme.ErrorContainer, PharmaTheme.Danger),
            StatCardVisualTone.Warning => (PharmaTheme.WarningSurface, PharmaTheme.WarningStrong),
            _ => (PharmaTheme.SurfaceContainerLow, PharmaTheme.PrimaryGreen)
        };

        var iconRect = new Rectangle(bounds.Right - 52, bounds.Y + 14, 36, 36);
        using (var iconBg = new SolidBrush(iconBack))
        {
            using var iconPath = RoundedRect(iconRect, 10);
            e.Graphics.FillPath(iconBg, iconPath);
        }

        TextRenderer.DrawText(
            e.Graphics,
            _iconText,
            PharmaTheme.SectionFont,
            iconRect,
            iconFore,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

        var textLeft = bounds.X + Padding.Left;
        var textWidth = bounds.Width - Padding.Horizontal - 48;

        var titleRect = new Rectangle(textLeft, bounds.Y + 12, textWidth, 24);
        TextRenderer.DrawText(
            e.Graphics,
            _title,
            PharmaTheme.SmallFont,
            titleRect,
            PharmaTheme.OnSurfaceVariant,
            TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

        var valueColor = _tone == StatCardVisualTone.Danger ? PharmaTheme.Danger : PharmaTheme.TextDark;
        var valueRect = new Rectangle(textLeft, bounds.Y + 36, textWidth, 36);
        TextRenderer.DrawText(
            e.Graphics,
            _value,
            PharmaTheme.StatValueFont,
            valueRect,
            valueColor,
            TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

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
        var d = Math.Min(radius * 2, Math.Min(bounds.Width, bounds.Height));
        path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
        path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
        path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
        path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }
}
